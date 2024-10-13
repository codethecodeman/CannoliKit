using CannoliKit.Modules.States;
using Discord;

namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// Handles pagination settings for a Cannoli Module.
    /// </summary>
    public sealed class Pagination
    {
        internal bool IsEnabled { get; private set; }

        internal int NumPages { get; set; }

        internal string? PaginationId { get; set; }

        internal CannoliModuleState State { get; set; }

        /// <summary>
        /// Label to display on the previous arrow button. Default value is Left Arrow emoji.
        /// </summary>
        public Emoji PreviousArrowEmoji { get; set; } = new("⬅️");

        /// <summary>
        /// Label to display on the next arrow button. Default value is Right Arrow emoji.
        /// </summary>
        public Emoji NextArrowEmoji { get; set; } = new("➡️");

        private const string DefaultPaginationId = "CannoliKit.Default";

        internal Pagination(CannoliModuleState state)
        {
            State = state;
        }

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
        /// <param name="paginationId">If specified, the current page number will be saved to the module state using this ID. Otherwise, a default ID is used.</param>
        /// <param name="pageNumberOverride">If specified, overrides the saved page number.</param>
        /// <returns></returns>
        public PaginationResult<T> Setup<T>(
            IEnumerable<T> items,
            Func<ListItem<T>, string> formatter,
            ListType listType = ListType.Bullet,
            int numItemsPerRow = 1,
            int numItemsPerPage = 10,
            int numItemsPerField = 10,
            bool resetListCounterBetweenPages = false,
            string? paginationId = null,
            int? pageNumberOverride = null
        )
        {
            if (IsEnabled)
            {
                throw new InvalidOperationException(
                    "Pagination is already set up.");
            }

            IsEnabled = true;
            paginationId ??= DefaultPaginationId;

            if (State.PageNumbers.TryGetValue(paginationId, out var currentPageNumber) == false)
            {
                currentPageNumber = 0;
                State.PageNumbers[paginationId] = currentPageNumber;
            }

            if (pageNumberOverride != null)
            {
                currentPageNumber = pageNumberOverride.Value;
                State.PageNumbers[paginationId] = pageNumberOverride.Value;
            }

            var result = new PaginationResult<T>(
                items,
                formatter,
                listType,
                numItemsPerRow,
                numItemsPerPage,
                numItemsPerField,
                resetListCounterBetweenPages,
                currentPageNumber);

            State.PageNumbers[paginationId] = result.PageNumber;
            NumPages = result.NumPages;
            PaginationId = paginationId;

            return result;
        }
    }
}
