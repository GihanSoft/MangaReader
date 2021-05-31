// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using MangaReader;
using MangaReader.Bootstrap;

using Microsoft.Extensions.DependencyInjection;

var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
Environment.SetEnvironmentVariable(Startup.ServiceConfigurer.CompanyNameKey, fileVersionInfo.CompanyName, EnvironmentVariableTarget.Process);
Environment.SetEnvironmentVariable(Startup.ServiceConfigurer.ProductNameKey, fileVersionInfo.ProductName, EnvironmentVariableTarget.Process);

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
