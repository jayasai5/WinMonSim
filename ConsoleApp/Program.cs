using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleApp
{
    /// <summary>
    /// class to store the window attributes like window name, its coordinates and handle
    /// </summary>
    class Window {
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

    class Program
    {
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);
        public static int BorderWidth = 7;
        public IntPtr currentWinHandle;
        public Rectangle currentWindow;
        public Thread RefreshBorderThread;
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        private const UInt32 SW_MAXIMIZE = 3;
        private const UInt32 SW_MINIMIZE = 2;
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern int ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hWnd, out Window.RECT lpRect);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "GetWindowText",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
            ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, 
            EnumDelegate lpEnumCallbackFunction, IntPtr lParam);
        static void Main(string[] args)
        {
            var collection = new List<Window>();
            //filter windows that are not visible and windows that do not have a name
            Program.EnumDelegate filter = delegate (IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = Program.GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();
                Window wnd = new Window();
                if (Program.IsWindowVisible(hWnd) && !string.IsNullOrEmpty(strTitle))
                {
                    wnd.WindowHandle = hWnd;
                    wnd.WindowName = strTitle;
                    Window.RECT coordinates;
                    if (Program.GetWindowRect(new HandleRef(null, hWnd), out coordinates))
                        wnd.WindowCoordinates = coordinates;
                    else
                        wnd.WindowCoordinates = new Window.RECT();
                    collection.Add(wnd);
                }
                return true;
            };

            if (Program.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var item in collection)
                {
                    drawBordersOnOpenWinds(item);
                }
            }
            Console.Read();
        }

        private static void drawBordersOnOpenWinds(Window window)
        {
            Rectangle myRect = new Rectangle();
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            windowPlacement.length = Marshal.SizeOf(windowPlacement);
            Program.GetWindowPlacement(window.WindowHandle, ref windowPlacement);
            myRect.X = window.WindowCoordinates.Left;
            myRect.Y = window.WindowCoordinates.Top;
            myRect.Width = (window.WindowCoordinates.Right - window.WindowCoordinates.Left)-1;
            myRect.Height = (window.WindowCoordinates.Bottom - window.WindowCoordinates.Top)-1;
            Console.WriteLine(window.WindowName);
            StringBuilder className = new StringBuilder(256);
            GetClassName(window.WindowHandle, className, className.Capacity);
            if (windowPlacement.showCmd == 1&&className.ToString()!= "ApplicationFrameWindow"&&className.ToString()!= "Windows.UI.Core.CoreWindow") {
                new Border().HighLight(myRect, Program.BorderWidth);
            }
        }
    }
}
