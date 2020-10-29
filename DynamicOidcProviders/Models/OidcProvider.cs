using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class OidcProvider
    {
        // We'll use this ID as the authentication scheme's name.
        public string OidcProviderId { get; set; }
        public string Name { get; set; }

        public DateTime CreationDate { get; set; }
        public string AuthorityUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> ScopesToRequest { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string ExpectedResponseType { get; set; }

        public OidcProvider()
        {
            this.ScopesToRequest = new List<string>();
        }
    }
}
