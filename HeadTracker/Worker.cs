using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HeadTracker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private Thread _listenerThread;
        private UdpListener _listener;
        private DesktopChanger _changer;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = new UdpListener();
            _changer = new DesktopChanger(_listener);
            _listenerThread = new Thread(async () => await _listener.Run(cancellationToken));
            _listenerThread.SetApartmentState(ApartmentState.STA);
            _listenerThread.Start();
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
