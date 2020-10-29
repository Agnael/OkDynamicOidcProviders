using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DynamicOidcProviders.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthenticationSchemeProvider _authSchemeProvider;
        private readonly IHttpContextAccessor _httpContext;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthenticationSchemeProvider authSchemeProvider,
            IHttpContextAccessor httpContext)
        {
            _logger = 
                logger
                ?? throw new ArgumentNullException(nameof(logger));

            _authSchemeProvider = 
                authSchemeProvider
                ?? throw new ArgumentNullException(nameof(authSchemeProvider));

            _httpContext = 
                httpContext
                ?? throw new ArgumentNullException(nameof(httpContext));
        }

        [HttpGet("/login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "~/";

            IEnumerable<AuthenticationScheme> registeredSchemes =
                await _authSchemeProvider.GetRequestHandlerSchemesAsync();

            IEnumerable<AuthenticationScheme> remoteSchemes =
                registeredSchemes
                .Where(scheme => typeof(IAuthenticationRequestHandler).IsAssignableFrom(scheme.HandlerType));

            LoginViewModel loginVm = new LoginViewModel();
            loginVm.AvailableExternalProviders = remoteSchemes;
            loginVm.ReturnUrl = returnUrl;

            return View(loginVm);
        }

        [HttpGet("/logout")]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            await _httpContext.HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [HttpGet("/challenge")]
        public async Task<IActionResult> StartChallenge(string scheme, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) 
                returnUrl = "~/";

            AuthenticationScheme foundScheme = await _authSchemeProvider.GetSchemeAsync(scheme);

            if (foundScheme == null)
                throw new InvalidOperationException($"Can't challenge the unexisting '{scheme}' scheme");

            if (!Url.IsLocalUrl(returnUrl))
                throw new Exception("Invalid return URL");

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                // Actual return url the external provider will be calling when authentication is finished
                RedirectUri = Url.Action(nameof(EndChallenge)),

                // Some metadata to use internally in our system once the external provider returned to us
                Items =
                {
                    { "scheme", scheme },
                    { "returnUrl", returnUrl }
                }
            };

            return Challenge(props, scheme);
        }

        [HttpGet]
        public async Task<IActionResult> EndChallenge()
        {
            // read external identity from the temporary cookie
            var externalAuthResult = await HttpContext.AuthenticateAsync("ExternalCookie");

            if (externalAuthResult?.Succeeded != true)
                throw new Exception("External authentication error");

            var localClaims = new List<Claim>();
            var localAuthProps = new AuthenticationProperties();

            // If the external system sent a session ID claim, it must be saved to use it for single sign out
            var externalSessionId = externalAuthResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);

            if (externalSessionId != null)
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, externalSessionId.Value));

            // If the external provider issued an id_token, we'll keep it for signout
            var id_token = externalAuthResult.Properties.GetTokenValue("id_token");

            if (id_token != null)
                localAuthProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });

            ClaimsPrincipal externalPrincipal = externalAuthResult.Principal;

            // The external SubjectId may be present in one of a few different claim types,
            // although this are the most common ones
            Claim subjectIdClaim = externalPrincipal.FindFirst(JwtClaimTypes.Subject);

            if (subjectIdClaim == null)
                subjectIdClaim = externalPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if(subjectIdClaim == null)
                throw new Exception("Couldn't determine the external SubjectId");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var externalClaims = externalPrincipal.Claims.ToList();
            externalClaims.Remove(subjectIdClaim);

            // Deletes the temporary cookie used on external authentication
            await HttpContext.SignOutAsync("ExternalCookie");

            // LOCAL SIGN IN
            // For the sake of simplicity, the external SubjectId will be reused as the local one as well, 
            // since we don't have our own local user database
            ClaimsIdentity localIdentity = new ClaimsIdentity(subjectIdClaim.Value);
            localIdentity.AddClaims(externalClaims);
            localIdentity.AddClaims(localClaims);

            ClaimsPrincipal localPrincipal = new ClaimsPrincipal(localIdentity);

            await HttpContext.SignInAsync(localPrincipal, localAuthProps);

            // Redirects using the metadata provided at the start of the challenge
            var returnUrl = externalAuthResult.Properties.Items["returnUrl"] ?? "~/";

            return Redirect(returnUrl);
        }
    }
}
