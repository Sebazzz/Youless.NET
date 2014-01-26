namespace YoulessNet {
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Represents a point of measurement in usage data
    /// </summary>
    public struct YoulessUsage : IEquatable<YoulessUsage> {
        private readonly DateTime _timestamp;
        private readonly int _usage;

        /// <summary>
        /// Gets the timestamp of this measure point
        /// </summary>
        public DateTime Timestamp {
            [DebuggerStepThrough] get { return this._timestamp; }
        }

        /// <summary>
        /// Gets the usage of power
        /// </summary>
        public int Usage {
            [DebuggerStepThrough] get { return this._usage; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public YoulessUsage(int usage, DateTime timestamp) {
            this._usage = usage;
            this._timestamp = timestamp;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "{1} @ {0:s}", this._timestamp, this._usage);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(YoulessUsage other) {
            return this._usage == other._usage && this._timestamp.Equals(other._timestamp);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is YoulessUsage && Equals((YoulessUsage) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() {
            unchecked {
                return (this._usage*397) ^ this._timestamp.GetHashCode();
            }
        }

        /// <summary>
        /// Compares boths structs for equality
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(YoulessUsage left, YoulessUsage right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares boths structs for non-equality
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(YoulessUsage left, YoulessUsage right) {
            return !left.Equals(right);
        }
    }
}