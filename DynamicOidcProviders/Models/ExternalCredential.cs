using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class ExternalCredential
    {
        public string OidcProviderId { get; set; }
        public string ExternalSubjectId { get; set; }
    }
}
