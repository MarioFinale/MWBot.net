﻿Option Strict On
Option Explicit On

Namespace WikiBot
#Disable Warning CA1036 ' Override methods on comparable types
    Public Class WikiTopic
        Implements IComparable(Of WikiTopic)
        Public Property Name As String
        Public Property Threads As New SortedSet(Of WikiTopicThread)
        Public Function CompareTo(other As WikiTopic) As Integer Implements IComparable(Of WikiTopic).CompareTo
            If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
            Return Me.Name().CompareTo(other.Name())
        End Function
        Sub New(ByVal topicName As String, threadList As SortedSet(Of WikiTopicThread))
            Name = topicName
            Threads = threadList
        End Sub
    End Class
End Namespace