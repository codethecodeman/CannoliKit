using Discord;
using System.Text;

namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// Pagination result.
    /// </summary>
    /// <typeparam name="T">Paginated type.</typeparam>
    public sealed class PaginationResult<T>
    {
        /// <summary>
        /// Number of items to display per Discord embed field.
        /// </summary>
        public int NumItemsPerField { get; }

        /// <summary>
        /// Number of items to display per page.
        /// </summary>
        public int NumItemsPerPage { get; }

        /// <summary>
        /// Number of items to display per row.
        /// </summary>
        public int NumItemsPerRow { get; }

        /// <summary>
        /// Number of items.
        /// </summary>
        public int NumItems { get; private set; }

        /// <summary>
        /// Number of pages.
        /// </summary>
        public int NumPages { get; private set; }

        /// <summary>
        /// Current page number. Zero based.
        /// </summary>
        public int PageNumber { get; internal set; }

        /// <summary>
        /// List type.
        /// </summary>
        public ListType ListType { get; }

        /// <summary>
        /// List items belonging to the current page.
        /// </summary>
        public IReadOnlyList<ListItem<T>> Items { get; }

        /// <summary>
        /// Embed field builders belonging to the current page.
        /// </summary>
        public IReadOnlyList<EmbedFieldBuilder> Fields { get; }

        private readonly IList<T> _items;
        private readonly Func<ListItem<T>, string> _formatter;
        private readonly bool _resetListCounterBetweenPages;
        private readonly int _listStartIndex;

        internal PaginationResult(
            IList<T> items,
            Func<ListItem<T>, string> formatter,
            ListType listType,
            int numItemsPerRow,
            int numItemsPerPage,
            int numItemsPerField,
            bool resetListCounterBetweenPages,
            int pageNumber)
        {
            ListType = listType;
            NumItemsPerRow = numItemsPerRow;
            NumItemsPerPage = numItemsPerPage;
            NumItemsPerField = numItemsPerField;
            PageNumber = pageNumber;

            _items = items;
            _formatter = formatter;
            _resetListCounterBetweenPages = resetListCounterBetweenPages;

            CalculatePages();

            _listStartIndex = NumItemsPerPage * PageNumber;

            Items = GetPageItems();
            Fields = GetPageEmbedBuilders();
        }

        private void CalculatePages()
        {
            NumItems = _items.Count;
            NumPages = (int)Math.Ceiling((double)NumItems / NumItemsPerPage);

            // If less than zero, reset to the last page.
            if (PageNumber < 0)
            {
                PageNumber = NumPages - 1;
            }

            // If greater than the page count, reset to the first page.
            if (PageNumber + 1 > NumPages)
            {
                PageNumber = 0;
            }
        }

        private List<ListItem<T>> GetPageItems()
        {
            var itemsOnPage = new List<ListItem<T>>();

            var properties = typeof(T).GetProperties();
            var maxPropertyLengths = new Dictionary<string, int>();

            foreach (var property in properties)
            {
                maxPropertyLengths[property.Name] = 0;
            }

            for (var i = _listStartIndex; i < _listStartIndex + NumItemsPerPage; i++)
            {
                if (i + 1 > NumItems) break;

                var listIndex = _resetListCounterBetweenPages ? i + 1 - _listStartIndex : i + 1;

                var marker = ListType switch
                {
                    ListType.Number => $"{listIndex}.",
                    ListType.Letter => $"{IntToLetters(listIndex + 1)}.",
                    ListType.Bullet => "-",
                    _ => throw new ArgumentOutOfRangeException(nameof(ListType), ListType, null)
                };

                itemsOnPage.Add(
                    new ListItem<T>(marker, _items[i]));

                foreach (var property in properties)
                {
                    var value = property.GetValue(_items[i])?.ToString();
                    if (value == null) continue;
                    if (maxPropertyLengths[property.Name] < value.Length)
                    {
                        maxPropertyLengths[property.Name] = value.Length;
                    }
                }
            }

            if (NumItems == 0)
            {
                return itemsOnPage;
            }

            var maxMarkerLength = itemsOnPage
                .Max(x => x.Marker.Length);

            foreach (var item in itemsOnPage)
            {
                item.Items = itemsOnPage;

                if (item.Marker.Length == maxMarkerLength) continue;

                item.Marker = item.Marker.PadLeft(maxMarkerLength);
            }



            return itemsOnPage;
        }

        private List<EmbedFieldBuilder> GetPageEmbedBuilders()
        {
            var numRows = (int)Math.Ceiling((double)Items.Count / NumItemsPerRow);
            var rowContents = new List<string>();
            var sb = new StringBuilder();

            for (var row = 0; row < numRows; row++)
            {
                for (var col = 0; col < NumItemsPerRow; col++)
                {
                    var index = col * numRows + row;
                    if (index >= Items.Count) break;
                    sb.Append(_formatter(Items[index]));
                }

                rowContents.Add(sb.ToString());
                sb.Clear();
            }

            var fields = new List<EmbedFieldBuilder>();
            var itemCounter = 0;

            for (var i = 0; i < rowContents.Count; i++)
            {
                sb.AppendLine(rowContents[i]);

                itemCounter += NumItemsPerRow;

                if (itemCounter < NumItemsPerField && (i + 1) != rowContents.Count) continue;

                fields.Add(new EmbedFieldBuilder()
                {
                    Name = "\u200b",
                    IsInline = false,
                    Value = sb.ToString().Trim(),
                });

                sb.Clear();
                itemCounter = 0;
            }

            if (fields.Count > 0)
            {
                fields[0].Name = $"Page {PageNumber + 1} of {NumPages}";
            }

            return fields;
        }

        private static string IntToLetters(int value)
        {
            var result = string.Empty;
            while (value > 0)
            {
                value--;
                var letter = (char)('A' + value % 26);
                result = letter + result;
                value /= 26;
            }

            return result;
        }
    }
}
