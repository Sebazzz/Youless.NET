namespace YoulessNet {
    /// <summary>
    /// Enumerates the connection statuses of the Youless
    /// </summary>
    public enum YoulessConnectionStatus {
        /// <summary>
        /// Unknown connection status (string empty or null)
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The Youless has a connection to the online service
        /// </summary>
        Success,

        /// <summary>
        /// The youless is not connected to an online service
        /// </summary>
        Failure,

    }
}