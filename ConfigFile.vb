Public Class ConfigFile
    Private _path As String
    Public ReadOnly Property Path As String
        Get
            Return _path
        End Get
    End Property

    Public ReadOnly Property Params As String()
        Get
            If Not IO.File.Exists(_path) Then
                IO.File.Create(_path).Close()
            End If
            Return IO.File.ReadAllLines(_path)
        End Get
    End Property

    Public Sub New(ByVal tPath As String)
        If Not IO.File.Exists(tPath) Then
            IO.File.Create(tPath).Close()
        End If
        _path = tPath
    End Sub

    Public Sub New(ByVal filePath As String, params As String())
        If IO.File.Exists(_path) Then
            IO.File.Delete(_path)
            IO.File.Create(_path).Close()
            IO.File.WriteAllLines(_path, params)
        End If
        _path = filePath
    End Sub

End Class