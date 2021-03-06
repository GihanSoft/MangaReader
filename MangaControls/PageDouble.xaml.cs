using MangaReader.PagesViewer;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace MangaReader.PagesViewer
{
    /// <summary>
    /// Interaction logic for PageDouble.xaml
    /// </summary>
    public partial class PageDouble : PagesViewer
    {
        private int _page;

        public override double Zoom
        {
            get
            {
                if (images is null)
                {
                    return 1;
                }
                return (System.Array.Find(images, image => image != null)?.GetBindingExpression(MaxHeightProperty)?
                       .ParentBinding.Converter as ZaribConverter)?.Zarib ?? 1;
            }

            set
            {
                ((ZaribConverter)BindingOperations.GetBinding(ImageFrameGrd, MaxWidthProperty).Converter).Zarib = value;
                ImageFrameGrd.SetBinding(MaxWidthProperty, BindingOperations
                    .GetBinding(ImageFrameGrd, MaxWidthProperty));
                if (images is null)
                {
                    return;
                }
                foreach (var image in images)
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
                if (images is null)
                {
                    return;
                }
                if (value > images.Length || value < 1) return;
                _page = value - 1;
                if (images[_page] is null)
                    LoadPage(_page);
                if (_page + 1 < images.Length && images[_page + 1] is null)
                    LoadPage(_page + 1);
                ImageFrameGrd.Children.Clear();
                if (images[_page].Source.Width > images[_page].Source.Height ||
                   _page + 1 == images.Length ||
                   images[_page + 1].Source.Width > images[_page + 1].Source.Height)
                {
                    Grid.SetColumn(images[_page], 0);
                    Grid.SetColumnSpan(images[_page], 2);
                    ImageFrameGrd.Children.Add(images[_page]);
                }
                else
                {
                    Grid.SetColumn(images[_page], 0);
                    Grid.SetColumn(images[_page + 1], 1);
                    Grid.SetColumnSpan(images[_page], 1);
                    Grid.SetColumnSpan(images[_page + 1], 1);
                    ImageFrameGrd.Children.Add(images[_page]);
                    ImageFrameGrd.Children.Add(images[_page + 1]);
                }
                Offset = 0;
                _ = Task.Run(() =>
                  {
                      Thread.Sleep(250);
                      if (_page + 2 < images.Length)
                          LoadPage(_page + 2);
                      if (_page + 1 < images.Length)
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
            if (images is null)
            {
                return;
            }
            if (images[page] is null)
            {
                Dispatcher.Invoke(async () =>
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
                    await LoadBitmap(page).ConfigureAwait(false);
                    images[page] = new Image { Source = bitmaps![page] };
                    ImageFrameGrd.SetBinding(MaxWidthProperty, widthBinding);
                    images[page].SetBinding(MaxHeightProperty, heightBinding);
                    images[page].SetBinding(HeightProperty, heightBinding);
                });
            }
        }
    }
}
