using ControlzEx.Theming;

using MangaReader.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MangaReader.Startup
{
    internal class Bootstrapper
    {
        private readonly SettingsManager settingsManager;
        private readonly Version currentVersion;

        public Bootstrapper(
            SettingsManager settingsManager,
            Version currentVersion)
        {
            this.settingsManager = settingsManager;
            this.currentVersion = currentVersion;
        }

        public void Bootstrap()
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
            ThemeManager.Current.ChangeTheme(App.Current, mainOptions?.Appearance?.Theme!);
        }

        private void FirstRunBootstraper()
        {
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

        private void OnVersionChange(Version previousVersion, Version currentVersion)
        {
            if (previousVersion.Major is 0 && currentVersion!.Major == 3)
            {
                FileInfo previousSettingsFileInfo = new(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Data",
                    "setting.ini"));
                if (previousSettingsFileInfo.Exists)
                {
                    Old.OldUpdate.UpdateSetting(previousSettingsFileInfo);
                }
                MainOptions? mainOptions = settingsManager.GetMainOptions();
                mainOptions.Version = currentVersion.ToString();
                settingsManager.SaveMainOptions(mainOptions);
            }
        }
    }
}
