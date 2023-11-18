namespace DisCannoli.Modules.Pagination
{
    public class ListItem<TItem>
    {
        public string? SelectMenuKey { get; set; }
        public string Label { get; set; }
        public TItem Item { get; }

        public ListItem(string? selectMenuKey, string label, TItem item)
        {
            SelectMenuKey = selectMenuKey;
            Label = label;
            Item = item;
        }
    }
}
