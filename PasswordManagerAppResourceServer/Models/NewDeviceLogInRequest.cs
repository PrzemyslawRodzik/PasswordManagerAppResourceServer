using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Models
{
    public class NewDeviceLogInRequest
    {
        public string IpAddress { get; set; }
        public string GuidDevice { get; set; }
        public int UserId { get; set; }
        public string OSName { get; set; }
        public string BrowserName { get; set; }
        
    }
}
