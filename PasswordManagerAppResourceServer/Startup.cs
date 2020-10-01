using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EmailService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PasswordManagerAppResourceServer.Data;
using PasswordManagerAppResourceServer.Handlers;
using PasswordManagerAppResourceServer.Interfaces;
using PasswordManagerAppResourceServer.Models;
using PasswordManagerAppResourceServer.Services;

namespace PasswordManagerAppResourceServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
           
        }
        
     

        public IConfiguration Configuration { get; }
        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var emailConfig = Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);
            services.AddHttpContextAccessor();
            services.AddDataProtection();


            services.AddSingleton<RsaSecurityKey>(provider => {
                // It's required to register the RSA key with depedency injection.
                // If you don't do this, the RSA instance will be prematurely disposed.

                RSA rsa = RSA.Create();
                rsa.ImportRSAPublicKey(
                    source: Convert.FromBase64String(Configuration["JwtSettings:Asymmetric:PublicKey"]),
                    bytesRead: out int _
                );

                return new RsaSecurityKey(rsa);
            });

            SecurityKey rsa = services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("JwtSettings:SecretEncyptionKey").Value));
            var tokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = rsa,
                TokenDecryptionKey = symmetricSecurityKey,
                ValidateIssuer = true,
                ValidIssuer = Configuration.GetSection("JwtSettings:Issuer").Value,
                ValidateAudience = true,
                ValidAudience = Configuration.GetSection("JwtSettings:Audience").Value,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                
               
            };
            services.AddAuthentication(x=>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
                {
                   config.TokenValidationParameters = tokenValidationParameters;
                    

                }); 
            
         
                  /*  options.Events.OnValidatePrincipal = async (context) =>
                    {
                // This refreshes the token everytime a validated request comes in.
                // This assumes sliding expiration token.
                var now = DateTime.UtcNow;
                        var claims = context.Principal.Claims.Where(claim => claim.Type != JwtRegisteredClaimNames.Jti && claim.Type != JwtRegisteredClaimNames.Iat && claim.Type != JwtRegisteredClaimNames.Aud).ToList();
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                // We use the static extension methods because of name clash
                await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context.HttpContext);
                        await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(context.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    };
                }); */
          

            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
			
			services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("SqliteConnection")));
			
			// Scoped services ->
			
			services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailSender, EmailSender>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
           
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
