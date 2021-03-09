using System;

namespace MangaReader.Data
{
    public class MainOptions
    {
        public const string Key = "C2620BE2-F092-4807-B867-3DD8426E9F45";

        public class AppearanceClass
        {
            public class WindowPositionClass
            {
                public double Top { get; set; }
                public double Left { get; set; }
                public double Height { get; set; }
                public double Width { get; set; }
                public byte WindowsState { get; set; }
            }

            public WindowPositionClass WindowPosition { get; set; } = new WindowPositionClass();

            public string? Theme { get; set; }
        }

        public AppearanceClass Appearance { get; set; } = new AppearanceClass();

        public string? MangaRootFolder { get; set; }
        public string? Version { get; set; }
    }

    public static class MainOptionsExtensions
    {
        public static MainOptions GetMainOptions(this SettingsManager src)
        {
            if (src is null)
            {
                throw new ArgumentNullException(nameof(src));
            }
            return src.Get<MainOptions>(MainOptions.Key)!;
        }

        public static void SaveMainOptions(this SettingsManager src, MainOptions mainOptions)
        {
            if (src is null)
            {
                throw new ArgumentNullException(nameof(src));
            }
            src.Save(MainOptions.Key, mainOptions);
        }
    }
}
