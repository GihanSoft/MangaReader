using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Gihan.Manga.Reader.Models;

namespace Gihan.Manga.Reader.Controllers
{
    [Serializable]
    public class SettingApi : IDisposable
    {
        [NonSerialized]
        static readonly string SaveFileDir;
        [NonSerialized]
        static readonly string SaveFolderDir;
        [NonSerialized]
        static BinaryFormatter binaryFormatter;

        /// <summary>
        /// Singleton pattern field
        /// </summary>
        [NonSerialized]
        static SettingApi _This;
        /// <summary>
        /// Singleton Pattern Property
        /// </summary>
        public static SettingApi This
        {
            get
            {
                if (_This == null)
                    _This = new SettingApi();
                return _This;
            }
        }

        public List<MangaInfo> MangaList { get; set; } = null;
        public string MangaRoot { get; set; } = null;
        public byte BackgroundColor { get; set; } = 0;
        public bool ShowLastManga { get; set; } = true;
        public int LastManga { get; set; } = 0;

        public int ThemeBase { get; set; } = 0;
        public int Accent { get; set; } = 2;
        public WindowState WinChooserState { get; set; } = WindowState.Normal;
        public double WinChooserHeight { get; set; } = 400;
        public double WinChooserWidth { get; set; } = 600;

        static SettingApi()
        {
            SaveFolderDir = @"Data";
            SaveFileDir = SaveFolderDir + @"\setting.ini";
            binaryFormatter = new BinaryFormatter();
        }

        SettingApi() => ReLoad();

        // باید این سیستم مسخره آی دی رو درست کنم و مثل بچه آدم کار کنه

        public void SortMangaList()
        {
            MangaList.Sort(NaturalStringComparer.Default.Compare);
            for (int i = 0; i < MangaList.Count; i++)
            {
                if (MangaList[i].Id != i)
                {
                    MangaList[i].Id = i;
                }
            }
        }

        internal void ReLoad()
        {
            if (!Directory.Exists(SaveFolderDir))
            {
                Directory.CreateDirectory(SaveFolderDir);
            }

            if (File.Exists(SaveFileDir))
            {
                var file = File.OpenRead(SaveFileDir);
                var obj = binaryFormatter.Deserialize(file);
                var setting = (SettingApi) obj;
                file.Close();
                this.MangaList = setting.MangaList;
                this.MangaRoot = setting.MangaRoot;
                this.BackgroundColor = setting.BackgroundColor;
                this.ShowLastManga = setting.ShowLastManga;
                this.LastManga = setting.LastManga;
                this.ThemeBase = setting.ThemeBase;
                this.Accent = setting.Accent;
                this.WinChooserState = setting.WinChooserState;
                this.WinChooserHeight = setting.WinChooserHeight;
                this.WinChooserWidth = setting.WinChooserWidth;
            }
            else MangaList = null;
            if (MangaList == null)
                MangaList = new List<MangaInfo>();
        }

        public void Save()
        {
            var file = File.OpenWrite(SaveFileDir);
            binaryFormatter.Serialize(file, this);
            file.Close();
        }

        public void Dispose() => Save();
    }
}
