using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class OidcProviderPeak
    {
        public string OidcProviderId { get; set; }
        public string Name { get; set; }

        public OidcProviderPeak()
        {

        }

        public OidcProviderPeak(string oidcProviderId, string name)
        {
            this.OidcProviderId = oidcProviderId;
            this.Name = name;
        }
    }
}
