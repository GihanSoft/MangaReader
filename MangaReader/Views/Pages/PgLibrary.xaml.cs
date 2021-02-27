using Gihan.Manga.Reader.Controllers;
using Gihan.Manga.Reader.Views;

using GihanSoft.Navigation;

using MangaReader.Controllers;
using MangaReader.Data;
using MangaReader.Data.Models;
using MangaReader.Views.Components;

using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgLibrary.xaml
    /// </summary>
    public partial class PgLibrary
    {
        private readonly DataDb dataDb;
        private readonly SettingsManager settingsManager;

        public PgLibrary(
            PageNavigator navigator,
            DataDb dataDb,
            SettingsManager settingsManager
            ) : base(navigator)
        {
            InitializeComponent();
            this.dataDb = dataDb;
            this.settingsManager = settingsManager;
        }

        public override StackPanel? LeftToolBar => (StackPanel)Resources["LeftToolBar"];

        public override Task Refresh()
        {
            Manga[] mangas = dataDb.Mangas.FindAll().OrderBy(m => m.Name, NaturalStringComparer.Default).ToArray();
            for (int i = 0; i < mangas.Length; i++)
            {
                Manga manga = mangas[i];
                if (ListPanel.Children.Count >= i + 1)
                {

                    if (((MangaItem)ListPanel.Children[i]).Manga.Id == manga.Id)
                    {
                        continue;
                    }
                    ListPanel.Children.RemoveAt(i);
                }
                MangaItem mangaItem = new(manga);
                mangaItem.Click += MangaItem_Click;
                ListPanel.Children.Insert(i, mangaItem);
            }
            return Task.CompletedTask;
        }

        private void MangaItem_Click(object sender, RoutedEventArgs e)
        {
            Navigator.GoTo<PgViewer>();
            ((PgViewer)Navigator.Current).View(((MangaItem)sender).Manga.Id.ToString());
        }

        protected override void Dispose(bool disposing)
        {
        }

        private void AddManga(string mangaPath)
        {
            DirectoryInfo directoryInfo = new(mangaPath);
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            FileInfo[] topFiles = directoryInfo.GetFiles();

            //FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

            bool noCompressedFile = !topFiles.Any(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase));

            if (subDirectories.Length == 0 &&
                noCompressedFile &&
                directoryInfo.EnumerateFiles().Any(f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)))
            {
                AddManga(directoryInfo.Parent.FullName);
            }

            if (subDirectories.Length == 0 && noCompressedFile)
            {
                MessageBox.Show(
                    "This folder is not a manga folder",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (dataDb.Mangas.FindOne(m => m.path == mangaPath) is not null)
            {
                MessageBox.Show(
                    "This manga already exist",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Array.Sort(topFiles, (x, y) => NaturalStringComparer.Default.Compare(x.Name, y.Name));

            string? coverUri = topFiles
                    .FirstOrDefault(f =>
                    {
                        string pureName = Path.GetFileNameWithoutExtension(f.Name);
                        return FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase) && pureName is "[cover]" or "cover";
                    })?.FullName
                    ?? topFiles.FirstOrDefault(f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase))?.FullName;

            if (coverUri is null)
            {
                FileSystemInfo firstChapter =
                    subDirectories.Concat(
                        topFiles.Where(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)) as IEnumerable<FileSystemInfo>)
                    .OrderBy(f => f.Name, NaturalStringComparer.Default)
                    .First();
                if (firstChapter is DirectoryInfo dir)
                {
                    var subFiles = dir.GetFiles("*", SearchOption.AllDirectories);
                    Array.Sort(subFiles, (x, y) => NaturalStringComparer.Default.Compare(x.Name, y.Name));

                    coverUri = subFiles.FirstOrDefault(f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase))?.FullName;
                    if (coverUri is null)
                    {
                        firstChapter = subFiles.FirstOrDefault(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase));
                    }
                }
                if (firstChapter is FileInfo file)
                {
                    CompressedPageProvider compressedPageProvider = new(file.FullName);
                    compressedPageProvider.LoadPageAsync(0).GetAwaiter().GetResult();
                    var page = Convert.ToBase64String(compressedPageProvider[0]?.ToArray());
                    coverUri = $"data:,{page}";
                }
            }

            dataDb.Mangas.Insert(new Manga
            {
                path = mangaPath,
                Name = directoryInfo.Name,
                CoverUri = coverUri,
            });

            Manga[] mangas = dataDb.Mangas.FindAll().OrderBy(m => m.Name, NaturalStringComparer.Default).ToArray();
            for (int i = 0; i < mangas.Length; i++)
            {
                Manga manga = mangas[i];
                if (manga.path != mangaPath)
                {
                    continue;
                }
                MangaItem mangaItem = new(manga);
                mangaItem.Click += MangaItem_Click;
                ListPanel.Children.Insert(i, mangaItem);
            }
        }

        private void BtnAddMangaRoot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnDeleteManga_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnEditManga_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CmdAddManga_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            MainOptions mainOptions = settingsManager.GetMainOptions();
            CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = mainOptions.MangaRootFolder,
                Multiselect = false
            };
            CommonFileDialogResult dialogResult = dialog.ShowDialog();
            if (dialogResult != CommonFileDialogResult.Ok)
            {
                return;
            }
            mainOptions.MangaRootFolder = Path.GetDirectoryName(dialog.FileName);
            settingsManager.SaveMainOptions(mainOptions);
            AddManga(dialog.FileName);
        }

        private void BtnAddManga_Click(object sender, RoutedEventArgs e)
        {
            CmdAddManga_Executed(this, null);
        }
    }
}
