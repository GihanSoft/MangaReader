// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using System.Windows;

using ControlzEx.Theming;

using GihanSoft.Navigation;

using LiteDB;

using MangaReader.Data;

using Microsoft.Extensions.DependencyInjection;

namespace MangaReader.Bootstrap
{
    public static class Startup
    {
        public const string CompanyNameKey = "CompanyName";
        public const string ProductNameKey = "ProductName";

        public class Initializer
        {
            private readonly SettingsManager settingsManager;
            private readonly Version currentVersion;

            public Initializer(
                SettingsManager settingsManager,
                Version currentVersion)
            {
                this.settingsManager = settingsManager;
                this.currentVersion = currentVersion;
            }

            public void Initialize()
            {
                var mainOptions = settingsManager.GetMainOptions();
                if (mainOptions is null)
                {
                    FirstRunBootstraper();
                    mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
                }
                var previusVersion = Version.Parse(mainOptions?.Version ?? "0.0");
                if (previusVersion != currentVersion)
                {
                    OnVersionChange(previusVersion, currentVersion);
                }
            }

            public void InitializeGUI()
            {
                var mainOptions = settingsManager.GetMainOptions();
                ThemeManager.Current.ChangeTheme(App.Current, mainOptions?.Appearance?.Theme);
            }

            private void FirstRunBootstraper()
            {
                settingsManager.Save(MainOptions.Key, new MainOptions
                {
                    Appearance = new MainOptions.AppearanceClass
                    {
                        Theme = "Light.Blue",
                        WindowPosition = new()
                        {
                            Top = (SystemParameters.PrimaryScreenHeight - 450) / 2,
                            Left = (SystemParameters.PrimaryScreenWidth - 800) / 2,
                            Height = 450,
                            Width = 800,
                            WindowsState = (byte)WindowState.Maximized,
                        },
                    },
                });
            }

            private void OnVersionChange(Version previousVersion, Version currentVersion)
            {
                //
            }
        }

        public class ServiceConfigurer
        {
            public void ConfigureServices(IServiceCollection services)
            {
                AddVersion(services);
                services.AddScoped<PageNavigator>();
                AddConnectionString(services);
                services.AddSingleton<LiteDatabase>();
                services.AddSingleton<DataDb>();
                services.AddScoped<SettingsManager>();
            }

            private void AddConnectionString(IServiceCollection services)
            {
                ConnectionString connectionString = new()
                {
                    Upgrade = true,
                    Connection = ConnectionType.Shared,
                    Filename = Path.Combine(
                        @$"%appdata%\%{CompanyNameKey}%\%{ProductNameKey}%",
                        "data.litedb"),
                };
                connectionString.Filename = Environment.ExpandEnvironmentVariables(connectionString.Filename);
                var dirPath = Path.GetDirectoryName(connectionString.Filename);
                if (dirPath is not null)
                {
                    Directory.CreateDirectory(dirPath);
                }
                services.AddSingleton(connectionString);
            }

            private static void AddVersion(IServiceCollection services)
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version is not null)
                {
                    services.AddSingleton(version);
                }
            }
        }
    }
}
