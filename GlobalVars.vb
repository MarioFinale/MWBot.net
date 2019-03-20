Option Strict On
Option Explicit On
Imports Utils.Utils
Imports LogEngine
Public Class GlobalVars
    Public Shared Codename As String = "MWBot.net"
    Public Shared MwBotVersion As String = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
    Public Shared MaxRetry As Integer = 3
    Public Shared ConfigFilePath As String = Exepath & "Config.cfg"
    Public Shared Log_Filepath As String = Exepath & "Log.psv"
    Public Shared UserPath As String = Exepath & "Users.psv"
    Public Shared SettingsPath As String = Exepath & "Settings.psv"
    Public Shared EventLogger As New LogEngine.LogEngine(Log_Filepath, UserPath, Codename)
    Public Shared SettingsProvider As New Settings(SettingsPath)


End Class
