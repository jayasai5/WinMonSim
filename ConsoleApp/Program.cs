using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
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
        

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(HandleRef hWnd, out Window.RECT lpRect);
        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
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
                    Console.WriteLine("window Name:"+ item.WindowName+" (x,y): (" + item.WindowCoordinates.Left + "," + item.WindowCoordinates.Top 
                        + "),(x,y): (" + item.WindowCoordinates.Right  + "," + item.WindowCoordinates.Bottom +")");
                }
            }
            Console.Read();
        }
    }
}
