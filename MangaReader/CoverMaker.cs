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

namespace MangaReader
{
    static class CoverMaker
    {
        static string CoverPath { get; }
        static CoverMaker()
        {
            CoverPath = "covers\\";
            DirectoryInfo directory = new DirectoryInfo(CoverPath);
            if (!directory.Exists)
            {
                directory.Create();
                directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
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
            tumbCover.Save(CoverPath + manga.ID + ".jpg", ImageFormat.Jpeg);
            SettingApi.This.MangaList[manga.ID].CoverAddress = Environment.CurrentDirectory + '\\' + CoverPath + manga.ID + ".jpg";
        }

        public static void AllCoverConvert()
        {
            foreach (var manga in SettingApi.This.MangaList)
            {
                if (manga.CoverAddress.StartsWith(Environment.CurrentDirectory + '\\' + CoverPath)) continue;
                CoverConvert(manga);
            }
        }
    }
}
