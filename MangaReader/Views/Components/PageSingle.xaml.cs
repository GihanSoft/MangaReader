using MangaReader.Views.XamlConverters;

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MangaReader.Views.Components
{
    /// <summary>
    /// Interaction logic for PageSingle.xaml
    /// </summary>
    public partial class PageSingle : PagesViewer
    {
        //public override double Zoom
        //{
        //    get => (Array.Find(images, image => image != null)?.GetBindingExpression(MaxWidthProperty)?
        //                .ParentBinding.Converter as ZaribConverter)?.Zarib ?? 1;
        //    set
        //    {
        //        foreach (var image in images)
        //        {
        //            if (image is null) continue;
        //            (BindingOperations.GetBinding(image, MaxWidthProperty)
        //                .Converter as ZaribConverter).Zarib = value;
        //            (BindingOperations.GetBinding(image, MaxHeightProperty)
        //                .Converter as ZaribConverter).Zarib = value;
        //            (BindingOperations.GetBinding(image, HeightProperty)
        //                .Converter as ZaribConverter).Zarib = value;
        //            image.SetBinding(MaxWidthProperty, BindingOperations.GetBinding(image, MaxWidthProperty));
        //            image.SetBinding(MaxHeightProperty, BindingOperations.GetBinding(image, MaxHeightProperty));
        //            image.SetBinding(HeightProperty, BindingOperations.GetBinding(image, HeightProperty));
        //        }

        //    }
        //}
        //public override double Offset
        //{
        //    get => Sv.VerticalOffset / Zoom;
        //    set => Sv.ScrollToVerticalOffset(value * Zoom);
        //}
        //public override int Page
        //{
        //    get => _page + 1;
        //    set
        //    {
        //        if (value >= images.Length || value < 0) return;
        //        _page = value;
        //        if (images[_page] is null)
        //            LoadPage(_page).GetAwaiter().GetResult();
        //        ImageFrameBrd.Child = images[_page];
        //        Offset = 0;
        //        _ = Task.Run(() =>
        //          {
        //              Thread.Sleep(250);
        //              if (_page + 1 < images.Length)
        //                  LoadPage(_page + 1);
        //              if (_page > 0)
        //                  LoadPage(_page - 1);
        //          });
        //    }
        //}

        public PageSingle()
        {
            InitializeComponent();
        }

        protected override void OnPageChanged(int page)
        {
            if (PagesProvider is null)
            {
                return;
            }
            MemoryStream? memoryStream = PagesProvider[page];
            if (memoryStream is null)
            {
                PagesProvider.LoadPageAsync(page).GetAwaiter().GetResult();
                memoryStream = PagesProvider[page];
            }

            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.StreamSource = memoryStream!;
            bitmap.EndInit();
            Img.SetCurrentValue(Image.SourceProperty, bitmap);
        }

        protected override void OnZoomChanged(double zoom)
        {
            ((ZaribConverter)Resources[nameof(ZaribConverter)]).SetCurrentValue(ZaribConverter.ZaribProperty, zoom);
            Img.GetBindingExpression(HeightProperty).UpdateTarget();
        }

        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
            {
                return;
            }

            int rtl = this.FlowDirection == FlowDirection.LeftToRight ? 1 : -1;

            switch (e.Key)
            {
                case Key.Left:
                    if (ScrollViewer.HorizontalOffset == 0)
                    {
                        SetCurrentValue(PageProperty, Page - 1 * rtl);
                    }
                    break;
                case Key.Right:
                    if (ScrollViewer.HorizontalOffset == ScrollViewer.ScrollableWidth)
                    {
                        SetCurrentValue(PageProperty, Page + 1 * rtl);
                    }
                    break;
                case Key.Up:
                    if (ScrollViewer.VerticalOffset == 0)
                    {
                        SetCurrentValue(PageProperty, Page - 1 * rtl);
                    }
                    break;
                case Key.Down:
                    if (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
                    {
                        SetCurrentValue(PageProperty, Page + 1 * rtl);
                    }
                    break;
                case Key.PageUp:
                    SetCurrentValue(PageProperty, Page - 1 * rtl);
                    break;
                case Key.PageDown:
                    SetCurrentValue(PageProperty, Page + 1 * rtl);
                    break;
                default:
                    break;
            }
        }

        //private async Task LoadPage(int page)
        //{
        //    if (images[page] is null)
        //    {
        //        var (widthBinding, heightBinding) = Dispatcher.Invoke(() =>
        //        {
        //            var widthBinding = new Binding()
        //            {
        //                Path = new PropertyPath(nameof(Sv.ViewportWidth), null),
        //                Source = Sv,
        //                Converter = new ZaribConverter { Zarib = Zoom },
        //            };
        //            var heightBinding = new Binding()
        //            {
        //                Path = new PropertyPath(nameof(Sv.ViewportHeight), null),
        //                Source = Sv,
        //                Converter = new ZaribConverter { Zarib = Zoom },
        //            };
        //            return (widthBinding, heightBinding);
        //        });
        //        await LoadBitmap(page).ConfigureAwait(false);
        //        images[page] = new Image { Source = bitmaps[page] };
        //        images[page].SetBinding(MaxWidthProperty, widthBinding);
        //        images[page].SetBinding(MaxHeightProperty, heightBinding);
        //        images[page].SetBinding(HeightProperty, heightBinding);
        //    }
        //}
    }
}
