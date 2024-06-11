namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// Specifies a list type.
    /// </summary>
    public enum ListType
    {
        /// <summary>
        /// Each list item has an associated number.
        /// </summary>
        Number,

        /// <summary>
        /// Each list item has an associated letter. Letters beyond Z will convert to Excel style letters, e.g. AA, AB, AC, etc.
        /// </summary>
        Letter,

        /// <summary>
        /// Each list item has a standard circle bullet.
        /// </summary>
        Bullet,
    }
}
