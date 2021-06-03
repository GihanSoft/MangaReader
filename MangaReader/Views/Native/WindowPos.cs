// -----------------------------------------------------------------------
// <copyright file="WindowPos.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace MangaReader.Views.Native
{
    // [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Win32 API")]
    // [SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Win32 API")]
    // [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Win32 API")]
    // [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Win32 API")]
    // [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Win32 API")]
    [SuppressMessage("Performance", "MA0008:Add StructLayoutAttribute", Justification = "<Pending>")]
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
