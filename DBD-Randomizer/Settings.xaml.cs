using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DBD_Randomizer
{
    public partial class Settings : Page
    {
        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string folderPath;
        private string filePath;
        private bool isSelectingHotkey = false;
        public ConvertKeyToVirtual ConvertKeyToVirtual { get; set; }

        public Settings()
        {
            folderPath = System.IO.Path.Combine(appDataPath, "DBD Randomizer");
            filePath = System.IO.Path.Combine(folderPath, "settings.json");
            InitializeComponent();
            LoadSettings();
        }




        private void LoadSettings()
        {
            if (File.Exists(filePath))
            {
                var settingsData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(filePath));
                if (settingsData.ContainsKey("Hotkeys"))
                {
                    var hotkeys = settingsData["Hotkeys"];
                    if (hotkeys.ContainsKey("KillerRandomizer"))
                    {
                        killerHotkeyText.Text = ConvertKeyToVirtual.ConvertVirtualCodeToKey(int.Parse(hotkeys["KillerRandomizer"]));
                    }
                    if (hotkeys.ContainsKey("SurvivorRandomizer"))
                    {
                        survivorHotkeyText.Text = ConvertKeyToVirtual.ConvertVirtualCodeToKey(int.Parse(hotkeys["SurvivorRandomizer"]));
                    }
                }
            }
        }

        private void SaveHotkey(string keyName, string keyValue)
        {
            Dictionary<string, Dictionary<string, string>> settingsData;
            if (File.Exists(filePath))
            {
                settingsData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(filePath));
            }
            else
            {
                settingsData = new Dictionary<string, Dictionary<string, string>>();
            }

            if (!settingsData.ContainsKey("Hotkeys"))
            {
                settingsData["Hotkeys"] = new Dictionary<string, string>();
            }

            settingsData["Hotkeys"][keyName] = keyValue;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(settingsData, Formatting.Indented));
        }

        private void SelectHotkey(Button button, TextBox textBox, string keyName)
        {
            if (isSelectingHotkey) return;

            isSelectingHotkey = true;
            button.Content = "Press a key...";
            DisableAllButtons();

            // Set focus to the main page (or container) so it can receive key events
            this.Focus();

            this.KeyDown += OnKeyDown;

            void OnKeyDown(object sender, KeyEventArgs e)
            {
                string keyPressed = e.Key.ToString();
                textBox.Text = keyPressed;
                var virtualkey = ConvertKeyToVirtual.ConvertKeyToVirtualCode(keyPressed);
                SaveHotkey(keyName, virtualkey.ToString());

                // Cleanup
                this.KeyDown -= OnKeyDown;
                button.Content = "Select Hotkey";
                isSelectingHotkey = false;
                EnableAllButtons();
            }
        }


        private void DisableAllButtons()
        {
            killerHotkeyButt.IsEnabled = false;
            survivorHotkeyButt.IsEnabled = false;
        }

        private void EnableAllButtons()
        {
            killerHotkeyButt.IsEnabled = true;
            survivorHotkeyButt.IsEnabled = true;
        }

        private void killerHotkeyButt_Click(object sender, RoutedEventArgs e)
        {
            SelectHotkey(killerHotkeyButt, killerHotkeyText, "KillerRandomizer");
        }

        private void survivorHotkeyButt_Click(object sender, RoutedEventArgs e)
        {
            SelectHotkey(survivorHotkeyButt, survivorHotkeyText, "SurvivorRandomizer");
        }


    }
}