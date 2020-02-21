using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Emulator
{
    public enum ChannelStatus
    {
        Unknown = -2,
        CriticalError = -1,
        Started = 1539,
        Stopped = 1540,
        Init = 1541,
        Opening = 1542,
        Playing = 1543,
        Pause = 1544,
        End = 1545,
        Error = 1546
    }

    public class EmulatorChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MediaPath { get; set; }
        public int RtspPort { get; set; }
        public bool Enabled { get; set; }

        [XmlIgnore]
        public ChannelStatus Status { get; set; }

        [XmlIgnore]
        public EmulatorEngine Engine { get; set; }

        public EmulatorChannel()
        {
        }

        public EmulatorChannel(int id, string name, string mediaPath, int rtspPort, bool enabled)
        {
            Id = id;
            Name = name;
            MediaPath = mediaPath;
            RtspPort = rtspPort;
            Enabled = enabled;
            Status = ChannelStatus.Unknown;
        }

    }
}
