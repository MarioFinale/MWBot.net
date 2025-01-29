Option Explicit On
Option Strict On
Imports System.Net
Imports System.Text.RegularExpressions
Imports MWBot.net.Utility.Utils
Imports MWBot.net.GlobalVars
Imports System.IO
Imports sImage = SixLabors.ImageSharp.Image
Imports System.Drawing
Imports System.Text.Json
Imports SixLabors.ImageSharp
Imports System.Net.Http
Imports System.Threading

Namespace WikiBot
    Public Class Image
        Property Name As String
        Property Author As String
        Property Uri As Uri
        Property License As String
        Property LicenseUrl As String
        Property Missing As Boolean
        Private Property TImage As sImage

        Private LastRequestDate As DateTime = DateTime.Now
        Private RateLimitSeconds As Double = 1.2

        ''' <summary>
        ''' Obtiene una imagen de la wiki dado el nombre exacto (Máximo 1000px de ancho). Como el proyecto eswiki obtiene las imágenes de Commons se consulta a la wiki misma aunque se retorne una URL a Commons.
        ''' </summary>
        ''' <param name="CommonsFileName"></param>
        ''' <param name="Workerbot"></param>
        Public Sub New(ByVal CommonsFileName As String, ByRef Workerbot As Bot)
            If Not (CommonsFileName.ToUpper.EndsWith(".PNG") Or CommonsFileName.ToUpper.EndsWith(".JPG") Or CommonsFileName.ToUpper.EndsWith(".SVG") Or CommonsFileName.ToUpper.EndsWith(".GIF") Or CommonsFileName.ToUpper.EndsWith(".JPEG")) Then
                Throw New ArgumentException("The file must be a image.", NameOf(CommonsFileName))
            End If
            Dim timg As Tuple(Of sImage, String()) = GetCommonsFile(CommonsFileName, Workerbot)
            If timg Is Nothing Then
                Missing = True
                Return
            End If
            TImage = timg.Item1
            Name = CommonsFileName
            Author = timg.Item2(2)
            License = timg.Item2(0)
            If Not String.IsNullOrWhiteSpace(timg.Item2(1)) Then
                LicenseUrl = timg.Item2(1)
            End If
            Uri = New Uri("https://commons.wikimedia.org/wiki/File:" & UrlWebEncode(CommonsFileName.Replace(" ", "_")))
        End Sub

        Private Function GetCommonsFile(ByVal CommonsFilename As String, ByVal WorkerBot As Bot) As Tuple(Of sImage, String())
            Dim responseString As String = NormalizeUnicodetext(WorkerBot.GETQUERY("action=query&format=json&titles=" & UrlWebEncode(CommonsFilename) & "&prop=imageinfo&iiprop=extmetadata|url&iiurlwidth=500"))
            Dim thumburlMatches As String() = TextInBetween(responseString, """thumburl"":""", """,")
            Dim licenceMatches As String() = TextInBetween(responseString, """LicenseShortName"":{""value"":""", """,")
            Dim licenceUrlMatches As String() = TextInBetween(responseString, """LicenseUrl"":{""value"":""", """,")
            Dim authorMatches As String() = TextInBetween(responseString, """Artist"":{""value"":""", """,")
            Dim matchString As String = "<[\S\s]+?>"
            Dim matchString2 As String = "\([\S\s]+?\)"

            Dim licence As String = String.Empty
            Dim licenceUrl As String = String.Empty
            Dim author As String = String.Empty

            If licenceMatches.Length > 0 Then
                licence = Regex.Replace(licenceMatches(0), matchString, "")
            End If
            If licenceUrlMatches.Length > 0 Then
                licenceUrl = Regex.Replace(licenceUrlMatches(0), matchString, "")
            End If
            If authorMatches.Length > 0 Then
                author = NormalizeAuthor(Regex.Replace(authorMatches(0), matchString, ""))
            End If

            Dim img As sImage = Nothing ' Declare img as Nothing initially
            If thumburlMatches.Length > 0 Then
                img = PicFromUrl(thumburlMatches(0), 6, WorkerBot)
            Else
                img = sImage.Load(New MemoryStream()) ' Load an empty image if no thumburl found
            End If

            Return New Tuple(Of sImage, String())(img, {licence, licenceUrl, author})
        End Function

        Private Function NormalizeAuthor(ByVal text As String) As String
            Dim matchString2 As String = "\([\S\s]+?\)"
            text = Regex.Replace(text, matchString2, "").Trim
            Dim authors As String() = text.Split(CType(Environment.NewLine, Char()))
            If authors.Length > 1 Then
                For i As Integer = 0 To authors.Length - 1
                    If Not String.IsNullOrWhiteSpace(authors(i)) Then
                        text = authors(i)
                        Exit For
                    End If
                Next
            End If
            If text.Contains(":") Then
                text = text.Split(":"c)(1).Trim
            End If
            If String.IsNullOrWhiteSpace(text) OrElse text.ToLower().Contains("unknown") Then
                text = "Desconocido"
            End If
            Return text
        End Function

        Private Function PicFromUrl(ByVal url As String, ByVal retries As Integer, ByVal WorkerBot As Bot) As sImage
            Dim img As sImage = Nothing

            For i As Integer = 0 To retries - 1 ' Loop from 0 to retries - 1
                Try
                    ' Calculate time since last request and wait if necessary
                    Dim timeSinceLastRequest As TimeSpan = DateTime.Now - LastRequestDate
                    If timeSinceLastRequest.TotalSeconds < RateLimitSeconds Then
                        Dim waitTime As Integer = CInt(Math.Ceiling(RateLimitSeconds - timeSinceLastRequest.TotalSeconds) * 1000)
                        Thread.Sleep(waitTime) ' Wait to respect rate limit
                    End If

                    Using stream As MemoryStream = WorkerBot.GetAsStream(url) ' Use Workerbot to get the stream
                        img = sImage.Load(stream)
                        ' Update the last request time after a successful request
                        LastRequestDate = DateTime.Now
                        Return img ' Return immediately on success
                    End Using
                Catch ex As HttpRequestException When i < retries - 1
                    ' Log the network error but continue to retry if possible
                    EventLogger.EX_Log($"Network error. Attempt {i + 1}/{retries}. URL: {url}. Error: {ex.Message}", "PicFromUrl")
                Catch ex As ImageFormatException When i < retries - 1
                    ' Log the image format error but continue to retry if possible
                    EventLogger.EX_Log($"Image format error. Attempt {i + 1}/{retries}. URL: {url}. Error: {ex.Message}", "PicFromUrl")
                Catch ex As Exception
                    ' Log any other exceptions but only if this is the last retry attempt
                    If i = retries - 1 Then
                        EventLogger.EX_Log($"Unexpected error on last attempt. URL: {url}. Error: {ex.Message}", "PicFromUrl")
                    End If
                End Try
            Next

            ' If we've exhausted all retries, log the failure and throw an exception
            EventLogger.EX_Log($"Failed to retrieve image after {retries} attempts. URL: {url}", "PicFromUrl")
            Throw New MWBot.net.MaxRetriesExeption()
        End Function
        ''' <summary>
        ''' Guarda la imagen (en PNG).
        ''' </summary>
        ''' <param name="Path"></param>
        Public Sub Save(ByVal Path As String)
            Dim tex As String() = Path.Split("."c)
            Dim ext As String = "." & tex(tex.Count - 1)
            Dim endname As String = ReplaceLast(Path, ext, ".png")
            TImage.SaveAsPng(endname)
        End Sub

    End Class



End Namespace

