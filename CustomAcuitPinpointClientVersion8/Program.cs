// Copyright (c) Acuit Development, Inc.
// THIS MATERIAL IS CONFIDENTIAL AND PROPRIETARY TO ACUIT DEVELOPMENT AND MAY NOT BE REPRODUCED, PUBLISHED OR DISCLOSED TO OTHERS WITHOUT COMPANY AUTHORIZATION.

using System;
using System.Collections.Specialized;
using Acuit.Pinpoint.Services.Client;

namespace CustomAcuitPinpointClientVersion8
{
    /// <summary>
    /// A simple example client that connects to Acuit Pinpoint Server to record test results.
    /// </summary>
    internal class Program
    {
        private static void Main()
        {
            // Initialize the Acuit Pinpoint connection and station settings. The following settings are the minimum ones that must be initialized, but there
            // are other options in SimpleStationOptions that can be customized. Note that because no worker badge number or password is specified, this example
            // assumes that the specified station type is configured for automatic worker logon in Acuit Pinpoint.
            var options = new SimpleStationOptions
            {
                PinpointServiceClient =
                {
                    PinpointHostName = "YOUR_SERVER_NAME_HERE" // The name of the server hosting Acuit Pinpoint. This will be the same for all stations in the plant.
                },
                LineName = "Line 1", // The line in Acuit Pinpoint where the station is located. This must be the name of a line configured in Acuit Pinpoint.
                StationTypeName = "Run Test", // The station type of this station. This must be the name of a station type configured for the line in Acuit Pinpoint.
                ClientVersion = "1.0.0" // Your client software version. This is required and is usually the version of your application or plug-in.
            };

            // A single instance of SimpleStation should be created and used for the lifetime of your application.
            var station = new SimpleStation(options);

            string unitSerialNumber = "1";
            string unitModelNumber = null; // We will assume that the unit record should already exist in Acuit Pinpoint, so the model number should be null.
            string testName = "Run Test"; // This must be the name of a test type configured for the line in Acuit Pinpoint.

            // A failed test could be recorded like this:
            try
            {
                // We'll use a simple dictionary to gather custom test data items to submit with the test result. Any number of items can be added.
                var testData = new OrderedDictionary
                {
                    { "ChargeAmount", 1.0 },
                    { "LeakFlow", 2.0 },
                    { "AlarmCode", "E0004" }
                };
                // Note that the above is syntactic sugar for:
                //var testData = new OrderedDictionary();
                //testData.Add("ChargeAmount", 1.0);
                //testData.Add("LeakFlow", 2.0);
                //testData.Add("AlarmCode", "E0004");
                station.AddTestResult(unitSerialNumber, unitModelNumber, testName, passed: false, "FAIL REASON", "Optional additional notes.", testData);
            }
            catch (Exception ex)
            {
                // Be sure to catch exceptions to handle connection problems or business logic faults.
                Console.WriteLine($"Error recording test result: {ex.Message}");
            }

            // A successful test could be recorded like this:
            try
            {
                var testData = new OrderedDictionary
                {
                    { "ChargeAmount", 1.0 },
                    { "LeakFlow", 0.0 }
                };
                station.AddTestResult(unitSerialNumber, unitModelNumber, testName, passed: true, reason: null, notes: null, testData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording test result: {ex.Message}");
            }

            // Note that there are asynchronous versions of these methods as well (e.g., AddTestResultAsync).
        }
    }
}
