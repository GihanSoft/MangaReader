using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using GihanSoft;

namespace MangaReader.PagesViewer
{
    /// <summary>
    /// Interaction logic for PageSingle.xaml
    /// </summary>
    public partial class PageSingle : PagesViewer
    {
        private int _page;

        public override double Zoom
        {
            get => Array.Find(images, image => image != null)?
                .GetBindingExpression(MaxWidthProperty)?
                .ParentBinding.Converter.As<ZaribConverter>()?.Zarib ?? 1;
            set
            {
                foreach (var image in images)
                {
                    if (image is null)
                    {
                        continue;
                    }

                    BindingOperations.GetBinding(image, MaxWidthProperty)
                        .Converter.As<ZaribConverter>().Zarib = value;
                    BindingOperations.GetBinding(image, MaxHeightProperty)
                        .Converter.As<ZaribConverter>().Zarib = value;
                    BindingOperations.GetBinding(image, HeightProperty)
                        .Converter.As<ZaribConverter>().Zarib = value;
                    image.SetBinding(MaxWidthProperty, BindingOperations.GetBinding(image, MaxWidthProperty));
                    image.SetBinding(MaxHeightProperty, BindingOperations.GetBinding(image, MaxHeightProperty));
                    image.SetBinding(HeightProperty, BindingOperations.GetBinding(image, HeightProperty));
                }
            }
        }
        public override double Offset
        {
            get => Sv.VerticalOffset / Zoom;
            set => Sv.ScrollToVerticalOffset(value * Zoom);
        }
        public override int Page
        {
            get => _page + 1;
            set
            {
                if (value >= images.Length || value < 0)
                {
                    return;
                }

                _page = value;
                if (images[_page] is null)
                {
                    LoadPage(_page).GetAwaiter().GetResult();
                }

                ImageFrameBrd.Child = images[_page];
                Offset = 0;
                Task.Run(() =>
                  {
                      Thread.Sleep(250);
                      if (_page + 1 < images.Length)
                      {
                          LoadPage(_page + 1);
                      }

                      if (_page > 0)
                      {
                          LoadPage(_page - 1);
                      }
                  });
            }
        }

        public PageSingle()
        {
            InitializeComponent();
        }

        private async Task LoadPage(int page)
        {
            if (images[page] is null)
            {
                var (widthBinding, heightBinding) = Dispatcher.Invoke(() =>
                {
                    var widthBinding = new Binding()
                    {
                        Path = new PropertyPath(nameof(Sv.ViewportWidth), null),
                        Source = Sv,
                        Converter = new ZaribConverter { Zarib = Zoom },
                    };
                    var heightBinding = new Binding()
                    {
                        Path = new PropertyPath(nameof(Sv.ViewportHeight), null),
                        Source = Sv,
                        Converter = new ZaribConverter { Zarib = Zoom },
                    };
                    return (widthBinding, heightBinding);
                });
                await LoadBitmap(page).ConfigureAwait(false);
                images[page] = new Image { Source = bitmaps[page] };
                images[page].SetBinding(MaxWidthProperty, widthBinding);
                images[page].SetBinding(MaxHeightProperty, heightBinding);
                images[page].SetBinding(HeightProperty, heightBinding);
            }
        }
    }
}
