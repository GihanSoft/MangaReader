using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
//using Gihan.Manga.Reader.Models;
using MangaReader;
using MangaReader.Models;

namespace Gihan.Manga.Reader.Controllers
{
    internal static class CoverMaker
    {
        private static string CoversPath { get; }
        public static string AbsoluteCoversPath { get; }
        static CoverMaker()
        {
            CoversPath = "covers\\";
            AbsoluteCoversPath = Path.Combine(Environment.CurrentDirectory, CoversPath);
            var directory = new DirectoryInfo(CoversPath);
            if (!directory.Exists)
            {
                directory.Create();
                directory.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            foreach (var item in directory.EnumerateFiles())
            {
                if (SettingApi.This.MangaList.All(m => m.CoverAddress != item.FullName))
                    item.Delete();
            }
        }

        public static string CoverPath(int mangaId)
        {
            return Path.Combine(AbsoluteCoversPath, SettingApi.This.MangaList[mangaId].Name + ".jpg");
        }

        private static Bitmap ResizeImage(Image image, int newHeight = 500)
        {
            var newWidth = (int)(image.Width * ((double)newHeight / image.Height));
            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
            }
            return newImage;
        }

        public static void CoverConvert(MangaInfo manga)
        {
            var preCover = Image.FromFile(manga.CoverAddress);
            var tumbCover = ResizeImage(preCover);
            tumbCover.Save(CoverPath(manga.Id), ImageFormat.Jpeg);
            SettingApi.This.MangaList[manga.Id].CoverAddress = CoverPath(manga.Id);
            preCover.Dispose();
            tumbCover.Dispose();
        }
    }
}
