using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WinSim
{
    public class IconBorders
    {
        public IconBorders()
        {
            getIconLocations();
        }
        public List<ShortCut> shortCuts = new List<ShortCut>();
        public const uint LVM_FIRST = 0x1000;
        public const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
        public const uint LVM_GETITEMTEXT = LVM_FIRST + 75;
        public const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_VM_READ = 0x0010;
        public const uint PROCESS_VM_WRITE = 0x0020;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RELEASE = 0x8000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_READWRITE = 4;
        public const int LVIF_TEXT = 0x0001;
        public IntPtr desktop = GetDesktopWindow();
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
           uint dwSize, uint dwFreeType);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           IntPtr lpBuffer, int nSize, ref uint vNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
           IntPtr lpBuffer, int nSize, ref uint vNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess,
            bool bInheritHandle, uint dwProcessId);
        [DllImport("user32.DLL")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.DLL")]
        public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);
        [DllImport("user32.DLL")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent,
            IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd,
            out uint dwProcessId);
        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();
        [StructLayout(LayoutKind.Sequential)]
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            public IntPtr pszText; // string
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;

        }
        public struct ShortCut {
            public string Name;
            public Point Location;
        }
        public void getIconLocations()
        {
            // get the handle of the desktop listview
            IntPtr vHandle = FindWindow("Progman", "Program Manager");
            vHandle = FindWindowEx(vHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr hWorkerW = IntPtr.Zero;
            if (vHandle == IntPtr.Zero) {
                do
                {
                    hWorkerW = FindWindowEx(desktop,hWorkerW,"WorkerW", null);
                    vHandle = FindWindowEx(hWorkerW,IntPtr.Zero, "SHELLDLL_DefView", null);
                } while (vHandle == IntPtr.Zero && hWorkerW != IntPtr.Zero);
            }
            vHandle = FindWindowEx(vHandle, IntPtr.Zero, "SysListView32", "FolderView");

            //Get total count of the icons on the desktop
            int vItemCount = SendMessage(vHandle, LVM_GETITEMCOUNT, 0, 0);
            uint vProcessId;
            GetWindowThreadProcessId(vHandle, out vProcessId);
            IntPtr vProcess = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ |
                PROCESS_VM_WRITE, false, vProcessId);
            IntPtr vPointer = VirtualAllocEx(vProcess, IntPtr.Zero, 4096,
                MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
            try
            {
                for (int j = 0; j < vItemCount; j++)
                {
                    byte[] vBuffer = new byte[128];
                    LVITEM[] vItem = new LVITEM[1];
                    vItem[0].mask = LVIF_TEXT;
                    vItem[0].iItem = j;
                    vItem[0].iSubItem = 0;
                    vItem[0].cchTextMax = vBuffer.Length;
                    vItem[0].pszText = (IntPtr)((int)vPointer + Marshal.SizeOf(typeof(LVITEM)));
                    uint vNumberOfBytesRead = 0;
                    WriteProcessMemory(vProcess, vPointer,
                        Marshal.UnsafeAddrOfPinnedArrayElement(vItem, 0),
                        Marshal.SizeOf(typeof(LVITEM)), ref vNumberOfBytesRead);
                    SendMessage(vHandle, LVM_GETITEMTEXT, j, vPointer.ToInt32());
                    ReadProcessMemory(vProcess,
                        (IntPtr)((int)vPointer + Marshal.SizeOf(typeof(LVITEM))),
                        Marshal.UnsafeAddrOfPinnedArrayElement(vBuffer, 0),
                        vBuffer.Length, ref vNumberOfBytesRead);
                    string vText = Encoding.Unicode.GetString(vBuffer, 0,
                        (int)vNumberOfBytesRead);
                    string IconName = vText.Substring(0, vText.IndexOf("\0"));
                    //Get icon location
                    SendMessage(vHandle, LVM_GETITEMPOSITION, j, vPointer.ToInt32());
                    Point[] vPoint = new Point[1];
                    ReadProcessMemory(vProcess, vPointer,
                        Marshal.UnsafeAddrOfPinnedArrayElement(vPoint, 0),
                        Marshal.SizeOf(typeof(Point)), ref vNumberOfBytesRead);
                    string IconLocation = vPoint[0].ToString();
                    string machine = IconName.Substring(IconName.Length - 4);
                    //Insert an item into the ListView
                    if (machine == "(m3)"||machine=="(m2)"||machine=="(m1)"||machine=="(m4)")
                    {
                        this.shortCuts.Add(new ShortCut() { Name = IconName, Location = vPoint[0] });
                    }
                    
                }
            }
            finally
            {
                VirtualFreeEx(vProcess, vPointer, 0, MEM_RELEASE);
                CloseHandle(vProcess);
            }
        }
    }
}