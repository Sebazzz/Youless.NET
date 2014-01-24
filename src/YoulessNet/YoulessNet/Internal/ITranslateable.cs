namespace YoulessNet {
    /// <summary>
    ///     Represents a class that can translate its raw properties in something useful
    /// </summary>
    internal interface ITranslateable {
        /// <summary>
        ///     Translates the current raw properties in something useful
        /// </summary>
        /// <exception cref="YoulessDataFormatException">Thrown when the data was in a unexpected format</exception>
        void Translate();
    }
}