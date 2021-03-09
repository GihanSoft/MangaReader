using System;
using System.Windows;
using System.Windows.Markup;

[assembly: CLSCompliant(true)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsDefinition(
    "http://gihansoft.ir/netfx/xaml/mangareader/pagesviewer",
    "MangaReader.PagesViewer")]