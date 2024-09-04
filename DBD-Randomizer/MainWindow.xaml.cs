using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DBD_Randomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        Randomizer randomizerpage = new Randomizer();
        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Content = randomizerpage;
        }

        

        private void RandomizerButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = randomizerpage;
            randomizerpage.LoadHotkeyFromSettings();
            randomizerpage.LoadCalibration();
        }

        private void CalibrateButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = new Calibrate();
            randomizerpage.disableHotkey();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Content = new Settings();
            randomizerpage.disableHotkey();
        }

        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/hSGpY3qV6F",
                UseShellExecute = true
            });
        }


        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://nightlight.gg/perks/list?_data=routes%2Fperks.list._index";
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderPath = System.IO.Path.Combine(appDataPath, "DBD Randomizer");
            string filePath = System.IO.Path.Combine(folderPath, "perkList.json");

            // Ensure the folder exists
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Download and save the file with a custom User-Agent
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                    string json = await client.GetStringAsync(url);
                    File.WriteAllText(filePath, json);
                    MessageBox.Show("Perk List has been updated successfully from NightLight", "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    MessageBox.Show("Error: Access forbidden (403).", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating: {ex.Message}", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}