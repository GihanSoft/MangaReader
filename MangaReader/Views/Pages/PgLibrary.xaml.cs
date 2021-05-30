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
using System.Windows.Controls;
using System.Collections.ObjectModel;

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
        private readonly PageNavigator pageNavigator;

        public PgLibrary(
            DataDb dataDb,
            SettingsManager settingsManager,
            PageNavigator pageNavigator
            )
        {
            this.dataDb = dataDb;
            this.settingsManager = settingsManager;
            this.pageNavigator = pageNavigator;
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

        public override async Task RefreshAsync()
        {
            Dispatcher.Invoke(() => SetCurrentValue(FocusableProperty, false));

            var resultMangas = dataDb.Mangas.FindAll()
                .Where(manga => (manga.Name?.Contains(TxtSearch.Text, StringComparison.OrdinalIgnoreCase)) ?? false)
                .OrderBy(m => m.Name, NaturalStringComparer.Default)
                .ToArray();

            var panelCount = Dispatcher.Invoke(() => ListPanel.Children.Count);
            for (int i = 0; i < Math.Max(resultMangas.Length, panelCount); i++)
            {
                var delay = Dispatcher.Invoke(() =>
                {
                    Manga? mangaI = i < resultMangas.Length ? resultMangas[i] : null;
                    MangaItem? mangaItemI = i < ListPanel.Children.Count ? ListPanel.Children[i] as MangaItem : null;

                    if (mangaI is null)
                    {
                        if (i < ListPanel.Children.Count)
                        {
                            ListPanel.Children.RemoveAt(i);
                            i--;
                        }
                        return false;
                    }
                    if (mangaItemI is null || mangaItemI.Manga!.Id != mangaI.Id)
                    {
                        if (mangaItemI is not null &&
                            NaturalStringComparer.Default.Compare(mangaI, mangaItemI.Manga) >= 0)
                        {
                            ListPanel.Children.Remove(mangaItemI);
                            i--;
                            return false;
                        }
                        mangaItemI = new();
                        mangaItemI.Manga = mangaI;
                        Binding binding = new()
                        {
                            Source = this,
                            Path = new PropertyPath(nameof(IsCheckActive), null),
                        };
                        mangaItemI.SetBinding(MangaItem.IsCheckActiveProperty, binding);
                        mangaItemI.Click += MangaItem_Click;
                        ListPanel.Children.Insert(i, mangaItemI);
                        if (i is 0)
                        {
                            mangaItemI.WorkingFocus();
                        }
                        return true;
                    }
                    if (i is 0)
                    {
                        mangaItemI.WorkingFocus();
                    }
                    return false;
                });
                if (delay)
                {
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
            if (resultMangas.Length is 0)
            {
                Dispatcher.Invoke(() =>
                {
                    //this.Focusable
                    SetCurrentValue(FocusableProperty, true);
                    KeyboardNavigationEx.Focus(this);
                });
            }
        }

        private void MangaItem_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MangaItem mangaItem)
            {
                return;
            }

            pageNavigator!.GoToAsync<PgViewer>();
            (pageNavigator.CurrentPage as PgViewer)?.View("manga://" + ((MangaItem)sender).Manga.Id);
            JumpList.AddToRecentCategory(new JumpTask()
            {
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName,
                Arguments = $"manga://{mangaItem.Manga.Id}",
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
                    MangaItem mangaItem = new();
                    mangaItem.Manga = manga;
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
            await RefreshAsync().ConfigureAwait(false);
        }

        private async void CmdAddMangaBatch_Executed(object? sender, ExecutedRoutedEventArgs? e)
        {
            foreach (string mangaDirectory in ShowAddMangaDialog(true))
            {
                await AddManga(mangaDirectory).ConfigureAwait(false);
                await Task.Delay(10).ConfigureAwait(false);
            }
            await RefreshAsync().ConfigureAwait(false);
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
            RefreshAsync().ConfigureAwait(false);
        }

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Back)
            {
                var txt = TxtSearch.Text.Length > 0 ? TxtSearch.Text[..^1] : null;
                TxtSearch.SetCurrentValue(TextBox.TextProperty, txt);
                RefreshAsync().ConfigureAwait(false);
                return;
            }

            var ch = KeyboardHelper.GetCharFromKey(e.Key);
            var x = ch > 0 && !char.IsControl(ch);
            if (x)
            {
                TxtSearch.Text += ch;
                RefreshAsync().ConfigureAwait(false);
                e.Handled = true;
            }
        }
    }
}
