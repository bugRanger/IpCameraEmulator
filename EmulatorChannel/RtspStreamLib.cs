namespace Emulator
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;

    public static class RtspStreamerLib
    {
        #region Constains

        public const int STATUS_UNKNOWN = -1;

        public const int STATUS_STARTED = 1539;

        public const int STATUS_STOPPED = 1540;

        public const int STATUS_INIT = 1541;

        public const int STATUS_OPENING = 1542;

        public const int STATUS_PLAYING = 1543;

        public const int STATUS_PAUSE = 1544;

        public const int STATUS_END = 1545;

        public const int STATUS_ERROR = 1546;

        public const int CODE_ERROR = -1;

        public const int CODE_SUCCESS = 0;

        #endregion Constains

        #region Properties

        public static string[] SupportVersions { get; } = { "2.2.6 Umbrella" };

        #endregion Properties

        #region Methods

        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateRtspStreamerLib();

        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyRtspStreamerLib(IntPtr lib);

        /// <summary>
        /// Начать трансляцию потока.
        /// </summary>
        /// <param name="lib">Указатель на библиотеку</param>
        /// <param name="streamName">Наименование потока</param>
        /// <param name="mediaPath">Путь до файла</param>
        /// <param name="portNumber">RTSP-порт</param>
        /// <returns>В случае успеха вернет 0, в противном случае вернет -1.</returns>
        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 StartStreamLib(IntPtr lib, byte[] streamName, byte[] mediaPath, Int32 portNumber);

        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopStreamLib(IntPtr lib);

        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetStreamStatusLib(IntPtr lib);

        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 GetStreamRateLib(IntPtr lib);

        [DllImport("RtspStreamerLib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetVlcVersionLib(IntPtr lib);

        public static string GetVlcVersion(IntPtr lib)
        {
            var retPtr = new byte[64];
            Marshal.Copy(GetVlcVersionLib(lib), retPtr, 0, 64);
            return Encoding.UTF8.GetString(retPtr.Take(Array.IndexOf(retPtr, (byte)0)).ToArray());
        }

        #endregion Methods
    }
}
