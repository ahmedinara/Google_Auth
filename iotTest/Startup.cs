using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iotTest
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
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
        .AddCookie(options =>
        {
            options.LoginPath = "/google-login"; // Must be lowercase
        })
        .AddGoogle(options =>
        {
            IConfigurationSection googleAuthNSection = Configuration.GetSection("Authentication:Google");

            options.ClientId = "322988291509-ovnr2drb92drgq9etp5bahmndt7t6anl.apps.googleusercontent.com";
            options.ClientSecret = "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";

            options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
            options.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");
            options.Scope.Add("https://www.googleapis.com/auth/calendar");
            options.Scope.Add("https://www.googleapis.com/auth/cloudiot");
            options.Scope.Add("https://www.googleapis.com/auth/cloud-platform");

            //this should enable a refresh-token, or so I believe
            options.AccessType = "offline";

            options.SaveTokens = true;

            options.Events.OnCreatingTicket = ctx =>
            {
                List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

                tokens.Add(new AuthenticationToken()
                {
                    Name = "TicketCreated",
                    Value = DateTime.UtcNow.ToString()
                });

                ctx.Properties.StoreTokens(tokens);

                return Task.CompletedTask;
            };
        });
          

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
