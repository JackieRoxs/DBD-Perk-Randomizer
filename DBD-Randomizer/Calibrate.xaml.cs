using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Interop;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DBD_Randomizer
{
    /// <summary>
    /// Interaction logic for Calibrate.xaml
    /// </summary>
    public partial class Calibrate : Page
    {
        private CalibrationOverlay overlay;
        private bool isDeadByDaylightActive;
        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string folderPath;
        private string filePath;



        public Calibrate()
        {
            InitializeComponent();
            folderPath = System.IO.Path.Combine(appDataPath, "DBD Randomizer");
            filePath = System.IO.Path.Combine(folderPath, "cords.json");
            LoadCordsData();
        }


        public class Coordinate
        {
            public string Name { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class GlobalMouseHook
        {
            private const int WH_MOUSE_LL = 14;
            private const int WM_LBUTTONDOWN = 0x0201;

            private static LowLevelMouseProc _proc = HookCallback;
            private static IntPtr _hookID = IntPtr.Zero;

            public static event EventHandler<Point> OnMouseLeftButtonDown;

            public static void Start()
            {
                _hookID = SetHook(_proc);
            }

            public static void Stop()
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

            private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
                {
                    MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    OnMouseLeftButtonDown?.Invoke(null, new Point(hookStruct.pt.x, hookStruct.pt.y));
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
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
            private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
        }

        private async void startCalibrationButton_Click(object sender, RoutedEventArgs e)
        {
            overlay = new CalibrationOverlay();
            overlay.Left = SystemParameters.WorkArea.Width - overlay.Width - 10;
            overlay.Top = 10;
            overlay.Show();

            overlay.UpdateMessage("Please open Dead by Daylight and make it the active window.");
            await Task.Run(() => WaitForActiveWindow("DeadByDaylight"));

            overlay.UpdateMessage("Go to Killer or Survivor and open the loadout. Press '0' when ready.");
            await WaitForKeyPress(Key.D0);

            Point perkSlot1 = await WaitForMouseClick("Click on the first perk slot.");
            Point perkSlot2 = await WaitForMouseClick("Click on the second perk slot.");
            Point perkSlot3 = await WaitForMouseClick("Click on the third perk slot.");
            Point perkSlot4 = await WaitForMouseClick("Click on the fourth perk slot.");
            Point searchBar = await WaitForMouseClick("Click on the perk search bar.");
            Point searchButt = await WaitForMouseClick("Click on the search button.");
            Point firstPerk = await WaitForMouseClick("Click on the first perk in the search results.");

            var cords = new
            {
                perkSlot1 = new[] { (int)perkSlot1.X, (int)perkSlot1.Y },
                perkSlot2 = new[] { (int)perkSlot2.X, (int)perkSlot2.Y },
                perkSlot3 = new[] { (int)perkSlot3.X, (int)perkSlot3.Y },
                perkSlot4 = new[] { (int)perkSlot4.X, (int)perkSlot4.Y },
                searchBar = new[] { (int)searchBar.X, (int)searchBar.Y },
                searchButt = new[] { (int)searchButt.X, (int)searchButt.Y },
                firstPerk = new[] { (int)firstPerk.X, (int)firstPerk.Y }
            };

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(cords, Newtonsoft.Json.Formatting.Indented));

            overlay.UpdateMessage("Calibration successful! The overlay will close shortly.");
            await Task.Delay(3000);
            overlay.Close();
            LoadCordsData();
        }


        private void WaitForActiveWindow(string processName)
        {
            while (true)
            {
                var activeWindowTitle = GetActiveWindowTitle();
                if (!string.IsNullOrEmpty(activeWindowTitle) && activeWindowTitle.Contains(processName))
                {
                    isDeadByDaylightActive = true;
                    break;
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private async Task WaitForKeyPress(Key key)
        {
            while (true)
            {
                if (Keyboard.IsKeyDown(key))
                {
                    break;
                }
                await Task.Delay(100);
            }
        }

        private TaskCompletionSource<Point> _globalClickTcs;

        private async Task<Point> WaitForMouseClick(string message)
        {
            overlay.UpdateMessage(message);
            _globalClickTcs = new TaskCompletionSource<Point>();

            GlobalMouseHook.OnMouseLeftButtonDown += GlobalMouseHook_OnMouseLeftButtonDown;
            GlobalMouseHook.Start();

            Point clickPosition = await _globalClickTcs.Task;

            GlobalMouseHook.OnMouseLeftButtonDown -= GlobalMouseHook_OnMouseLeftButtonDown;
            GlobalMouseHook.Stop();

            return clickPosition;
        }

        private void GlobalMouseHook_OnMouseLeftButtonDown(object sender, Point e)
        {
            _globalClickTcs.TrySetResult(e);
        }

        private void LoadCordsData()
        {
            if (File.Exists(filePath))
            {
                var cordsData = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(File.ReadAllText(filePath));
                List<Coordinate> coordinates = new List<Coordinate>();

                foreach (var entry in cordsData)
                {
                    coordinates.Add(new Coordinate
                    {
                        Name = entry.Key,
                        X = entry.Value[0],
                        Y = entry.Value[1]
                    });
                }

                cordData.ItemsSource = coordinates;
                cordData.Visibility = Visibility.Visible;
                calDataDisplay.Visibility = Visibility.Visible;
            }
            else
            {
                cordData.Visibility = Visibility.Collapsed;
                calDataDisplay.Visibility = Visibility.Collapsed;
            }
        }

        private Point GetMousePosition()
        {
            var position = System.Windows.Forms.Control.MousePosition;
            return new Point(position.X, position.Y);
        }

        private string GetActiveWindowTitle()
        {
            var hwnd = GetForegroundWindow();
            var length = GetWindowTextLength(hwnd);
            var builder = new System.Text.StringBuilder(length);
            GetWindowText(hwnd, builder, length + 1);
            return builder.ToString();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder lpString, int cch);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hwnd);
    }
}

