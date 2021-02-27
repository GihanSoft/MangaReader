namespace GihanSoft.Navigation.Events
{
    public class NavigatingEventArgs : NavigatedEventArgs
    {
        public NavigatingEventArgs(Page current, Page future)
            : base(current, future)
        {
        }

        public bool Cancel { get; set; }
    }
}