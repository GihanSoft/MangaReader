using System;
using System.Diagnostics;
using System.IO;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace Gihan.Manga.Reader.Controllers
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
            string tarPath;
            tarPath = ExtractPath + archivePath.Substring(archivePath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            tarPath = tarPath.Substring(0, tarPath.LastIndexOf(".", StringComparison.Ordinal));
            try
            {
                using (Stream source = File.OpenRead(archivePath))
                {
                    var reader = ReaderFactory.Open(source);

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
                var process = new Process();
                process.StartInfo.FileName = "unrar";
                process.StartInfo.Arguments = $"x -ad \"{archivePath}\" \"{Path.Combine(Environment.CurrentDirectory, ExtractPath)}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                //var stdOut = process.StandardOutput.ReadToEnd();
                var stdErrMsg = process.StandardError.ReadToEnd();

                if (stdErrMsg.Length > 0)
                {
                    System.Windows.MessageBox.Show(e.ToString(), "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    throw;
                }
                else
                {
                    return tarPath;
                }
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
