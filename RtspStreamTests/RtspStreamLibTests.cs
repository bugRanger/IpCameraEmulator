namespace RtspStreamTests
{
    using System;
    using System.Text;
    using System.Threading;

    using NUnit.Framework;

    using Emulator;

    [TestFixture]
    public class RtspStreamLibTests
    {
        #region Constains

        private const string STREAM_NAME_S = "RtspLibStreamForTests";

        private const string MEDIA_FILE_NAME_S = "test.mp4";

        private const int RTSP_PORT_I = 18888;

        #endregion Constains

        #region Fields

        private byte[] _streamName;

        private byte[] _mediaFile;
        
        private IntPtr _rtspLib;

        #endregion Fields

        [SetUp]
        public void SetUp()
        {
            _streamName = Encoding.UTF8.GetBytes(STREAM_NAME_S);
            _mediaFile = Encoding.UTF8.GetBytes($"{AppDomain.CurrentDomain.BaseDirectory}\\{MEDIA_FILE_NAME_S}");

            _rtspLib = RtspStreamerLib.CreateRtspStreamerLib();
        }

        [TearDown]
        public void TearDown()
        {
            RtspStreamerLib.DestroyRtspStreamerLib(_rtspLib);
        }

        [Test]
        public void PlayingMediaTest()
        {
            // Act
            int code = RtspStreamerLib.StartStreamLib(_rtspLib, _streamName, _mediaFile, RTSP_PORT_I);
            bool streaming = RtspStreamerLib.GetStreamStatusLib(_rtspLib);
            int status = Wait(() => RtspStreamerLib.GetStreamRateLib(_rtspLib), s => s == RtspStreamerLib.STATUS_PLAYING);
            
            // Assert
            Assert.AreNotEqual(_rtspLib, IntPtr.Zero);
            Assert.AreEqual(code, RtspStreamerLib.CODE_SUCCESS);
            Assert.AreEqual(status, RtspStreamerLib.STATUS_PLAYING);
            Assert.IsTrue(streaming);
        }

        [Test]
        public void StoppedMediaTest()
        {
            // Arrage
            PlayingMediaTest();

            // Act
            RtspStreamerLib.StopStreamLib(_rtspLib);
            bool streaming = RtspStreamerLib.GetStreamStatusLib(_rtspLib);
            int status = Wait(() => RtspStreamerLib.GetStreamRateLib(_rtspLib), s => s == RtspStreamerLib.STATUS_STOPPED);

            // Assert
            Assert.AreEqual(status, RtspStreamerLib.STATUS_STOPPED);
            Assert.IsFalse(streaming);
        }

        [Test]
        public void StoppedMediaNoStartedTest()
        {
            // Act
            RtspStreamerLib.StopStreamLib(_rtspLib);
            bool streaming = RtspStreamerLib.GetStreamStatusLib(_rtspLib);
            int status = Wait(() => RtspStreamerLib.GetStreamRateLib(_rtspLib), s => s == RtspStreamerLib.STATUS_UNKNOWN);

            // Assert
            Assert.AreEqual(status, RtspStreamerLib.STATUS_UNKNOWN);
            Assert.IsFalse(streaming);
        }

        [Test]
        [TestCase(null)]
        [TestCase(new byte[0])]
        public void NotFoundMediaFileTest(byte[] mediaFile)
        {
            // Act
            int code = RtspStreamerLib.StartStreamLib(_rtspLib, _streamName, mediaFile, RTSP_PORT_I);
            bool streaming = RtspStreamerLib.GetStreamStatusLib(_rtspLib);
            int status = Wait(() => RtspStreamerLib.GetStreamRateLib(_rtspLib), s => s == RtspStreamerLib.STATUS_UNKNOWN);

            // Assert
            Assert.AreEqual(code, RtspStreamerLib.CODE_ERROR);
            Assert.AreEqual(status, RtspStreamerLib.STATUS_UNKNOWN);
            Assert.IsFalse(streaming);
        }

        [Test]
        [TestCase(int.MinValue)]
        [TestCase(ushort.MinValue)]
        [TestCase(ushort.MaxValue + 1)]
        public void IncorrectPortTest(int port)
        {
            // Act
            int code = RtspStreamerLib.StartStreamLib(_rtspLib, _streamName, _mediaFile, port);
            bool streaming = RtspStreamerLib.GetStreamStatusLib(_rtspLib);
            int status = Wait(() => RtspStreamerLib.GetStreamRateLib(_rtspLib), s => s == RtspStreamerLib.STATUS_UNKNOWN);

            // Assert
            Assert.AreEqual(code, RtspStreamerLib.CODE_ERROR);
            Assert.AreEqual(status, RtspStreamerLib.STATUS_UNKNOWN);
            Assert.IsFalse(streaming);
        }

        [Test]
        public void SupportVersionTest()
        {
            // Act
            string version = RtspStreamerLib.GetVlcVersion(_rtspLib);

            // Assert
            Assert.IsTrue(Array.IndexOf(RtspStreamerLib.SupportVersions, version) > -1);
        }

        protected static T Wait<T>(Func<T> action, Func<T, bool> condition)
        {
            T result = default(T);

            for (var i = 0; i < 5; i++)
            {
                result = action();
                if (condition(result))
                    break;

                Thread.Sleep(100);
            }

            return result;
        }
    }
}
