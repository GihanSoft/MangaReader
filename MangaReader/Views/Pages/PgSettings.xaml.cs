using ControlzEx.Theming;

using GihanSoft.Navigation;

using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

using MangaReader.Data;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgSettings.xaml
    /// </summary>
    public partial class PgSettings
    {
        private readonly Version version;
        private readonly SettingsManager settingsManager;
        private MainOptions mainOptions;

        public PgSettings(
            PageNavigator navigator,
            Version version,
            SettingsManager settingsManager)
            : base(navigator)
        {
            InitializeComponent();

            Navigator.Navigating += Navigator_Navigating;
            this.version = version;
            this.settingsManager = settingsManager;

            mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);
        }

        public override void Refresh()
        {
            mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);

            TxtVersion.Text = version.ToString();

            CboThemeBase.ItemsSource = ThemeManager.Current.BaseColors;
            CboThemeColor.ItemsSource = ThemeManager.Current.ColorSchemes;

            CboThemeBase.SelectedItem = ThemeManager.Current.DetectTheme()?.BaseColorScheme;
            CboThemeColor.SelectedItem = ThemeManager.Current.DetectTheme()?.ColorScheme;
        }

        private async void Navigator_Navigating(object sender, GihanSoft.Navigation.Events.NavigatingEventArgs e)
        {
            if (e.Current == this && false /* any thing changed flag */)
            {
                e.Cancel = true;

                var result = await ((MetroWindow)App.Current.MainWindow).ShowMessageAsync(
                    "Warning",
                    "Are sure to cancel all changes",
                    MessageDialogStyle.AffirmativeAndNegative);
                if (result == MessageDialogResult.Affirmative)
                {
                    BtnCancel_Click(null, null);
                }
            }
        }

        private void CboThemeBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboThemeBase.SelectedItem is not string newBaseTheme)
            {
                return;
            }
            ThemeManager.Current.ChangeThemeBaseColor(App.Current, newBaseTheme);
            mainOptions.Appearance.Theme = ThemeManager.Current.DetectTheme()?.Name ?? "Light.Blue";
        }

        private void CboTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboThemeColor.SelectedItem is not string newThemeColor)
            {
                return;
            }
            ThemeManager.Current.ChangeThemeColorScheme(App.Current, newThemeColor);
            mainOptions.Appearance.Theme = ThemeManager.Current.DetectTheme()?.Name ?? "Light.Blue";
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnApply_Click(sender, e);
            Navigator.GoBack();
        }

        private void Cancel() {
            mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key);

            ThemeManager.Current.ChangeTheme(App.Current, mainOptions.Appearance.Theme);
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs? e)
        {
            Cancel();
            Navigator.GoBack();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            settingsManager.Save(MainOptions.Key, mainOptions);
        }

        protected override void Dispose(bool disposing)
        {
            Cancel();
        }
    }
}
