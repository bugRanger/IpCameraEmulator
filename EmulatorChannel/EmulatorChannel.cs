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
        Unknown = RtspStreamerLib.STATUS_UNKNOWN,
        Init = RtspStreamerLib.STATUS_INIT,
        Started = RtspStreamerLib.STATUS_STARTED,
        Stopped = RtspStreamerLib.STATUS_STOPPED,
        Opening = RtspStreamerLib.STATUS_OPENING,
        Playing = RtspStreamerLib.STATUS_PLAYING,
        Pause = RtspStreamerLib.STATUS_PAUSE,
        End = RtspStreamerLib.STATUS_END,
        Error = RtspStreamerLib.STATUS_ERROR
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
