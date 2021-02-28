using System;

namespace GihanSoft.Navigation.Events
{
    public class NavigatedEventArgs : EventArgs
    {
        public NavigatedEventArgs(Page current, Page future)
        {
            Current = current;
            Future = future;
        }

        public Page Current { get; }
        public Page Future { get; }
    }
}