using CannoliKit.Models;

namespace CannoliKit.Enums
{
    /// <summary>
    /// Specifies a Cannoli Route type.
    /// </summary>
    /// <seealso cref="CannoliRoute"/>
    public enum RouteType
    {
        /// <summary>
        /// A route for Discord message component interactions.
        /// </summary>
        MessageComponent = 1,

        /// <summary>
        /// A route for Discord modal interactions.
        /// </summary>
        Modal = 2,
    }
}
