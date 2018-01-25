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


namespace MangaReader
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
        }
    }
}
