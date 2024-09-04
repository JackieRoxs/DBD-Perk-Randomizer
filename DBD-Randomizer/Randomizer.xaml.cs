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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;
using System.Windows.Forms;

namespace DBD_Randomizer
{
    /// <summary>
    /// Interaction logic for Randomizer.xaml
    /// </summary>
    public partial class Randomizer : Page
    {
        ConvertKeyToVirtual ConvertKeyToVirtual { get; set; }
        private bool hotkeyEnabled = false;
        const int INPUT_MOUSE = 0;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const int INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_SCANCODE = 0x0008;
        private const int KILLER_HOTKEY_ID = 9001; // ID for Killer randomizer hotkey
        private const int SURVIVOR_HOTKEY_ID = 9002; // ID for Survivor randomizer hotkey
        public uint killerRandomizerKey = 0x76; // Default key: F7
        public uint survivorRandomizerKey = 0x78; // Default key: F9

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private bool isDeadByDaylightActive;
        private string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DBD Randomizer");
        private string calibrationPath;
        private string settingPath;
        private string perkPath;


        public Randomizer()
        {
            InitializeComponent();
            calibrationPath = System.IO.Path.Combine(folderPath, "cords.json");
            settingPath = System.IO.Path.Combine(folderPath, "settings.json");
            perkPath = System.IO.Path.Combine(folderPath, "perkList.json");
            Loaded += Randomizer_Loaded;
            Unloaded += Randomizer_Unloaded;
            LoadHotkeyFromSettings();
            LoadCalibration();
        }

        public void LoadCalibration()
        {
            if (File.Exists(calibrationPath))
            {
                calibrationWarning.Visibility = Visibility.Collapsed; // Hide warning label
                SurvivorRandomButt.IsEnabled = true; // Enable Survivor button
                KillerRandomButt.IsEnabled = true; // Enable Killer button
            }
            else
            {
                calibrationWarning.Visibility = Visibility.Visible; // Show warning label
                SurvivorRandomButt.IsEnabled = false; // Disable Survivor button
                KillerRandomButt.IsEnabled = false; // Disable Killer button
            }
        }

        public void LoadHotkeyFromSettings()
        {
            if (File.Exists(settingPath))
            {
                var json = File.ReadAllText(settingPath);
                dynamic settings = JsonConvert.DeserializeObject(json);
                killerRandomizerKey = (uint)settings.Hotkeys.KillerRandomizer;
                survivorRandomizerKey = (uint)settings.Hotkeys.SurvivorRandomizer;
            }
        }

        private void Randomizer_Loaded(object sender, RoutedEventArgs e)
        {
            var windowHandle = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            HwndSource source = HwndSource.FromHwnd(windowHandle);
            source.AddHook(HwndHook);
            RegisterHotKey(windowHandle, KILLER_HOTKEY_ID, 0x0000, killerRandomizerKey);
            RegisterHotKey(windowHandle, SURVIVOR_HOTKEY_ID, 0x0000, survivorRandomizerKey);
        }

        public void Randomizer_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var windowHandle = new WindowInteropHelper(window).Handle;
                UnregisterHotKey(windowHandle, KILLER_HOTKEY_ID);
                UnregisterHotKey(windowHandle, SURVIVOR_HOTKEY_ID);
            }
        }


        private IntPtr HwndHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            { 
                if (wParam.ToInt32() == KILLER_HOTKEY_ID && hotkeyEnabled)
                {
                    RandomizePerks("Survivor");
                    handled = true;
                }
                else if (wParam.ToInt32() == SURVIVOR_HOTKEY_ID && hotkeyEnabled)
                {
                    RandomizePerks("Killer");
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ToggleHotkeyState()
        {
            hotkeyEnabled = !hotkeyEnabled;

            hotkeyButt.Background = new SolidColorBrush(hotkeyEnabled ? Colors.DarkGreen : Colors.DarkRed);
        }

        private void hotkeyButt_Click(object sender, RoutedEventArgs e)
        {
            ToggleHotkeyState();
        }

        public void disableHotkey()
        {
            hotkeyEnabled = false;
        }

        public class Perk
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string Role { get; set; }
            public string Img { get; set; }
            public string Tier { get; set; }
            public int Rank { get; set; }
            public string Character { get; set; }
            public string Description { get; set; }
        }


        public class PerkSelection
        {
            public int[] PerkSlot1 { get; set; }
            public int[] PerkSlot2 { get; set; }
            public int[] PerkSlot3 { get; set; }
            public int[] PerkSlot4 { get; set; }
            public int[] SearchBar { get; set; }
            public int[] SearchButt { get; set; }
            public int[] FirstPerk { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public int type;
            public MOUSEKEYBDHARDWAREINPUT mkhi;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private void RandomizePerks(string type)
        {
            // Deserialize the calibration file for coordinates
            var calibrationData = JsonConvert.DeserializeObject<PerkSelection>(File.ReadAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DBD Randomizer", "cords.json")));

            var perksData = JsonConvert.DeserializeObject<Dictionary<string, List<Perk>>>(File.ReadAllText(perkPath));

            // Get the list of perks
            var perks = perksData["perks"];

            // Filter perks based on the role type
            var filteredPerks = perks.Where(p => p.Role.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

            // Randomly select 4 perks from the filtered list
            Random random = new Random();
            var randomPerks = filteredPerks.OrderBy(x => random.Next()).Take(4).Select(p => p.Name).ToList();

            // Simulate the process for each perk slot using randomPerks
            ClickAndSelectPerk(calibrationData.PerkSlot1, calibrationData.SearchBar, randomPerks[0], calibrationData.SearchButt, calibrationData.FirstPerk);
            ClickAndSelectPerk(calibrationData.PerkSlot2, calibrationData.SearchBar, randomPerks[1], calibrationData.SearchButt, calibrationData.FirstPerk);
            ClickAndSelectPerk(calibrationData.PerkSlot3, calibrationData.SearchBar, randomPerks[2], calibrationData.SearchButt, calibrationData.FirstPerk);
            ClickAndSelectPerk(calibrationData.PerkSlot4, calibrationData.SearchBar, randomPerks[3], calibrationData.SearchButt, calibrationData.FirstPerk);
        }


        private static void ClickAndSelectPerk(int[] perkSlot, int[] searchBar, string perkName, int[] searchButt, int[] firstPerk)
        {
            // Move to the perk slot and click
            SetCursorPos(perkSlot[0], perkSlot[1]);
            Thread.Sleep(50);
            MouseClick();

            Thread.Sleep(350); // Wait for UI response

            // Move to the search bar and click
            SetCursorPos(searchBar[0], searchBar[1]);
            Thread.Sleep(50);
            MouseClick();

            Thread.Sleep(100); // Wait for search bar to be ready

            // Set the perk name to the clipboard
            System.Windows.Forms.Clipboard.SetText(perkName);

            // Simulate paste action (Ctrl + V)
            SendKeys.SendWait("^{v}"); // Use SendWait to ensure it completes before proceeding

            Thread.Sleep(350); // Wait for pasting to complete

            // Move to the search button and click
            SetCursorPos(searchButt[0], searchButt[1]);

            Thread.Sleep(50);
            MouseClick();

            Thread.Sleep(350); // Wait for search to process

            // Move to the first perk and click
            SetCursorPos(firstPerk[0], firstPerk[1]);

            Thread.Sleep(50);
            MouseClick();

            Thread.Sleep(350); // Wait for perk selection
        }


        private static void MouseClick()
        {
            INPUT[] inputs = new INPUT[2];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].mkhi.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            inputs[1].type = INPUT_MOUSE;
            inputs[1].mkhi.mi.dwFlags = MOUSEEVENTF_LEFTUP;

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void KeyPress(char key)
        {
            ushort scanCode = (ushort)key;
            INPUT[] inputs = new INPUT[2];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].mkhi.ki.wScan = scanCode;
            inputs[0].mkhi.ki.dwFlags = KEYEVENTF_SCANCODE;
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].mkhi.ki.wScan = scanCode;
            inputs[1].mkhi.ki.dwFlags = KEYEVENTF_SCANCODE | 0x0002; // KEYEVENTF_KEYUP

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }



        private void SurvivorRandomButt_Click(object sender, RoutedEventArgs e)
        {
            RandomizePerks("Survivor");
        }

        private void KillerRandomButt_Click(object sender, RoutedEventArgs e)
        {
            RandomizePerks("Killer");
        }
    }
}
