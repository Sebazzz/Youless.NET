namespace YoulessNet {
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a <see cref="YoulessStatus"/> with some raw properties attached
    /// </summary>
    [DataContract]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "This class is instantiated via Reflection through SimpleJson")]
    internal class RawYoulessStatus : YoulessStatus, ITranslateable {
        /// <summary>
        /// Gets the raw connection status
        /// </summary>
        /// <remarks>
        /// Either 'OK' or something else.
        /// </remarks>
        [DataMember(Name = "conn")]
        public string RawConnectionStatus { get; set; }

        /// <summary>
        /// Gets the raw total counter
        /// </summary>
        /// <remarks>
        /// For some reason this is a JSON string, using a comma as decimal seperator
        /// </remarks>
        [DataMember(Name = "cnt")]
        public string RawTotalCounter { get; set; }

        /// <summary>
        /// Gets the raw deviation
        /// </summary>
        /// <remarks>
        /// Can be empty or contain some HTML like: (&amp;plusmn;1%)
        /// </remarks>
        [DataMember(Name = "dev")]
        public string RawDeviation { get; set; }

        /// <summary>
        /// Gets the value to the next online update, in case the Youless is connected
        /// </summary>
        /// <remarks>
        /// The value is surrounded by '(' and ')'
        /// </remarks>
        [DataMember(Name = "sts")]
        public string RawNextOnlineUpdate { get; set; }

        /// <summary>
        /// Causes the raw data to be transformed in something useful
        /// </summary>
        /// <exception cref="YoulessDataFormatException"></exception>
        public void Translate() {
            NumberFormatInfo commaDecimalFormatInfo = new NumberFormatInfo();
            commaDecimalFormatInfo.NumberDecimalSeparator = ",";

            // correct level
            if (this.MovingAverageLevel == 0) {
                this.MovingAverageLevel = null;
            }

            // parse connection status
            if (!String.IsNullOrEmpty(this.RawConnectionStatus)) {
                this.ConnectionStatus = String.Equals("OK", this.RawConnectionStatus, StringComparison.OrdinalIgnoreCase) ?
                    YoulessConnectionStatus.Success : YoulessConnectionStatus.Failure;
            }

            // parse total counter
            double totalCounter;
            if (!Double.TryParse(this.RawTotalCounter, NumberStyles.AllowDecimalPoint, commaDecimalFormatInfo, out totalCounter)) {
                throw new YoulessDataFormatException("Couldn't parse value '"+this.RawTotalCounter+"' as a double for 'Total Counter'");
            }

            this.TotalCounter = totalCounter;

            // parse dev
            if (!String.IsNullOrEmpty(this.RawDeviation)) {
                string dev = this.RawDeviation.Trim('(', ')');
                dev = dev.Replace("&plusmn;", String.Empty);

                int idev;
                if (!Int32.TryParse(dev, NumberStyles.Integer, CultureInfo.InvariantCulture, out idev)) {
                    throw new YoulessDataFormatException("Couldn't parse value '"+dev+"' as a integer for 'Deviation'");
                }

                this.ReflectionDeviation = idev;
            }

            // parse connection counter
            if (!String.IsNullOrEmpty(this.RawNextOnlineUpdate)) {
                string upd = this.RawDeviation.Trim('(', ')');

                int iupd;
                if (!Int32.TryParse(upd, NumberStyles.Integer, CultureInfo.InvariantCulture, out iupd)) {
                    throw new YoulessDataFormatException("Couldn't parse value '" + upd + "' as a integer for 'Next Online Status Update'");
                }

                this.NextOnlineStatusUpdate = iupd;
            }
        }
    }
}