using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeadTracker
{
    public class HeadPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }
    }

    public class UdpListener
    {
        public event EventHandler<HeadPosition> HeadPositionChanged;

        public async Task Run(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (true)
                {
                    HeadPositionChanged?.Invoke(this, new HeadPosition { } );
                    await Task.Delay(2000);
                }
            }
        }
    }
}
