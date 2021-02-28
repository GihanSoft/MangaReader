using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using ControlzEx.Theming;

using GihanSoft.Navigation;

using LiteDB;

using MangaReader.Data;

using Microsoft.Extensions.DependencyInjection;

namespace MangaReader
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : IDisposable
    {
        public static new App Current => (App)Application.Current;

        public static void ShowError(Exception err)
        {
            if (err is null)
            {
                throw new ArgumentNullException(nameof(err));
            }
            MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private readonly ServiceProvider mainServiceProvider;
        private readonly IServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;
        private bool disposedValue;

        public App()
        {
            InitializeComponent();

            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            mainServiceProvider = services.BuildServiceProvider();
            serviceScope = mainServiceProvider.CreateScope();
            serviceProvider = serviceScope.ServiceProvider;
            services.AddSingleton(ServiceProvider);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new Version(3, 0));

            services.AddScoped<PageNavigator>();
            string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"GihanSoft\MangaReader",
                    "data.litedb");
            Directory.CreateDirectory(Path.GetDirectoryName(appDataPath));
            services.AddSingleton(new ConnectionString
            {
                Filename = appDataPath,
                Upgrade = true,
                Connection = ConnectionType.Shared
            });
            services.AddScoped<DataDb>();
            services.AddScoped<SettingsManager>();
        }

        public IServiceProvider ServiceProvider => serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SettingsManager settingsManager
                = ActivatorUtilities.GetServiceOrCreateInstance<SettingsManager>(ServiceProvider);
            MainOptions? mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
            if (mainOptions is null)
            {
                FirstRunBootstraper();
                mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
            }
            ThemeManager.Current.ChangeTheme(this, mainOptions!.Appearance.Theme!);
        }

        private void FirstRunBootstraper()
        {
            SettingsManager settingsManager
                = ActivatorUtilities.GetServiceOrCreateInstance<SettingsManager>(ServiceProvider);
            settingsManager.Save(MainOptions.Key, new MainOptions
            {
                Appearance = new MainOptions.AppearanceClass
                {
                    Theme = "Light.Blue",
                    WindowPosition = new MainOptions.AppearanceClass.WindowPositionClass
                    {
                        Top = (SystemParameters.PrimaryScreenHeight - 450) / 2,
                        Left = (SystemParameters.PrimaryScreenWidth - 800) / 2,
                        Height = 450,
                        Width = 800,
                        WindowsState = (byte)WindowState.Maximized
                    }
                },
            });
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError(e.Exception);
            e.Handled = true;
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
