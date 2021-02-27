namespace GihanSoft.Navigation.Events
{
    public class NavigatedEventArgs
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