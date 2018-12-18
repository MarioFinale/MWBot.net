Option Strict On
Option Explicit On
Imports MWBot.net.WikiBot

Public Class GlobalVars
    Public Shared BotCodename As String = "MWBot.net"
    Public Shared Exepath As String = AppDomain.CurrentDomain.BaseDirectory
    Public Shared DirSeparator As String = IO.Path.DirectorySeparatorChar
    ''' <summary>
    ''' El separador de decimales varia segun SO y configuracion regional, eso puede afectar los calculos.
    ''' </summary>
    Public Shared DecimalSeparator As String = String.Format(CType(1.1, String)).Substring(1, 1)

    Public Shared OS As String = My.Computer.Info.OSFullName & " " & My.Computer.Info.OSVersion
    Public Shared MwBotVersion As String = Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
    Public Shared User_Filepath As String = Exepath & "Users.psv"
    Public Shared Log_Filepath As String = Exepath & "Log.psv"
    Public Shared ConfigFilePath As String = Exepath & "Config.cfg"
    Public Shared SettingsPath As String = Exepath & "Settings.psv"
    Public Shared MaxRetry As Integer = 3
    Public Shared Uptime As DateTime

End Class
