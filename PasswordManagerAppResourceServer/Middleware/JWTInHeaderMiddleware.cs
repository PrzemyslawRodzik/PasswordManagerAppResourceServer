using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientApp.Middleware
{
    public class JWTInHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public JWTInHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {

            var tokenString = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6Inp4Y0B6eGMuY29tIiwiQWRtaW4iOiIwIiwiVHdvRmFjdG9yQXV0aCI6IjAiLCJQYXNzd29yZE5vdGlmaWNhdGlvbnMiOiIwIiwiQXV0aFRpbWUiOiIzMCIsIm5iZiI6MTYwMTEwNzUwNCwiZXhwIjoxNjAxMTExMTA0LCJpc3MiOiJodHRwczovL2xvY2FsaG9zdDo0NDMyNC8iLCJhdWQiOiJodHRwczovL2xvY2FsaG9zdDo0NDMyNC8ifQ.TV_UskXgHFW6aLseGAHewyFyJZDGnx9bAVQR79oDnMU";
            if (!context.Request.Headers.ContainsKey("Authorization"))
                context.Request.Headers.Append("Authorization", "Bearer " + tokenString);
            




            await _next.Invoke(context);
        }
    }
}
