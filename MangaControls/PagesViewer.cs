using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MangaReader.PagesViewer
{
    public abstract class PagesViewer : UserControl
    {
        public static PagesViewer? GetPagesViewer(ViewMode viewMode)
        {
            return viewMode switch
            {
                ViewMode.PageSingle => new PageSingle(),
                ViewMode.PageDouble => new PageDouble(),
                ViewMode.RailSingle => new RailSingle(),
                ViewMode.RailDouble => new RailDouble(),
                ViewMode.Webtoon => new Webtoon(),
                _ => throw new NotSupportedException(),
            };
        }

        private PagesProvider? pagesProvider;

        protected BitmapImage?[]? bitmaps;
        protected Image?[]? images;

        public abstract double Zoom { get; set; }
        public abstract double Offset { get; set; }
        public abstract int Page { get; set; }

        protected async Task LoadBitmap(int page)
        {
            if (pagesProvider is null)
            {
                throw new PagesViewerException();
            }

            if (bitmaps![page] != null) return;
            bitmaps[page] = new();
            await pagesProvider.LoadPageAsync(page).ConfigureAwait(false);
            bitmaps[page].BeginInit();
            bitmaps[page].SetCurrentValue(BitmapImage.StreamSourceProperty, pagesProvider[page]);
            bitmaps[page].EndInit();
        }

        public virtual void View(PagesProvider pagesProvider, int page)
        {
            if (pagesProvider is null)
            {
                throw new ArgumentNullException(nameof(pagesProvider));
            }
            this.pagesProvider = pagesProvider;
            bitmaps = new BitmapImage[pagesProvider.Count];
            images = new Image[pagesProvider.Count];
            Page = page;
        }
    }
}