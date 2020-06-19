using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
using ReportDist.Data;
using Mistware.Utils;

namespace ReportDist
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config.Setup("Config/configure.json", Directory.GetCurrentDirectory(), null, "ReportDist");

            // Standard single line in Main:
            // CreateWebHostBuilder(args).Build().Run();

            //1. Get the IWebHost which will host this application.
            var host = CreateWebHostBuilder(args).UseKestrel().Build();

            //2. Find the service layer within our scope.
            using (var scope = host.Services.CreateScope())
            {   
                //3. Get the instance of AppDBContext in our services layer
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<DataContext>();
            }

            //Continue to run the application
            host.Run();
        }

         public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("hostsettings.json", optional: true)
            .AddCommandLine(args)
            .Build();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel((context, options) => 
                { 
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                    options.Limits.MaxRequestBodySize = 52428800; 
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<CatalogueCheckService>();
                }) 
                .UseStartup<Startup>();
        }  
    }
}
