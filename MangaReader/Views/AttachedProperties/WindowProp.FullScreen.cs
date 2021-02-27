using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using ControlzEx.Standard;

using GihanSoft;

using MahApps.Metro.Controls;

namespace GihanSoft.Views.AttachedProperties
{
    public static partial class WindowProp
    {
        public readonly static DependencyProperty RealRestoreBoundsProperty = DependencyProperty.RegisterAttached(
            "RealRestoreBounds",
            typeof(Rect),
            typeof(WindowProp),
            new PropertyMetadata(null));

        public readonly static DependencyProperty PreWindowsStateProperty = DependencyProperty.RegisterAttached(
            "PreWindowsState",
            typeof(WindowState),
            typeof(WindowProp),
            new PropertyMetadata(null));

        public readonly static DependencyProperty FullScreenProperty = DependencyProperty.RegisterAttached(
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
                HwndSource hwndSource = HwndSource.FromHwnd(windowInteropHelper.EnsureHandle());

                static IntPtr LockHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
                {
                    if (msg == 0x46)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        var wp = Marshal.PtrToStructure<WINDOWPOS>(lParam);
                        wp.flags |= SWP.NOMOVE | SWP.NOSIZE;
#pragma warning restore CS0618 // Type or member is obsolete
                        Marshal.StructureToPtr(wp, lParam, false);
                    }
                    return IntPtr.Zero;
                }

                if ((bool)args.NewValue)
                {
                    win.SetRealRestoreBounds(win.RestoreBounds);
                    win.Height = SystemParameters.PrimaryScreenHeight;
                    win.Width = SystemParameters.PrimaryScreenWidth;
                    win.Top = 0;
                    win.Left = 0;
                    win.ResizeMode = ResizeMode.NoResize;
                    win.WindowStyle = WindowStyle.None;
                    if (win is MetroWindow metroWindow)
                    {
                        metroWindow.ShowTitleBar = false;
                        metroWindow.ShowCloseButton = false;
                        metroWindow.GlowBrush = null;
                    }
                    SetPreWindowsState(win, win.WindowState);
                    win.WindowState = WindowState.Normal;
                    win.BorderThickness = new Thickness(0);
                    hwndSource.AddHook(LockHook);
                }
                else
                {
                    hwndSource.RemoveHook(LockHook);
                    win.WindowState = GetPreWindowsState(win);
                    Rect restoreBounds = win.GetRealRestoreBounds();
                    win.Height = restoreBounds.Height;
                    win.Width = restoreBounds.Width;
                    win.Top = restoreBounds.Top;
                    win.Left = restoreBounds.Left;
                    win.ResizeMode = ResizeMode.CanResize;
                    win.WindowStyle = WindowStyle.SingleBorderWindow;
                    if (win is MetroWindow metroWindow)
                    {
                        metroWindow.BorderThickness = new Thickness(1);
                        metroWindow.ShowTitleBar = true;
                        metroWindow.ShowCloseButton = true;
                    }
                }
            }));

        public static bool GetFullScreen(Window obj)
        {
            return (bool)obj.GetValue(FullScreenProperty);
        }
        public static void SetFullScreen(Window obj, bool value)
        {
            obj.SetValue(FullScreenProperty, value);
        }

        public static WindowState GetPreWindowsState(this Window window)
        {
            return (WindowState)window.GetValue(PreWindowsStateProperty);
        }
        public static void SetPreWindowsState(this Window window, WindowState value)
        {
            window.SetValue(PreWindowsStateProperty, value);
        }

        public static Rect GetRealRestoreBounds(this Window window)
        {
            return (Rect)window.GetValue(RealRestoreBoundsProperty);
        }
        public static void SetRealRestoreBounds(this Window window, Rect value)
        {
            window.SetValue(RealRestoreBoundsProperty, value);
        }
    }
    public static partial class WindowPropertiesExtensions
    {
        public static bool GetFullScreen(this Window window)
        {
            return (bool)window.GetValue(WindowProp.FullScreenProperty);
        }
        public static void SetFullScreen(this Window window, bool value)
        {
            window.SetValue(WindowProp.FullScreenProperty, value);
        }
    }
}
