using ControlzEx.Theming;

using LiteDB;

using MangaReader.Data;
using MangaReader.Data.Models;

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace MangaReader.Old
{
    internal static class OldUpdate
    {
        public static void UpdateSetting(FileInfo preSettingFileInfo)
        {
            if (preSettingFileInfo is null)
            {
                throw new ArgumentNullException(nameof(preSettingFileInfo));
            }
            BinaryFormatter binaryFormatter = new();

            using Stream settingsFileStream = preSettingFileInfo.OpenRead();

#pragma warning disable CA2300 // Do not use insecure deserializer BinaryFormatter
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            object? settingsObj = binaryFormatter.Deserialize(settingsFileStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
#pragma warning restore CA2300 // Do not use insecure deserializer BinaryFormatter

            SettingApi preSettings = (SettingApi)settingsObj;

            string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"GihanSoft\MangaReader",
                    "data.litedb");
            Directory.CreateDirectory(Path.GetDirectoryName(appDataPath)!);

            ConnectionString connectionString = new()
            {
                Filename = appDataPath,
                Upgrade = true,
                Connection = ConnectionType.Shared
            };
            using LiteDatabase database = new(connectionString);
            DataDb dataDb = new(database);
            SettingsManager settingsManager = new(dataDb);
            MainOptions mainOptions = settingsManager.GetMainOptions() ?? new MainOptions();
            mainOptions.Version = "3.0";
            mainOptions.MangaRootFolder = preSettings.MangaRoot;

            var baseT = ThemeManager.Current.BaseColors[preSettings.ThemeBase];
            var theme = ThemeManager.Current.Themes.FirstOrDefault(t =>
                t.BaseColorScheme == baseT && t.ColorScheme == "Blue");
            if (theme is not null)
            {
                mainOptions.Appearance.Theme = theme.Name;
            }
            mainOptions.Appearance.WindowPosition = new MainOptions.AppearanceClass.WindowPositionClass
            {
                Top = (SystemParameters.PrimaryScreenHeight - preSettings.WinChooserHeight) / 2,
                Left = (SystemParameters.PrimaryScreenWidth - preSettings.WinChooserWidth) / 2,
                Height = preSettings.WinChooserHeight,
                Width = preSettings.WinChooserWidth,
                WindowsState = (byte)preSettings.WinChooserState,
            };
            settingsManager.SaveMainOptions(mainOptions);

            dataDb.Mangas.Insert(preSettings.MangaList.Select(x =>
            {
                if (!double.TryParse(x.Zoom, NumberStyles.Any, CultureInfo.InvariantCulture, out double zoom))
                {
                    zoom = 100;
                }
                zoom /= 100;
                int page = (int)(x.CurrentPlace / zoom / SystemParameters.FullPrimaryScreenHeight);
                FileInfo? coverFile = x.CoverAddress is not null ? new(x.CoverAddress) : null;
                string? cover = null;
                if (coverFile?.Exists ?? false)
                {
                    cover = coverFile.FullName;
                    using var reader = coverFile.OpenRead();
                    using var memStream = new MemoryStream();
                    reader.CopyTo(memStream);
                    memStream.Position = 0;
                    cover = "data:," + Convert.ToBase64String(memStream.ToArray());
                }

                return new Manga
                {
                    Path = x.Address,
                    Name = x.Name,
                    Cover = cover,
                    CurrentChapter = x.CurrentChapter,
                    CurrentPage = page,
                    Zoom = zoom
                };
            }));
            dataDb.Database.Commit();

            DirectoryInfo? coverDir = preSettingFileInfo.Directory?.Parent?.EnumerateDirectories("covers").FirstOrDefault();
            if (coverDir is not null && coverDir.Exists)
            {
                coverDir.Delete(true);
            }
#if !DEBUG
            DirectoryInfo? refDir = preSettingFileInfo.Directory?.Parent?.EnumerateDirectories("ref").FirstOrDefault();
            if (refDir is not null && refDir.Exists)
            {
                refDir.Delete(true);
            }
#endif
            var preVersionFilePaths = new[]
            {
                "MangaReader.exe.config",
                "Newtonsoft.Json.dll",
                "Newtonsoft.Json.xml",
                "System.Net.Http.dll",
                "System.Windows.Interactivity.dll",
                "UnRAR.exe",
                "Updater.exe",
                "Updater.exe.config"
            };
            FileInfo[]? preVersionFiles = preSettingFileInfo.Directory?.Parent?.EnumerateFiles()?
                .Where(f => preVersionFilePaths.Contains(f.Name, StringComparer.OrdinalIgnoreCase))?.ToArray();
            if (preVersionFiles is not null)
            {
                foreach (var file in preVersionFiles)
                {
                    file.Delete();
                }
            }

            if (coverDir is not null && coverDir.Exists)
            {
                coverDir.Delete(true);
            }

            settingsFileStream.Close();
            preSettingFileInfo.Directory?.Delete(true);
        }

    }
}
