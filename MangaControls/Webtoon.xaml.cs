using MangaReader.PagesViewer;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MangaReader.PagesViewer
{
    /// <summary>
    /// Interaction logic for Webtoon.xaml
    /// </summary>
    public partial class Webtoon : PagesViewer
    {
        protected double _zoom;

        public override double Zoom
        {
            get => _zoom;
            set
            {
                var dZoom = value / _zoom;
                foreach (var image in images)
                {
                    image.Width *= dZoom;
                }
            }
        }
        public override double Offset
        {
            get => Sv.VerticalOffset - images.Take(Page - 1).Sum(i => i.ActualHeight);
            set => Sv.ScrollToVerticalOffset(Sv.VerticalOffset + (value - Offset));
        }
        public override int Page
        {
            get
            {
                if (images is null) return 0;
                double offset = 0;
                for (int i = 0; i < images.Length; i++)
                {
                    offset += images[i].ActualHeight;
                    if (Sv.VerticalOffset < offset)
                    {
                        return i + 1;
                    }
                }
                return images.Length;
            }
            set
            {
                if (value == 1)
                {
                    LoadPage(0);
                    _ = Task.Run(() =>
                      {
                          Thread.Sleep(250);
                          if (bitmaps?.Length > 1)
                              LoadPage(1);
                          if (bitmaps?.Length > 2)
                              LoadPage(2);
                      });
                }
                Sv.ScrollToVerticalOffset(images.Take(value - 1).Sum(i => i.ActualHeight));
            }
        }

        public Webtoon()
        {
            InitializeComponent();
            _zoom = 1;
        }

        public override void View(PagesProvider pagesProvider, int page)
        {
            base.View(pagesProvider, page);
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = new Image { Width = 700 * Zoom, Height = 1000 };
                ImagesRailSp.Children.Add(images[i]);
            }
        }

        private void LoadPage(int page)
        {
            Dispatcher.Invoke(() =>
            {
                if (images[page].Source is null)
                {
                    LoadBitmap(page);
                    images[page].Source = bitmaps[page];
                    images[page].Height = double.NaN;
                }
            }, System.Windows.Threading.DispatcherPriority.Normal, default);
        }
        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ViewportHeightChange != 0 && e.ViewportHeight != e.ViewportHeightChange)
            {
                //Page = (int)Math.Round(e.VerticalOffset / (e.ViewportHeight - e.ViewportHeightChange)) + 1;
                //var preOffset = e.VerticalOffset - ((e.ViewportHeight - e.ViewportHeightChange) * (Page - 1));
                //Offset = preOffset * (e.ViewportHeight / (e.ViewportHeight - e.ViewportHeightChange));
            }
            if (e.VerticalChange != 0)
            {
                var prePage = 0;
                for (double offset = 0; prePage < images.Length; prePage++)
                {
                    offset += images[prePage].ActualHeight;
                    if ((e.VerticalOffset - e.VerticalChange) < offset)
                        break;
                }
                var currentPage = 0;
                for (double offset = 0; currentPage < images.Length - 1; currentPage++)
                {
                    offset += images[currentPage].ActualHeight != 0 ?
                        images[currentPage].ActualHeight : images[currentPage].Height;
                    if (e.VerticalOffset < offset)
                        break;
                }

                if (prePage != currentPage)
                {
                    LoadPage(currentPage);
                    _ = Task.Run(() =>
                      {
                          Thread.Sleep(250);
                          if (currentPage + 2 < images.Length)
                              LoadPage(currentPage + 2);
                          if (currentPage + 1 < images.Length)
                              LoadPage(currentPage + 1);
                          if (currentPage > 0)
                              LoadPage(currentPage - 1);
                          if (currentPage > 1)
                              LoadPage(currentPage - 2);
                      });
                }
            }
        }
    }
}
