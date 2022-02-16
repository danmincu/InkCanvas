using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TransparentInkCanvas
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        

        public App()
        {
            _hookID = SetHook(_proc);
            this.Exit += App_Exit;
            this.Deactivated += App_Deactivated;
            
            //Application.Run();
           
        }

        private void App_Deactivated(object sender, EventArgs e)
        {
            //Window window = (Window)sender;
            if (Current.MainWindow != null)
            { Current.MainWindow.Topmost = true; }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
             UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        static UInt32 MI_WP_SIGNATURE = 0xFF515700;
        static UInt32 SIGNATURE_MASK = 0xFFFFFF00;
        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (Current.MainWindow as IStylus)?.IsDrawing() == false)
            {
                   
                //Console.WriteLine((MouseMessages)wParam);
                
                if (MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
                {
                  //  Console.WriteLine((MouseMessages)wParam);
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    //var MI_WP_SIGNATURE = 0xFF515700;
                    //var SIGNATURE_MASK = 0xFFFFFF00;

                    if (((hookStruct.dwExtraInfo.ToInt64()) & SIGNATURE_MASK) == MI_WP_SIGNATURE)
                    {
                       //Console.WriteLine(" Pen Move" + (MouseMessages)wParam);
                        (App.Current.MainWindow as IStylus)?.StylusHover();
                    }
                    else
                    {
                        (App.Current.MainWindow as IStylus)?.MouseMove();
                    }


                    //if (hookStruct.dwExtraInfo != System.IntPtr.Zero)
                    //{
                    //    Console.WriteLine(hookStruct.dwExtraInfo);
                    //    Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
                    //}
                }
                //if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                //{
                //    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                //    if (((hookStruct.dwExtraInfo.ToInt64()) & SIGNATURE_MASK) == MI_WP_SIGNATURE)
                //    {
                //        //Console.WriteLine(" Pen Move" + (MouseMessages)wParam);
                //        (App.Current.MainWindow as IStylus)?.StylusHover();
                //    }
                //    //    Console.WriteLine(hookStruct.dwExtraInfo);
                //    //    Console.WriteLine(hookStruct.flags);
                //    //    Console.WriteLine(hookStruct.mouseData);
                //    //    Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
                //}
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetMessageExtraInfo();


        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
