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
            services.AddSingleton<IFile>(ConfigureFileSystem());
         
            CheckConfiguration();
            
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
                //.AddAzureAD(options => Configuration.Bind("AzureAd", options));
                .AddAzureAD(options => AzureADConfig(options));
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
        }

        public void AzureADConfig(AzureADOptions opts)
        {
            opts.Instance     = Config.Get("AAD_Instance");
            opts.Domain       = Config.Get("AAD_Domain");
            opts.TenantId     = Config.Get("AAD_TenantId");
            opts.ClientId     = Config.Get("AAD_ClientId");
            opts.CallbackPath = Config.Get("AAD_CallbackPath");

            Log.Me.Debug("----------------------------------------------------------------------------------------------");
            Log.Me.Debug("Azure Active Directory (aka authentication) Configuration:");
            Log.Me.Debug("");
            Log.Me.Debug("AAD_Instance    : " + (opts.Instance     ?? ""));
            Log.Me.Debug("AAD_Domain      : " + (opts.Domain       ?? ""));
            Log.Me.Debug("AAD_TenantId    : " + (opts.TenantId     ?? ""));
            Log.Me.Debug("AAD_ClientId    : " + (opts.ClientId     ?? ""));
            Log.Me.Debug("AAD_CallbackPath: " + (opts.CallbackPath ?? "")); 

            bool ok = opts.Instance.HasValue() && opts.Domain.HasValue() && opts.TenantId.HasValue();
            ok = ok && opts.ClientId.HasValue() && opts.CallbackPath.HasValue();

            if (!ok)
            {
                Log.Me.Fatal("Missing AzureAD Configuration. Use ASPNETCORE_ENVIRONMENT='Development' to see what is wrong.");
                System.Environment.Exit(8);
            }
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

        public void CheckConfiguration()
        {
            Log.Me.Info("==================================================================================================");
            Log.Me.Info("Report Distribution Starting");

            bool filesysok = false;
            

            // Check SQL Config, AppVersion and Developer Id
            string conn   = Config.Get("SQLConnection");
            string type   = Config.Get("SQLServerType");
            string appver = Config.Get("AppVersion");
            if (!appver.HasValue()) Config.Set("AppVersion", "ReportDist (unknown version)");
            string devuid = Config.Get("DeveloperUserId");
            bool othersok = conn.HasValue() && type.HasValue() && devuid.HasValue();
            if (othersok) if (type != "MySQL" && type != "SQLServer") othersok=false;
            
            if (Config.Debug)
            {
                Log.Me.Debug("Configuration: ");
                Log.Me.Debug("");
                Log.Me.Debug("SQLConnection:   " + (conn ?? ""));
                Log.Me.Debug("SQLServerType:   " + (type ?? ""));
                Log.Me.Debug("AppVersion:      " + (appver ?? ""));
                Log.Me.Debug("DeveloperUserId: " + (devuid ?? ""));

                filesysok = DebugFileSystemConfig();
                CatalogueAPI.Me.Debug();
                EmailConfig.Me.Debug();
                //DebugDeployment();
            }
            else
            {
                filesysok = DebugFileSystemConfig();
            }

            if (!System.IO.Directory.Exists("Config/appsettings.json"))
            {
                Log.Me.Fatal("Could not find Config/appsettings.json. Use ASPNETCORE_ENVIRONMENT='Development' to see what is wrong.");
                System.Environment.Exit(8);
            }

            if (!filesysok || !othersok || !CatalogueAPI.Me.IsValid() || !EmailConfig.Me.IsValid())
            {
                Log.Me.Fatal("Missing/invalid configuration. Use ASPNETCORE_ENVIRONMENT='Development' to see what is wrong.");
                System.Environment.Exit(8);
            }
        }

        public IFile ConfigureFileSystem()
        {
            string conn = Config.Get("AzureConnectionString");
     		string cont = Config.Get("AzureContainer");
            string root = Config.Get("LocalFilesRoot");
            string logs = Config.Get("Logs");
		    IFile fsys = FileBootstrap.SetupFileSys(conn,cont,root,logs);
            Log.Me.LogFile = "ReportDist.log"; // Overide default from Boostrap
            Log.Me.DebugOn = Config.Debug;

            return fsys;
        }

        public bool DebugFileSystemConfig()
        {
            string conn   = Config.Get("AzureConnectionString");
     		string cont   = Config.Get("AzureContainer");
            string root   = Config.Get("LocalFilesRoot");
            string logs   = Config.Get("Logs");
            string upload = Config.Get("UploadDirectory");
            string outbox = Config.Get("OutboxDirectory");
            string maxlen = Config.Get("FileSizeLimit");

            Log.Me.Debug("----------------------------------------------------------------------------------------------");
            Log.Me.Debug("File System Configuration:");
            Log.Me.Debug("");
            Log.Me.Debug("AzureConnectionString: " + (conn ?? ""));
            Log.Me.Debug("AzureContainer:        " + (cont ?? ""));
            Log.Me.Debug("LocalFilesRoot:        " + (root ?? ""));
            Log.Me.Debug("Logs:                  " + (logs ?? ""));
            Log.Me.Debug("UploadDirectory:       " + (upload ?? ""));
            Log.Me.Debug("OutboxDirectory:       " + (outbox ?? ""));
            Log.Me.Debug("FileSizeLimit:         " + (maxlen ?? ""));
            bool ok = root.HasValue() || (conn.HasValue() && cont.HasValue());
            ok = ok && logs.HasValue() && upload.HasValue() && outbox.HasValue() && maxlen.HasValue();
            if (ok) Log.Me.Debug("Everything is configured.");
            else    Log.Me.Warn("Some File System configuration is missing");

            return ok;
        }

        public void DebugDeployment()
        {
            string here = System.IO.Directory.GetCurrentDirectory();
            ListFiles(here);
            ListFolders(here);
        }

        public void ListFolders(string here)
        {
            foreach (string folder in System.IO.Directory.GetDirectories(here))
            {
                Log.Me.Debug("Folder: "+ folder);
                ListFiles(folder);
                ListFolders(folder);
            }
        }
        public void ListFiles(string folder)
        {
            foreach (string file in System.IO.Directory.EnumerateFiles(folder))
            {
                Log.Me.Debug("File  : "+ file);
            }
        }
    }
}
