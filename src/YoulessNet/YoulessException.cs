namespace YoulessNet {
    using System;
    using System.Runtime.Serialization;
    using JetBrains.Annotations;

    /// <summary>
    ///     Represents an exception that occurs when an error occurs during a request to the Youless API
    /// </summary>
    public class YoulessException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessException"/> class
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public YoulessException(string message, Exception innerException) : base(message, innerException) {}
        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessException"/> class
        /// </summary>
        /// <param name="message"></param>
        public YoulessException(string message) : base(message) {}
        /// <summary>
        /// Initializes a new instance of the <see cref="YoulessException"/> class
        /// </summary>
        public YoulessException() {}
    }
}