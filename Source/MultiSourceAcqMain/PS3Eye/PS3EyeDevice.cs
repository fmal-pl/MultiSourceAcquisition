using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Interop;

namespace MultiSourceAcqMain.PS3Eye
{
    public class PS3EyeDevice  : DependencyObject, IDisposable
    {        
        #region [ CLEyeMulticam Imports ]

        // get camera count
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CLEyeGetCameraCount();

        // get camera uuid
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Guid CLEyeGetCameraUUID(int camId);

        // create camera
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CLEyeCreateCamera(Guid camUUID, CLEyeCameraColorMode mode, CLEyeCameraResolution res, float frameRate);

        // destroy camera
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeDestroyCamera(IntPtr camera);

        // start
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeCameraStart(IntPtr camera);

        // stop
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeCameraStop(IntPtr camera);

        // led
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeCameraLED(IntPtr camera, bool on);

        // set param
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeSetCameraParameter(IntPtr camera, CLEyeCameraParameter param, int value);

        // get param
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CLEyeGetCameraParameter(IntPtr camera, CLEyeCameraParameter param);

        // get frame dim
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeCameraGetFrameDimensions(IntPtr camera, ref int width, ref int height);

        // get frame
        [DllImport("CLEyeMulticam.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CLEyeCameraGetFrame(IntPtr camera, IntPtr pData, int waitTimeout);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool UnmapViewOfFile(IntPtr hMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hHandle);
        #endregion
        
        private IntPtr _map = IntPtr.Zero;
        private IntPtr _section = IntPtr.Zero;
        private IntPtr _camera = IntPtr.Zero;

        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public int ImgSize { get; set; }
        public int FrameStride { get; set; }
        public bool IsCreated { get; set; }

        #region [ Properties ]

        public float Framerate{ get; set; }
        public CLEyeCameraColorMode ColorMode{ get; set; }
        public CLEyeCameraResolution Resolution{ get; set; }
        public bool AutoGain 
        {
            get
            {
                if (_camera == null)    return false;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_AUTO_GAIN) != 0;
            }
            set
            {
                if (_camera == null)    return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_AUTO_GAIN, value ? 1 : 0);
            }
        }
        public int Gain
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_GAIN);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_GAIN, value);
            }
        }
        public bool AutoExposure
        {
            get
            {
                if (_camera == null) return false;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_AUTO_EXPOSURE) != 0;
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_AUTO_EXPOSURE, value ? 1 : 0);
            }
        }
        public int Exposure
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_EXPOSURE);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_EXPOSURE, value);
            }
        }
        public bool AutoWhiteBalance
        {
            get
            {
                if (_camera == null) return true;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_AUTO_WHITEBALANCE) != 0;
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_AUTO_WHITEBALANCE, value ? 1 : 0);
            }
        }
        public int WhiteBalanceRed
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_WHITEBALANCE_RED);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_WHITEBALANCE_RED, value);
            }
        }
        public int WhiteBalanceGreen
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_WHITEBALANCE_GREEN);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_WHITEBALANCE_GREEN, value);
            }
        }
        public int WhiteBalanceBlue
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_WHITEBALANCE_BLUE);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_WHITEBALANCE_BLUE, value);
            }
        }
        public bool HorizontalFlip
        {
            get
            {
                if (_camera == null) return false;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_HFLIP) != 0;
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_HFLIP, value ? 1 : 0);
            }
        }
        public bool VerticalFlip
        {
            get
            {
                if (_camera == null) return false;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_VFLIP) != 0;
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_VFLIP, value ? 1 : 0);
            }
        }
        public int HorizontalKeystone
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_HKEYSTONE);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_HKEYSTONE, value);
            }
        }
        public int VerticalKeystone
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_VKEYSTONE);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_VKEYSTONE, value);
            }
        }
        public int XOffset
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_XOFFSET);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_XOFFSET, value);
            }
        }
        public int YOffset
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_YOFFSET);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_YOFFSET, value);
            }
        }
        public int Rotation
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_ROTATION);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_ROTATION, value);
            }
        }
        public int Zoom
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_ZOOM);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_ZOOM, value);
            }
        }
        public int LensCorrection1
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSCORRECTION1);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSCORRECTION1, value);
            }
        }
        public int LensCorrection2
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSCORRECTION2);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSCORRECTION2, value);
            }
        }
        public int LensCorrection3
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSCORRECTION3);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSCORRECTION3, value);
            }
        }
        public int LensBrightness
        {
            get
            {
                if (_camera == null) return 0;
                return CLEyeGetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSBRIGHTNESS);
            }
            set
            {
                if (_camera == null) return;
                CLEyeSetCameraParameter(_camera, CLEyeCameraParameter.CLEYE_LENSBRIGHTNESS, value);
            }
        }

        #endregion

        #region [ Static ]
        public static int CameraCount { get { return CLEyeGetCameraCount(); } }
        public static Guid CameraUUID(int idx){ return CLEyeGetCameraUUID(idx); }
        #endregion

        public PS3EyeDevice()
        {
            // set default values
            Framerate = 15;
            ColorMode = CLEyeCameraColorMode.CLEYE_COLOR_PROCESSED;
            Resolution = default(CLEyeCameraResolution);            
        }

        public bool Create(Guid cameraGuid)
        {
            int w = 0, h = 0;
            _camera = CLEyeCreateCamera(cameraGuid, ColorMode, Resolution, Framerate);

            if (_camera == IntPtr.Zero) return false;
            
            CLEyeCameraGetFrameDimensions(_camera, ref w, ref h);
            FrameWidth = w;
            FrameHeight = h;
            

            if (ColorMode == CLEyeCameraColorMode.CLEYE_COLOR_PROCESSED || ColorMode == CLEyeCameraColorMode.CLEYE_COLOR_RAW)
            {
                uint imageSize = (uint)w * (uint)h * 4;
                ImgSize = (int)imageSize;
                FrameStride = w * 4;

                // create memory section and map
                _section = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, imageSize, null);
                _map = MapViewOfFile(_section, 0xF001F, 0, 0, imageSize);
                //BitmapSource = Imaging.CreateBitmapSourceFromMemorySection(_section, w, h, PixelFormats.Bgr32, w * 4, 0) as InteropBitmap;
            }
            else
            {
                uint imageSize = (uint)w * (uint)h;
                ImgSize = (int)imageSize;
                FrameStride = w;

                // create memory section and map
                _section = CreateFileMapping(new IntPtr(-1), IntPtr.Zero, 0x04, 0, imageSize, null);
                _map = MapViewOfFile(_section, 0xF001F, 0, 0, imageSize);
                //BitmapSource = Imaging.CreateBitmapSourceFromMemorySection(_section, w, h, PixelFormats.Gray8, w, 0) as InteropBitmap;
            }
            // Invoke event
            //if (BitmapReady != null) BitmapReady(this, null);
            //BitmapSource.Invalidate();
            IsCreated = true;
            return true;
        }

        public void Start()
        {
            //AutoExposure = true;
            AutoExposure = false;
            CLEyeCameraStart(_camera);
        }

        public BitmapSource GetFrame()
        {
            if (CLEyeCameraGetFrame(_camera, _map, 500))
            {
                BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromMemorySection(_section, FrameWidth, FrameHeight, PixelFormats.Bgr32, FrameStride, 0) as InteropBitmap;
                bmpSrc.Freeze();
                return bmpSrc;
            }
            return null;
        }

        public byte[] GetFrameBytes()
        {
            if (CLEyeCameraGetFrame(_camera, _map, 500))
            {
                byte [] bytes = new byte[ImgSize];
                Marshal.Copy(_map, bytes, 0, ImgSize);
                return bytes;
            }
            return null;
        }

        // returns pointer to COPIED bytes
        //public byte[] GetFrameIntPtr()
        //{
        //    if (CLEyeCameraGetFrame(_camera, _map, 500))
        //    {
        //        byte[] bytes = new byte[ImgSize];
        //        IntPtr ptrCopy = Marshal.AllocHGlobal(ImgSize);
        //        Marshal.Copy(_section, ptrCopy, 0, ImgSize);
        //        return bytes;
        //    }
        //    return null;
        //}
        
        #region Cleanup

        ~PS3EyeDevice()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (disposing)
                {
                    // free managed resources
                    //Stop();
                    CLEyeCameraStop(_camera);
                    CLEyeDestroyCamera(_camera);
                }
                // free native resources if there are any.
                Destroy();
                IsCreated = false;
            }));
        }
        public void Destroy()
        {
            if (_map != IntPtr.Zero)
            {
                UnmapViewOfFile(_map);
                _map = IntPtr.Zero;
            }
            if (_section != IntPtr.Zero)
            {
                CloseHandle(_section);
                _section = IntPtr.Zero;
            }
        }

        #endregion
    }

    public enum CLEyeCameraColorMode
    {
        CLEYE_MONO_PROCESSED,
        CLEYE_COLOR_PROCESSED,
        CLEYE_MONO_RAW,
        CLEYE_COLOR_RAW,
        CLEYE_BAYER_RAW
    };
    public enum CLEyeCameraResolution
    {
        CLEYE_QVGA,
        CLEYE_VGA
    };
    public enum CLEyeCameraParameter
    {
        // camera sensor parameters
        CLEYE_AUTO_GAIN,			// [false, true]
        CLEYE_GAIN,					// [0, 79]
        CLEYE_AUTO_EXPOSURE,		// [false, true]
        CLEYE_EXPOSURE,				// [0, 511]
        CLEYE_AUTO_WHITEBALANCE,	// [false, true]
        CLEYE_WHITEBALANCE_RED,		// [0, 255]
        CLEYE_WHITEBALANCE_GREEN,	// [0, 255]
        CLEYE_WHITEBALANCE_BLUE,	// [0, 255]
        // camera linear transform parameters
        CLEYE_HFLIP,				// [false, true]
        CLEYE_VFLIP,				// [false, true]
        CLEYE_HKEYSTONE,			// [-500, 500]
        CLEYE_VKEYSTONE,			// [-500, 500]
        CLEYE_XOFFSET,				// [-500, 500]
        CLEYE_YOFFSET,				// [-500, 500]
        CLEYE_ROTATION,				// [-500, 500]
        CLEYE_ZOOM,					// [-500, 500]
        // camera non-linear transform parameters
        CLEYE_LENSCORRECTION1,		// [-500, 500]
        CLEYE_LENSCORRECTION2,		// [-500, 500]
        CLEYE_LENSCORRECTION3,		// [-500, 500]
        CLEYE_LENSBRIGHTNESS		// [-500, 500]
    };

}
