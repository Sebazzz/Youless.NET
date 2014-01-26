namespace YoulessNet.Internal {
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Raw parsing helper struct for usage data of <see cref="YoulessUsageData"/>
    /// </summary>
    [DataContract]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Instantiated by SimpleJson deserializer")]
    internal class RawYoulessUsageData : YoulessUsageData, ITranslateable {
        /// <summary>
        /// Gets the raw unit (either "kWh" or "watt")
        /// </summary>
        [DataMember(Name = "un")]
        public string RawUnit { get; set; }

        /// <summary>
        /// Gets the start time of measurement for the current block of usage data. Format: 2014-01-26T00:00:00
        /// </summary>
        [DataMember(Name = "tm")]
        public string RawStartTime { get; set; }

        /// <summary>
        /// Gets the difference in seconds for each entry in the current block of usage data
        /// </summary>
        [DataMember(Name = "dt")]
        public int RawDifference { get; set; }

        /// <summary>
        /// Gets the measurements in the current block of usage data. The array is terminated with a 'null' value.
        /// </summary>
        [DataMember(Name="val")]
        public string[] RawMeasurements { get; set; }


        /// <summary>
        ///     Translates the current raw properties in something useful
        /// </summary>
        /// <exception cref="YoulessDataFormatException">Thrown when the data was in a unexpected format</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StartTimeStamp")]
        public void Translate() {
            NumberFormatInfo commaDecimalFormatInfo = new NumberFormatInfo();
            commaDecimalFormatInfo.NumberDecimalSeparator = ",";

            // parse unit
            if (!String.IsNullOrEmpty(this.RawUnit)) {
                if (String.Equals(this.RawUnit, "kwh", StringComparison.OrdinalIgnoreCase)) {
                    this.Unit = UsageUnit.KilowattHour;
                } else if (String.Equals(this.RawUnit, "watt", StringComparison.OrdinalIgnoreCase)) {
                    this.Unit = UsageUnit.Watt;
                }
            }

            // parse start time
            DateTime startDate;
            if (!DateTime.TryParseExact(this.RawStartTime, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal,
                    out startDate)) {
                throw new YoulessDataFormatException("Could not parse '"+this.RawStartTime+"' as 'StartTimeStamp'");  
            }
            this.StartTimestamp = startDate;

            // parse raw measurements
            DateTime timeStamp = startDate;

            YoulessUsage[] measurements = new YoulessUsage[this.RawMeasurements.Length-1]; // youless array is null terminated
            for (int i = 0; i < measurements.Length; i++) {
                string youlessMeasurement = this.RawMeasurements[i];

                // parse value
                int value;
                if (!Int32.TryParse(youlessMeasurement,
                    NumberStyles.AllowThousands | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite,
                    commaDecimalFormatInfo,
                    out value)) {
                    throw new YoulessDataFormatException("Couldn't parse '"+youlessMeasurement+"' as value for measurement");
                }

                measurements[i] = new YoulessUsage(value, timeStamp);
                timeStamp = timeStamp.AddSeconds(this.RawDifference);
            }

            this.Measurements = new ReadOnlyCollection<YoulessUsage>(measurements);
        }
    }
}