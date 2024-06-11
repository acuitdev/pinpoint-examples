Imports Acuit.Pinpoint.Client2
Imports Acuit.Pinpoint.Server.Client

''' <summary>
''' A simple example client that connections to Acuit Pinpoint Server to record test results.
''' </summary>
''' <remarks>
''' It assumes that the station type is configured for automatic worker logon, so worker badge number/passwords don't need to be used to log on/off.
'''
''' Error handling isn't shown here, but try/catch should be used to watch for and handle things like Acuit Pinpoint Server connection problems
''' or business logic faults.
'''
''' This example demonstrates calling the synchronous versions of the API methods. Note that there are asynchronous versions of all methods as well.
''' </remarks>
Friend Module Module1

    Private ReadOnly _options As New PinpointClientOptions()
    Private _pinpointStationSettings As StationSettings
    Private _workerStationId As Integer?

    Public Sub Main()

        ' Retrieve the station settings at startup to get the auto-log-on worker identifier. This only needs to be done once.
        ClientHelper.Use(CreatePinpointClient(),
                         Sub(pinpointClient)
                             _pinpointStationSettings = pinpointClient.GetStationSettings3(_options.LineName, _options.StationTypeName, _options.StationName, _options.ClientVersion)
                             If Not _pinpointStationSettings.AutoLogOnWorker Then Throw New InvalidOperationException("The station type " + _options.StationTypeName + " is not configured for auto log on.")
                         End Sub)

        ' Record a unit scan
        Dim unitStationId As Integer = OnUnitScanned("1")

        ' A failed test could be recorded like this:
        Dim testData = New List(Of CustomTestDataItem) From { ' Any number of test data items can be added here
                New CustomTestDataItem With {
                    .Name = "ChargeAmount",
                    .Value = "1.0" ' Values must be converted to strings
                },
                New CustomTestDataItem With {
                    .Name = "LeakFlow",
                    .Value = "2.0",
                    .Status = CustomTestDataStatus.Bad ' Each item can optionally have Good or Bad specified, which will cause them to appear in green or red in Pinpoint
                }
            }
        OnTestAttemptCompleted(unitStationId, passed:=False, "FAIL REASON", "Optional additional notes.", testData)

        ' A successful test could be recorded like this:
        testData = New List(Of CustomTestDataItem) From {
                New CustomTestDataItem With {
                    .Name = "ChargeAmount",
                    .Value = "1.0"
                },
                New CustomTestDataItem With {
                    .Name = "LeakFlow",
                    .Value = "0.0",
                    .Status = CustomTestDataStatus.Good
                }
            }
        OnTestAttemptCompleted(unitStationId, passed:=True, failReason:=Nothing, notes:=Nothing, testData)

        OnUnitReleased(unitStationId)

        unitStationId = OnUnitScanned("2")

        ' And so on...

    End Sub

    ''' <summary>
    ''' Creates a client used to communicate with Acuit Pinpoint Server.
    ''' </summary>
    Private Function CreatePinpointClient() As PinpointClient
        If String.IsNullOrEmpty(_options.UserName) Then
            Return New PinpointClient(_options.PinpointServerName, _options.UseTransportSecurity)
        Else
            Return New PinpointClient(_options.PinpointServerName, _options.Domain, _options.UserName, _options.Password)
        End If
    End Function

    ''' <summary>
    ''' This is what should happen when you start working on a unit.
    ''' </summary>
    ''' <param name="serialNumber">The unit serial number.</param>
    ''' <returns>The unit station identifier of the unit scanned into the station.</returns>
    Private Function OnUnitScanned(serialNumber As String) As Integer
        Dim unitScanStatus As UnitScanStatus =
            ClientHelper.Use(CreatePinpointClient(),
                             Function(pinpointClient)

                                 ' Record the worker logon at the station if necessary, obtaining a worker-station identifier that we'll use below.
                                 If Not _workerStationId.HasValue Then
                                     Dim workerLogOnStatus As Acuit.Pinpoint.Client2.WorkerLogOnStatus = pinpointClient.WorkerLogOn2(_options.LineName, _options.StationTypeName, _options.StationName, _pinpointStationSettings.AutoLogOnWorkerId)
                                     _workerStationId = workerLogOnStatus.WorkerStationId
                                 End If

                                 ' Record a unit scan for a new unit, obtaining a unit-station identifier and other unit information.
                                 ' The model number should be left blank, since the unit record should already exist in Pinpoint.
                                 Return pinpointClient.UnitScanned4(_options.LineName, _workerStationId.Value, serialNumber, String.Empty)
                             End Function)

        ' Update our worker-station identifier if it changes due to a shift change.
        If unitScanStatus.WorkerLogOnStatus IsNot Nothing Then _workerStationId = unitScanStatus.WorkerLogOnStatus.WorkerStationId

        Dim modelNumber As String = unitScanStatus.Unit.ModelNumber ' This is the unit model number, which is usually the actual model number (i.e., the model number on the box), which might be a trade-branded model number.
        Dim modelNumberAlias As String = unitScanStatus.Unit.ModelNumberAlias ' This is the unit model number alias, which is usually the internal equivalent model number and should normally be the one used to do things like look up test parameters.

        ' If there are any unit workflow errors, they should be displayed to the operator and should normally prevent the unit from being tested.
        For Each workflowError As Acuit.Pinpoint.Client2.WorkflowError In unitScanStatus.WorkflowErrors
            Console.WriteLine("Error: " + workflowError.Message)
        Next

        ' Return the unit-station identifier, which will be needed to record test results.
        Return unitScanStatus.UnitStation.UnitStationId
    End Function

    ''' <summary>
    ''' This is what should happen every time a passed or failed test completes, to record the result.
    ''' </summary>
    ''' <param name="unitStationId">Ths unit-station identifier, from the last unit scan.</param>
    ''' <param name="passed">Whether the test passed or failed.</param>
    ''' <param name="failReason">An optional short failure reason (max 50 characters).</param>
    ''' <param name="notes">Optional additional notes (max 100 characters).</param>
    ''' <param name="testData">Optional test data.</param>
    Private Sub OnTestAttemptCompleted(unitStationId As Integer, passed As Boolean, failReason As String, notes As String, testData As List(Of CustomTestDataItem))
        ClientHelper.Use(CreatePinpointClient(),
                         Sub(pinpointClient)
                             pinpointClient.AddTestResult2(_options.LineName, unitStationId, _options.TestTypeName, passed, failReason, notes, UnitTest.EncodeTestDataAsXml(testData))
                         End Sub)
    End Sub

    ''' <summary>
    ''' This is what should happen when a unit is released from the station.
    ''' </summary>
    ''' <param name="unitStationId">Ths unit-station identifier, from the last unit scan.</param>
    Private Sub OnUnitReleased(unitStationId As Integer)
        ClientHelper.Use(CreatePinpointClient(),
                         Sub(pinpointClient)
                             pinpointClient.UnitReleased3(_options.LineName, unitStationId)
                         End Sub)
    End Sub

End Module
