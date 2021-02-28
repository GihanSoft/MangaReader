using MangaReader.PagesViewer;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gihan.Manga.Views.Custom
{
    /// <summary>
    /// Interaction logic for PageDouble.xaml
    /// </summary>
    public partial class PageDouble : PagesViewer
    {
        private int _page;

        public override double Zoom
        {
            get => (System.Array.Find(_images, image => image != null)?.GetBindingExpression(MaxHeightProperty)?
                        .ParentBinding.Converter as ZaribConverter)?.Zarib ?? 1;
            set
            {
                ((ZaribConverter)BindingOperations.GetBinding(ImageFrameGrd, MaxWidthProperty).Converter).Zarib = value;
                ImageFrameGrd.SetBinding(MaxWidthProperty, BindingOperations
                    .GetBinding(ImageFrameGrd, MaxWidthProperty));
                foreach (var image in _images)
                {
                    if (image is null) continue;
                    ((ZaribConverter)BindingOperations.GetBinding(image, MaxHeightProperty).Converter).Zarib = value;
                    ((ZaribConverter)BindingOperations.GetBinding(image, HeightProperty).Converter).Zarib = value;
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
                if (_page + 1 < _images.Length && _images[_page + 1] is null)
                    LoadPage(_page + 1);
                ImageFrameGrd.Children.Clear();
                if (_images[_page].Source.Width > _images[_page].Source.Height ||
                   _page + 1 == _images.Length ||
                   _images[_page + 1].Source.Width > _images[_page + 1].Source.Height)
                {
                    Grid.SetColumn(_images[_page], 0);
                    Grid.SetColumnSpan(_images[_page], 2);
                    ImageFrameGrd.Children.Add(_images[_page]);
                }
                else
                {
                    Grid.SetColumn(_images[_page], 0);
                    Grid.SetColumn(_images[_page + 1], 1);
                    Grid.SetColumnSpan(_images[_page], 1);
                    Grid.SetColumnSpan(_images[_page + 1], 1);
                    ImageFrameGrd.Children.Add(_images[_page]);
                    ImageFrameGrd.Children.Add(_images[_page + 1]);
                }
                Offset = 0;
                _ = Task.Run(() =>
                  {
                      Thread.Sleep(250);
                      if (_page + 2 < _images.Length)
                          LoadPage(_page + 2);
                      if (_page + 1 < _images.Length)
                          LoadPage(_page + 1);
                      if (_page > 0)
                          LoadPage(_page - 1);
                      if (_page > 1)
                          LoadPage(_page - 2);
                  });
            }
        }

        public PageDouble()
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
                    _images[page] = new Image { Source = bitmaps[page] };
                    ImageFrameGrd.SetBinding(MaxWidthProperty, widthBinding);
                    _images[page].SetBinding(MaxHeightProperty, heightBinding);
                    _images[page].SetBinding(HeightProperty, heightBinding);
                });
            }
        }
    }
}
