using Microsoft.AspNetCore.Http;
using PasswordManagerAppResourceServer.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PasswordManagerAppResourceServer.CustomExceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionMiddleware(RequestDelegate next)
        {
            
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            
            int httpStatusCode = (int)HttpStatusCode.InternalServerError;

            if (ex is AuthenticationException || ex is UserServiceException)
                httpStatusCode = (int)HttpStatusCode.BadRequest;


            context.Response.ContentType = "application/json";
            context.Response.StatusCode = httpStatusCode;

            return context.Response.WriteAsync(new ApiResponse()
            {
                Success = false,
                Messages = new string[] { ex.Message }
            }.ToString()
            );
        }
    }
}
