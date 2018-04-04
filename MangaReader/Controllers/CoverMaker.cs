using MangaReader.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader.Controllers
{
    static class CoverMaker
    {
        static string CoversPath { get; }
        public static string AbsoluteCoversPath { get; }
        static CoverMaker()
        {
            CoversPath = "covers\\";
            AbsoluteCoversPath = Path.Combine(Environment.CurrentDirectory, CoversPath);
            DirectoryInfo directory = new DirectoryInfo(CoversPath);
            if (!directory.Exists)
            {
                directory.Create();
                directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            foreach (var item in directory.EnumerateFiles())
            {
                if (!SettingApi.This.MangaList.Any(m => m.CoverAddress == item.FullName))
                    item.Delete();
            }
        }

        public static string CoverPath(int mangaID)
        {
            return Path.Combine(AbsoluteCoversPath, SettingApi.This.MangaList[mangaID].Name + ".jpg");
        }

        private static Bitmap ResizeImage(Image image, int NewHeight = 500)
        {
            var NewWidth = (int)(image.Width * ((double)NewHeight / image.Height));
            Bitmap NewImage = new Bitmap(NewWidth, NewHeight);
            using (Graphics graphics = Graphics.FromImage(NewImage))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, new Rectangle(0, 0, NewWidth, NewHeight));
            }
            return NewImage;
        }

        public static void CoverConvert(MangaInfo manga)
        {
            var preCover = Image.FromFile(manga.CoverAddress);
            var tumbCover = ResizeImage(preCover);
            tumbCover.Save(CoverPath(manga.ID), ImageFormat.Jpeg);
            SettingApi.This.MangaList[manga.ID].CoverAddress = CoverPath(manga.ID);
            preCover.Dispose();
            tumbCover.Dispose();
        }

        /*
        publuc static void AllCoverConvert()
        {
            for (int i = SettingApi.This.MangaList.Count - 1; i >= 0; i--)
            {
                var manga = SettingApi.This.MangaList[i];
                if (manga.CoverAddress.StartsWith(Environment.CurrentDirectory + '\\' + CoverPath)) continue;
                CoverConvert(manga);
            }
        }
        */
    }
}
