using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.Handlers
{
    public class CustomJwtDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string algorithm;
        private readonly TokenValidationParameters validationParameters;
        private readonly IConfiguration _config;
        



        public CustomJwtDataFormat(string algorithm, TokenValidationParameters validationParameters, IConfiguration config)
        {
            this.algorithm = algorithm;
            this.validationParameters = validationParameters;
            _config = config;
            
        }

        public AuthenticationTicket Unprotect(string protectedText)
            => Unprotect(protectedText, null);

        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = null;
            SecurityToken validToken = null;

            try
            {
                principal = handler.ValidateToken(protectedText, this.validationParameters, out validToken);

                var validJwt = validToken as JwtSecurityToken;

                if (validJwt == null)
                {
                    throw new ArgumentException("Invalid JWT");
                }

                if (!validJwt.Header.Alg.Equals(algorithm, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Algorithm must be '{algorithm}'");
                }

            }
            catch (SecurityTokenValidationException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            // Token validation passed
            return new AuthenticationTicket(principal, new Microsoft.AspNetCore.Authentication.AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public string Protect(AuthenticationTicket data) => Protect(data, null);

        public string Protect(AuthenticationTicket data, string purpose)
        {
            
            int expiration = Convert.ToInt32(_config.GetSection("JwtSettings:Expiration").Value);

            var jwt = new JwtSecurityToken(
                issuer: validationParameters.ValidIssuer,
                audience: validationParameters.ValidAudience,
                claims: data.Principal.Claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(expiration),
                signingCredentials: new SigningCredentials(validationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            
            return encodedJwt;
        }
    }
}
