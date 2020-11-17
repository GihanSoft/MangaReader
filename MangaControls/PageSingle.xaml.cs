using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gihan.Manga.Views.Custom
{
    /// <summary>
    /// Interaction logic for PageSingle.xaml
    /// </summary>
    public partial class PageSingle : PagesViewer
    {
        private int _page;

        public override double Zoom
        {
            get => (_images.FirstOrDefault(image => image != null)?.GetBindingExpression(MaxWidthProperty)?
                        .ParentBinding.Converter as ZaribConverter)?.Zarib ?? 1;
            set
            {
                foreach (var image in _images)
                {
                    if (image is null) continue;
                    (BindingOperations.GetBinding(image, MaxWidthProperty)
                        .Converter as ZaribConverter).Zarib = value;
                    (BindingOperations.GetBinding(image, MaxHeightProperty)
                        .Converter as ZaribConverter).Zarib = value;
                    (BindingOperations.GetBinding(image, HeightProperty)
                        .Converter as ZaribConverter).Zarib = value;
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
                if (value > _images.Length || value < 1) return;
                _page = value - 1;
                if (_images[_page] is null)
                    LoadPage(_page);
                ImageFrameBrd.Child = _images[_page];
                Offset = 0;
                Task.Run(() =>
                {
                    Thread.Sleep(250);
                    if (_page + 1 < _images.Length)
                        LoadPage(_page + 1);
                    if (_page > 0)
                        LoadPage(_page - 1);
                });
            }
        }

        public PageSingle()
        {
            InitializeComponent();
        }

        private void LoadPage(int page)
        {
            LoadPageStream(page);
            if (_images[page] is null)
            {
                Dispatcher.Invoke(() =>
                {
                    var widthBinding = new Binding(nameof(Sv.ViewportWidth))
                    {
                        Source = Sv,
                        Converter = new ZaribConverter { Zarib = Zoom },
                    };
                    var heightBinding = new Binding(nameof(Sv.ViewportHeight))
                    {
                        Source = Sv,
                        Converter = new ZaribConverter { Zarib = Zoom },
                    };
                    LoadBitmap(page);
                    _images[page] = new Image { Source = _bitmaps[page] };
                    _images[page].SetBinding(MaxWidthProperty, widthBinding);
                    _images[page].SetBinding(MaxHeightProperty, heightBinding);
                    _images[page].SetBinding(HeightProperty, heightBinding);
                });
            }
        }
    }
}
