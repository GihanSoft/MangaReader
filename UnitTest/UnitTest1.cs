using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Gihan.Manga.Models.Enums;
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
            var imgs = new[]
            {
                @"E:\Entertainment\Manga\Adonis Next Door\01\Adonis Next Door 001\1.jpg",
                @"E:\Entertainment\Manga\Adonis Next Door\01\Adonis Next Door 001\2.jpg"
            };
            var win = new Window();
            var pv = new PageViewer()
            {
                //Width = 100,
                //Height = 100,
                BorderThickness = new Thickness(5),
                BorderBrush = new SolidColorBrush(Colors.Black)
            };
            win.Content = pv;
            pv.MouseEnter += (sender, args) => (sender as PageViewer).NightMode = true;
            pv.MouseLeave += (sender, args) => (sender as PageViewer).NightMode = false;
            pv.ImagesStream = imgs.Select(i => new FileStream(i, FileMode.Open)).ToArray();
            pv.ViewMode = ViewMode.RailSingle;
            var app = new Application { MainWindow = win };
            app.Startup += App_Startup;
            app.Run();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            (sender as Application).MainWindow.Show();
        }
    }
}
