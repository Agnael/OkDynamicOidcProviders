using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class OidcProviderInMemoryStore : IOidcProviderStore
    {
        private readonly List<OidcProvider> _providers;
        private static readonly object _lock = new object();

        public OidcProviderInMemoryStore(List<OidcProvider> providers)
        {
            _providers = providers;
        }

        public async Task Create(OidcProvider newProvider)
        {
            _providers.Add(newProvider);
        }

        public async Task<OidcProvider> GetById(string oidcProviderId)
        {
            OidcProvider foundProvider = 
                _providers.FirstOrDefault(x => x.OidcProviderId == oidcProviderId);

            return foundProvider;
        }

        public async Task Delete(string oidcProviderId)
        {
            int foundProviderIdx =
                _providers.FindIndex(x => x.OidcProviderId == oidcProviderId);

            if(foundProviderIdx != -1)
                _providers.RemoveAt(foundProviderIdx);
        }

        public async Task<bool> IsExisting(string oidcProviderId)
        {
            bool isExisting = 
                _providers.Any(x => x.OidcProviderId == oidcProviderId);

            return isExisting;
        }

        public async Task<IEnumerable<string>> GetAllIds()
        {
            IEnumerable<string> allProviderIds =
                _providers.Select(x => x.OidcProviderId);

            return allProviderIds;
        }

        public async Task<IEnumerable<OidcProvider>> GetAll()
        {
            return _providers;
        }
    }
}
