using MangaReader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader
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
        public bool ShowLastManga { get; set; } = false;
        public int LastManga { get; set; } = -1;

        public int ThemeBase { get; set; } = 0;
        public int Accent { get; set; } = 2;

        static SettingApi()
        {
            SaveFolderDir = @"Data";
            SaveFileDir = SaveFolderDir + @"\setting.ini";
            binaryFormatter = new BinaryFormatter();
        }

        SettingApi()
        {
            if (!Directory.Exists(SaveFolderDir))
            {
                Directory.CreateDirectory(SaveFolderDir);
            }

            if (File.Exists(SaveFileDir))
            {
                var file = File.OpenRead(SaveFileDir);
                var setting = (SettingApi)binaryFormatter.Deserialize(file);
                file.Close();
                this.MangaList = setting.MangaList;
                this.MangaRoot = setting.MangaRoot;
                this.BackgroundColor = setting.BackgroundColor;
                this.ShowLastManga = setting.ShowLastManga;
                this.LastManga = setting.LastManga;
            }
            if (MangaList == null)
                MangaList = new List<MangaInfo>();
        }

        public async void Save()
        {
            var file = File.OpenWrite(SaveFileDir);
            await Task.Run(() => binaryFormatter.Serialize(file, this));
            file.Close();
        }

        public void Dispose() => Save();

        public void SortMangaList()
        {
            MangaList.Sort(NaturalStringComparer.Default.Compare);
            for (int i = 0; i < MangaList.Count; i++)
            {
                MangaList[i].ID = i;
            }
        }
    }
}
