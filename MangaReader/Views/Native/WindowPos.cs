using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader.Views.Native
{
    //
    // Summary:
    //     SetWindowPos options
    [CLSCompliant(false)]
    [Flags]
    public enum SWP : uint
    {
        //
        // Summary:
        //     Retains the current size (ignores the cx and cy parameters).
        NOSIZE = 1,
        //
        // Summary:
        //     Retains the current position (ignores X and Y parameters).
        NOMOVE = 2,
        //
        // Summary:
        //     Retains the current Z order (ignores the hWndInsertAfter parameter).
        NOZORDER = 4,
        //
        // Summary:
        //     Does not redraw changes. If this flag is set, no repainting of any kind occurs.
        //     This applies to the client area, the nonclient area (including the title bar
        //     and scroll bars), and any part of the parent window uncovered as a result of
        //     the window being moved. When this flag is set, the application must explicitly
        //     invalidate or redraw any parts of the window and parent window that need redrawing.
        NOREDRAW = 8,
        //
        // Summary:
        //     Does not activate the window. If this flag is not set, the window is activated
        //     and moved to the top of either the topmost or non-topmost group (depending on
        //     the setting of the hWndInsertAfter parameter).
        NOACTIVATE = 16,
        //
        // Summary:
        //     Draws a frame (defined in the window's class description) around the window.
        DRAWFRAME = 32,
        //
        // Summary:
        //     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE
        //     message to the window, even if the window's size is not being changed. If this
        //     flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being
        //     changed.
        FRAMECHANGED = 32,
        //
        // Summary:
        //     Displays the window.
        SHOWWINDOW = 64,
        //
        // Summary:
        //     Hides the window.
        HIDEWINDOW = 128,
        //
        // Summary:
        //     Discards the entire contents of the client area. If this flag is not specified,
        //     the valid contents of the client area are saved and copied back into the client
        //     area after the window is sized or repositioned.
        NOCOPYBITS = 256,
        //
        // Summary:
        //     Does not change the owner window's position in the Z order.
        NOOWNERZORDER = 512,
        //
        // Summary:
        //     Same as the SWP_NOOWNERZORDER flag.
        NOREPOSITION = 512,
        //
        // Summary:
        //     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        NOSENDCHANGING = 1024,
        TOPMOST = 1563,
        //
        // Summary:
        //     Prevents generation of the WM_SYNCPAINT message.
        DEFERERASE = 8192,
        //
        // Summary:
        //     If the calling thread and the thread that owns the window are attached to different
        //     input queues, the system posts the request to the thread that owns the window.
        //     This prevents the calling thread from blocking its execution while other threads
        //     process the request.
        ASYNCWINDOWPOS = 16384
    }

    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>")]
    [SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "<Pending>")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "<Pending>")]
    public struct WindowPos
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        [CLSCompliant(false)]
        public SWP flags;
    }
}
