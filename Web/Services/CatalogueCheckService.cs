using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReportDist.Data;
using Mistware.Utils;

namespace ReportDist
{
    /// This service runs in the background every hour and checks whether reports have arrived in
    /// the Catalogue, such that CIDs can be obtained for those reports and the reports be released
    /// for sending. 
    ///
    /// It was based on an MS article: Background tasks with hosted services in ASP.NET Core
    /// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
    public class CatalogueCheckService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer timer;
        protected readonly DataContext _context;


        private readonly IServiceScopeFactory scopeFactory;

        public CatalogueCheckService(IServiceScopeFactory scopeFactory) 
        {
            this.scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Log.Me.Info("Catalogue Check Service running - checking for CIDs hourly.");

            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            string result = "";
            using (var scope = scopeFactory.CreateScope())
            {
                DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();
                result = context.PendingReportRepo.GetCIDs();
            }
            
            if (result.Left(7) == "WARNING") 
                Log.Me.Error("Catalogue Check Service encountered problem. Count: " + count.ToString() + " Result: " + result);    
            else Log.Me.Info("Catalogue Check Service working. Count: " + count.ToString() + " Result: " + result);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            Log.Me.Info("Catalogue Check Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}    