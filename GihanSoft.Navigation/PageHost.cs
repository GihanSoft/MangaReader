using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GihanSoft.Navigation
{
    public class PageHost : UserControl
    {
        public PageHost(PageNavigator navigator)
        {
            Navigator = navigator;
            Binding binding = new()
            {
                Source = Navigator,
                Path = new PropertyPath(nameof(Navigator.Current), null)
            };
            SetBinding(ContentProperty, binding);
        }

        #region Property
        public PageNavigator Navigator { get; }
        #endregion
    }
}
