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
    public partial class App
    {
        public static new App Current => (App)Application.Current;

        public static void ShowError(Exception err)
        {
            MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private readonly IServiceProvider serviceProvider;
        private readonly IServiceScope serviceScope;

        public App()
        {
            InitializeComponent();

            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            serviceScope = services.BuildServiceProvider().CreateScope();
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
                Upgrade = true
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
            MainOptions mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
            if (mainOptions is null)
            {
                FirstRunBootstraper();
                mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
            }
            ThemeManager.Current.ChangeTheme(this, mainOptions.Appearance.Theme);

            if (e.Args.Length > 0)
            {
                throw new NotSupportedException();

                //var trueType = true;
                //foreach (var item in e.Args)
                //{
                //    var exist = System.IO.File.Exists(item);
                //    var isCompressedFile = FileTypeList.CompressedType.Any(t => item.ToLower().EndsWith(t));
                //    if (!exist || !isCompressedFile)
                //        trueType = false;
                //}
                //if (!trueType && !e.Args.Contains("-m"))
                //{
                //    Environment.Exit(0);
                //}
                //StartupUri = new Uri("Views/WinMain.xaml", UriKind.Relative);
            }
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
            serviceScope.Dispose();
            //SettingApi.This.Dispose();
            //CompressApi.CleanExtractPath();
        }
    }
}
