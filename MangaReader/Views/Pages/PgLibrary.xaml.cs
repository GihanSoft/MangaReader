using System.Windows;
using ControlzEx;

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
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Data;
using MahApps.Metro.Controls;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgLibrary.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class PgLibrary
    {
        /// <summary>Identifies the <see cref="IsCheckActive"/> dependency property.</summary>
        public static readonly DependencyProperty IsCheckActiveProperty = DependencyProperty.Register(
            nameof(IsCheckActive),
            typeof(bool),
            typeof(PgLibrary),
            new PropertyMetadata(default(bool)));

        private readonly DataDb dataDb;
        private readonly SettingsManager settingsManager;

        public PgLibrary(
            DataDb dataDb,
            SettingsManager settingsManager
            )
        {
            this.dataDb = dataDb;
            this.settingsManager = settingsManager;

            Refresh += RefreshMethod;

            InitializeComponent();

            SpDelete.SetBinding(VisibilityHelper.IsVisibleProperty, new Binding()
            {
                Path = new(nameof(IsCheckActive), null),
                Source = this
            });
        }

        public bool IsCheckActive
        {
            get => (bool)GetValue(IsCheckActiveProperty);
            set => SetValue(IsCheckActiveProperty, value);
        }

        public void RefreshMethod()
        {
            Task.Run(async () =>
            {
                Dispatcher.Invoke(() => this.SetCurrentValue(FocusableProperty, false));

                Manga[] mangas = dataDb.Mangas.FindAll().OrderBy(m => m.Name, NaturalStringComparer.Default).ToArray();
                for (int i = 0; i < mangas.Length; i++)
                {
                    Manga manga = mangas[i];
                    Dispatcher.Invoke(() =>
                    {
                        if (ListPanel.Children.Count >= i + 1)
                        {
                            if (((MangaItem)ListPanel.Children[i]).Manga.Id == manga.Id)
                            {
                                if (i is 0)
                                {
                                    MoveFocus(new(FocusNavigationDirection.Next));
                                }
                                return;
                            }
                            ListPanel.Children.RemoveAt(i);
                        }
                        MangaItem mangaItem = new(manga);
                        Binding binding = new()
                        {
                            Source = this,
                            Path = new PropertyPath(nameof(IsCheckActive), null),
                        };
                        mangaItem.SetBinding(MangaItem.IsCheckActiveProperty, binding);
                        mangaItem.Click += MangaItem_Click;
                        ListPanel.Children.Insert(i, mangaItem);
                        if (i is 0)
                        {
                            MoveFocus(new(FocusNavigationDirection.Next));
                        }
                    });
                    await Task.Delay(10).ConfigureAwait(false);
                }
                if (mangas.Length is 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.SetCurrentValue(FocusableProperty, true);
                        KeyboardNavigationEx.Focus(this);
                    });
                }
            });
        }

        private void MangaItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MangaItem mangaItem)
            {
                return;
            }

            PageNavigator!.GoTo<PgViewer>();
            ((PgViewer)PageNavigator.Current).View("manga://" + ((MangaItem)sender).Manga.Id);
            JumpList.AddToRecentCategory(new JumpTask()
            {
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName,
                Arguments = $"{mangaItem.Manga.Id}",
                Title = mangaItem.Manga.Name
            });
        }

        private async Task AddManga(string mangaPath)
        {
            DirectoryInfo directoryInfo = new(mangaPath);
            DirectoryInfo[] subDirectories = directoryInfo.GetDirectories();
            FileInfo[] topFiles = directoryInfo.GetFiles();

            if (directoryInfo.Parent is null)
            {
                MessageBox.Show(
                    "Don't use drive root as manga folder ://",
                    "!!!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            bool noCompressedFile = !topFiles.Any(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase));

            if (subDirectories.Length == 0 &&
                noCompressedFile &&
                directoryInfo.EnumerateFiles().Any(f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)))
            {
                await AddManga(directoryInfo.Parent.FullName).ConfigureAwait(false);
                return;
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

            if (dataDb.Mangas.FindOne(m => m.Path == mangaPath) is not null)
            {
                MessageBox.Show(
                    "This manga already exist",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            Array.Sort(topFiles, (x, y) => NaturalStringComparer.Default.Compare(x.Name, y.Name));

            string? coverUri = Array.Find(topFiles, f =>
                    {
                        string pureName = Path.GetFileNameWithoutExtension(f.Name);
                        return FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase) && pureName is "[cover]" or "cover";
                    })?.FullName
                    ?? Array.Find(topFiles, f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase))?.FullName;

            if (coverUri is null)
            {
                FileSystemInfo[] chapters = subDirectories.Cast<FileSystemInfo>()
                    .Concat(topFiles.Where(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)))
                    .OrderBy(f => f.Name, NaturalStringComparer.Default)
                    .ToArray();
                for (int i = 0; i < chapters.Length; i++)
                {
                    FileSystemInfo firstChapter = chapters[i];
                    if (firstChapter is DirectoryInfo dir)
                    {
                        FileInfo[] subFiles = dir.GetFiles("*", SearchOption.AllDirectories);
                        Array.Sort(subFiles, (x, y) => NaturalStringComparer.Default.Compare(x.Name, y.Name));

                        coverUri = Array.Find(subFiles, f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase))?.FullName;
                        if (coverUri is null)
                        {
                            try
                            {
                                firstChapter = subFiles.First(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase));
                            }
                            catch (InvalidOperationException)
                            {
                                continue;
                            }
                        }
                    }
                    if (firstChapter is FileInfo file)
                    {
                        using CompressedPageProvider compressedPageProvider = new(file.FullName);
                        await compressedPageProvider.LoadPageAsync(0).ConfigureAwait(false);
                        var page = Convert.ToBase64String(compressedPageProvider[0]!.ToArray());
                        coverUri = $"data:,{page}";
                    }
                    break;
                }
            }

            dataDb.Mangas.Insert(new Manga
            {
                Path = mangaPath,
                Name = directoryInfo.Name,
                Cover = coverUri,
            });

            Manga[] mangas = dataDb.Mangas.FindAll().OrderBy(m => m.Name, NaturalStringComparer.Default).ToArray();
            for (int i = 0; i < mangas.Length; i++)
            {
                Manga manga = mangas[i];
                if (manga.Path != mangaPath)
                {
                    continue;
                }
                Dispatcher.Invoke(() =>
                {
                    MangaItem mangaItem = new(manga);
                    Binding binding = new()
                    {
                        Source = this,
                        Path = new PropertyPath(nameof(IsCheckActive), null),
                    };
                    mangaItem.SetBinding(MangaItem.IsCheckActiveProperty, binding);
                    mangaItem.Click += MangaItem_Click;
                    ListPanel.Children.Insert(i, mangaItem);
                });
            }
        }

        private IEnumerable<string> ShowAddMangaDialog(bool batch)
        {
            MainOptions mainOptions = settingsManager.GetMainOptions();
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                Multiselect = false,
                InitialDirectory = mainOptions.MangaRootFolder,
                Title = $"Please Select {(batch ? "Root " : null)}Manga Folder",
            };
            CommonFileDialogResult dialogResult = dialog.ShowDialog();
            if (dialogResult != CommonFileDialogResult.Ok)
            {
                return Enumerable.Empty<string>();
            }
            mainOptions.MangaRootFolder = batch ? dialog.FileName : Path.GetDirectoryName(dialog.FileName);
            settingsManager.SaveMainOptions(mainOptions);
            return batch ? Directory.GetDirectories(dialog.FileName) : new[] { dialog.FileName };
        }

        private async void CmdAddManga_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            foreach (string mangaDirectory in ShowAddMangaDialog(false))
            {
                await AddManga(mangaDirectory).ConfigureAwait(false);
            }
            RefreshMethod();
        }

        private async void CmdAddMangaBatch_Executed(object? sender, ExecutedRoutedEventArgs? e)
        {
            foreach (string mangaDirectory in ShowAddMangaDialog(true))
            {
                await AddManga(mangaDirectory).ConfigureAwait(false);
                await Task.Delay(10).ConfigureAwait(false);
            }
            RefreshMethod();
        }

        private void CmdToggleDeleteState_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetCurrentValue(IsCheckActiveProperty, !IsCheckActive);
        }

        private void CmdSelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (MangaItem item in ListPanel.Children)
            {
                item.IsChecked = true;
            }
        }

        private void CmdDeselectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (MangaItem item in ListPanel.Children)
            {
                item.IsChecked = false;
            }
        }

        private void CmdDeleteForever_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MangaItem[] toDeleteMangaItems = ListPanel
                .Children
                .Cast<MangaItem>()
                .Where(m => m.IsChecked)
                .ToArray();
            MessageBoxResult dialogResult = MessageBox.Show(
                $"Are you sure you want to delete {toDeleteMangaItems.Length} manga?",
                "Delete Managa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (dialogResult != MessageBoxResult.Yes)
            {
                return;
            }
            IEnumerable<int> toDeleteMangaIds = toDeleteMangaItems
                .Select(m => m.Manga.Id);
            foreach (MangaItem mangaItem in toDeleteMangaItems)
            {
                ListPanel.Children.Remove(mangaItem);
            }
            dataDb.Mangas.DeleteMany(m => toDeleteMangaIds.Contains(m.Id));
            SetCurrentValue(IsCheckActiveProperty, false);
            RefreshMethod();
        }
    }
}
