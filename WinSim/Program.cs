using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;


namespace WinSim
{
    static class Program
    {
        private static Process process;
        private static Border border;
        private static Color BorderColor;
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
        public class Window
        {
            public string WindowName { get; set; }
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;        // x position of upper-left corner
                public int Top;         // y position of upper-left corner
                public int Right;       // x position of lower-right corner
                public int Bottom;      // y position of lower-right corner
            }
            public RECT WindowCoordinates { get; set; }
            public IntPtr WindowHandle { get; set; }
        }
        public static int BorderWidth = 7;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hWnd, out Window.RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0) {
                Application.Run(new CreateIcons());
            }
            else
            {
                string machine = args[0];
                string path = args[1];
                Window window = new Window();
                process = Process.Start(path);
                window.WindowName = process.ProcessName;
                window.WindowHandle = getWindowHandle(process);
                Window.RECT coordinates;
                if (machine == "m1")
                {
                    BorderColor = Color.Blue;
                }
                else
                {
                    BorderColor = Color.Red;
                }
                WINDOWPLACEMENT oldWindowPlacement = new WINDOWPLACEMENT();
                WINDOWPLACEMENT newWindowPlacement = new WINDOWPLACEMENT();
                oldWindowPlacement.length = Marshal.SizeOf(oldWindowPlacement);
                newWindowPlacement.length = Marshal.SizeOf(newWindowPlacement);
                bool changed = true;
                while (!process.HasExited)
                {
                    Thread.Sleep(100);
                    if (Program.GetWindowRect(new HandleRef(null, window.WindowHandle), out coordinates))
                        window.WindowCoordinates = coordinates;
                    else
                        window.WindowCoordinates = new Window.RECT();
                    Program.GetWindowPlacement(window.WindowHandle, ref newWindowPlacement);
                    if (ComparePlacements(oldWindowPlacement, newWindowPlacement))
                    {
                        changed = true;
                    }
                    else {
                        changed = false;
                    }
                    oldWindowPlacement = newWindowPlacement;
                    if (changed)
                    {
                        switch (newWindowPlacement.showCmd)
                        {
                            case 1:
                                drawBorderAndMonitor(window, BorderColor);
                                break;
                            case 2:
                                border.Dispose();
                                break;
                            case 3:
                                drawBorderAndMonitor(window, BorderColor);
                                break;
                        }
                    }
                }
                border.Dispose();
            }
        }
        private static bool ComparePlacements(WINDOWPLACEMENT placement1, WINDOWPLACEMENT placement2) {
            if (placement1.showCmd != placement2.showCmd || placement1.length != placement2.length || placement1.ptMaxPosition != placement2.ptMaxPosition || placement1.ptMinPosition != placement2.ptMinPosition || placement1.rcNormalPosition != placement2.rcNormalPosition) {
                return true;
            }
            return false;
        }
        private static IntPtr getWindowHandle(Process proc) {
            
            if (!proc.HasExited)
            {
                while (!proc.HasExited)
                {
                    Thread.Sleep(10);
                    proc.Refresh();
                    if (proc.MainWindowHandle.ToInt32() != 0)
                    {
                        return proc.MainWindowHandle;
                    }
                }
                return IntPtr.Zero;
            }
            else {
                return Program.GetForegroundWindow();
            }

            
        }
        private static void drawBorderAndMonitor(Window window,Color BorderColor)
        {
            Rectangle myRect = new Rectangle();
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            windowPlacement.length = Marshal.SizeOf(windowPlacement);
            Program.GetWindowPlacement(window.WindowHandle, ref windowPlacement);
            myRect.X = window.WindowCoordinates.Left;
            myRect.Y = window.WindowCoordinates.Top;
            myRect.Width = (window.WindowCoordinates.Right - window.WindowCoordinates.Left) - 1;
            myRect.Height = (window.WindowCoordinates.Bottom - window.WindowCoordinates.Top) - 1;
            Console.WriteLine(window.WindowName);
            StringBuilder className = new StringBuilder(256);
            GetClassName(window.WindowHandle, className, className.Capacity);
            if (border != null) { border.Dispose(); }
            border = new Border(BorderColor);
            if (windowPlacement.showCmd == 1 && className.ToString() != "ApplicationFrameWindow" && className.ToString() != "Windows.UI.Core.CoreWindow")
            {
                border.HighLight(myRect, Program.BorderWidth);
            }
            //border.Dispose();
        }
    }
}
