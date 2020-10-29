using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class MyOidcPostconfigurator : IPostConfigureOptions<OpenIdConnectOptions>
    {
        private readonly IOidcProviderStore _oidcProviderStore;

        public MyOidcPostconfigurator(IOidcProviderStore oidcProviderStore)
        {
            _oidcProviderStore = oidcProviderStore;
        }

        public void PostConfigure(string name, OpenIdConnectOptions options)
        {
            OidcProvider provider = 
                Task.Run<OidcProvider>(async () => {
                    OidcProvider foundProvider = await _oidcProviderStore.GetById(name);
                    return foundProvider;
                })
                .Result;

            if (provider != null)
            {
                options.SignInScheme = "ExternalCookie";
                options.Authority = provider.AuthorityUrl;
                options.ClientId = provider.ClientId;
                options.ClientSecret = provider.ClientSecret;
                options.ResponseType = provider.ExpectedResponseType;
                options.RequireHttpsMetadata = provider.RequireHttpsMetadata;

                // Callback paths must be unique per provider
                options.CallbackPath = $"/callbacks/oidc/{provider.OidcProviderId}/signin";
                options.SignedOutCallbackPath = $"/callbacks/oidc/{provider.OidcProviderId}/signout";

                options.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = async context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();
                    }
                };
            }
            else
            {
                throw new InvalidOperationException("Trying to use an unexisting OIDC provider");
            }
        }
    }
}
