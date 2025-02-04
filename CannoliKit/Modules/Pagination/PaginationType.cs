namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// Specified a pagination type.
    /// </summary>
    public enum PaginationType
    {
        /// <summary>
        /// Uses arrows unless the number of pages is greater than 5.
        /// </summary>
        Automatic,

        /// <summary>
        /// Uses arrows.
        /// </summary>
        Arrows,

        /// <summary>
        /// Uses a select menu.
        /// </summary>
        SelectMenu
    }
}
