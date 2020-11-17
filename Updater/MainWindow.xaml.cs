using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Updater.Models;
using Updater.Models.Enums;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        Controllers.Updater Updater { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BtnOK.Visibility = Visibility.Collapsed;
            BtnStart.Visibility = Visibility.Visible;
            BtnRetry.Visibility = Visibility.Collapsed;
            
            Updater = new Controllers.Updater();
            bool isNeedUpdate = false;
            try
            {
                isNeedUpdate = Updater.IsNeedUpdate;
            }
            catch
            {
                MessageBox.Show("مشکل اتصال به اینترنت", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnRetry.Visibility = Visibility.Visible;
                BtnStart.Visibility = Visibility.Collapsed;
                return;
            }
            
            if (!isNeedUpdate)
            {
                MessageBox.Show("برنامه به روز است", "", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    Process.Start("MangaReader.exe");
                }
                catch { }
                Application.Current.Shutdown();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Updater.UpdateInfo.UpdateMode == UpdateMode.InnerUpdate)
                Process.Start("MangaReader.exe");
            Close();
        }

        private void BtnRetry_Click(object sender, RoutedEventArgs e)
        {
            MetroWindow_Loaded(this, new RoutedEventArgs());
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            BtnOK.Visibility = Visibility.Visible;
            BtnStart.Visibility = Visibility.Collapsed;

            Updater.Start();
        }
    }
}
