using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddAuthentication(o =>
                {
                    o.DefaultScheme = AzureADDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = AzureADDefaults.OpenIdScheme;
                    o.DefaultForbidScheme = AzureADDefaults.AuthenticationScheme;
                })
                .AddAzureAD(options => Configuration.Bind("AzureAd", options));

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.ResponseType = "id_token code";
                options.Authority += "/v2.0/";

                options.Scope.Add("User.Read");
                options.Scope.Add("Directory.Read.All");

                options.SaveTokens = true;

                options.Events = new OpenIdConnectEvents()
                {
                    OnAuthorizationCodeReceived = async context =>
                    {
                        // After authorization code has been received.
                        // Do you want to execute something here?
                        await Task.CompletedTask;
                    }
                };
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
