using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using ControlzEx.Theming;

using GihanSoft.Navigation;

using MangaReader.Data;
using MangaReader.Exceptions;
using MangaReader.Options;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgSettings.xaml
    /// </summary>
    [SuppressMessage("Performance", "CA1812:...", Justification = "WPF object")]
    internal partial class PgSettings
    {
        private readonly Version version;
        private readonly SettingsManager settingsManager;
        private readonly PageNavigator pageNavigator;
        private MainOptions mainOptions;

        public PgSettings(
            Version version,
            SettingsManager settingsManager,
            PageNavigator pageNavigator
            )
        {
            InitializeComponent();

            this.version = version;
            this.settingsManager = settingsManager;
            this.pageNavigator = pageNavigator;
            mainOptions = settingsManager?.Get<MainOptions>(MainOptions.Key) ?? throw new NotInitializedException();
        }

        public override Task RefreshAsync()
        {
            mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key) ?? throw new NotInitializedException();

            TxtVersion.SetCurrentValue(System.Windows.Documents.Run.TextProperty, version.ToString());

            CboThemeBase.SetCurrentValue(ItemsControl.ItemsSourceProperty, ThemeManager.Current.BaseColors);
            CboThemeColor.SetCurrentValue(ItemsControl.ItemsSourceProperty, ThemeManager.Current.ColorSchemes);

            CboThemeBase.SetCurrentValue(Selector.SelectedItemProperty, ThemeManager.Current.DetectTheme()?.BaseColorScheme);
            CboThemeColor.SetCurrentValue(Selector.SelectedItemProperty, ThemeManager.Current.DetectTheme()?.ColorScheme);
            return base.RefreshAsync();
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
            pageNavigator.GoBackAsync();
        }

        private void Cancel()
        {
            mainOptions = settingsManager.Get<MainOptions>(MainOptions.Key) ?? throw new NotInitializedException();

            ThemeManager.Current.ChangeTheme(App.Current, mainOptions.Appearance.Theme ?? "Light.Blue");
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs? e)
        {
            Cancel();
            pageNavigator.GoBackAsync();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e) => settingsManager.Save(MainOptions.Key, mainOptions);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Cancel();
            }
        }
    }
}
