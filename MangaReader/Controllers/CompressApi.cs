using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Readers;
using System.IO;

namespace MangaReader.Controllers
{
    static class CompressApi
    {
        static string ExtractPath { get; }

        static CompressApi()
        {
            ExtractPath = "extractpath\\";
            CleanExtractPath();
            try
            {
                if (!Directory.Exists(ExtractPath))
                {
                    Directory.CreateDirectory(ExtractPath).Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static string OpenArchive(string archivePath)
        {
            try
            {
                string tarPath;
                using (Stream source = File.OpenRead(archivePath))
                {
                    var reader = ReaderFactory.Open(source);

                    tarPath = ExtractPath + archivePath.Substring(archivePath.LastIndexOf("\\") + 1);
                    tarPath = tarPath.Substring(0, tarPath.LastIndexOf("."));
                    DirectoryInfo tarDirectory = new DirectoryInfo(tarPath);

                    if (tarDirectory.Exists)
                        tarDirectory.Delete(true);
                    tarDirectory.Create();
                    reader.WriteAllToDirectory(tarPath,
                        new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                }
                return tarPath;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString(), "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw;
            }
        }

        public static void CleanExtractPath()
        {
            try
            {
                if (Directory.Exists(ExtractPath))
                    Directory.Delete(ExtractPath, true);
            }
            catch
            {
                try
                {
                    if (Directory.Exists(ExtractPath))
                        Directory.Delete(ExtractPath, true);
                }
                catch (Exception err)
                {
                    System.Windows.MessageBox.Show(err.ToString(), "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}
