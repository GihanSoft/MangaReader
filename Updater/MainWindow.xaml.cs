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

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        byte currentFileNumber = 0;
        UpdateData updateData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WebClient_InnerDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var downloadedFile = updateData.SourceFiles[currentFileNumber].
                Substring(updateData.SourceFiles[currentFileNumber].LastIndexOf("/") + 1) + ".new";
            var existFile = downloadedFile.Substring(0, downloadedFile.Length - 4);
            if (File.Exists(existFile))
            {
                if (File.Exists(existFile + ".old"))
                    File.Delete(existFile + ".old");
                Directory.Move(existFile, existFile + ".old");
            }
            Directory.Move(downloadedFile, existFile);

            currentFileNumber++;
            if (currentFileNumber + 1 > updateData.SourceFiles.Count)
                return;

            WebClient webClient = sender as WebClient;
            var toDownload = updateData.SourceFiles[currentFileNumber];
            var fName = toDownload.Substring(toDownload.LastIndexOf("/") + 1) + ".new";
            try
            {
                webClient.DownloadFileAsync(new System.Uri(toDownload), fName);
            }
            catch
            {
                MessageBox.Show("مشکل اتصال به اینترنت", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnRetry.Visibility = Visibility.Visible;
                BtnOK.Visibility = Visibility.Visible;
            }
        }
        private void WebClient_FullDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            currentFileNumber++;
            if (currentFileNumber + 1 > updateData.SourceFiles.Count)
            {
                Process.Start(@"setup\setup.exe");
                return;
            }

            WebClient webClient = sender as WebClient;
            var toDownload = updateData.SourceFiles[currentFileNumber];
            var fName = @"setup\" + toDownload.Substring(toDownload.LastIndexOf("/") + 1);
            try
            {
                webClient.DownloadFileAsync(new System.Uri(toDownload), fName);
            }
            catch
            {
                MessageBox.Show("مشکل اتصال به اینترنت", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnRetry.Visibility = Visibility.Visible;
                BtnOK.Visibility = Visibility.Visible;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BtnOK.Visibility = Visibility.Collapsed;
            BtnStart.Visibility = Visibility.Visible;
            BtnRetry.Visibility = Visibility.Collapsed;

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage result;
            try
            {
                result = httpClient.GetAsync("http://kuroneko3.tk/kn/api/UpdateInfo/").Result;
            }
            catch
            {
                MessageBox.Show("مشکل اتصال به اینترنت", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnRetry.Visibility = Visibility.Visible;
                BtnStart.Visibility = Visibility.Collapsed;
                return;
            }
            if (!result.IsSuccessStatusCode)
                return;

            string jsonResult = result.Content.ReadAsStringAsync().Result;
            updateData = Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateData>(jsonResult);

            if (updateData.VersionCode <= UpdateData.ProgramVersionCode)
            {
                MessageBox.Show("برنامه به روز است", "", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    Process.Start("MangaReader.exe");
                }
                catch { }
                System.Environment.Exit(0);
            }
        }

        void StartDownlaodInner()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += WebClient_InnerDownloadFileCompleted;
            var toDownload = updateData.SourceFiles[0];
            var fName = toDownload.Substring(toDownload.LastIndexOf("/") + 1) + ".new";
            try
            {
                webClient.DownloadFileAsync(new System.Uri(toDownload), fName);
            }
            catch
            {
                MessageBox.Show("مشکل اتصال به اینترنت", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnOK.Visibility = Visibility.Visible;
                BtnStart.Visibility = Visibility.Collapsed;
            }
        }
        void StartDownloadFull()
        {
            if (!Directory.Exists(@"setup"))
                Directory.CreateDirectory(@"setup");
            if (File.Exists(@"setup\KN offline Manga Reader installer.msi"))
                File.Delete(@"setup\KN offline Manga Reader installer.msi");
            if (File.Exists(@"setup\setup.exe"))
                File.Delete(@"setup\setup.exe");

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += WebClient_FullDownloadFileCompleted;

            updateData.SourceFiles.Clear();
            updateData.SourceFiles.Add("http://kuroneko3.tk/kn/setup/KN%20offline%20Manga%20Reader%20installer.msi");
            updateData.SourceFiles.Add("http://kuroneko3.tk/kn/setup/setup.exe");

            var toDownload = updateData.SourceFiles[0];
            var fName = @"setup\" + toDownload.Substring(toDownload.LastIndexOf("/") + 1);

            try
            {
                webClient.DownloadFileAsync(new System.Uri(toDownload), fName);
            }
            catch
            {
                MessageBox.Show("مشکل اتصال به اینترنت", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnOK.Visibility = Visibility.Visible;
                BtnStart.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (updateData.UpdateMode == UpdateMode.InnerUpdate)
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

            if (updateData.UpdateMode == UpdateMode.InnerUpdate && updateData.LeastVersion > UpdateData.ProgramVersionCode)
                StartDownlaodInner();
            else
                StartDownloadFull();
        }
    }
}
