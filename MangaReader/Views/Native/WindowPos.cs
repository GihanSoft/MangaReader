// -----------------------------------------------------------------------
// <copyright file="WindowPos.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace MangaReader.Views.Native
{
    /// <summary>
    /// SetWindowPos options.
    /// </summary>
    //[SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "Win32 API")]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Win32 API")]
    [Flags]
    //[SuppressMessage("Readability", "RCS1234:Duplicate enum value.", Justification = "Win32 API")]
    internal enum SWP : uint
    {
        /// <summary>
        /// none :|
        /// </summary>
        None = 0,

        /// <summary>
        /// Retains the current size (ignores the cx and cy parameters).
        /// </summary>
        NOSIZE = 1,

        /// <summary>
        /// Retains the current position (ignores X and Y parameters).
        /// </summary>
        NOMOVE = 2,

        /// <summary>
        /// Retains the current Z order (ignores the hWndInsertAfter parameter).
        /// </summary>
        NOZORDER = 4,

        /// <summary>
        /// Does not redraw changes. If this flag is set, no repainting of any kind occurs.
        /// This applies to the client area, the nonclient area (including the title bar
        /// and scroll bars), and any part of the parent window uncovered as a result of
        /// the window being moved. When this flag is set, the application must explicitly
        /// invalidate or redraw any parts of the window and parent window that need redrawing.
        /// </summary>
        NOREDRAW = 8,

        /// <summary>
        /// Does not activate the window. If this flag is not set, the window is activated
        /// and moved to the top of either the topmost or non-topmost group (depending on
        /// the setting of the hWndInsertAfter parameter).
        /// </summary>
        NOACTIVATE = 16,

        /// <summary>
        /// Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        DRAWFRAME = 32,

        /// <summary>
        /// Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE
        /// message to the window, even if the window's size is not being changed. If this
        /// flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being
        /// changed.
        /// </summary>
        FRAMECHANGED = 32,

        /// <summary>
        /// Displays the window.
        /// </summary>
        SHOWWINDOW = 64,

        /// <summary>
        /// Hides the window.
        /// </summary>
        HIDEWINDOW = 128,

        /// <summary>
        /// Discards the entire contents of the client area. If this flag is not specified,
        /// the valid contents of the client area are saved and copied back into the client
        /// area after the window is sized or repositioned.
        /// </summary>
        NOCOPYBITS = 256,

        /// <summary>
        /// Does not change the owner window's position in the Z order.
        /// </summary>
        NOOWNERZORDER = 512,

        /// <summary>
        /// Same as the SWP_NOOWNERZORDER flag.
        /// </summary>
        NOREPOSITION = 512,

        /// <summary>
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        NOSENDCHANGING = 1024,

        /// <summary>
        /// Prevents generation of the WM_SYNCPAINT message.
        /// </summary>
        DEFERERASE = 8192,

        /// <summary>
        /// If the calling thread and the thread that owns the window are attached to different
        /// input queues, the system posts the request to the thread that owns the window.
        /// This prevents the calling thread from blocking its execution while other threads
        /// process the request.
        /// </summary>
        ASYNCWINDOWPOS = 16384,
    }

    //[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Win32 API")]
    //[SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Win32 API")]
    //[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Win32 API")]
    //[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Win32 API")]
    //[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Win32 API")]
    internal struct WindowPos
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public SWP flags;
    }
}
