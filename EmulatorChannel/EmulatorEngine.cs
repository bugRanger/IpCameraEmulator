using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Emulator
{

    public class EmulatorEngine : IDisposable
    {
        #region Fields

        private IntPtr _rstpStreamLibPointer = IntPtr.Zero;

        #endregion Fields

        #region Properties

        public string StreamName { get; }

        public string MediaPath { get; }

        public int RtspPort { get; }

        #endregion Properties

        #region Constructors\Destructors

        public EmulatorEngine(string streamName, string mediaPath, int rtpsPort)
        {
            try
            {
                StreamName = streamName;
                MediaPath = mediaPath;
                RtspPort = rtpsPort;

                _rstpStreamLibPointer = RtspStreamerLib.CreateRtspStreamerLib();
            }
            catch
            {
                throw;
            }
        }

        ~EmulatorEngine()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }
            // free native resources if there are any.
            RtspStreamerLib.DestroyRtspStreamerLib(_rstpStreamLibPointer);
            _rstpStreamLibPointer = IntPtr.Zero;
        }

        #endregion Constructors\Destructors
        
        #region Methods

        public void Start()
        {
            try
            {
                if (!File.Exists(MediaPath))
                    throw new InvalidOperationException("Invalid media file");
                
                Console.WriteLine("Loading media (" + MediaPath + ")...");
                StartStream();
                Console.WriteLine("Started stream on Port " + RtspPort);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                StopStream();
            }
            catch
            {
                throw;
            }
        }

        public ChannelStatus GetChannelStatus()
        {
            try
            {
                return Enum.TryParse(RtspStreamerLib.GetStreamRateLib(_rstpStreamLibPointer).ToString(), out ChannelStatus status)
                            ? status
                            : ChannelStatus.Unknown;
            }
            catch
            {
                throw;
            }
        }

        public string GetVlcLibraryVersion()
        {
            try
            {
                return RtspStreamerLib.GetVlcVersion(_rstpStreamLibPointer);
            }
            catch
            {
                return "<Unknown>";
            }
        }

        private void StartStream()
        {
            try
            {
                byte[] streamName = Encoding.UTF8.GetBytes(StreamName);
                byte[] mediaPath = Encoding.UTF8.GetBytes(MediaPath);

                RtspStreamerLib.StartStreamLib(_rstpStreamLibPointer, streamName, mediaPath, RtspPort);
            }
            catch
            {
                throw;
            }
        }

        private void StopStream()
        {
            try
            {
                RtspStreamerLib.StopStreamLib(_rstpStreamLibPointer);
            }
            catch
            {
                throw;
            }
        }

        #endregion Methods
    }
}
