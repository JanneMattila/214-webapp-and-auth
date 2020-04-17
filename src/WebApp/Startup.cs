using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    Configuration.Bind("AzureAd", options);

                    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    options.Authority += "/v2.0/";

                    options.Scope.Add("User.Read");
                    options.Scope.Add("Directory.Read.All");

                    options.UseTokenLifetime = true;
                    options.SaveTokens = true;
                    options.TokenValidationParameters.NameClaimType = "name";

                    options.Events = new OpenIdConnectEvents()
                    {
                        OnAuthorizationCodeReceived = async context =>
                        {
                            // After authorization code has been received.
                            // Do you want to execute something here?
                            await Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            // After authorization code has been received.
                            // Do you want to execute something here?
                            var claims = new List<Claim>()
                            {
                                new Claim("demotype", "demovalue1")
                            };
                            context.Principal.AddIdentity(new ClaimsIdentity(claims));
                            await Task.CompletedTask;
                        }
                    };
                });

            // See "Products" page which uses this policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomClaim", policy =>
                {
                    policy.RequireClaim("demotype", "demovalue1", "demovalue2");
                });
            });

            services.AddControllers();
            services.AddRazorPages();
        }

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
                endpoints.MapRazorPages();
            });
        }
    }
}
