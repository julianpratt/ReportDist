using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mistware.Files;
using Mistware.Utils;
using ReportDist.Data;

namespace ReportDist
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            string conn = Config.Get("AzureConnectionString");
     		string cont = Config.Get("AzureContainer");
            string root = Config.Get("LocalFilesRoot");
            string logs = Config.Get("Logs");
		    IFile fsys = FileBootstrap.SetupFileSys(conn,cont,root,logs);
            services.AddSingleton<IFile>(fsys);
            Log.Me.LogFile = "ReportDist.log"; // Overide default from Boostrap
            
            Log.Me.Info("==================================================================================================");
            Log.Me.Info("Report Distribution Starting");
            Log.Me.Info("Configuration: " + Config.DebugConfig());
     
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.Name = ".ReportDist.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            if (!Env.IsDevelopment())
            {
                services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => AzureADConfig());
            }
           
            services.AddMvc(options =>
            {
                if (Env.IsDevelopment())
                {
                    options.Filters.Add<AllowAnonymousFilter>();
                }
                else
                {
                    var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }
                options.EnableEndpointRouting = false;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
                
            services.AddDbContext<DataContext>();

            Log.Me.Info("Successfully through ConfigureServices");
        }

        public AzureADOptions AzureADConfig()
        {
            AzureADOptions options = new AzureADOptions();
            options.Instance     = Config.Get("AAD_Instance");
            options.Domain       = Config.Get("AAD_Domain");
            options.TenantId     = Config.Get("AAD_TenantId");
            options.ClientId     = Config.Get("AAD_ClientId");
            options.CallbackPath = Config.Get("AAD_CallbackPath"); 

            return options;
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Log.Me.Info("Just in Configure");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Log.Me.Info("In Development mode");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();

            if (!env.IsDevelopment()) app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
