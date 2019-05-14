using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class HostedServices : IRunnable
{
    public async Task Run()
    {
        using (var host = CreateHostBuilder().Build())
        {
            this.PrintStart();

            await host.StartAsync();
            await Task.Delay(5000);

            this.PrintStop();
            await host.StopAsync();
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args = null) =>
        Host.CreateDefaultBuilder(args ?? Array.Empty<string>())
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
            });

    class Worker : BackgroundService
    {
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this.PrintStart();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                this.PrintWorking();
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.PrintStopped();

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            this.PrintDisposed();

            base.Dispose();
        }
    }
}
