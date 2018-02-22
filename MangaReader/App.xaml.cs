using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var path = Environment.GetCommandLineArgs()[0];
            Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf('\\'));

            CoverMaker.AllCoverConvert();

            if (e.Args.Length > 0)
            {
                bool trueType = true;
                foreach (var item in e.Args)
                {
                    bool exist = System.IO.File.Exists(item);
                    bool isCompressedFile = item.ToLower().EndsWith(".zip") || item.ToLower().EndsWith(".rar");
                    if (!exist || !isCompressedFile)
                        trueType = false;
                }
                if (!trueType)
                    Environment.Exit(0);

                StartupUri = new Uri("Views/WinMain.xaml", UriKind.Relative);
            }

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            SettingApi.This.Dispose();
            CompressApi.CleanExtractPath();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
