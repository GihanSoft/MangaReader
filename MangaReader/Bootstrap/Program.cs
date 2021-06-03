// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using MangaReader;
using MangaReader.Bootstrap.Startup;

using Microsoft.Extensions.DependencyInjection;

var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
Environment.SetEnvironmentVariable(ServiceConfigurer.CompanyNameKey, fileVersionInfo.CompanyName, EnvironmentVariableTarget.Process);
Environment.SetEnvironmentVariable(ServiceConfigurer.ProductNameKey, fileVersionInfo.ProductName, EnvironmentVariableTarget.Process);

ServiceCollection services = new();
ServiceConfigurer serviceConfigurer = new();
serviceConfigurer.ConfigureServices(services);

var serviceProvider = services.BuildServiceProvider();

var initializer = ActivatorUtilities.GetServiceOrCreateInstance<Initializer>(serviceProvider);
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
