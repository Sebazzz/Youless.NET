namespace YoulessNet {
    using System;

    /// <summary>
    /// Represents an exception that occurs if authentication to the Youless API fails
    /// </summary>
    public class YoulessAuthenticationException : YoulessException {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public YoulessAuthenticationException(string message, Exception innerException) : base(message, innerException) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public YoulessAuthenticationException(string message) : base(message) {}

        /// <summary>
        /// 
        /// </summary>
        public YoulessAuthenticationException() {}
    }
}