Option Strict On
Option Explicit On
Public Class GlobalVars
    Public Shared Codename As String = "MWBot.net"
    Public Shared MwBotVersion As String = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
    Public Shared MaxRetry As Integer = 3
    Public Shared Uptime As DateTime
End Class
