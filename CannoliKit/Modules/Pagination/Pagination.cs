using Discord;

namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// Handles pagination settings for a Cannoli Module.
    /// </summary>
    public sealed class Pagination
    {
        internal bool IsEnabled { get; private set; }
        internal int PageNumber { get; set; }
        internal int NumPages { get; set; }

        /// <summary>
        /// Label to display on the previous arrow button. Default value is Left Arrow emoji.
        /// </summary>
        public Emoji PreviousArrowEmoji { get; set; } = new("⬅️");

        /// <summary>
        /// Label to display on the next arrow button. Default value is Right Arrow emoji.
        /// </summary>
        public Emoji NextArrowEmoji { get; set; } = new("➡️");

        internal Pagination() { }

        /// <summary>
        /// Setup pagination for this module.
        /// </summary>
        /// <typeparam name="T">Type to be used for pagination.</typeparam>
        /// <param name="items">List of items to paginate.</param>
        /// <param name="formatter">Formatter to display list items in the Discord embed.</param>
        /// <param name="listType">List type. Default is Bullet.</param>
        /// <param name="numItemsPerRow">Number of items to display per row. Default value is 1.</param>
        /// <param name="numItemsPerPage">Number of items to display per page. Default value is 10.</param>
        /// <param name="numItemsPerField">Number of items to display per Discord embed field. Default value is 10.</param>
        /// <param name="resetListCounterBetweenPages">If list type is numbered, indicates if numbering should reset to 1 on each page.</param>
        /// <returns></returns>
        public PaginationResult<T> Setup<T>(
            IList<T> items,
            Func<ListItem<T>, string> formatter,
            ListType listType = ListType.Bullet,
            int numItemsPerRow = 1,
            int numItemsPerPage = 10,
            int numItemsPerField = 10,
            bool resetListCounterBetweenPages = false
        )
        {
            IsEnabled = true;

            var result = new PaginationResult<T>(
                items,
                formatter,
                listType,
                numItemsPerRow,
                numItemsPerPage,
                numItemsPerField,
                resetListCounterBetweenPages,
                PageNumber);

            NumPages = result.NumPages;

            return result;
        }
    }
}
