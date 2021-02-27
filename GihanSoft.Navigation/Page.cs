using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GihanSoft.Navigation
{
    public abstract class Page : UserControl, IDisposable
    {
        public Page(PageNavigator navigator)
        {
            Navigator = navigator;
        }

        public PageNavigator Navigator { get; }

        public virtual StackPanel? LeftToolBar { get; } = null;

        public virtual Task Refresh() { return Task.CompletedTask; }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
