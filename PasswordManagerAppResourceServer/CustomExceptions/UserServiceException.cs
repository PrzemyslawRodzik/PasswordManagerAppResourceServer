using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.CustomExceptions
{
    public class UserServiceException : Exception
    {
        public UserServiceException()
        {
        }

        public UserServiceException(string message)
            : base(message)
        {
        }

        public UserServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
