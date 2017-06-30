namespace YoulessNet {
    using System;
    using System.Runtime.Serialization;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an exception that occurs if authentication to the Youless API fails
    /// </summary>
    public class YoulessAuthenticationException : YoulessException {
        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessException"/> class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public YoulessAuthenticationException(string message, Exception innerException) : base(message, innerException) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessException"/> class
        /// </summary>
        /// <param name="message"></param>
        public YoulessAuthenticationException(string message) : base(message) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessException"/> class
        /// </summary>
        public YoulessAuthenticationException() { }
    }
}