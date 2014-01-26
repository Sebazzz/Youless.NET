namespace YoulessNet {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a block of measurement in usage data
    /// </summary>
    public class YoulessUsageData {
        /// <summary>
        /// Gets or sets the start date and time in measurement of usage data
        /// </summary>
        public DateTime StartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the unit the data is measured in
        /// </summary>
        public UsageUnit Unit { get; set; }

        /// <summary>
        /// Gets or sets an array of the measurements taken
        /// </summary>
        public ReadOnlyCollection<YoulessUsage> Measurements { get; internal set; }

        /// <summary>
        /// Merges the <paramref name="other"/> instance with the current instance
        /// </summary>
        /// <param name="other"></param>
        internal YoulessUsageData MergeWith(YoulessUsageData other) {

            YoulessUsageData mergedInstance = new YoulessUsageData();

            // check for unit
            if (this.Unit != other.Unit) {
                throw new InvalidOperationException("Cannot merge instances with different units");
            }

            // determine which measurements to take first
            YoulessUsage[] measurements = new YoulessUsage[other.Measurements.Count + this.Measurements.Count];

            ReadOnlyCollection<YoulessUsage> firstMeasurements = this.StartTimestamp > other.StartTimestamp ? other.Measurements : this.Measurements;
            ReadOnlyCollection<YoulessUsage> secondMeasurements = this.StartTimestamp > other.StartTimestamp ? this.Measurements : other.Measurements;

            for (int i = 0; i < measurements.Length; i++) {
                if (i < firstMeasurements.Count) {
                    measurements[i] = firstMeasurements[i];
                } else {
                    int ptr = i - firstMeasurements.Count;
                    measurements[i] = secondMeasurements[ptr];
                }
            }

            mergedInstance.Measurements = new ReadOnlyCollection<YoulessUsage>(measurements);

            // set other props
            mergedInstance.StartTimestamp = this.StartTimestamp > other.StartTimestamp
                ? other.StartTimestamp
                : this.StartTimestamp;

            mergedInstance.Unit = this.Unit;

            return mergedInstance;
        }
    }
}