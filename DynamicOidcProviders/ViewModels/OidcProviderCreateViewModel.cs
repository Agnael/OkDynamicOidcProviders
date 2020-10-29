using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class OidcProviderCreateViewModel
    {
        [Required]
        public string ProviderId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string AuthorityUrl { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }

        [Required]
        public string ExpectedResponseType { get; set; }

        [Required]
        public string ScopesToRequest { get; set; }

        [Required]
        public bool RequireHttpsMetadata { get; set; }
    }
}
