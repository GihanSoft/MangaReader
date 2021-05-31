// -----------------------------------------------------------------------
// <copyright file="WindowProp.FullScreen.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

using MangaReader.Views.Native;

namespace GihanSoft.Views.AttachedProperties
{
    public static class WindowProp
    {
        public static readonly DependencyProperty RealRestoreBoundsProperty = DependencyProperty.RegisterAttached(
            "RealRestoreBounds",
            typeof(Rect),
            typeof(WindowProp),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PreWindowsStateProperty = DependencyProperty.RegisterAttached(
            "PreWindowsState",
            typeof(WindowState),
            typeof(WindowProp),
            new PropertyMetadata(null));

        public static readonly DependencyProperty FullScreenProperty = DependencyProperty.RegisterAttached(
            "FullScreen",
            typeof(bool),
            typeof(WindowProp),
            new PropertyMetadata(false, (sender, args) =>
            {
                if (sender is not Window win)
                {
                    return;
                }

                WindowInteropHelper windowInteropHelper = new(win);
                var hwndSource = HwndSource.FromHwnd(windowInteropHelper.EnsureHandle());

                HwndSourceHook lockHook = new(
                    (IntPtr _, int msg, IntPtr _, IntPtr lParam, ref bool _) =>
                    {
                        if (msg is 0x46)
                        {
                            var wp = Marshal.PtrToStructure<WindowPos>(lParam);
                            wp.flags |= SWP.NOMOVE | SWP.NOSIZE;
                            Marshal.StructureToPtr(wp, lParam, false);
                        }

                        return IntPtr.Zero;
                    });

                if ((bool)args.NewValue)
                {
                    win.SetRealRestoreBounds(win.RestoreBounds);
                    win.SetPreWindowsState(win.WindowState);

                    win.SetCurrentValue(Window.TopProperty, 0d);
                    win.SetCurrentValue(Window.LeftProperty, 0d);
                    win.SetCurrentValue(FrameworkElement.HeightProperty, SystemParameters.PrimaryScreenHeight);
                    win.SetCurrentValue(FrameworkElement.WidthProperty, SystemParameters.PrimaryScreenWidth);
                    win.SetCurrentValue(Window.ResizeModeProperty, ResizeMode.NoResize);
                    win.SetCurrentValue(Window.WindowStyleProperty, WindowStyle.None);

                    var winType = win.GetType();
                    while (winType is not null && winType is not { FullName: "MahApps.Metro.Controls.MetroWindow" })
                    {
                        winType = winType.BaseType;
                    }
                    if (winType is not null &&
                        (winType.FullName?.Equals("MahApps.Metro.Controls.MetroWindow", StringComparison.OrdinalIgnoreCase) ?? false))
                    {
                        var showTitleBarProperty =
                            winType.GetField("ShowTitleBarProperty")?
                            .GetValue(win)
                            as DependencyProperty;

                        if (showTitleBarProperty is not null)
                        {
                            win.SetCurrentValue(showTitleBarProperty, false);
                        }

                        var showCloseButtonProperty =
                            winType.GetField("ShowCloseButtonProperty")?
                            .GetValue(win)
                            as DependencyProperty;
                        if (showCloseButtonProperty is not null)
                        {
                            win.SetCurrentValue(showCloseButtonProperty, false);
                        }
                    }
                    win.WindowState = WindowState.Normal;
                    win.BorderThickness = new Thickness(0);
                    hwndSource.AddHook(lockHook);
                }
                else
                {
                    hwndSource.RemoveHook(lockHook);
                    var restoreBounds = win.GetRealRestoreBounds();
                    win.SetCurrentValue(Window.WindowStateProperty, win.GetPreWindowsState());
                    win.SetCurrentValue(Window.TopProperty, restoreBounds.Top);
                    win.SetCurrentValue(Window.LeftProperty, restoreBounds.Left);
                    win.SetCurrentValue(FrameworkElement.HeightProperty, restoreBounds.Height);
                    win.SetCurrentValue(FrameworkElement.WidthProperty, restoreBounds.Width);
                    win.SetCurrentValue(Window.ResizeModeProperty, ResizeMode.CanResize);
                    win.SetCurrentValue(Window.WindowStyleProperty, WindowStyle.ThreeDBorderWindow);
                    win.SetCurrentValue(Control.BorderThicknessProperty, new Thickness(1));

                    var winType = win.GetType();
                    while (winType is not null && winType is not { FullName: "MahApps.Metro.Controls.MetroWindow" })
                    {
                        winType = winType.BaseType;
                    }
                    if (winType is not null &&
                        (winType.FullName?.Equals("MahApps.Metro.Controls.MetroWindow", StringComparison.OrdinalIgnoreCase) ?? false))
                    {
                        var showTitleBarProperty =
                            winType.GetField("ShowTitleBarProperty")?
                            .GetValue(win)
                            as DependencyProperty;

                        if (showTitleBarProperty is not null)
                        {
                            win.SetCurrentValue(showTitleBarProperty, true);
                        }

                        var showCloseButtonProperty =
                            winType.GetField("ShowCloseButtonProperty")?
                            .GetValue(win)
                            as DependencyProperty;
                        if (showCloseButtonProperty is not null)
                        {
                            win.SetCurrentValue(showCloseButtonProperty, true);
                        }
                    }
                }
            }));

        /// <summary>Helper for getting <see cref="FullScreenProperty"/> from <paramref name="window"/>.</summary>
        /// <param name="window"><see cref="Window"/> to read <see cref="FullScreenProperty"/> from.</param>
        /// <returns>FullScreen property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetFullScreen(Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            return (bool)window.GetValue(FullScreenProperty);
        }

        /// <summary>Helper for setting <see cref="FullScreenProperty"/> on <paramref name="window"/>.</summary>
        /// <param name="window"><see cref="Window"/> to set <see cref="FullScreenProperty"/> on.</param>
        /// <param name="value">FullScreen property value.</param>
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static void SetFullScreen(Window window, bool value)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            window.SetValue(FullScreenProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static WindowState GetPreWindowsState(this Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            return (WindowState)window.GetValue(PreWindowsStateProperty);
        }

        public static void SetPreWindowsState(this Window window, WindowState value)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            window.SetValue(PreWindowsStateProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static Rect GetRealRestoreBounds(this Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            return (Rect)window.GetValue(RealRestoreBoundsProperty);
        }

        public static void SetRealRestoreBounds(this Window window, Rect value)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            window.SetValue(RealRestoreBoundsProperty, value);
        }
    }

    public static class WindowPropertiesExtensions
    {
        public static bool GetFullScreen(this Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            return (bool)window.GetValue(WindowProp.FullScreenProperty);
        }

        public static void SetFullScreen(this Window window, bool value)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            window.SetCurrentValue(WindowProp.FullScreenProperty, value);
        }
    }
}
