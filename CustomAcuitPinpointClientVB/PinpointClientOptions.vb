''' <summary>
''' Acuit Pinpoint client options.
''' </summary>
''' <remarks>
''' These are settings that should typically be configurable.
''' </remarks>
Public Class PinpointClientOptions

    ''' <summary>
    ''' Gets or sets the name of the local Acuit Pinpoint Server instance. This will be the same for all stations in the plant.
    ''' </summary>
    Public Property PinpointServerName As String = "TODO"

    ''' <summary>
    ''' Gets or sets whether to use transport security. If this is <see langword="true"/>, Transport Layer Security (TLS) over TCP is used, with the client's credentials validated via Windows.
    ''' </summary>
    ''' <remarks>
    ''' This can be used when a user on the same domain as the Acuit Pinpoint Server is logged into Windows on the client PC.
    ''' </remarks>
    Public Property UseTransportSecurity As Boolean = True

    ''' <summary>
    ''' Gets or sets the domain name to use when connecting using explicit credentials.
    ''' </summary>
    Public Property Domain As String

    ''' <summary>
    ''' Gets or sets the user name to use when connecting using explicit credentials.
    ''' </summary>
    Public Property UserName As String

    ''' <summary>
    ''' Gets or sets the password to use when connecting using explicit credentials.
    ''' </summary>
    Public Property Password As String

    ''' <summary>
    ''' Gets or sets the name of the line in Acuit Pinpoint where the station is located. This must match a line name configured in Acuit Pinpoint.
    ''' </summary>
    ''' <returns></returns>
    Public Property LineName As String = "Line 1"

    ''' <summary>
    ''' Gets or sets the station type name for this station. This must match a station type configured for the line in Acuit Pinpoint.
    ''' </summary>
    Public Property StationTypeName As String = "Run Test"

    ''' <summary>
    ''' Gets or sets the station name. This can be anything, but usually the PC name is used.
    ''' </summary>
    Public Property StationName As String = Environment.MachineName

    ''' <summary>
    ''' Gets or sets the client software version. This can be anything, but should usually be the version of your application or plug-in.
    ''' </summary>
    Public Property ClientVersion As String = "1.0.0"

    ''' <summary>
    ''' Gets or sets the test type name. This must match a test type configured for the line in Acuit Pinpoint.
    ''' </summary>
    Public Property TestTypeName As String = "Run Test"
End Class
