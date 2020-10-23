using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Responses
{
    public class FailedResponse
    {
       
            public IEnumerable<string> Errors { get; set; }
        
    }
}
