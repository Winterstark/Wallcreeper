using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Wallcreeper
{
    class Misc
    {
        public static string GetDirPath(string path)
        {
            return path.Substring(0, path.LastIndexOf('\\'));
        }

        public static string GetFilename(string path)
        {
            return path.Substring(path.LastIndexOf('\\') + 1);
        }

        public static string GetArchiver()
        {
            //look for 7zip
            string path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\7-Zip", "Path", "").ToString();

            if (path != "")
            {
                if (path[path.Length - 1] != '\\')
                    path = path + '\\';

                return path + "7zG.exe";
            }
            else
            {
                //try winrar
                path = Registry.GetValue(@"HKEY_CLASSES_ROOT\WinRAR\shell\open\command", "", "").ToString();

                if (path[0] == '"')
                    path = path.Substring(1);
                if (path.Contains('"'))
                    path = path.Substring(0, path.IndexOf('"'));
                
                return path;
            }
        }
    }

    public class Theme
    {
        public string name, wallDir, date, time, weather;
        public bool overpower;

        public Theme(string name, string wallDir, string date, string time, string weather, bool overpower)
        {
            this.name = name;
            this.wallDir = wallDir;
            this.date = date;
            this.time = time;
            this.weather = weather;
            this.overpower = overpower;
        }

        public string SaveTxt()
        {
            return name + Environment.NewLine + wallDir.Replace(System.Windows.Forms.Application.StartupPath + "\\wall_themes\\", "") + Environment.NewLine + date + Environment.NewLine + time + Environment.NewLine + weather + Environment.NewLine + overpower + Environment.NewLine;
        }
    }

    class Personalization
    {
        #region DLLImports
        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        public static int WM_CLOSE = 0x10;

        enum ShowWindowCommands : int
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);
        #endregion

        public static void Show()
        {
            System.Diagnostics.Process.Start("control.exe", "/name Microsoft.Personalization");
        }

        public static void Close()
        {
            try
            {
                var collection = new List<string>();
                EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
                {
                    StringBuilder strbTitle = new StringBuilder(255);
                    int nLength = GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                    string strTitle = strbTitle.ToString();

                    if (IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false && strTitle == "Personalization")
                        SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    return true;
                };

                if (EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
                    ;
            }
            catch (Exception exc)
            {
                //asdf
            }
        }

        public static void Hide()
        {
            try
            {
                var collection = new List<string>();
                EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
                {
                    StringBuilder strbTitle = new StringBuilder(255);
                    int nLength = GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                    string strTitle = strbTitle.ToString();

                    if (IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false && strTitle == "Personalization")
                        ShowWindow(hWnd, ShowWindowCommands.Hide);

                    return true;
                };

                if (EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
                    ;
            }
            catch (Exception exc)
            {
                //asdf
            }
        }
    }
}
