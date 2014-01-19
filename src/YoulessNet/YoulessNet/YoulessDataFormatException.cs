namespace YoulessNet {
    using System;

    /// <summary>
    /// Represents an unexpected error in the data format of the Youless API
    /// </summary>
    public class YoulessDataFormatException : Exception {
        /// <summary>
        /// 
        /// </summary>
        public YoulessDataFormatException() {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public YoulessDataFormatException(string message, Exception innerException) : base(message, innerException) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public YoulessDataFormatException(string message) : base(message) {}
    }
}