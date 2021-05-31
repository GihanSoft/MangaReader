// -----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    internal partial class App
    {
        /// <summary>
        /// Key to access args stored in <see cref="App.Properties"/>.
        /// </summary>
        public const string ArgumentsKey = "Arguments";

        /// <summary>
        /// Gets current instance of <see cref="App"/> class.
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary>
        /// Log errors is %appdata%/GihanSoft/MangaReader/log.txt file.
        /// </summary>
        /// <param name="err"><see cref="Exception"/> to be logged.</param>
        /// <param name="prefix">optional prefix for log.</param>
        public static void LogError(Exception err, string? prefix = null)
        {
            if (err is null)
            {
                throw new ArgumentNullException(nameof(err));
            }

            var logPath = Environment.ExpandEnvironmentVariables(@"%appdata%\GihanSoft\MangaReader\log.txt");
            using var writter = File.AppendText(logPath);

            var errType = err.GetType();
            writter.WriteLine(DateTime.Now);
            if (prefix is not null)
            {
                writter.WriteLine($"# {prefix} #");
            }

            writter.WriteLine(errType.Name + ' ' + new string(Enumerable.Repeat('-', 80 - errType.Name.Length).ToArray()));
            writter.WriteLine(err.Message);
            writter.WriteLine(err.ToString());
            writter.WriteLine(new string(Enumerable.Repeat('-', 80).ToArray()));
            writter.WriteLine();
        }

        /// <summary>
        /// Log and show error.
        /// </summary>
        /// <param name="err"><see cref="Exception"/> to be logged.</param>
        public static void ShowError(Exception err)
        {
            if (err is null)
            {
                throw new ArgumentNullException(nameof(err));
            }

            LogError(err);
            MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            DispatcherUnhandledException += (_, e) =>
            {
                ShowError(e.Exception);
                e.Handled = true;
            };
        }

        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e is null)
            {
                return;
            }

            if(e.Args is { Length: > 0 })
            {
                Properties[ArgumentsKey] = e.Args;
            }
        }
    }
}
