using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementPortal.Models
{
    public class AuthorizationSettings
    {
        public string PinHash { get; private set; }

        public AuthorizationSettings(IConfiguration configuration)
        {
            PinHash = configuration.GetValue<string>("PinHash");
        }
    }
}
