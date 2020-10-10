using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public IEnumerable<string> Messages { get; set; }



        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
