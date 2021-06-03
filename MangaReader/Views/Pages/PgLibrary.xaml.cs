// -----------------------------------------------------------------------
// <copyright file="PgLibrary.xaml.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

using ControlzEx;

using GihanSoft;
using GihanSoft.Navigation;
using GihanSoft.String;

using MahApps.Metro.Controls;

using MangaReader.Controllers;
using MangaReader.Data;
using MangaReader.Data.Models;
using MangaReader.Exceptions;
using MangaReader.Options;
using MangaReader.Views.Components;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace MangaReader.Views.Pages
{
    /// <summary>
    /// Interaction logic for PgLibrary.xaml
    /// </summary>
    [SuppressMessage("Performance", "CA1812:BoolToIconConverter is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it static (Shared in Visual Basic.", Justification = "WPF object")]
    internal partial class PgLibrary
    {
        /// <summary>Identifies the <see cref="IsCheckActive"/> dependency property.</summary>
        public readonly static DependencyProperty IsCheckActiveProperty = DependencyProperty.Register(
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
            PageNavigator pageNavigator)
        {
            this.dataDb = dataDb;
            this.settingsManager = settingsManager;
            this.pageNavigator = pageNavigator;
            InitializeComponent();

            SpDelete.SetBinding(VisibilityHelper.IsVisibleProperty, new Binding()
            {
                Path = new(nameof(IsCheckActive), null),
                Source = this,
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
                .OrderBy(m => m.Name, NaturalComparer.InvariantCultureIgnoreCase)
                .ToArray();

            var panelCount = Dispatcher.Invoke(() => ListPanel.Children.Count);
            for (var i = 0; i < Math.Max(resultMangas.Length, panelCount); i++)
            {
                var delay = Dispatcher.Invoke(() =>
                {
                    var mangaI = i < resultMangas.Length ? resultMangas[i] : null;
                    var mangaItemI = i < ListPanel.Children.Count ? ListPanel.Children[i] as MangaItem : null;

                    if (mangaI is null)
                    {
                        if (i < ListPanel.Children.Count)
                        {
                            ListPanel.Children.RemoveAt(i);
                            i--;
                        }
                        return false;
                    }
                    if (mangaItemI is null || (mangaItemI.Manga?.Id ?? 0) != mangaI.Id)
                    {
                        if (mangaItemI is not null &&
                            NaturalComparer.Compare(mangaI.Name, mangaItemI.Manga?.Name, StringComparison.InvariantCultureIgnoreCase) >= 0)
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
                        i = Math.Min(Math.Max(i, 0), ListPanel.Children.Count);
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
            if (resultMangas is { Length: 0 })
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
            (pageNavigator.CurrentPage as PgViewer)?.View("manga://" + (sender.As<MangaItem>().Manga?.Id ?? 0).ToString(CultureInfo.InvariantCulture));
            JumpList.AddToRecentCategory(new JumpTask()
            {
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName,
                Arguments = $"manga://{(mangaItem.Manga?.Id ?? 0).ToString(CultureInfo.InvariantCulture)}",
                Title = mangaItem.Manga?.Name ?? string.Empty,
            });
        }

        private async Task AddMangaAsync(string mangaPath)
        {
            DirectoryInfo directoryInfo = new(mangaPath);
            var subDirectories = directoryInfo.GetDirectories();
            var topFiles = directoryInfo.GetFiles();

            if (directoryInfo is { Parent: null })
            {
                MessageBox.Show(
                    "Don't use drive root as manga folder ://",
                    "!!!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            var noCompressedFile = !topFiles.Any(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase));

            if (subDirectories.Length == 0 &&
                noCompressedFile &&
                directoryInfo.EnumerateFiles().Any(f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)) &&
                directoryInfo.Parent is not null)
            {
                await AddMangaAsync(directoryInfo.Parent.FullName).ConfigureAwait(false);
                return;
            }

            if (subDirectories is { Length: 0 } && noCompressedFile)
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
                throw new DuplicateMangaException("This manga already exist");
            }

            Array.Sort(topFiles, (x, y) => NaturalComparer.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

            var coverUri = Array.Find(topFiles, f =>
                    {
                        var pureName = Path.GetFileNameWithoutExtension(f.Name);
                        return FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase) && pureName is "[cover]" or "cover";
                    })?.FullName
                    ?? Array.Find(topFiles, f => FileTypeList.ImageTypes.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase))?.FullName;

            if (coverUri is null)
            {
                var chapters = subDirectories.Cast<FileSystemInfo>()
                    .Concat(topFiles.Where(f => FileTypeList.CompressedType.Contains(f.Extension, StringComparer.InvariantCultureIgnoreCase)))
                    .OrderBy(f => f.Name, NaturalComparer.InvariantCultureIgnoreCase)
                    .ToArray();
                for (var i = 0; i < chapters.Length; i++)
                {
                    var firstChapter = chapters[i];
                    if (firstChapter is DirectoryInfo dir)
                    {
                        var subFiles = dir.GetFiles("*", SearchOption.AllDirectories);
                        Array.Sort(subFiles, (x, y) => NaturalComparer.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase));

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

            var mangas = dataDb.Mangas.FindAll().OrderBy(m => m.Name, NaturalComparer.InvariantCultureIgnoreCase).ToArray();
            for (var i = 0; i < mangas.Length; i++)
            {
                var manga = mangas[i];
                if (!string.Equals(manga.Path, mangaPath, StringComparison.OrdinalIgnoreCase))
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
            var mainOptions = settingsManager.GetMainOptions();
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                Multiselect = false,
                InitialDirectory = mainOptions.MangaRootFolder,
                Title = $"Please Select {(batch ? "Root " : null)}Manga Folder",
            };
            var dialogResult = dialog.ShowDialog();
            if (dialogResult is not CommonFileDialogResult.Ok)
            {
                return Enumerable.Empty<string>();
            }
            mainOptions.MangaRootFolder = batch ? dialog.FileName : Path.GetDirectoryName(dialog.FileName);
            settingsManager.SaveMainOptions(mainOptions);
            return batch ? Directory.GetDirectories(dialog.FileName) : new[] { dialog.FileName };
        }

        private void CmdAddManga_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            foreach (var mangaDirectory in ShowAddMangaDialog(false))
            {
                AddMangaAsync(mangaDirectory).ConfigureAwait(false);
            }

            RefreshAsync().ConfigureAwait(false);
        }

        private void CmdAddMangaBatch_Executed(object? sender, ExecutedRoutedEventArgs? e)
        {
            foreach (var mangaDirectory in ShowAddMangaDialog(true))
            {
                try
                {
                    AddMangaAsync(mangaDirectory).ConfigureAwait(false);
                }
                catch (DuplicateMangaException err)
                {
                    App.LogError(err);
                }
                Task.Delay(10).ConfigureAwait(false);
            }

            RefreshAsync().ConfigureAwait(false);
        }

        private void CmdToggleDeleteState_Executed(object sender, ExecutedRoutedEventArgs e) => SetCurrentValue(IsCheckActiveProperty, !IsCheckActive);

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
            var toDeleteMangaItems = ListPanel
                .Children
                .Cast<MangaItem>()
                .Where(m => m.IsChecked)
                .ToArray();
            var dialogResult = MessageBox.Show(
                $"Are you sure you want to delete {toDeleteMangaItems.Length} manga?",
                "Delete Managa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (dialogResult is not MessageBoxResult.Yes)
            {
                return;
            }

            var toDeleteMangaIds = toDeleteMangaItems
                .Select(mangaItem => mangaItem.Manga?.Id ?? 0);
            foreach (var mangaItem in toDeleteMangaItems)
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
