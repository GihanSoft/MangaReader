using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Gihan.Manga.Models.Enums;

namespace Gihan.Manga.Views.Custom
{
    /// <summary>
    /// Interaction logic for PageViewer.xaml
    /// </summary>
    public partial class PageViewer
    {
        private Panel _pagesPanel;
        private ScrollViewer _scrollViewer;
        private Border _brdCover;
        private ViewMode _viewMode;
        private Stream[] _imagesStream;
        private bool _nightMode;
        private double _zoom;

        private Image[] Images { get; set; }
        private BitmapImage[] BitmapImages { get; set; }

        public Stream[] ImagesStream
        {
            get => _imagesStream;
            set
            {
                Dispatcher.Invoke(() =>
                {
                    var imageses = new List<BitmapImage>();
                    foreach (var stream in value)
                    {
                        var bitmapImg = new BitmapImage();
                        bitmapImg.BeginInit();
                        bitmapImg.StreamSource = stream;
                        bitmapImg.EndInit();
                        imageses.Add(bitmapImg);
                        if (stream is FileStream) stream.Close();
                    }

                    BitmapImages = imageses.ToArray();
                    Images = BitmapImages.Select(image => new Image()
                    {
                        Source = image,
                        HorizontalAlignment = HorizontalAlignment.Center,

                    }).ToArray();
                });
                _imagesStream = value;
            }
        }

        public ViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                //if (value == _viewMode) return;
                switch (value)
                {
                    case ViewMode.RailSingle:
                        _pagesPanel = new StackPanel()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        foreach (var image in Images)
                        {
                            _pagesPanel.Children.Add(image);
                        }
                        break;
                    case ViewMode.RailDouble:
                        _pagesPanel = new WrapPanel()
                        {
                            MaxWidth = double.IsNaN(Width) ? ActualWidth : Width,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        foreach (var image in Images)
                        {
                            _pagesPanel.Children.Add(image);
                        }
                        break;
                    case ViewMode.PageSingle:
                        break;
                    case ViewMode.PageDouble:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                _scrollViewer.Content = _pagesPanel;
                _viewMode = value;
            }
        }

        public bool NightMode
        {
            get => _nightMode;
            set
            {
                Color color;
                switch (value)
                {
                    case true:
                        color = Color.FromArgb(100, 255, 200, 150);
                        break;
                    case false:
                        color = Color.FromArgb(255, 0, 0, 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _brdCover.Background = new SolidColorBrush(color);

                _nightMode = value;
            }
        }

        public double Zoom
        {
            get => _zoom;
            set
            {
                foreach (var image in Images)
                {
                    var d = value / _zoom;
                    Dispatcher.Invoke(() =>
                        image.Width = image.Width * d
                        );
                }
                _zoom = value;
            }
        }

        public double StandardScrollOffset => _scrollViewer.VerticalOffset;

        public PageViewer()
        {
            InitializeComponent();

            _scrollViewer = new ScrollViewer() { Name = "_scrollViewer" };
            _brdCover = new Border()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            Binding binding = new Binding();
            binding.Source = _scrollViewer.ViewportWidth;
            //BindingOperations.SetBinding(_brdCover, Border.WidthProperty, binding);


            GridMain.Children.Add(_scrollViewer);
            GridMain.Children.Add(_brdCover);
        }
    };
}
