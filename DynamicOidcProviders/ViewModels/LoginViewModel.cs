using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicOidcProviders
{
    public class LoginViewModel
    {
        public IEnumerable<AuthenticationScheme> AvailableExternalProviders { get; set; }
        public string ReturnUrl { get; set; }

        public LoginViewModel()
        {
            AvailableExternalProviders = new List<AuthenticationScheme>();
        }
    }
}
