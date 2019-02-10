using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Gihan.Manga.Reader.Controllers;
using MahApps.Metro;
using MangaReader;

namespace Gihan.Manga.Reader
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var path = Environment.GetCommandLineArgs()[0];
            Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf('\\'));

            if (e.Args.Length > 0)
            {
                var trueType = true;
                foreach (var item in e.Args)
                {
                    var exist = System.IO.File.Exists(item);
                    var isCompressedFile = FileTypeList.CompressedType.Any(t => item.ToLower().EndsWith(t));
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

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError(e.Exception);
            e.Handled = true;
        }

        public static void ShowError(Exception err)
        {
            MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
