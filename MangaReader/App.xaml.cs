using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MangaReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                bool trueType = true;
                foreach (var item in e.Args)
                {
                    bool exist = System.IO.File.Exists(item);
                    bool isCompressedFile = item.ToLower().EndsWith(".zip") || item.ToLower().EndsWith(".rar");
                    if (!exist || !isCompressedFile)
                        trueType = false;
                }
                if (!trueType)
                    Environment.Exit(0);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            SettingApi.This.Dispose();
        }
    }
}
