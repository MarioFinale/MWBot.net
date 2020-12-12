Option Strict On
Option Explicit On
Imports MWBot.net.Utility
Imports MWBot.net.Utility.Utils
Public NotInheritable Class GlobalVars
    Public Shared Codename As String = "MWBot.net"
    Public Shared MwBotVersion As String = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
    Public Shared MaxRetry As Integer = 3
    Public Shared ConfigFilePath As String = Exepath & "Config.cfg"
    Public Shared Log_Filepath As String = Exepath & "Log.psv"
    Public Shared UserPath As String = Exepath & "Users.psv"
    Public Shared SettingsPath As String = Exepath & "Settings.psv"
    Public Shared EventLogger As New SimpleLogger(Log_Filepath, UserPath, Codename, True)
    Public Shared SettingsProvider As New Settings(SettingsPath)


End Class
