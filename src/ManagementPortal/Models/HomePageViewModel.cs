using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementPortal.Models
{
    public class HomePageViewModel
    {
        public bool Authorized { get; set; }

        public string Pin { get; set; }

        public string RemoteConsoleUrl { get; set; }

        public string Output { get; set; }
    }
}
