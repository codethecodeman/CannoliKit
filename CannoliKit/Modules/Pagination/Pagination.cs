using Discord;
using System.Text;

namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// Handles pagination settings for a Cannoli Module. Also provides pagination utilities.
    /// </summary>
    public sealed class Pagination
    {
        /// <summary>
        /// Label to display on the previous arrow button. Default value is Left Arrow emoji.
        /// </summary>
        public Emoji PreviousArrowEmoji { get; set; } = new("⬅️");

        /// <summary>
        /// Label to display on the next arrow button. Default value is Right Arrow emoji.
        /// </summary>
        public Emoji NextArrowEmoji { get; set; } = new("➡️");

        /// <summary>
        /// Indicates if pagination is enabled. Default value is false.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Number of items to display per Discord embed field. Default value is 10.
        /// </summary>
        public int NumItemsPerField { get; set; } = 10;

        /// <summary>
        /// Number of items to display per page. Default value is 10.
        /// </summary>
        public int NumItemsPerPage { get; set; } = 10;

        /// <summary>
        /// Number of items to display per row. Default value is 1.
        /// </summary>
        public int NumItemsPerRow { get; set; } = 1;

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

        private int _listStartIndex;
        private bool _isSetup;

        internal Pagination() { }

        /// <summary>
        /// Sets the item count to be handled by pagination. This must be set before calling other pagination methods.
        /// </summary>
        /// <param name="itemCount"></param>
        public void SetItemCount(int itemCount)
        {
            NumItems = itemCount;
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

            _listStartIndex = NumItemsPerPage * PageNumber;

            _isSetup = true;
        }

        /// <summary>
        /// Given an overall list of items, get a <see cref="ListItem{TItem}"/> list for the current page.
        /// </summary>
        /// <typeparam name="TItem">Base item type.</typeparam>
        /// <param name="items">List of all items.</param>
        /// <param name="listType">List type.</param>
        /// <param name="resetListCounterBetweenPages">In the case of a numbered list, indicates if the numbering should reset with each page.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public List<ListItem<TItem>> GetListItems<TItem>(List<TItem> items, ListType? listType = null, bool resetListCounterBetweenPages = false)
        {
            EnsureSettingsExist();

            var pagedItems = new List<ListItem<TItem>>();

            for (var i = _listStartIndex; i < _listStartIndex + NumItemsPerPage; i++)
            {
                if (i + 1 > items.Count) break;

                var listIndex = resetListCounterBetweenPages ? i + 1 - _listStartIndex : i + 1;

                var marker = listType switch
                {
                    ListType.Number => $"{listIndex}.",
                    ListType.Letter => $"{IntToLetters(listIndex + 1)}.",
                    ListType.Bullet => "-",
                    null => string.Empty,
                    _ => throw new ArgumentOutOfRangeException(nameof(listType), listType, null)
                };

                pagedItems.Add(
                    new ListItem<TItem>(marker, items[i]));
            }

            if (items.Count <= 0) return pagedItems;

            var maxMarkerLength = pagedItems
                .Max(x => x.Marker.Length);

            foreach (var item in pagedItems)
            {
                if (item.Marker.Length == maxMarkerLength) continue;

                item.Marker = item.Marker.PadLeft(maxMarkerLength);
            }

            return pagedItems;
        }

        /// <summary>
        /// Get an <see cref="EmbedFieldBuilder"/> list which visually represents the provided items.
        /// </summary>
        /// <param name="items">List of items represented as a string.</param>
        /// <returns><see cref="EmbedFieldBuilder"/> list.</returns>
        public List<EmbedFieldBuilder> GetEmbedFieldBuilders(IList<string> items)
        {
            EnsureSettingsExist();

            var numRows = (int)Math.Ceiling((double)items.Count / NumItemsPerRow);
            var rowContents = new List<string>();
            var sb = new StringBuilder();

            for (var row = 0; row < numRows; row++)
            {
                for (var col = 0; col < NumItemsPerRow; col++)
                {
                    var index = col * numRows + row;
                    if (index >= items.Count) break;
                    sb.Append(items[index]);
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

        private void EnsureSettingsExist()
        {
            if (IsEnabled == false)
            {
                throw new InvalidOperationException(
                    "Pagination must be enabled prior to using this feature.");
            }

            if (_isSetup == false)
            {
                throw new InvalidOperationException(
                    "Pagination item count must be set prior to using this feature.");
            }
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
