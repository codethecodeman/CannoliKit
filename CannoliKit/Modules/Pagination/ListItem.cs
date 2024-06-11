﻿namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// A list item generated by <see cref="Pagination"/>.
    /// </summary>
    /// <typeparam name="TItem">Base item type.</typeparam>
    public sealed class ListItem<TItem>
    {
        /// <summary>
        /// Visual list marker. E.g. a number or letter.
        /// </summary>
        /// <seealso cref="ListType"/>
        public string Marker { get; set; }

        /// <summary>
        /// Base item.
        /// </summary>
        public TItem Item { get; }

        internal ListItem(string marker, TItem item)
        {
            Marker = marker;
            Item = item;
        }
    }
}
