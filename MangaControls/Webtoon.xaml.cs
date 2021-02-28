using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gihan.Manga.Views.Custom
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
                foreach (var image in _images)
                {
                    image.Width *= dZoom;
                }
            }
        }
        public override double Offset
        {
            get => Sv.VerticalOffset - _images.Take(Page - 1).Sum(i => i.ActualHeight);
            set => Sv.ScrollToVerticalOffset(Sv.VerticalOffset + (value - Offset));
        }
        public override int Page
        {
            get
            {
                if (_images is null) return 0;
                double offset = 0;
                for (int i = 0; i < _images.Length; i++)
                {
                    offset += _images[i].ActualHeight;
                    if (Sv.VerticalOffset < offset)
                    {
                        return i + 1;
                    }
                }
                return _images.Length;
            }
            set
            {
                if (value == 1)
                {
                    LoadPage(0);
                    _ = Task.Run(() =>
                      {
                          Thread.Sleep(250);
                          if (_sourceStreams?.Length > 1)
                              LoadPage(1);
                          if (_sourceStreams?.Length > 2)
                              LoadPage(2);
                      });
                }
                Sv.ScrollToVerticalOffset(_images.Take(value - 1).Sum(i => i.ActualHeight));
            }
        }

        public Webtoon()
        {
            InitializeComponent();
            _zoom = 1;
        }

        protected override void SetSourceStreams(IEnumerable<Stream> streams)
        {
            base.SetSourceStreams(streams);
            for (int i = 0; i < _images.Length; i++)
            {
                _images[i] = new Image { Width = 700 * Zoom, Height = 1000 };
                ImagesRailSp.Children.Add(_images[i]);
            }
        }

        private void LoadPage(int page)
        {
            LoadPageStream(page);
            Dispatcher.Invoke(() =>
            {
                if (_images[page].Source is null)
                {
                    LoadBitmap(page);
                    _images[page].Source = _bitmaps[page];
                    _images[page].Height = double.NaN;
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
                for (double offset = 0; prePage < _images.Length; prePage++)
                {
                    offset += _images[prePage].ActualHeight;
                    if ((e.VerticalOffset - e.VerticalChange) < offset)
                        break;
                }
                var currentPage = 0;
                for (double offset = 0; currentPage < _images.Length - 1; currentPage++)
                {
                    offset += _images[currentPage].ActualHeight != 0 ?
                        _images[currentPage].ActualHeight : _images[currentPage].Height;
                    if (e.VerticalOffset < offset)
                        break;
                }

                if (prePage != currentPage)
                {
                    LoadPage(currentPage);
                    _ = Task.Run(() =>
                      {
                          Thread.Sleep(250);
                          if (currentPage + 2 < _images.Length)
                              LoadPage(currentPage + 2);
                          if (currentPage + 1 < _images.Length)
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
