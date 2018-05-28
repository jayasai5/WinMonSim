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
    public static class Program
    {
        private static Process process;
        private static Border border;
        private static Border taskbarBorder;
        private static Color BorderColor;
        private static Color[] Colors = { Color.Pink, Color.Blue, Color.Red, Color.Green };
        private static Config config = new Config();
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
            // load configuration settings
            load_config();
            if (args == null || args.Length == 0) {
                // application is running in configuration mode
                Application.Run(new CreateIcons(config));
            }
            else
            {
                int machine = 1;
                // application is running to create and monitor a border
                try
                {
                    //set the border color of the application based on the machine index
                    machine = Int32.Parse(args[0]);
                    BorderColor = Colors[config.colors[machine-1]];
                }
                catch (Exception e) {
                    MessageBox.Show("Invalid arguments", "Error");
                    Application.Exit();
                }
                string path = args[1]; //path to the executable to be lauched
                Window window = new Window();
                //start the executable from the path in command line arguments
                process = Process.Start(path);
                window.WindowName = process.ProcessName;
                //wait for the application to start and grab the window handle to draw a border around
                window.WindowHandle = getWindowHandle(process);
                Window.RECT coordinates;
                // maintain both the current window placement and old window position to monitor any changes to the window.
                WINDOWPLACEMENT oldWindowPlacement = new WINDOWPLACEMENT();
                WINDOWPLACEMENT newWindowPlacement = new WINDOWPLACEMENT();
                bool background = false;
                oldWindowPlacement.length = Marshal.SizeOf(oldWindowPlacement);
                newWindowPlacement.length = Marshal.SizeOf(newWindowPlacement);
                bool changed = true;
                // run an infinite loop over the process to check if the process has exited
                while (!process.HasExited)
                {
                    //sleep for 100ms to avoid hogging resources
                    Thread.Sleep(30);
                    if (Program.GetWindowRect(new HandleRef(null, window.WindowHandle), out coordinates))
                        window.WindowCoordinates = coordinates;
                    else
                        window.WindowCoordinates = new Window.RECT();
                    Program.GetWindowPlacement(window.WindowHandle, ref newWindowPlacement);
                    // window went into background
                    if (GetForegroundWindow() != window.WindowHandle) { background = true; }
                    // check if there is any changes to the window by using the old window handle
                    if (ComparePlacements(oldWindowPlacement, newWindowPlacement))
                    {
                        changed = true;
                    }
                    else {
                        changed = false;
                    }
                    //the window was in the background and it came to foreground so redraw
                    if (background && GetForegroundWindow() == window.WindowHandle) {
                        changed = true;
                        background = false;
                    }
                    oldWindowPlacement = newWindowPlacement;
                    if (changed)
                    {
                        // if the window has changed then redraw the border accordingly
                        switch (newWindowPlacement.showCmd)
                        {
                            case 1:
                                //normal
                                drawBorder(window,false);
                                break;
                            case 2:
                                //minimized
                                border.Dispose();
                                //taskbarBorder.Dispose();
                                break;
                            case 3:
                                //maximized
                                drawBorder(window, true);
                                break;
                        }
                    }
                }
                //process exited so dispose the window borders
                border.Dispose();
                //taskbarBorder.Dispose();
            }
        }
        /// <summary>
        /// loads the configuration of the application from settings
        /// </summary>
        /// <returns>
        /// configuration object
        /// </returns>
        private static void load_config()
        {
            config.no_of_screens = (int)Properties.Settings.Default["screens"];
            config.colors = Array.ConvertAll(Properties.Settings.Default["colors"].ToString().Split(' '), s => int.Parse(s));
        }
        /// <summary>
        /// this method compares two window placement objects and report any changes to it
        /// </summary>
        /// <param name="placement1">old window placement</param>
        /// <param name="placement2">new window placement</param>
        /// <returns>if the old and new placements are same</returns>
        private static bool ComparePlacements(WINDOWPLACEMENT placement1, WINDOWPLACEMENT placement2) {
            if (placement1.showCmd != placement2.showCmd // window is either maximized of minimized 
                || placement1.length != placement2.length // window dimensions changed
                || placement1.ptMaxPosition != placement2.ptMaxPosition 
                || placement1.ptMinPosition != placement2.ptMinPosition 
                || placement1.rcNormalPosition != placement2.rcNormalPosition)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// This method waits and returns the corresponding window handle after the process started
        /// </summary>
        /// <param name="proc">The process that is started</param>
        /// <returns>Window handle of the process</returns>
        private static IntPtr getWindowHandle(Process proc) {
            // check if process has exited after starting
            if (!proc.HasExited)
            {
                while (!proc.HasExited)
                {
                    // keep refreshing till the process window handle is not zero
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
                // If the process killed itself and spawned a new window
                return Program.GetForegroundWindow();
            }

            
        }
        /// <summary>
        /// This method draws a border around the window
        /// </summary>
        /// <param name="window">the window object around which to draw border</param>
        /// <param name="desktop">whether the window is maximized or not</param>
        private static void drawBorder(Window window,bool desktop)
        {
            Rectangle myRect = new Rectangle();
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            windowPlacement.length = Marshal.SizeOf(windowPlacement);
            Program.GetWindowPlacement(window.WindowHandle, ref windowPlacement);
            myRect.X = window.WindowCoordinates.Left;
            myRect.Y = window.WindowCoordinates.Top;
            myRect.Width = (window.WindowCoordinates.Right - window.WindowCoordinates.Left) - 1;
            myRect.Height = (window.WindowCoordinates.Bottom - window.WindowCoordinates.Top) - 1;
            // check if border already exists if so kill it
            if (border != null) { border.Dispose(); }
            Rectangle taskbar = getTaskBarRectangle();
            border = new Border(BorderColor);
            //draw the border
            border.HighLight(myRect, desktop,Program.BorderWidth);
            //taskbarBorder = new Border(BorderColor);
            //taskbarBorder.HighLight(taskbar, desktop, BorderWidth);

            //border.Dispose();
        }

        private static Rectangle getTaskBarRectangle()
        {
            //get the working area i.e the area that does not include taskbar
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            //get the bounds of the desktop
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            //subtract the working area from bounds and we get the taskbar rectangle
            if (Screen.PrimaryScreen.WorkingArea.Top > 0)
            {
                // taskbar is on the top of the screen
                return new Rectangle(bounds.X, bounds.Y, bounds.Width, (bounds.Height - workingArea.Height));
            }
            else if (Screen.PrimaryScreen.WorkingArea.Left > 0)
            {
                //taskbar is on the left
                return new Rectangle(bounds.X, bounds.Y, (bounds.Width - workingArea.Width), bounds.Height);
            }
            else if (Screen.PrimaryScreen.WorkingArea.Bottom > 0)
            {
                //taskbar is on the bottom
                return new Rectangle(bounds.X, (workingArea.Height-bounds.Y), bounds.Width, (bounds.Height - workingArea.Height));
            }
            else {
                //taskbar is on the right
                return new Rectangle((bounds.X + workingArea.Width), bounds.Y, bounds.Width, bounds.Height);
            }
        }
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        public class Config
        {
            public int no_of_screens;
            public int[] colors;
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
    }
}
