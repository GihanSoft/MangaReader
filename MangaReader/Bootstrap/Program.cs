using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using MangaReader;
using MangaReader.Bootstrap;

using Microsoft.Extensions.DependencyInjection;

var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
Environment.SetEnvironmentVariable(Startup.CompanyNameKey, fileVersionInfo.CompanyName, EnvironmentVariableTarget.Process);
Environment.SetEnvironmentVariable(Startup.ProductNameKey, fileVersionInfo.ProductName, EnvironmentVariableTarget.Process);

ServiceCollection services = new();
Startup.ServiceConfigurer serviceConfigurer = new();
serviceConfigurer.ConfigureServices(services);

var serviceProvider = services.BuildServiceProvider();

var initializer = ActivatorUtilities.GetServiceOrCreateInstance<Startup.Initializer>(serviceProvider);
initializer.Initialize();

Thread staThread = new(() =>
{
    App app = new();
    app.InitializeComponent();
    initializer.InitializeGUI();
    app.MainWindow = ActivatorUtilities.GetServiceOrCreateInstance<MainWindow>(serviceProvider);
    app.Run(app.MainWindow);

    serviceProvider.Dispose();
});
staThread.SetApartmentState(ApartmentState.STA);
staThread.Start();