using System;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;

namespace DesktopChanger
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

    public class DesktopChanger
    {
        public void Run(CancellationToken stoppingToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4242);
            var sock = new UdpClient(endpoint);
            
            for(int i = VirtualDesktop.Desktop.Count; i - 3 > 0; i--)
            {
                VirtualDesktop.Desktop.Create();
                System.Console.WriteLine("VirtualDesktop.Desktop.Create()");
            }

            var oldState = Front(DesktopState.Left);
            var active = true;
            var responsive = false;
            void HotKeyPressed(HotKey hotKey) {
                switch (hotKey.Key)
                {
                    case VirtualKeyCode.KEY_A:
                        active = !active;
                        break;
                    case VirtualKeyCode.KEY_R:
                        responsive = !responsive;
                        break;
                }
            };

            var pitchLimit = -15.0;

            SoundPlayer changeDesktopSound = new SoundPlayer(Resources.click);
            bool activeSoundIsPlaying = false;
            SoundPlayer activeSound = new SoundPlayer(Resources.synth);

            using var hotKeyManager = new HotKeyManager();
            using var subscription = hotKeyManager.HotKeyPressed.Subscribe(HotKeyPressed);
            using var ctrl1 = hotKeyManager.Register(VirtualKeyCode.KEY_A, Modifiers.Alt);
            using var ctrl2 = hotKeyManager.Register(VirtualKeyCode.KEY_R, Modifiers.Alt);
            while (!stoppingToken.IsCancellationRequested)
            {
                var sender = new IPEndPoint(IPAddress.Any, 0);
                var data = sock.Receive(ref sender);

                if (!active) continue;

                var pos = new HeadPosition
                {
                    X = BitConverter.ToDouble(data, 0),
                    Y = BitConverter.ToDouble(data, 8),
                    Z = BitConverter.ToDouble(data, 16),
                    Yaw = BitConverter.ToDouble(data, 24),
                    Pitch = BitConverter.ToDouble(data, 32),
                    Roll = BitConverter.ToDouble(data, 40),
                };

                if (!responsive && pos.Pitch > pitchLimit)
                {
                    if (activeSoundIsPlaying)
                    {
                        activeSound.Stop();
                        activeSoundIsPlaying = false;
                    }
                    continue;
                }
                if (!responsive && pos.Pitch <= pitchLimit)
                {
                    if (!activeSoundIsPlaying)
                    {
                        activeSound.PlayLooping();
                        activeSoundIsPlaying = true;
                    }
                }

                var newState = UpdateState(oldState, pos);

                if (newState != oldState)
                {
                    changeDesktopSound.Play();
                    oldState = newState;
                    activeSoundIsPlaying = false;
                }
            }
        }

        internal DesktopState Left(DesktopState state)
        {
            if (state != DesktopState.Left)
            {
                VirtualDesktop.Desktop.FromIndex(0).MakeVisible();
            }
            return DesktopState.Left;
        }

        internal DesktopState Front(DesktopState state)
        {
            if (state != DesktopState.Front)
            {
                VirtualDesktop.Desktop.FromIndex(1).MakeVisible();
                System.Console.WriteLine("VirtualDesktop.Desktop.FromIndex(1);");
            }
            return DesktopState.Front;
        }

        internal DesktopState Right(DesktopState state)
        {
            if (state != DesktopState.Right)
            {
                VirtualDesktop.Desktop.FromIndex(2).MakeVisible();
                System.Console.WriteLine("VirtualDesktop.Desktop.FromIndex(2);");
            }
            return DesktopState.Right;
        }

        internal DesktopState UpdateState(DesktopState state, HeadPosition pos)
        {

            var (leftLimit, rightLimit) = (state) switch
            {
                (DesktopState.Left) => (-35, 35.0),
                (DesktopState.Front) => (-35.0, 35.0),
                (DesktopState.Right) => (-35.0, 35),
            };

            if (pos.Yaw < leftLimit)
            {
                return Left(state);
            }
            else if (pos.Yaw > rightLimit)
            {
                return Right(state);
            }
            return Front(state);
        }
    }
}
