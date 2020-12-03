using PasswordManagerAppResourceServer.Dtos;

using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManagerAppResourceServer.Responses
{
    public class ShareLoginModelResponse
    {
        public LoginDataDto LoginData { get; set; }
        public string ReceiverEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
