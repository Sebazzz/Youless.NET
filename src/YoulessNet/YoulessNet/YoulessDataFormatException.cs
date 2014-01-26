namespace YoulessNet {
    using System;
    using System.Runtime.Serialization;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents an unexpected error in the data format of the Youless API
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public class YoulessDataFormatException : YoulessException {
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

#if !NETFX_CORE
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception><exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        protected YoulessDataFormatException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {}
#endif
    }
}