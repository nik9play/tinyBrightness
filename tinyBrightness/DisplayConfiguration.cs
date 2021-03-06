﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace tinyBrightness
{
    class DisplayConfiguration
    {
        private const int MONITOR_DEFAULTTONEAREST = 2;

        private const int PHYSICAL_MONITOR_DESCRIPTION_SIZE = 128;

        private const int MC_CAPS_BRIGHTNESS = 0x2;

        [StructLayout(LayoutKind.Sequential)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U2, SizeConst = PHYSICAL_MONITOR_DESCRIPTION_SIZE)]
            public char[] szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private extern static bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = false)]
        private extern static IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetMonitorCapabilities(IntPtr hMonitor, out uint pdwMonitorCapabilities, out uint pdwSupportedColorTemperatures);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        private extern static bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct MONITORINFOEX
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        #region Public

        public static IntPtr GetCurrentMonitor()
        {
            if (!GetCursorPos(out POINT point))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
        }

        public static IntPtr GetMonitorByBounds(System.Drawing.Rectangle Bound)
        {
            POINT point = new POINT { x = Bound.X + 1, y = Bound.Y + 1 };

            return MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
        }

        public static PHYSICAL_MONITOR[] GetPhysicalMonitors(IntPtr hMonitor)
        {
            uint dwNumberOfPhysicalMonitors;
            if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out dwNumberOfPhysicalMonitors))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            PHYSICAL_MONITOR[] physicalMonitorArray = new PHYSICAL_MONITOR[dwNumberOfPhysicalMonitors];
            if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, dwNumberOfPhysicalMonitors, physicalMonitorArray))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return physicalMonitorArray;
        }

        public static void DestroyPhysicalMonitors(PHYSICAL_MONITOR[] physicalMonitorArray)
        {
            if (!DestroyPhysicalMonitors((uint)physicalMonitorArray.Length, physicalMonitorArray))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private static uint GetMonitorCapabilities(PHYSICAL_MONITOR physicalMonitor)
        {
            uint dwMonitorCapabilities, dwSupportedColorTemperatures;
            if (!GetMonitorCapabilities(physicalMonitor.hPhysicalMonitor, out dwMonitorCapabilities, out dwSupportedColorTemperatures))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return dwMonitorCapabilities;
        }

        public static bool GetBrightnessSupport(PHYSICAL_MONITOR physicalMonitor)
        {
            return (GetMonitorCapabilities(physicalMonitor) & MC_CAPS_BRIGHTNESS) != 0;
        }

        public static double GetMonitorBrightness(PHYSICAL_MONITOR physicalMonitor)
        {
            if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out uint dwMinimumBrightness, out uint dwCurrentBrightness, out uint dwMaximumBrightness))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return (double)(dwCurrentBrightness - dwMinimumBrightness) / (double)(dwMaximumBrightness - dwMinimumBrightness);
        }

        public static void SetMonitorBrightness(PHYSICAL_MONITOR physicalMonitor, double brightness, uint dwMinimumBrightness, uint dwMaximumBrightness)
        {
            if (!SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, (uint)(dwMinimumBrightness + (dwMaximumBrightness - dwMinimumBrightness) * brightness)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public struct MonitorExtremums
        {
            public uint Min;
            public uint Max;
            public uint Current;
        }

        public static MonitorExtremums GetMonitorExtremums(PHYSICAL_MONITOR physicalMonitor)
        {
            if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out uint dwMinimumBrightness, out uint dwCurrentBrightness, out uint dwMaximumBrightness))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new MonitorExtremums
            {
                Min = dwMinimumBrightness,
                Max = dwMaximumBrightness,
                Current = dwCurrentBrightness
            };
        }

        public static double SetBrightnessOffset(PHYSICAL_MONITOR physicalMonitor, double offset)
        {
            if (!GetMonitorBrightness(physicalMonitor.hPhysicalMonitor, out uint dwMinimumBrightness, out uint dwCurrentBrightness, out uint dwMaximumBrightness))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            double CurrentBrightness = (double)(dwCurrentBrightness - dwMinimumBrightness) / (double)(dwMaximumBrightness - dwMinimumBrightness);
            double brightness = CurrentBrightness + offset;
            
            if (brightness > 1) brightness = 1;
            else if (brightness < 0) brightness = 0;

            if (!SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, (uint)(dwMinimumBrightness + (dwMaximumBrightness - dwMinimumBrightness) * brightness)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return brightness;
        }

        public static void SetBrightnessOffset(PHYSICAL_MONITOR physicalMonitor, double offset, double CurrentBrightness, uint dwMinimumBrightness, uint dwMaximumBrightness)
        {
            double brightness = CurrentBrightness + offset;

            if (brightness > 1) brightness = 1;
            else if (brightness < 0) brightness = 0;

            if (!SetMonitorBrightness(physicalMonitor.hPhysicalMonitor, (uint)(dwMinimumBrightness + (dwMaximumBrightness - dwMinimumBrightness) * brightness)))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        #endregion
    }
}
