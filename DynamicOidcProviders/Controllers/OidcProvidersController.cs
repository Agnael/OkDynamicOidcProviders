using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DynamicOidcProviders.Controllers
{
    [Authorize]
    public class OidcProvidersController : Controller
    {
        private readonly ILogger<OidcProvidersController> _logger;
        private readonly IOidcProviderStore _oidcProviderStore;

        public OidcProvidersController(
            ILogger<OidcProvidersController> logger,
            IOidcProviderStore oidcProviderStore)
        {
            _logger = logger;
            _oidcProviderStore = oidcProviderStore;
        }

        [HttpGet("/oidc-providers")]
        public async Task<IActionResult> List()
        {
            var providers = await _oidcProviderStore.GetAll();

            var providerPms = 
                providers.Select(x => new OidcProviderPeak(x.OidcProviderId, x.Name));

            return View(providerPms);
        }

        [HttpGet("/oidc-providers/create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("/oidc-providers/create")]
        public IActionResult Create(OidcProviderCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                OidcProvider newProvider = new OidcProvider();
                newProvider.OidcProviderId = vm.ProviderId.ToLower();
                newProvider.Name = vm.Name;
                newProvider.AuthorityUrl = vm.AuthorityUrl;
                newProvider.ClientId = vm.ClientId;
                newProvider.ClientSecret = vm.ClientSecret;
                newProvider.CreationDate = DateTime.UtcNow;
                newProvider.ExpectedResponseType = vm.ExpectedResponseType;
                newProvider.RequireHttpsMetadata = vm.RequireHttpsMetadata;
                newProvider.ScopesToRequest = vm.ScopesToRequest.Split(" ").ToList();

                _oidcProviderStore.Create(newProvider);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.ToString());
                return View(vm);
            }

            return RedirectToAction(nameof(List));
        }

        [HttpGet("/oidc-providers/{providerId}/edit")]
        public IActionResult Edit(OidcProvider newProvider)
        {
            return View();
        }

        [HttpGet("/oidc-providers/{providerId}/delete")]
        public IActionResult Delete([FromRoute] string providerId)
        {
            return RedirectToAction(nameof(List));
        }
    }
}
