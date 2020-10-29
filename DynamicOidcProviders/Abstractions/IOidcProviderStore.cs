using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public interface IOidcProviderStore
    {
        Task<OidcProvider> GetById(string oidcProviderId);
        Task<IEnumerable<OidcProvider>> GetAll();
        Task Create(OidcProvider newProvider);
        Task Delete(string oidcProviderId);
        Task<bool> IsExisting(string oidcProviderId);
        Task<IEnumerable<string>> GetAllIds();
    }
}
