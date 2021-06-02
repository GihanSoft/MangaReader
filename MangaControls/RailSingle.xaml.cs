using MangaReader.PagesViewer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

using static System.Windows.Data.BindingOperations;

namespace MangaReader.PagesViewer
{
    /// <summary>
    /// Interaction logic for RailSingle.xaml
    /// </summary>
    public partial class RailSingle : PagesViewer
    {
        public override double Zoom
        {
            get => (images.First()?.GetBindingExpression(MaxHeightProperty)?
                        .ParentBinding?.Converter as ZaribConverter)?.Zarib ?? 1;
            set
            {
                var page = Page;
                var offset = Offset;
                double? dZoom = null;
                foreach(var image in images)
                {
                    if(image is null)
                    {
                        continue;
                    }

                    if(dZoom is null)
                    {
                        dZoom = value / (GetBinding(image, MaxHeightProperty).Converter as ZaribConverter).Zarib;
                    } (GetBinding(image, MaxHeightProperty).Converter as ZaribConverter).Zarib = value;
                    (GetBinding(image, HeightProperty).Converter as ZaribConverter).Zarib = value;
                    image.SetBinding(MaxHeightProperty, GetBinding(image, MaxHeightProperty));
                    image.SetBinding(HeightProperty, GetBinding(image, HeightProperty));
                }
                Page = page;
                Offset = offset * (dZoom ?? 1);
            }
        }
        public override double Offset
        {
            get => Sv.VerticalOffset - (images.First()?.Height ?? Sv.ViewportHeight) * (Page - 1);
            set
            {
                var re = (images.First()?.Height ?? Sv.ViewportHeight) * (Page - 1) + value;
                Sv.ScrollToVerticalOffset(re);
            }
        }
        public override int Page
        {
            get => (int)Math.Round(Sv.VerticalOffset / (images?.First()?.Height ?? 1)) + 1;
            set
            {
                if(value is 1)
                {
                    LoadPage(0);
                }

                Sv.ScrollToVerticalOffset(images.First().Height * (value - 1));
            }
        }

        public RailSingle()
        {
            InitializeComponent();
        }

        private void LoadPage(int page)
        {
            Dispatcher.Invoke(() =>
            {
                if(images[page].Source is null)
                {
                    LoadBitmap(page);
                    images[page].SetCurrentValue(Image.SourceProperty, bitmaps[page]);
                }
            }, System.Windows.Threading.DispatcherPriority.Normal, default);
        }
        public override void View(PagesProvider pagesProvider, int page)
        {
            base.View(pagesProvider, page);
            for(var i = 0; i < images.Length; i++)
            {
                var heightBinding = new Binding(nameof(Sv.ViewportHeight))
                {
                    Source = Sv,
                    Converter = new ZaribConverter { Zarib = Zoom },
                };
                images[i] = new Image();
                images[i].SetBinding(MaxHeightProperty, heightBinding);
                images[i].SetBinding(HeightProperty, heightBinding);
                ImagesRailSp.Children.Add(images[i]);
            }
        }

        private void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(e.ViewportHeightChange != 0 && e.ViewportHeight != e.ViewportHeightChange)
            {
                Page = (int)Math.Round(e.VerticalOffset / (e.ViewportHeight - e.ViewportHeightChange)) + 1;
                var preOffset = e.VerticalOffset - ((e.ViewportHeight - e.ViewportHeightChange) * (Page - 1));
                Offset = preOffset * (e.ViewportHeight / (e.ViewportHeight - e.ViewportHeightChange));
                return;
            }
            if(e.VerticalChange != 0)
            {
                var prePage = (int)Math.Round((e.VerticalOffset - e.VerticalChange) / e.ViewportHeight);
                var currentPage = (int)Math.Round(e.VerticalOffset / e.ViewportHeight);
                if(prePage != currentPage)
                {
                    LoadPage(currentPage);
                    _ = Task.Run(() =>
                      {
                          Thread.Sleep(250);
                          if(currentPage + 1 < images.Length)
                          {
                              LoadPage(currentPage + 1);
                          }

                          if(currentPage > 0)
                          {
                              LoadPage(currentPage - 1);
                          }
                      });
                }
            }
        }
    }
}
