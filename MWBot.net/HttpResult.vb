Option Strict On
Option Explicit On
Imports System.Net
Imports System.Collections.Generic

Namespace WikiBot

    ''' <summary>
    ''' Resultado detallado de una solicitud HTTP (contenido, URI final, código de estado y headers).
    ''' </summary>
    Public Class HttpResult

        ''' <summary>
        ''' Cuerpo de la respuesta (ya adaptado de encoding).
        ''' </summary>
        Public Property Content As String

        ''' <summary>
        ''' URI final tras seguir redirecciones.
        ''' </summary>
        Public Property FinalUri As Uri

        ''' <summary>
        ''' Código de estado HTTP de la respuesta.
        ''' </summary>
        Public Property StatusCode As HttpStatusCode

        ''' <summary>
        ''' Headers de la respuesta (valores múltiples unidos por coma).
        ''' </summary>
        Public Property Headers As Dictionary(Of String, String)

        ''' <summary>
        ''' Inicializa una nueva instancia de HttpResult.
        ''' </summary>
        Public Sub New(content As String, finalUri As Uri, statusCode As HttpStatusCode, headers As Dictionary(Of String, String))
            Me.Content = content
            Me.FinalUri = finalUri
            Me.StatusCode = statusCode
            Me.Headers = If(headers, New Dictionary(Of String, String))
        End Sub

    End Class

End Namespace
