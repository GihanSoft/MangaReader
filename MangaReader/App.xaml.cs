using System;
using System.IO;
using System.Linq;
using System.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MangaReader
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IDisposable
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

        private readonly ServiceProvider mainServiceProvider;
        private readonly IServiceScope serviceScope;
        private bool disposedValue;

        public App()
        {
            InitializeComponent();

            this.DispatcherUnhandledException += (_, e) =>
            {
                ShowError(e.Exception);
                e.Handled = true;
            };

            var buildConfiguration = new Func<IServiceProvider?, IConfiguration>(
                _ => new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build());
            IServiceCollection services = new ServiceCollection();
            services.AddTransient(buildConfiguration);

            Startup.ServiceConfigurer serviceConfigurer = new(buildConfiguration(null));
            serviceConfigurer.ConfigureServices(services);

            mainServiceProvider = services.BuildServiceProvider();
            serviceScope = mainServiceProvider.CreateScope();
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

            var bootstrapper = ActivatorUtilities.CreateInstance<Startup.Bootstrapper>(serviceScope.ServiceProvider);
            bootstrapper.Bootstrap();

            this.MainWindow = (ActivatorUtilities.CreateInstance<MainWindow>(serviceScope.ServiceProvider));

            MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    serviceScope.Dispose();
                    mainServiceProvider.Dispose();
                    // TO DO: dispose managed state (managed objects)
                }

                // TO DO: free unmanaged resources (unmanaged objects) and override finalizer
                // TO DO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
