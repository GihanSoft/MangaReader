using MahApps.Metro;
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
            
            if (e.Args.Length > 0)
            {
                bool trueType = true;
                foreach (var item in e.Args)
                {
                    bool exist = System.IO.File.Exists(item);
                    bool isCompressedFile = FileTypeList.CompressedType.Any(t => item.ToLower().EndsWith(t));
                    if (!exist || !isCompressedFile)
                        trueType = false;
                }
                if (!trueType)
                    Environment.Exit(0);

                StartupUri = new Uri("Views/WinMain.xaml", UriKind.Relative);
            }

            var theme = ThemeManager.AppThemes.ToArray()[SettingApi.This.ThemeBase];
            var accent = ThemeManager.Accents.ToArray()[SettingApi.This.Accent];
            ThemeManager.ChangeAppStyle(Current, accent, theme);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            SettingApi.This.Dispose();
            CompressApi.CleanExtractPath();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
