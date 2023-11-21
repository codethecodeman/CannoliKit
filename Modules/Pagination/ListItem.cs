namespace CannoliKit.Modules.Pagination
{
    public sealed class ListItem<TItem>
    {
        public string Marker { get; set; }
        public TItem Item { get; }

        internal ListItem(string marker, TItem item)
        {
            Marker = marker;
            Item = item;
        }
    }
}
