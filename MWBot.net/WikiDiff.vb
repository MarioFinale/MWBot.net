Imports MWBot.net.WikiBot

Public Class WikiDiff

    ReadOnly Property OldId As Integer
    ReadOnly Property NewId As Integer
    ReadOnly Property Diffs As IReadOnlyCollection(Of Tuple(Of String, String))

    Sub New(ByVal tOldid As Integer, tNewid As Integer, tDiff As ICollection(Of Tuple(Of String, String)))
        OldId = tOldid
        NewId = tNewid
        Diffs = CType(tDiff, IReadOnlyCollection(Of Tuple(Of String, String)))
    End Sub

End Class
