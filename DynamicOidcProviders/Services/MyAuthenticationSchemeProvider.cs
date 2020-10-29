using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class MyAuthenticationSchemeProvider : AuthenticationSchemeProvider
    {
        private readonly IOidcProviderStore _oidcProviderStore;
        private readonly Type _oidcHandlerType;

        public MyAuthenticationSchemeProvider(
                IOptions<AuthenticationOptions> options,
                IOidcProviderStore oidcProviderStore)
            : base(options)
        {
            this._oidcProviderStore = oidcProviderStore;

            if (this._oidcProviderStore == null)
                throw new ArgumentNullException($"An implementation of {typeof(IOidcProviderStore).FullName} must be registered in the dependency container");

            this._oidcHandlerType = typeof(OpenIdConnectHandler);
        }

        public override async Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
        {
            List<AuthenticationScheme> schemeList = new List<AuthenticationScheme>();
            IEnumerable<string> dynamicOidcProviderIds = await _oidcProviderStore.GetAllIds();

            foreach (string oidcProviderId in dynamicOidcProviderIds)
                schemeList.Add(new AuthenticationScheme(oidcProviderId, oidcProviderId, _oidcHandlerType));

            return schemeList;
        }

        public override async Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            AuthenticationScheme scheme = await base.GetSchemeAsync(name);

            if (scheme == null)
            {
                bool isExisting = await _oidcProviderStore.IsExisting(name);

                if (isExisting)
                    scheme = new AuthenticationScheme(name, name, _oidcHandlerType);
            }

            return scheme;
        }

        public override async Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
        {
            List<AuthenticationScheme> allSchemes = new List<AuthenticationScheme>();

            IEnumerable<AuthenticationScheme> localSchemes = await base.GetAllSchemesAsync();

            if (localSchemes != null)
                allSchemes.AddRange(localSchemes);

            IEnumerable<string> dynamicOidcProviderIds = await _oidcProviderStore.GetAllIds();

            if (dynamicOidcProviderIds != null)
            {
                foreach (string oidcProviderId in dynamicOidcProviderIds)
                    allSchemes.Add(new AuthenticationScheme(oidcProviderId, oidcProviderId, _oidcHandlerType));
            }

            return allSchemes;
        }
    }
}