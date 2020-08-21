using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace BugTracker
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment _environment;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            _environment = env;
        }

        /// <summary>
        /// Configure dependency injection container
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {

            //If on remote server, use provided environmental variable to access the database
            services.AddNpgsqlDataContext(_environment.IsDevelopment() ? Configuration.GetConnectionString("DefaultConnection")
                : Environment.GetEnvironmentVariable("DATABASE_URL"));

            services.AddHttpContextAccessor();
            services.AddLocalization(options => options.ResourcesPath = Configuration.GetSection("Resources").Value);
            services.AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));

            }).AddRazorRuntimeCompilation()
              .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix, options => options.ResourcesPath = Configuration.GetSection("Resources").Value);

            services.AddMvc().AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                o.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            }).AddDataAnnotationsLocalization(o => o.DataAnnotationLocalizerProvider = (type, factory) =>
            {
                return factory.Create(typeof(SharedResources));
            });


            var provider = new CookieRequestCultureProvider()
            {
                CookieName = "BugTracker.PrefferedCulture"
            };
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                new CultureInfo("en"),
                new CultureInfo("ru")
                };
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Insert(0, provider);
            });

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 8;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireDigit = false;
                config.Password.RequireUppercase = false;
                config.Password.RequiredUniqueChars = 2;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();
        }


        /// <summary>
        /// Configure middleware pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithRedirects("/Error/{0}");
                app.UseHsts();
            }
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //who are you
            app.UseAuthentication();

            //are you allowed
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
