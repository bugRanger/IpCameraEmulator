using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emulator;

namespace IpCameraEmulator
{
    class Program
    {
        public class Statistic
        {
            private ChannelStatus _status;

            public int BadResponce { get; private set; }

            public ChannelStatus Status
            {
                get => _status;
                set
                {
                    if (value != ChannelStatus.Playing)
                        BadResponce += 1;
                    else
                        BadResponce = 0;

                    _status = value;
                }
            }

            public bool IsAlive() => BadResponce != DEFAULT_BAD_STATUS_COUNT;
        }

        private const int DEFAULT_BAD_STATUS_COUNT = 10;

        public static Dictionary<EmulatorEngine, Statistic> Engines { get; }

        static Program()
        {
            Engines = new Dictionary<EmulatorEngine, Statistic>();
        }

        static void Main(string[] args)
        {
            void CheckTimer(object state)
            {
                var cached = Engines.ToArray();
                foreach (var item in cached)
                {
                    item.Value.Status = item.Key.GetChannelStatus();
                    if (!item.Value.IsAlive())
                        Console.WriteLine($"ERROR! {item.Key.StreamName}:{item.Key.RtspPort} - {item.Value.Status}, bar response {item.Value.BadResponce}");
                }
            }

            using (var updateTmr = new Timer(CheckTimer, null, -1, -1))
            {
                string media = $"{Environment.CurrentDirectory}\\test.mp4";
                int rtspPort = 8554;
                int portCount = 50;

                try
                {
                    // TODO Разбор параметров без исключения и параметров libvlc.
                    media = args[0];
                    rtspPort = Convert.ToInt32(args[1]);
                    portCount = Convert.ToInt32(args[2]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Task.Run(() =>
                {
                    for (var i = 0; i < portCount; i++)
                    {
                        var channel = new EmulatorEngine($"EmuCam-{1 + i}", media, rtspPort + i);
                        channel.Start();

                        Engines.Add(channel, new Statistic { Status = ChannelStatus.Unknown });
                    }
                });

                updateTmr.Change(0, 1500);

                ConsoleKeyInfo infoKey;
                do
                {
                    infoKey = Console.ReadKey();
                    switch (infoKey.Key)
                    {
                        case ConsoleKey.R:
                            Console.WriteLine("Restarting the camera with errors");
                            foreach (var engine in Engines.Where(w => !w.Value.IsAlive()).Select(s => s.Key))
                                engine.Start();
                            break;

                        case ConsoleKey.B:
                            Console.WriteLine("Stopped cameras");
                            foreach (var engine in Engines.Keys)
                                engine.Stop();
                            break;

                        case ConsoleKey.Q:
                            Console.WriteLine("Exit");
                            break;

                        default:
                            break;
                    }
                }
                while (infoKey.Key != ConsoleKey.Q);
            }
        }
    }
}
