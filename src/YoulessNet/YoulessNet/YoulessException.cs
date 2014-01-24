namespace YoulessNet {
    using System;

    /// <summary>
    ///     Represents an exception that occurs when an error occurs during a request to the Youless API
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public class YoulessException : Exception {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public YoulessException(string message, Exception innerException) : base(message, innerException) {}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public YoulessException(string message) : base(message) {}
        /// <summary>
        /// 
        /// </summary>
        public YoulessException() {}
    }
}