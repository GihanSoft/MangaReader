// -----------------------------------------------------------------------
// <copyright file="PagesViewer.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;

using MangaReader.PagesViewer;

namespace MangaReader.Views.Components.PagesViewers
{
    public abstract class PagesViewer : Border
    {
        /// <summary>Identifies the <see cref="Page"/> dependency property.</summary>
        public readonly static DependencyProperty PageProperty = DependencyProperty.Register(
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
        public readonly static DependencyProperty ZoomProperty = DependencyProperty.Register(
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

        public virtual void View(PagesProvider pagesProvider, int page)
        {
            if (pagesProvider is null)
            {
                throw new ArgumentNullException(nameof(pagesProvider));
            }
            PagesProvider = pagesProvider;
            if (Page == page)
            {
                OnPageChanged(page, 0);
            }
            else
            {
                SetCurrentValue(PageProperty, page);
            }
        }

        protected abstract void OnPageChanged(int page, int previousPage);
        protected abstract void OnZoomChanged(double zoom);
    }
}
