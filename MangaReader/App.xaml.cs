using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace MangaReader
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public const string ArgumentsKey = "Arguments";

        //static part ---------------------------------------------------

        public static new App Current => (App)Application.Current;

        public static void ShowError(Exception err)
        {
            if (err is null)
            {
                throw new ArgumentNullException(nameof(err));
            }
            var logPath = Environment.ExpandEnvironmentVariables(@"%appdata%\GihanSoft\MangaReader\log.txt");
            using var writter = File.AppendText(logPath);
            var errType = err.GetType();
            MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            writter.WriteLine(DateTime.Now);
            writter.WriteLine(errType.Name + ' ' + new string(Enumerable.Repeat('-', 80 - errType.Name.Length).ToArray()));
            writter.WriteLine(err.Message);
            writter.WriteLine(err.ToString());
            writter.WriteLine(new string(Enumerable.Repeat('-', 80).ToArray()));
            writter.WriteLine();
        }

        //instance part -------------------------------------------------

        public App()
        {
            DispatcherUnhandledException += (_, e) =>
            {
                ShowError(e.Exception);
                e.Handled = true;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e is null)
            {
                return;
            }
            if (e.Args.Length > 0)
            {
                Properties[ArgumentsKey] = e.Args;
            }
        }
    }
}
