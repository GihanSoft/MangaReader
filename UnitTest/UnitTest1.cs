using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Gihan.Manga;
using Gihan.Manga.Views.Custom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SetStreamTest()
        {
            //pv.ViewMode = ViewMode.RailSingle;
            var win = new Window();
            var app = new Application { MainWindow = win };
            app.Startup += App_Startup;
            app.Run();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            (sender as Application).MainWindow.Show();
            var imgs = Directory.GetFiles(@"D:\Entertainment\Manga\Black Cat\Volume1\Black Cat - Chapter 1\Black Cat - 1");
            var grid = new Grid();
            //var imgs = Directory.GetFiles(@"D:\Entertainment\Manga\Tower of Gods\1", "*.jpg");
            var cboMode = new ComboBox
            {
                ItemsSource = Enum.GetNames(typeof(ViewMode))
            };
            PagesViewer pv = null;
            cboMode.SelectionChanged += (s, ea) =>
            {
                if (pv != null) grid.Children.Remove(pv);
                var imageSources = pv?.SourceImageStreams ?? imgs.Select(i => new FileStream(i, FileMode.Open));
                pv = PagesViewerFactory.GetPagesViewer((ViewMode)Enum
                        .Parse(typeof(ViewMode), cboMode.SelectedItem as string));
                pv.BorderThickness = new Thickness(5);
                pv.BorderBrush = new SolidColorBrush(Colors.RoyalBlue);
                Grid.SetRow(pv, 1);
                pv.FlowDirection = FlowDirection.RightToLeft;
                pv.SourceImageStreams = imageSources;
                grid.Children.Add(pv);
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition());
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            var btnN = new Button { Content = "=>" };
            btnN.Click += (s, ea) => pv.Page++;
            var btnP = new Button { Content = "<=" };
            btnP.Click += (s, ea) => pv.Page--;
            var txt = new TextBox { Text = "100" };
            txt.LostFocus += (s, ea) =>
            {
                var su = double.TryParse(txt.Text, out var zoom);
                if (su) pv.Zoom = zoom / 100;
                else txt.Text = "" + pv.Zoom * 100;
            };
            sp.Children.Add(cboMode);
            sp.Children.Add(btnN);
            sp.Children.Add(btnP);
            sp.Children.Add(txt);

            grid.Children.Add(sp);
            (sender as Application).MainWindow.Content = grid;
        }
    }
}
