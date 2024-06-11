using System;

namespace CustomAcuitPinpointClient
{
    /// <summary>
    /// Acuit Pinpoint client options.
    /// </summary>
    /// <remarks>
    /// These are settings that should typically be configurable.
    /// </remarks>
    internal class PinpointClientOptions
    {
        /// <summary>
        /// Gets or sets the name of the local Acuit Pinpoint Server instance. This will be the same for all stations in the plant.
        /// </summary>
        public string PinpointServerName { get; set; } = "TODO";

        /// <summary>
        /// Gets or sets whether to use transport security. If this is <see langword="true"/>, Transport Layer Security (TLS) over TCP is used, with the client's credentials validated via Windows.
        /// </summary>
        /// <remarks>
        /// This can be used when a user on the same domain as the Acuit Pinpoint Server is logged into Windows on the client PC.
        /// </remarks>
        public bool UseTransportSecurity { get; set; } = true;

        // If the PC is not on the Rheem domain with a domain user logged into Windows, will need these settings:

        /// <summary>
        /// Gets or sets the domain name to use when connecting using explicit credentials.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the user name to use when connecting using explicit credentials.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password to use when connecting using explicit credentials.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the name of the line in Acuit Pinpoint where the station is located. This must match a line name configured in Acuit Pinpoint.
        /// </summary>
        public string LineName { get; set; } = "AC1 Line";

        /// <summary>
        /// Gets or sets the station type name for this station. This must match a station type configured for the line in Acuit Pinpoint.
        /// </summary>
        public string StationTypeName { get; set; } = "Run Test";

        /// <summary>
        /// Gets or sets the station name. This can be anything, but usually the PC name is used.
        /// </summary>
        public string StationName { get; set; } = Environment.MachineName;

        /// <summary>
        /// Gets or sets the client software version. This can be anything, but should usually be the version of your application or plug-in.
        /// </summary>
        public string ClientVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the test type name. This must match a test type configured for the line in Acuit Pinpoint.
        /// </summary>
        public string TestTypeName { get; set; } = "Run Test";
    }
}
