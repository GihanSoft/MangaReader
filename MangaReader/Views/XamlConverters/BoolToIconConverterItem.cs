// -----------------------------------------------------------------------
// <copyright file="BoolToIconConverter.cs" company="GihanSoft">
// Copyright (c) 2021 GihanSoft. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

using MahApps.Metro.IconPacks;

namespace AnimePlayer.Views.XamlConverters
{
    [SuppressMessage("Performance", "CA1812:BoolToIconConverter is an internal class that is apparently never instantiated. If so, remove the code from the assembly. If this class is intended to contain only static members, make it static (Shared in Visual Basic.", Justification = "WPF object")]
    internal class BoolToIconConverterItem
    {
        public PackIconMaterialKind Kind { get; set; }
        public bool Value { get; set; }
    }
}
