using System;
using System.Collections.Generic;
using Acuit.Pinpoint.Client2;
using Acuit.Pinpoint.Server.Client;

namespace CustomAcuitPinpointClient
{
    /// <summary>
    /// A simple example client that connections to Acuit Pinpoint Server to record test results.
    /// </summary>
    /// <remarks>
    /// It assumes that the station type is configured for automatic worker logon, so worker badge number/passwords don't need to be used to log on/off.
    /// 
    /// Error handling isn't shown here, but try/catch should be used to watch for and handle things like Acuit Pinpoint Server connection problems
    /// or business logic faults.
    ///
    /// This example demonstrates calling the synchronous versions of the API methods. Note that there are asynchronous versions of all methods as well.
    /// </remarks>
    internal class Program
    {
        private static readonly PinpointClientOptions s_options = new PinpointClientOptions();

        private static StationSettings s_pinpointStationSettings;
        private static int? s_workerStationId;

        private static void Main()
        {
            // Retrieve the station settings at startup to get the auto-log-on worker identifier. This only needs to be done once.
            ClientHelper.Use(CreatePinpointClient(), pinpointClient =>
            {
                s_pinpointStationSettings = pinpointClient.GetStationSettings3(s_options.LineName, s_options.StationTypeName, s_options.StationName, s_options.ClientVersion);
                if (!s_pinpointStationSettings.AutoLogOnWorker)
                    throw new InvalidOperationException($"Station type \"{s_options.StationTypeName}\" is not configured for auto log on.");
            });

            // Record a unit scan
            int unitStationId = OnUnitScanned("1");

            // A failed test could be recorded like this:
            var testData = new List<CustomTestDataItem>
            {
                // Any number of test data items can be added here
                new CustomTestDataItem { Name = "ChargeAmount", Value = "1.0" }, // Values must be converted to strings
                new CustomTestDataItem { Name = "LeakFlow", Value = "2.0", Status = CustomTestDataStatus.Bad }, // Each item can optionally have Good or Bad specified, which will cause them to appear in green or red in Pinpoint
            };
            OnTestAttemptCompleted(unitStationId, passed: false, "FAIL REASON", "Optional additional notes.", testData);

            // A successful test could be recorded like this:
            testData = new List<CustomTestDataItem>
            {
                new CustomTestDataItem { Name = "ChargeAmount", Value = "1.0" },
                new CustomTestDataItem { Name = "LeakFlow", Value = "0.0", Status = CustomTestDataStatus.Good },
            };
            OnTestAttemptCompleted(unitStationId, passed: true, failReason: null, notes: null, testData);

            OnUnitReleased(unitStationId);

            unitStationId = OnUnitScanned("2");

            // And so on...
        }

        /// <summary>
        /// Creates a client used to communicate with Acuit Pinpoint Server.
        /// </summary>
        private static PinpointClient CreatePinpointClient()
        {
            if (string.IsNullOrEmpty(s_options.UserName))
                return new PinpointClient(s_options.PinpointServerName, s_options.UseTransportSecurity);
            else
                return new PinpointClient(s_options.PinpointServerName, s_options.Domain, s_options.UserName, s_options.Password);
        }

        /// <summary>
        /// This is what should happen when you start working on a unit.
        /// </summary>
        /// <param name="serialNumber">The unit serial number.</param>
        /// <returns>The unit station identifier of the unit scanned into the station.</returns>
        private static int OnUnitScanned(string serialNumber)
        {
            UnitScanStatus unitScanStatus = ClientHelper.Use(CreatePinpointClient(), pinpointClient =>
            {
                // Record the worker logon at the station if necessary, obtaining a worker-station identifier that we'll use below.
                if (!s_workerStationId.HasValue)
                {
                    Acuit.Pinpoint.Client2.WorkerLogOnStatus workerLogOnStatus = pinpointClient.WorkerLogOn2(s_options.LineName, s_options.StationTypeName, s_options.StationName, s_pinpointStationSettings.AutoLogOnWorkerId);
                    s_workerStationId = workerLogOnStatus.WorkerStationId;
                }

                // Record a unit scan for a new unit, obtaining a unit-station identifier and other unit information.
                // The model number should be left blank, since the unit record should already exist in Pinpoint.
                return pinpointClient.UnitScanned4(s_options.LineName, s_workerStationId.Value, serialNumber, string.Empty);
            });

            // Update our worker-station identifier if it changes due to a shift change.
            if (unitScanStatus.WorkerLogOnStatus != null)
                s_workerStationId = unitScanStatus.WorkerLogOnStatus.WorkerStationId;

            string modelNumber = unitScanStatus.Unit.ModelNumber; // This is the unit model number, which is usually the actual model number (i.e., the model number on the box), which might be a trade-branded model number.
            string modelNumberAlias = unitScanStatus.Unit.ModelNumberAlias; // This is the unit model number alias, which is usually the internal equivalent model number and should normally be the one used to do things like look up test parameters.

            // If there are any unit workflow errors, they should be displayed to the operator and should normally prevent the unit from being tested.
            foreach (Acuit.Pinpoint.Client2.WorkflowError workflowError in unitScanStatus.WorkflowErrors)
                Console.WriteLine($"Error: {workflowError.Message}");

            // Return the unit-station identifier, which will be needed to record test results.
            return unitScanStatus.UnitStation.UnitStationId;
        }

        /// <summary>
        /// This is what should happen every time a passed or failed test completes, to record the result.
        /// </summary>
        /// <param name="unitStationId">Ths unit-station identifier, from the last unit scan.</param>
        /// <param name="passed">Whether the test passed or failed.</param>
        /// <param name="failReason">An optional short failure reason (max 50 characters).</param>
        /// <param name="notes">Optional additional notes (max 100 characters).</param>
        /// <param name="testData">Optional test data.</param>
        private static void OnTestAttemptCompleted(int unitStationId, bool passed, string failReason, string notes, List<CustomTestDataItem> testData)
        {
            ClientHelper.Use(CreatePinpointClient(), pinpointClient =>
            {
                _ = pinpointClient.AddTestResult2(s_options.LineName, unitStationId, s_options.TestTypeName, passed, failReason, notes, UnitTest.EncodeTestDataAsXml(testData));
            });
        }

        /// <summary>
        /// This is what should happen when a unit is released from the station.
        /// </summary>
        /// <param name="unitStationId">Ths unit-station identifier, from the last unit scan.</param>
        private static void OnUnitReleased(int unitStationId)
        {
            // Can optionally inform Pinpoint that the unit is being released from the station; not strictly necessary.
            ClientHelper.Use(CreatePinpointClient(), pinpointClient =>
            {
                pinpointClient.UnitReleased3(s_options.LineName, unitStationId);
            });
        }
    }
}
