﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using SharpDX.DXGI;
using SharpDX.Direct3D11;

using BetterLiveScreen.Recording.Video;
using BetterLiveScreen.Recording.Video.NvEncoder;
using BetterLiveScreen.Recording.Video.WGC;

using D3D11Device = SharpDX.Direct3D11.Device;
using NvEncoder = BetterLiveScreen.Recording.Video.NvEncoder.Encoder;
using NvFormat = BetterLiveScreen.Recording.Video.NvEncoder.Format;
using System.Runtime.InteropServices;
using System.Threading;

namespace BetterLiveScreen.Recording.Types
{
    public class CaptureSupports
    {
        //Video - Capture
        public bool DesktopDuplication { get; set; }
        public bool WGC { get; set; }

        //Video - Encoding
        public bool Nvenc { get; set; }

        //Audio
        public bool WinCaptureAudio { get; set; }

        public static CaptureSupports Default => new CaptureSupports()
        {
            DesktopDuplication = true,
            WGC = true,
            Nvenc = true,
            WinCaptureAudio = true
        };

        public static bool SupportsDesktopDuplication()
        {
            return SupportsDesktopDuplication(RescreenSettings.PrimaryMonitor);
        }
        
        public static bool SupportsDesktopDuplication(MonitorInfo monitor)
        {
            var factory = new Factory1();
            var deviceFromMonitor = FScreen.GetAdapterOutput(monitor, factory);

            //Get first adapter
            var adapter = factory.GetAdapter1(deviceFromMonitor.Item1);
            //Get device from adapter
            var device = new D3D11Device(adapter);
            //Get front buffer of the adapter
            var output = adapter.GetOutput(deviceFromMonitor.Item2);
            var output1 = output.QueryInterface<Output1>();

            bool supported = SupportsDesktopDuplication(output1, device, out var duplicatedOutput);
            if (supported) duplicatedOutput.Dispose();

            return supported;
        }

        public static bool SupportsDesktopDuplication(Output1 output1, D3D11Device device, out OutputDuplication duplicatedOutput)
        {
            try
            {
                duplicatedOutput = output1.DuplicateOutput(device);
            }
            catch (Exception ex)
            {
                Rescreen.Supports.DesktopDuplication = false;

                Debug.WriteLine($"[Error] Desktop Duplication Not Supported. Error Message : {ex}");
                duplicatedOutput = null;

                return false;
            }

            return true;
        }

        public static bool SupportsWGC()
        {
            if (!WGCHelper.IsInitialized && !WGCHelper.Initialize())
            {
                Rescreen.Supports.WGC = false;
                Debug.WriteLine("[Warning] WGC Not Supported.");

                return false;
            }
            return true;
        }

        public static bool SupportsNvenc()
        {
            return SupportsNvenc(RescreenSettings.PrimaryMonitor);
        }

        public static bool SupportsNvenc(MonitorInfo monitor)
        {
            var factory = new Factory1();
            var deviceFromMonitor = FScreen.GetAdapterOutput(monitor, factory);

            //Get first adapter
            var adapter = factory.GetAdapter1(deviceFromMonitor.Item1);
            //Get device from adapter
            var device = new D3D11Device(adapter);

            return SupportsNvenc(
                Rescreen.ScreenActualSize.Width,
                Rescreen.ScreenActualSize.Height,
                Rescreen.FpsIfUnfixed60,
                BitrateInfo.GetBitrateFromMbps(Rescreen.GetBitrateInfoBySize(Rescreen.ScreenActualSize.Height, Rescreen.FpsIfUnfixed60).MbpsAverage),
                device,
                (s, e) => { },
                out _);
        }

        public static bool SupportsNvenc(int width, int height, int fps, int bitrate, D3D11Device device, EventHandler<(IntPtr, int)> onEncoded, out NvEncoder encoder)
        {
            encoder = new NvEncoder();

            EncoderDesc setting = new EncoderDesc()
            {
                width = width,
                height = height,
                frameRate = fps,
                format = NvFormat.B8G8R8A8_UNORM,
                bitRate = bitrate
            };
            setting.maxFrameSize = setting.bitRate / setting.frameRate;

            encoder.Create(setting, device);
            encoder.onEncoded += onEncoded;

            if (!encoder.isValid)
            {
                Rescreen.Supports.Nvenc = false;
                Debug.WriteLine("[Warning] Nvenc Encoding Not Supported.");

                return false;
            }

            return true;
        }
    }
}
