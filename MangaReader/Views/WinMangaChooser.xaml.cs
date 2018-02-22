﻿using MahApps.Metro.Controls;
using MangaReader.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MangaReader.Views
{
    /// <summary>
    /// Interaction logic for MangaChooser.xaml
    /// </summary>
    public partial class WinMangaChooser : MetroWindow
    {
        public static RoutedCommand newMangaCmd = new RoutedCommand();
        public static RoutedCommand selectMangaRootCmd = new RoutedCommand();
        public static RoutedCommand deleteMangaCmd = new RoutedCommand();
        public static RoutedCommand editMangaCmd = new RoutedCommand();
        public static RoutedCommand no_tlbCmd = new RoutedCommand();

        public WinMangaChooser()
        {
            newMangaCmd.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            selectMangaRootCmd.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
            deleteMangaCmd.InputGestures.Add(new KeyGesture(Key.Delete));
            editMangaCmd.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            no_tlbCmd.InputGestures.Add(new KeyGesture(Key.Escape));

            InitializeComponent();

            Task.Run(() => FirstAddMangaItems());

            BtnAddManga.ToolTip = "افزودن مانگا\nhotkey: Ctrl + N";
            BtnAddMangaRoot.ToolTip = "افزودن پوشه ریشه مانگا\nhotkey: Ctrl + M";
            BtnRemoveManga.ToolTip = "حذف مانگا از لیست\nhotkey: Delete";
            BtnEditManga.ToolTip = "ویرایش مانگا موجود\nhotkey: Ctrl + E";
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                try
                {
                    if (await Updater.UpdateChecker.Check())
                    {
                        BtnUpdate.Background = (Brush)FindResource("HighlightBrush");
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// add all MangaItem.
        /// call it in new Thread
        /// </summary>
        void FirstAddMangaItems()
        {
            SettingApi.This.SortMangaList();
            Dispatcher.Invoke(() => MangaPanel.Children.Clear());
            foreach (var item in SettingApi.This.MangaList)
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            var mngItem = new MangaItem(item);
                            mngItem.Click += MangaItem_Click;
                            MangaPanel.Children.Add(mngItem);
                        }
                        catch (Exception err)
                        {
                            var result = MessageBox.Show(err.ToString() + "\n\n" + $"Remove {item.Name}?",
                                "Error", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);
                            if (result == MessageBoxResult.Yes)
                                SettingApi.This.MangaList.Remove(item);
                        }
                    });
                }
                catch (Exception err)
                {
                    var result = MessageBox.Show(err.ToString() + "\n\n" + $"Remove {item.Name}?",
                                "Error", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No);
                    if (result == MessageBoxResult.Yes)
                        SettingApi.This.MangaList.Remove(item);
                    continue;
                }
                Thread.Sleep(1);
            }
            Dispatcher.Invoke(() =>
            {
                if (SettingApi.This.ShowLastManga)
                    (MangaPanel.Children[SettingApi.This.LastManga] as MangaItem).MangaButton.Focus();
                else if (MangaPanel.Children.Count > 0)
                    (MangaPanel.Children[0] as MangaItem).MangaButton.Focus();
            });
        }

        private void BtnAddManga_Click(object sender, RoutedEventArgs e)
        {
            var folderSelector = new System.Windows.Forms.FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = "پوشه مانگا یا چپتر را انتخاب کنید"
            };
            if (!string.IsNullOrEmpty(SettingApi.This.MangaRoot))
                folderSelector.SelectedPath = SettingApi.This.MangaRoot;
            var result = folderSelector.ShowDialog();
            if (!string.IsNullOrEmpty(folderSelector.SelectedPath) && result == System.Windows.Forms.DialogResult.OK)
            {
                SettingApi.This.MangaRoot = folderSelector.SelectedPath;
                AddManga(folderSelector.SelectedPath);
                Task.Run(() => FirstAddMangaItems());
            }
        }
        private void BtnAddMangaRoot_Click(object sender, RoutedEventArgs e)
        {
            var folderSelector = new System.Windows.Forms.FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = "پوشه ریشه مانگاها را انتخاب کنید"
            };
            if (!string.IsNullOrEmpty(SettingApi.This.MangaRoot))
                folderSelector.SelectedPath = SettingApi.This.MangaRoot;
            var result = folderSelector.ShowDialog();

            if (!string.IsNullOrEmpty(folderSelector.SelectedPath) && result == System.Windows.Forms.DialogResult.OK)
            {
                SettingApi.This.MangaRoot = folderSelector.SelectedPath;
                List<string> mangaFolders;
                try
                {
                    mangaFolders = Directory.EnumerateDirectories(folderSelector.SelectedPath).ToList();
                }
                catch
                {
                    MessageBox.Show("مشکلی در افزودن مانگاها به وجود آمد.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.RightAlign);
                    return;
                }
                foreach (var item in mangaFolders)
                {
                    AddManga(item, false);
                }
                Task.Run(() => FirstAddMangaItems());
            }
        }

        private void MangaItem_Click(object sender, RoutedEventArgs e)
        {
            var clickedManga = (sender as MangaItem).Manga;
            new WinMain(clickedManga).Show();
            Close();
        }

        private void BtnRemoveManga_Click(object sender, RoutedEventArgs e)
        {
            foreach (MangaItem manga in MangaPanel.Children)
            {
                manga.IsCheckActive = true;
            }
            YN_tlb.Visibility = Visibility.Visible;
        }
        private void No_tlb_Click(object sender, RoutedEventArgs e)
        {
            foreach (MangaItem item in MangaPanel.Children)
            {
                item.IsCheckActive = false;
                item.IsChecked = false;
            }
            YN_tlb.Visibility = Visibility.Collapsed;
        }
        private void Yes_tlb_Click(object sender, RoutedEventArgs e)
        {
            for (int j = MangaPanel.Children.Count - 1; j >= 0; j--)
            {
                var item = MangaPanel.Children[j] as MangaItem;
                if (item.IsChecked == true)
                    SettingApi.This.MangaList.Remove(item.Manga);
            }
            SettingApi.This.SortMangaList();
            Task.Run(() => FirstAddMangaItems());

            YN_tlb.Visibility = Visibility.Collapsed;
        }
        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (MangaItem item in MangaPanel.Children)
            {
                item.IsChecked = true;
            }
        }

        private void BtnEditManga_Click(object sender, RoutedEventArgs e)
        {
            foreach (MangaItem item in MangaPanel.Children)
            {
                item.IsEditActive = !item.IsEditActive;
            }
        }

        private void AddManga(String mangaPath, bool showDuplicateErroe = true)
        {
            if (SettingApi.This.MangaList.Any(manga => manga.Address == mangaPath))
            {
                if (showDuplicateErroe)
                    MessageBox.Show("مانگا جدید انتخاب کنید", "خطا مانگا تکراری",
                        MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                return;
            }

            var isManga = IsManga(mangaPath);
            switch (isManga)
            {
                case MangaFolderStastus.not:
                    MessageBox.Show("فولدر انتخاب شده، فولدر مانگا نیست.", "خطا فولدر نامعتبر",
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.RtlReading);
                    return;
                case MangaFolderStastus.manga:
                    SettingApi.This.MangaList.Add(new MangaInfo()
                    {
                        ID = SettingApi.This.MangaList.Count,
                        Name = mangaPath.Substring(mangaPath.LastIndexOf('\\') + 1),
                        Address = mangaPath,
                        CurrentChapter = 0,
                        CurrentPlace = 0,
                        CoverAddress = null
                    });
                    break;
                case MangaFolderStastus.chapter:
                    var rootFolder = mangaPath.Substring(0, mangaPath.LastIndexOf('\\') + 1);
                    var subFolders = Directory.EnumerateDirectories(rootFolder).ToList();
                    subFolders.Sort(NaturalStringComparer.Default.Compare);
                    var currentCh = subFolders.FindIndex(m => m == mangaPath);
                    SettingApi.This.MangaList.Add(new MangaInfo()
                    {
                        ID = SettingApi.This.MangaList.Count,
                        Name = rootFolder.Substring(rootFolder.LastIndexOf('\\') + 1),
                        Address = rootFolder,
                        CurrentChapter = currentCh,
                        CurrentPlace = 0,
                        CoverAddress = null
                    });
                    break;
            }
        }

        private MangaFolderStastus IsManga(string mangaPath)
        {
            if (!Directory.Exists(mangaPath)) return MangaFolderStastus.not;
            List<string> subDir;
            try
            {
                subDir = Directory.EnumerateDirectories(mangaPath).ToList();
            }
            catch { return MangaFolderStastus.not; }

            List<string> list;

            if (subDir.Count > 0)
            {
                try
                {
                    list = Directory.EnumerateFiles(subDir[0], "*.*", SearchOption.AllDirectories).Where(file =>
                        FileTypeList.ImageTypes.Any(t => file.ToLower().EndsWith(t))).ToList();
                }
                catch { return MangaFolderStastus.not; }
                if (list.Count > 0)
                    return MangaFolderStastus.manga;
            }

            try
            {
                list = Directory.EnumerateFiles(mangaPath, "*.*", SearchOption.AllDirectories).Where(file =>
                    FileTypeList.ImageTypes.Any(t => file.ToLower().EndsWith(t))).ToList();
            }
            catch { return MangaFolderStastus.not; }
            if (subDir.Count == 0 && list.Count > 0)
                return MangaFolderStastus.chapter;

            return MangaFolderStastus.not;
        }

        private void NewMangaCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BtnAddManga_Click(this, new RoutedEventArgs());
        }
        private void SelectMangaRootCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BtnAddMangaRoot_Click(this, new RoutedEventArgs());
        }
        private void DeleteMangaCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BtnRemoveManga_Click(this, new RoutedEventArgs());
        }
        private void EditMangaCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BtnEditManga_Click(this, new RoutedEventArgs());
        }
        private void No_tlbCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (YN_tlb.Visibility == Visibility.Visible)
                No_tlb_Click(this, new RoutedEventArgs());
        }

        private void MetroWindow_DragEnter(object sender, DragEventArgs e)
        {
            var DraggedItems = e.Data.GetData("FileName") as string[];
            bool allow = true;
            foreach (var item in DraggedItems)
            {
                if (!Directory.Exists(item))
                    allow = false;
            }
            if (!allow)
                AllowDrop = false;
        }
        private void MetroWindow_DragLeave(object sender, DragEventArgs e)
        {
            AllowDrop = true;
        }
        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            var DroppedItems = e.Data.GetData("FileName") as string[];
            foreach (var item in DroppedItems)
            {
                if (IsManga(item) == MangaFolderStastus.manga)
                {
                    AddManga(item, false);
                    Task.Run(() => FirstAddMangaItems());
                }
            }
        }

        private void MangaPanel_KeyUp(object sender, KeyEventArgs e)
        {
            if ((MangaPanel.Children[0] as MangaItem).IsEditActive) return;
            if (e.KeyboardDevice.Modifiers == ModifierKeys.None && e.Key >= Key.A && e.Key <= Key.Z)
            {
                foreach (MangaItem item in MangaPanel.Children)
                {
                    if (item.MangaTitle.ToLower().StartsWith(e.Key.ToString().ToLower()))
                    {
                        var ps = (((Keyboard.FocusedElement as Button).Parent as Grid).Parent as Border).Parent as MangaItem;
                        if (ps.Manga.ID + 1 == SettingApi.This.MangaList.Count)
                            continue;
                        if ((char.ToLower(ps.MangaTitle[0]) == char.ToLower(item.MangaTitle[0])) &&
                            item.Manga.ID <= ps.Manga.ID &&
                            SettingApi.This.MangaList[ps.Manga.ID + 1].Name.ToLower()[0] == ps.MangaTitle.ToLower()[0])
                            continue;
                        Keyboard.Focus(item.SelectButton);
                        item.Focus();
                        break;
                    }
                }
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var winAbout = new WinAbout();
            winAbout.ShowDialog();
        }
        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var winSetting = new WinSetting();
            winSetting.ShowDialog();
        }
        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("Updater.exe");
            Close();
        }

        private void BtnGoToSite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://kuroneko.in");
        }
    }

    enum MangaFolderStastus : byte
    {
        not, manga, chapter
    }
}