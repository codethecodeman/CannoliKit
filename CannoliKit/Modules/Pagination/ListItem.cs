﻿namespace CannoliKit.Modules.Pagination
{
    /// <summary>
    /// A list item generated by <see cref="Pagination"/>.
    /// </summary>
    /// <typeparam name="T">Base item type.</typeparam>
    public sealed class ListItem<T>
    {
        /// <summary>
        /// Visual list marker. E.g. a number or letter.
        /// </summary>
        /// <seealso cref="ListType"/>
        public string Marker { get; set; }

        /// <summary>
        /// Base item.
        /// </summary>
        public T Item { get; }

        internal List<ListItem<T>> Items { get; set; } = null!;

        /// <summary>
        /// Finds the max string length by evaluating all list items in the page.
        /// Useful as a mix-in when specifying a pagination formatter, so that you may pad quoted strings.
        /// </summary>
        /// <param name="itemlengthFunc">Function used to evaluate all list items in the page.</param>
        /// <returns>Max string length after evaluating all list items.</returns>
        public int MaxLengthOf(Func<T, string> itemlengthFunc)
        {
            var maxLength = 0;

            foreach (var item in Items)
            {
                var length = itemlengthFunc(item.Item).Length;
                if (length > maxLength) maxLength = length;
            }

            return maxLength;
        }

        internal ListItem(string marker, T item)
        {
            Marker = marker;
            Item = item;
        }
    }
}
