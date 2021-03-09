using System.Windows;
using MangaReader.PagesViewer;

using System;
using System.Windows.Controls;

namespace MangaReader.Views.Components
{
    public abstract class PagesViewer : Border
    {
        public static readonly DependencyProperty PageProperty = DependencyProperty.Register(
            nameof(Page),
            typeof(int),
            typeof(PagesViewer),
            new PropertyMetadata((d, e) =>
            {
                if (d is not PagesViewer pagesViewer)
                {
                    return;
                }

                pagesViewer.OnPageChanged((int)e.NewValue, (int)e.OldValue);
            }));

        /// <summary>Identifies the <see cref="Zoom"/> dependency property.</summary>
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            nameof(Zoom),
            typeof(double),
            typeof(PagesViewer),
            new PropertyMetadata(1d, (d, e) =>
            {
                if (d is not PagesViewer pagesViewer)
                {
                    return;
                }

                pagesViewer.OnZoomChanged((double)e.NewValue);
            }));

        protected PagesProvider? PagesProvider { get; private set; }

        public int Page
        {
            get => (int)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }
        //public abstract double Offset { get; set; }

        public virtual void View(PagesProvider pagesProvider, int page)
        {
            if (pagesProvider is null)
            {
                throw new ArgumentNullException(nameof(pagesProvider));
            }
            this.PagesProvider = pagesProvider;
            if (this.Page == page)
            {
                OnPageChanged(page, 0);
            }
            else
            {
                this.SetCurrentValue(PageProperty, page);
            }
        }

        protected abstract void OnPageChanged(int page, int previousPage);
        protected abstract void OnZoomChanged(double zoom);
    }
}