using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace HeadTracker
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HeadPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Pitch { get; set; }
        public double Roll { get; set; }
    }

    public enum DesktopState { Left, Front, Right }
    public enum DesktopStateChange { Left, Right }

    public static  class DesktopChanger
    {
        public static void Run(CancellationToken stoppingToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4242);
            var sock = new UdpClient(endpoint);
            var state = DesktopState.Front;


            while (!stoppingToken.IsCancellationRequested)
            {
                var sender = new IPEndPoint(IPAddress.Any, 0);
                var data = sock.Receive(ref sender);

                var pos = new HeadPosition
                {
                    X = BitConverter.ToDouble(data, 0),
                    Y = BitConverter.ToDouble(data, 8),
                    Z = BitConverter.ToDouble(data, 16),
                    Yaw = BitConverter.ToDouble(data, 24),
                    Pitch = BitConverter.ToDouble(data, 32),
                    Roll = BitConverter.ToDouble(data, 40),
                };

                if (pos.Pitch < -45.0)
                {
                    return;
                }
                var margin = 15;
                switch (state) {
                    case DesktopState.Front:
                        {
                            if (pos.Yaw < -45 - margin)
                            {
                                ChangeDesktop(DesktopStateChange.Left);
                            }
                            else if (pos.Yaw > 45 + margin)
                            {
                                ChangeDesktop(DesktopStateChange.Right);
                            }
                            break;
                        }
                    case DesktopState.Left:
                        {
                            if (pos.Yaw < -45 + margin)
                            {
                                ChangeDesktop(DesktopStateChange.Right);
                            }
                            break;
                        }
                    case DesktopState.Right:
                        {
                            if (pos.Yaw > 45 - margin)
                            {
                                ChangeDesktop(DesktopStateChange.Left);
                            }
                            break;
                        }
                }
            }
        }
        public static void ChangeDesktop(DesktopStateChange direction)
        {
            // CTRL-C (effectively a copy command in many situations)
            var sim = new InputSimulator();
            var k = sim.Keyboard;
            switch (direction)
            {
                case DesktopStateChange.Left:
                    {
                        k.ModifiedKeyStroke(new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.LWIN }, VirtualKeyCode.LEFT);
                        break;
                    }
                case DesktopStateChange.Right:
                    {
                        k.ModifiedKeyStroke(new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.LWIN }, VirtualKeyCode.RIGHT);
                        break;
                    }
            }
            
        }
    }
}
