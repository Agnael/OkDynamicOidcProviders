using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class SimpleUser
    {
        public string UserId { get; set; }
        public string PlainPassword { get; set; }

        public List<ExternalCredential> ExternalCredentials { get; set; }

        public SimpleUser()
        {
            ExternalCredentials = new List<ExternalCredential>();
        }
    }
}
