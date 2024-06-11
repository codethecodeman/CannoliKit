namespace CannoliKit.Enums
{
    /// <summary>
    /// Specifies a deferral type to be used with incoming Discord interactions.
    /// </summary>
    public enum DeferralType
    {
        /// <summary>
        /// Interaction will not be deferred.
        /// </summary>
        None,

        /// <summary>
        /// Interaction will be immediately deferred
        /// with the ephemeral flag enabled.
        /// </summary>
        Ephemeral,

        /// <summary>
        /// Interaction will be immediately deferred
        /// with the ephemeral flag disabled.
        /// </summary>
        Public
    }
}
