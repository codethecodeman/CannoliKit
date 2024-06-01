namespace CannoliKit.Enums
{
    public enum DeferralType
    {
        /// <summary>
        /// Request will not be deferred.
        /// </summary>
        None,

        /// <summary>
        /// Request will be immediately deferred
        /// with the ephemeral flag enabled.
        /// </summary>
        Ephemeral,

        /// <summary>
        /// Request will be immediately deferred
        /// with the ephemeral flag disabled.
        /// </summary>
        Public
    }
}
