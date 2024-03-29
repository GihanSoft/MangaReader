﻿using MangaReader.Views.XamlConverters;

using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MangaReader.Views.Components.PagesViewers
{
    /// <summary>
    /// Interaction logic for PageSingle.xaml
    /// </summary>
    public partial class PageSingle : PagesViewer
    {
        public PageSingle()
        {
            InitializeComponent();
        }

        protected override void OnPageChanged(int page, int previousPage)
        {
            if (PagesProvider is null)
            {
                return;
            }
            if (page is < 0 || page >= PagesProvider.Count)
            {
                SetCurrentValue(PageProperty, previousPage);
                return;
            }
            MemoryStream? memoryStream = PagesProvider[page];
            if (memoryStream is null)
            {
                PagesProvider.LoadPageAsync(page).GetAwaiter().GetResult();
                memoryStream = PagesProvider[page];
            }

            Img.SetCurrentValue(Image.SourceProperty, BitmapFrame.Create(memoryStream));
        }

        protected override void OnZoomChanged(double zoom)
        {
            ((ZaribConverter)Resources[nameof(ZaribConverter)]).SetCurrentValue(ZaribConverter.ZaribProperty, zoom);
            Img.GetBindingExpression(HeightProperty).UpdateTarget();
            if (ScrollViewer.ScrollableWidth is 0d && zoom is > 1)
            {
                Task.Run(async () =>
                {
                    while (Dispatcher.Invoke(() => ScrollViewer.ScrollableWidth) is 0d)
                    {
                        await Task.Delay(10).ConfigureAwait(false);
                    }
                    Dispatcher.Invoke(() => ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.ScrollableWidth / 2));
                });
            }
            else
            {
                ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.ScrollableWidth / 2);
            }
        }

        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
            {
                if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown)
                {
                    KeyEventArgs args = new(Keyboard.PrimaryDevice, e.InputSource, e.Timestamp, e.Key);
                    args.RoutedEvent = KeyDownEvent;
                    ((FrameworkElement)Parent).RaiseEvent(args);
                }
                return;
            }

            int rtl = this.FlowDirection == FlowDirection.LeftToRight ? 1 : -1;

            switch (e.Key)
            {
                case Key.Left:
                    if (ScrollViewer.HorizontalOffset == 0)
                    {
                        SetCurrentValue(PageProperty, Page - (1 * rtl));
                    }
                    break;
                case Key.Right:
                    if (ScrollViewer.HorizontalOffset == ScrollViewer.ScrollableWidth)
                    {
                        SetCurrentValue(PageProperty, Page + (1 * rtl));
                    }
                    break;
                case Key.Up:
                    if (ScrollViewer.VerticalOffset == 0)
                    {
                        SetCurrentValue(PageProperty, Page - (1 * rtl));
                    }
                    break;
                case Key.Down:
                    if (ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight)
                    {
                        SetCurrentValue(PageProperty, Page + (1 * rtl));
                    }
                    break;
                case Key.PageUp:
                    SetCurrentValue(PageProperty, Page - (1 * rtl));
                    break;
                case Key.PageDown:
                    SetCurrentValue(PageProperty, Page + (1 * rtl));
                    break;
                default:
                    break;
            }
        }

        private void PagesViewer_GotFocus(object sender, RoutedEventArgs e)
        {
            ControlzEx.KeyboardNavigationEx.Focus(ScrollViewer);
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(Keyboard.Modifiers is ModifierKeys.Control)
            {
                if (e.Delta > 0)
                {
                    Zoom += 0.05;
                }
                else
                {
                    Zoom -= 0.05;
                }
            }
        }
    }
}
