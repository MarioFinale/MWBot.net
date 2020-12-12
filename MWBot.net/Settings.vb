Option Explicit On
Option Strict On
Imports System.IO

Namespace Utility
    Public Class Settings
        Private StrSettings As New Dictionary(Of String, String)
        Private IntSettings As New Dictionary(Of String, Integer)
        Private SettingsIndex As New HashSet(Of String)
        Private _filePath As String

        ''' <summary>
        ''' Inicializa el proveedor de valores.
        ''' </summary>
        ''' <param name="filepath">Ruta del archivo a usar. Si el archivo no existe se crea.</param>
        Public Sub New(ByVal filepath As String)
            _filePath = filepath

            If Not File.Exists(filepath) Then
                File.Create(filepath).Close()
            End If

            For Each l As String In File.ReadLines(filepath)
                Dim vars As String() = l.Split("|"c)
                If vars.Count = 2 Then
                    If Not SettingsIndex.Contains(vars(0)) Then
                        If IsNumeric(vars(1)) Then
                            IntSettings.Add(vars(0), Integer.Parse(vars(1)))
                            SettingsIndex.Add(vars(0))
                        Else
                            StrSettings.Add(vars(0), vars(1))
                            SettingsIndex.Add(vars(0))
                        End If
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Guarda los valores en el archivo.
        ''' </summary>
        Private Sub SaveConfig()
            Dim lines As New List(Of String)
            For Each var As String In SettingsIndex
                If StrSettings.Keys.Contains(var) Then
                    lines.Add(var & "|" & StrSettings(var))
                ElseIf IntSettings.Keys.Contains(var) Then
                    lines.Add(var & "|" & IntSettings(var).ToString)
                End If
            Next
            Try
                File.WriteAllLines(_filePath, lines.ToArray)
            Catch ex As IO.IOException
                ' EventLogger.EX_Log(ex.Message, "SaveConfig")
            End Try
        End Sub

        ''' <summary>
        ''' Indica si existe un valor con el nombre indicado.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <returns></returns>
        Public Function Contains(ByVal setting As String) As Boolean
            Return SettingsIndex.Contains(setting)
        End Function

        ''' <summary>
        ''' Obtiene el valor solicitado, si no existe retorna un error.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <returns>Objeto que contiene el valor, el tipo debe interpretarse.</returns>
        Public Function [Get](ByVal setting As String) As Object
            If SettingsIndex.Contains(setting) Then
                If StrSettings.Keys.Contains(setting) Then
                    Return StrSettings(setting)
                ElseIf IntSettings.Keys.Contains(setting) Then
                    Return IntSettings(setting)
                End If
            End If
            Throw New MissingFieldException
        End Function

        ''' <summary>
        ''' Añade un valor.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <param name="value">Contenido del valor.</param>
        ''' <returns></returns>
        Public Function NewVal(ByVal setting As String, value As Integer) As Boolean
            If Not SettingsIndex.Contains(setting) Then
                IntSettings.Add(setting, value)
                SettingsIndex.Add(setting)
                SaveConfig()
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Añade un valor.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <param name="value">Contenido del valor.</param>
        ''' <returns></returns>
        Public Function NewVal(ByVal setting As String, value As String) As Boolean
            If Not SettingsIndex.Contains(setting) Then
                StrSettings.Add(setting, value)
                SettingsIndex.Add(setting)
                SaveConfig()
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Modifica un valor existente. Si el valor no existe o ya existe con un tipo distinto retorna falso.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <param name="value">Contenido del valor.</param>
        ''' <returns></returns>
        Public Function [Set](ByVal setting As String, value As String) As Boolean
            If SettingsIndex.Contains(setting) Then
                If IntSettings.Keys.Contains(setting) Then
                    Return False
                End If
                StrSettings(setting) = value
                SaveConfig()
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Modifica un valor existente. Si el valor no existe o ya existe con un tipo distinto retorna falso.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <param name="value">Contenido del valor.</param>
        ''' <returns></returns>
        Public Function [Set](ByVal setting As String, value As Integer) As Boolean
            If SettingsIndex.Contains(setting) Then
                If StrSettings.Keys.Contains(setting) Then
                    Return False
                End If
                IntSettings(setting) = value
                SaveConfig()
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Quita un valor. Si el valor no existe retorna falso.
        ''' </summary>
        ''' <param name="setting">Nombre del valor.</param>
        ''' <returns></returns>
        Public Function Remove(ByVal setting As String) As Boolean
            If SettingsIndex.Contains(setting) Then
                If StrSettings.Keys.Contains(setting) Then
                    StrSettings.Remove(setting)
                    SettingsIndex.Remove(setting)

                ElseIf IntSettings.Keys.Contains(setting) Then
                    IntSettings.Remove(setting)
                    SettingsIndex.Remove(setting)
                End If
            Else
                Return False
            End If
            Return True
        End Function
    End Class
End Namespace
