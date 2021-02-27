using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GihanSoft.Navigation
{
    public class Frame : UserControl
    {
        public Frame(PageNavigator navigator)
        {
            Navigator = navigator;
            Binding binding = new()
            {
                Source = Navigator,
                Path = new PropertyPath(nameof(Navigator.Current))
            };
            SetBinding(ContentProperty, binding);
        }

        #region Property
        public PageNavigator Navigator { get; }
        #endregion
    }
}
