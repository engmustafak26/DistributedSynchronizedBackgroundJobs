using DistributedSynchronizedBackgroundJobs.Cache;
using DistributedSynchronizedBackgroundJobs.NumericRandomGenerator;
using System.Diagnostics;

namespace DistributedSynchronizedBackgroundJobs.BackgroundJobs
{
    public class TimedHostedBackgroundService : BackgroundService
    {
        private const string cachKey = "JobSynchronization";
        private const int scaleDownFactor = 10;
        private const int TimeOutMilliSeconds = scaleDownFactor * 1000;
        private const int CacheExpirationInSeconds = scaleDownFactor * 2;

        private int executionCount = 0;
        private readonly ILogger<TimedHostedBackgroundService> _logger;
        private readonly INumericRandomGenerator _numericRandomGenerator;
        private readonly ICache cache;
        private Timer? _timer = null;

        public TimedHostedBackgroundService(ILogger<TimedHostedBackgroundService> logger, INumericRandomGenerator numericRandomGenerator, ICache cache)
        {
            _logger = logger;
            _numericRandomGenerator = numericRandomGenerator;
            this.cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int randomNumeric = _numericRandomGenerator.Generate(maxNumberThreshold: 100);
            int scaledDownRandomNumeric = randomNumeric % scaleDownFactor;
            await Task.Delay(scaledDownRandomNumeric * 1000);

            var message = await cache.GetAsync<SynchronizeCachedMessage>(cachKey);
            if (message is { IsStarted: true })
            {
                await Task.Delay(TimeOutMilliSeconds); // monitor to take control if "the instance that has been started work" fails to start properly
                message = await cache.GetAsync<SynchronizeCachedMessage>(cachKey);
                if (message is { IsCompleted: true })
                    return;

                await SynchronizeWorkInstancesAndDoWork(message);
            }

            await SynchronizeWorkInstancesAndDoWork(message);

        }

        private async Task SynchronizeWorkInstancesAndDoWork(SynchronizeCachedMessage? message)
        {
            SynchronizeCachedMessage synchronizeMessage = new()
            {
                IsStarted = true,
                StartDate = DateTime.UtcNow,
                ServerName = System.Environment.MachineName,
                ProcessId = Process.GetCurrentProcess().Id
            };
            message = await cache.GetAsync<SynchronizeCachedMessage>(cachKey); //extra check if any instance start work
            if (message is null)
            {
                await cache.SetAsync(cachKey, synchronizeMessage, TimeSpan.FromSeconds(CacheExpirationInSeconds));

                await PrepareWorkAsync(); // simulate delay when start process or exception happens prevent execution on this instance
                                          // so another instance take control

                message = await cache.GetAsync<SynchronizeCachedMessage>(cachKey);
                message.IsCompleted = true;
                message.CompleteDate = DateTime.UtcNow;
                await cache.SetAsync(cachKey, message, TimeSpan.FromSeconds(CacheExpirationInSeconds));

                StartWork();
            }


        }

        private void StartWork()
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
        }

        private async Task PrepareWorkAsync()
        {
            await Task.Delay(500);
            // throw new Exception("SomeThing Went Wrong"); // uncomment this line to simulate instance fail to start
        }
        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Timed Hosted Service is working. Count: {Count}", count);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }


    }
}
