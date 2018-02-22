using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace MangaReader.Views
{
    /// <summary>
    /// Interaction logic for WinSetting.xaml
    /// </summary>
    public partial class WinSetting : MetroWindow
    {
        public WinSetting()
        {
            InitializeComponent();
            var version = Updater.UpdateData.ProgramVersionCode;
            RtVersion.Text = "." + (version % 100).ToString();
            version /= 100;
            RtVersion.Text = "." + (version % 100).ToString() + RtVersion.Text;
            version /= 100;
            RtVersion.Text = version.ToString() + RtVersion.Text;

            CboThemeBase.ItemsSource = ThemeManager.AppThemes.Select(t => t.Name);
            CboTheme.ItemsSource = ThemeManager.Accents.Select(a => a.Name);
            
            CboThemeBase.SelectedIndex = SettingApi.This.ThemeBase;
            CboTheme.SelectedIndex = SettingApi.This.Accent;
        }

        private void CboThemeBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var theme = ThemeManager.AppThemes.ToArray()[CboThemeBase.SelectedIndex].Name;
            ThemeManager.ChangeAppTheme(Application.Current, theme);
        }

        private void CboTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var accent = ThemeManager.Accents.ToArray()[CboTheme.SelectedIndex];
            var baseTheme = ThemeManager.AppThemes.ToArray()[CboThemeBase.SelectedIndex];
            ThemeManager.ChangeAppStyle(Application.Current, accent, baseTheme);
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var theme = ThemeManager.AppThemes.ToArray()[SettingApi.This.ThemeBase];
            var accent = ThemeManager.Accents.ToArray()[SettingApi.This.Accent];
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            SettingApi.This.ThemeBase = CboThemeBase.SelectedIndex;
            SettingApi.This.Accent = CboTheme.SelectedIndex;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            BtnApply_Click(null, null);
            Close();
        }
    }
}
