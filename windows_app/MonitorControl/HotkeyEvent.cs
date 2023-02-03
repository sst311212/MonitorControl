using NHotkey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorControl
{
    internal class CustomEventArgs : EventArgs
    {
        public string Name { get; set; }

        public bool Handled { get; set; }

        public CustomEventArgs(string name)
        {
            Name = name;
            Handled = false;
        }
    }

    internal class HotkeyEvent
    {
        private static void Monitor_PowerSwitch()
        {
            IntPtr hMonitor = NativeAPI.MonitorFromWindow(IntPtr.Zero, NativeAPI.MONITORINFOF_PRIMARY);
            NativeAPI.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out int dwMonitors);
            PHYSICAL_MONITOR[] phMonitors = new PHYSICAL_MONITOR[dwMonitors];
            NativeAPI.GetPhysicalMonitorsFromHMONITOR(hMonitor, dwMonitors, phMonitors);
            NativeAPI.GetVCPFeatureAndVCPFeatureReply(phMonitors[0].hPhysicalMonitor, (byte)VCPCode.PowerMode, out _, out int dwPwrMode, out _);
            PowerMode dwSwitchMode = (dwPwrMode < (int)PowerMode.Off) ? PowerMode.BtnOff : PowerMode.On;
            NativeAPI.SetVCPFeature(phMonitors[0].hPhysicalMonitor, (byte)VCPCode.PowerMode, (int)dwSwitchMode);
            NativeAPI.DestroyPhysicalMonitors(dwMonitors, phMonitors);
        }

        private static void Monitor_InputSwitch(InputSelect Source)
        {
            IntPtr hMonitor = NativeAPI.MonitorFromWindow(IntPtr.Zero, NativeAPI.MONITORINFOF_PRIMARY);
            NativeAPI.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out int dwMonitors);
            PHYSICAL_MONITOR[] phMonitors = new PHYSICAL_MONITOR[dwMonitors];
            NativeAPI.GetPhysicalMonitorsFromHMONITOR(hMonitor, dwMonitors, phMonitors);
            NativeAPI.SetVCPFeature(phMonitors[0].hPhysicalMonitor, (byte)VCPCode.InputSelect, (int)Source);
            NativeAPI.DestroyPhysicalMonitors(dwMonitors, phMonitors);
        }

        public static void Hotkey_EventTrigger<T>(object sender, T e)
        {
            CustomEventArgs args;
            if (e.GetType() == typeof(CustomEventArgs))
                args = e as CustomEventArgs;
            else
                args = new CustomEventArgs((e as HotkeyEventArgs).Name);

            switch (args.Name)
            {
                case "PowerSwitch":
                    Monitor_PowerSwitch();
                    break;
                case "DisplayPort_1":
                    Monitor_InputSwitch(InputSelect.DisplayPort_1);
                    break;
                case "DisplayPort_2":
                    Monitor_InputSwitch(InputSelect.DisplayPort_2);
                    break;
                case "HDMI_1":
                    Monitor_InputSwitch(InputSelect.HDMI_1);
                    break;
                case "HDMI_2":
                    Monitor_InputSwitch(InputSelect.HDMI_2);
                    break;
            }
            args.Handled = true;
        }
    }
}
