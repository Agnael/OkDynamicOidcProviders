using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DynamicOidcProviders
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // IMPORTANT: this postconfigurator MUST be registered BEFORE the "AddOpenIdConnect()" call.
            services.AddTransient<IPostConfigureOptions<OpenIdConnectOptions>, MyOidcPostconfigurator>();

            services
                .AddAuthentication("LocalCookie")   // *Sets the the default scheme.
                .AddCookie("LocalCookie", conf => { // *Registers a cookie for local login.
                    conf.LoginPath = "/login";
                    conf.LogoutPath = "/logout";
                })           
                .AddCookie("ExternalCookie")        // *Registers a cookie for external login.
                .AddOpenIdConnect(                  // *Registers a single OIDC handler that will
                    "oidcHandlerHub",               //    process ALL of our providers, getting
                    "oidcHandlerHub",               //    their configuration on runtime depending on 
                    _ => { });                      //    which one is currently getting challenged.

            services.AddTransient<IAuthenticationSchemeProvider, MyAuthenticationSchemeProvider>();

            // Registers provider store, seeded with the configuration we previously had
            // in the authentication service configuration.
            // Its a singleton because its a non-static in-memory store
            services.AddSingleton<IOidcProviderStore>(_ => 
                new OidcProviderInMemoryStore(
                    new List<OidcProvider> {
                        new OidcProvider
                        {
                            OidcProviderId = "google",
                            Name = "Google",
                            AuthorityUrl = "https://accounts.google.com",
                            ClientId = "<YOUR GOOGLE CLIENT_ID>",
                            ClientSecret = "<YOUR GOOGLE CLIENT_SECRET>",
                            ExpectedResponseType = "code"
                        }
                    }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}