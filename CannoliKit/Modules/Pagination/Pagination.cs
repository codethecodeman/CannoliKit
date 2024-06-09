using Discord;
using System.Text;

namespace CannoliKit.Modules.Pagination
{
    public sealed class Pagination
    {
        public bool IsEnabled { get; set; }
        public int NumItemsPerField { get; set; } = 10;
        public int NumItemsPerPage { get; set; } = 10;
        public int NumItemsPerRow { get; set; } = 1;
        public int NumItems { get; private set; }
        public int NumPages { get; private set; }
        public int PageNumber { get; internal set; }
        private int ListStartIndex { get; set; }

        private bool _isSetup;

        internal Pagination() { }

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

            ListStartIndex = NumItemsPerPage * PageNumber;

            _isSetup = true;
        }

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

        public List<ListItem<TItem>> GetListItems<TItem>(List<TItem> items, ListType? listType = null, bool resetListCounterBetweenPages = false)
        {
            EnsureSettingsExist();

            var pagedItems = new List<ListItem<TItem>>();

            for (var i = ListStartIndex; i < ListStartIndex + NumItemsPerPage; i++)
            {
                if (i + 1 > items.Count) break;

                var listIndex = resetListCounterBetweenPages ? i + 1 - ListStartIndex : i + 1;

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

        private void EnsureSettingsExist()
        {
            if (IsEnabled == false)
            {
                throw new InvalidOperationException(
                    "Pagination must be enabled prior to using this feature");
            }

            if (_isSetup == false)
            {
                throw new InvalidOperationException(
                    "Pagination item count must be set prior to using this feature");
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
