// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Windows;

using ControlzEx.Theming;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using MangaReader.Data;

namespace MangaReader.Bootstrap.Startup
{
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
            if(mainOptions is null)
            {
                FirstRunBootstraper();
                mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
            }
            var previusVersion = Version.Parse(mainOptions?.Version ?? "0.0");
            if(previusVersion != currentVersion)
            {
                OnVersionChange(previusVersion, currentVersion);
            }
        }

        public void InitializeGUI()
        {
            var mainOptions = settingsManager.GetMainOptions();
            if(mainOptions.Appearance.Theme is null)
            {
                ThemeManager.Current.SyncTheme(ThemeSyncMode.SyncAll);
            }
            else
            {
                ThemeManager.Current.ChangeTheme(App.Current, mainOptions.Appearance.Theme);
            }
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

        private static void OnVersionChange(Version previousVersion, Version currentVersion)
        {
            if(App.Current?.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.ShowMessageAsync(
                    "new version",
                    $"Application updated!\n{previousVersion} to {currentVersion}");
            }
        }
    }
}
