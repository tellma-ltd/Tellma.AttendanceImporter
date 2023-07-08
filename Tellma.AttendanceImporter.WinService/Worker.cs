using Microsoft.Extensions.Options;

namespace Tellma.AttendanceImporter.WinService
{
    public class Worker : BackgroundService
    {
        // DI container
        private readonly IServiceProvider _serviceProvider;
        private readonly ImporterOptions _options;

        public Worker(IServiceProvider serviceProvider, IOptions<ImporterOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Worker>>();
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                try
                {
                    var importer = scope.ServiceProvider.GetRequiredService<TellmaAttendanceImporter>();
                    await importer.ImportToTellma(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled Error");
                    // Don't throw. Instead, wait for period then try again
                }

                await Task.Delay(_options.PeriodInMinutes * 60 * 1000, stoppingToken);
            }
        }
    }
}