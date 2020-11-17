using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

using static System.Windows.Data.BindingOperations;

namespace Gihan.Manga.Views.Custom
{
    /// <summary>
    /// Interaction logic for RailSingle.xaml
    /// </summary>
    public partial class RailSingle : PagesViewer
    {
        public override double Zoom
        {
            get => (_images.First()?.GetBindingExpression(MaxHeightProperty)?
                        .ParentBinding?.Converter as ZaribConverter)?.Zarib ?? 1;
            set
            {
                var page = Page;
                var offset = Offset;
                double? dZoom = null;
                foreach (var image in _images)
                {
                    if (image is null) continue;
                    if (dZoom is null)
                        dZoom = value / (GetBinding(image, MaxHeightProperty).Converter as ZaribConverter).Zarib;
                    (GetBinding(image, MaxHeightProperty)
                        .Converter as ZaribConverter).Zarib = value;
                    (GetBinding(image, HeightProperty)
                        .Converter as ZaribConverter).Zarib = value;
                    image.SetBinding(MaxHeightProperty, GetBinding(image, MaxHeightProperty));
                    image.SetBinding(HeightProperty, GetBinding(image, HeightProperty));
                }
                Page = page;
                Offset = offset * (dZoom ?? 1);
            }
        }
        public override double Offset
        {
            get => Sv.VerticalOffset - (_images.First()?.Height ?? Sv.ViewportHeight) * (Page - 1);
            set
            {
                var re = (_images.First()?.Height ?? Sv.ViewportHeight) * (Page - 1) + value;
                Sv.ScrollToVerticalOffset(re);
            }
        }
        public override int Page
        {
            get
            {
                return (int)Math.Round(Sv.VerticalOffset / (_images?.First()?.Height ?? 1)) + 1;
            }
            set
            {
                if (value == 1)
                    LoadPage(0);
                Sv.ScrollToVerticalOffset(_images.First().Height * (value - 1));
            }
        }

        public RailSingle()
        {
            InitializeComponent();
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
                }
            }, System.Windows.Threading.DispatcherPriority.Normal, default);
        }
        protected override void SetSourceStreams(IEnumerable<Stream> streams)
        {
            base.SetSourceStreams(streams);
            for (int i = 0; i < _images.Length; i++)
            {
                var heightBinding = new Binding(nameof(Sv.ViewportHeight))
                {
                    Source = Sv,
                    Converter = new ZaribConverter { Zarib = Zoom },
                };
                _images[i] = new Image();
                _images[i].SetBinding(MaxHeightProperty, heightBinding);
                _images[i].SetBinding(HeightProperty, heightBinding);
                ImagesRailSp.Children.Add(_images[i]);
            }
        }

        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ViewportHeightChange != 0 && e.ViewportHeight != e.ViewportHeightChange)
            {
                Page = (int)Math.Round(e.VerticalOffset / (e.ViewportHeight - e.ViewportHeightChange)) + 1;
                var preOffset = e.VerticalOffset - ((e.ViewportHeight - e.ViewportHeightChange) * (Page - 1));
                Offset = preOffset * (e.ViewportHeight / (e.ViewportHeight - e.ViewportHeightChange));
                return;
            }
            if (e.VerticalChange != 0)
            {
                var prePage = (int)Math.Round((e.VerticalOffset - e.VerticalChange) / e.ViewportHeight);
                var currentPage = (int)Math.Round(e.VerticalOffset / e.ViewportHeight);
                if (prePage != currentPage)
                {
                    LoadPage(currentPage);
                    Task.Run(() =>
                    {
                        Thread.Sleep(250);
                        if (currentPage + 1 < _images.Length)
                            LoadPage(currentPage + 1);
                        if (currentPage > 0)
                            LoadPage(currentPage - 1);
                    });
                }
            }
        }
    }
}
