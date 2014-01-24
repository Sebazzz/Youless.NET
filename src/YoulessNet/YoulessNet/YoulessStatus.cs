namespace YoulessNet {
    using System.Runtime.Serialization;

    /// <summary>
    ///     Represents the current status of the Youless energy meter
    /// </summary>
    [DataContract]
    public class YoulessStatus {
        /// <summary>
        ///     Gets the total amount of energy used (in <c>kWh</c>)
        /// </summary>
        [IgnoreDataMember]
        public double TotalCounter { get; set; }

        /// <summary>
        ///     Gets the current power usage (in <c>W</c>)
        /// </summary>
        [DataMember(Name = "pwr")]
        public int CurrentUsage { get; set; }

        /// <summary>
        ///     Gets the luminosity of the reflection (in case of analog meters)
        /// </summary>
        [DataMember(Name = "lvl")]
        public int? MovingAverageLevel { get; set; }

        /// <summary>
        /// Gets the difference in percents of the <see cref="MovingAverageLevel"/>
        /// </summary>
        [IgnoreDataMember]
        public int? ReflectionDeviation { get; set; }

        /// <summary>
        ///     Gets the time in seconds until the next online status update
        /// </summary>
        [IgnoreDataMember]
        public int NextOnlineStatusUpdate { get; set; }

        /// <summary>
        ///     Gets the connection status of the youless
        /// </summary>
        [IgnoreDataMember]
        public YoulessConnectionStatus ConnectionStatus { get; set; }
    }
}