Option Explicit On
Option Strict On
Imports System.Net
Imports System.Text.RegularExpressions
Imports Utils.Utils
Imports MWBot.net.GlobalVars
Imports System.IO
Imports sImage = System.Drawing.Image
Imports System.Drawing
Imports System.Text.Json

Namespace WikiBot
    Public Class Image
        Property Name As String
        Property Author As String
        Property Uri As Uri
        Property License As String
        Property LicenseUrl As String
        Property Missing As Boolean

        Private _image As sImage

        ''' <summary>
        ''' Obtiene una imagen de la wiki dado el nombre exacto (Máximo 1000px de ancho). Como el proyecto eswiki obtiene las imágenes de Commons se consulta a la wiki misma aunque se retorne una URL a Commons.
        ''' </summary>
        ''' <param name="CommonsFileName"></param>
        ''' <param name="Workerbot"></param>
        Public Sub New(ByVal CommonsFileName As String, ByRef Workerbot As Bot)
            If Not (CommonsFileName.ToUpper.EndsWith(".PNG") Or CommonsFileName.ToUpper.EndsWith(".JPG") Or CommonsFileName.ToUpper.EndsWith(".SVG") Or CommonsFileName.ToUpper.EndsWith(".GIF") Or CommonsFileName.ToUpper.EndsWith(".JPEG")) Then
                Throw New ArgumentException("The file must be a image.", "CommonsFileName")
            End If
            Dim timg As Tuple(Of sImage, String()) = GetCommonsFile(CommonsFileName, Workerbot)
            If timg Is Nothing Then
                Missing = True
                Return
            End If
            _image = timg.Item1
            Name = CommonsFileName
            Author = timg.Item2(2)
            License = timg.Item2(0)
            If Not String.IsNullOrWhiteSpace(timg.Item2(1)) Then
                LicenseUrl = timg.Item2(1)
            End If
            Uri = New Uri("https://commons.wikimedia.org/wiki/File:" & UrlWebEncode(CommonsFileName.Replace(" ", "_")))
        End Sub

        Private Function NormalizeAuthor(ByVal text As String) As String
            Dim regx1 As String = "<[\S\s]+?>"
            Dim regx2 As String = "\([\S\s]+?\)"
            text = Regex.Replace(text, regx1, "")
            text = Regex.Replace(text, regx2, "").Trim()
            Dim artists As String() = text.Split(Environment.NewLine.ToCharArray())
            If artists.Count() > 1 Then
                For aindx As Integer = 0 To artists.Count() - 1
                    If Not String.IsNullOrWhiteSpace(artists(aindx)) Then text = artists(aindx)
                Next
            End If
            If text.Contains(":") Then text = text.Split(":"c)(1).Trim()
            If String.IsNullOrWhiteSpace(text) Or (text.ToLower().Contains("unknown")) Then text = "Desconocido"
            Return text
        End Function

        Private Function GetCommonsFile(ByVal CommonsFilename As String, ByVal Workerbot As Bot) As Tuple(Of sImage, String())
            Dim fileInfo As New List(Of String)
            Dim queryString As String = "action=query&format=json&titles=File:" & UrlWebEncode(CommonsFilename) & "&prop=imageinfo&iiprop=extmetadata|url&iiurlwidth=1000"
            Dim responsestring As String = Workerbot.GETQUERY(queryString)
            Dim queryResponse As JsonDocument = GetJsonDocument(responsestring)
            Dim queryElement As JsonElement = GetJsonElement(queryResponse, "query")
            Dim qpages As JsonElement = queryElement.GetProperty("pages")
            For Each qpage As JsonProperty In qpages.EnumerateObject
                Dim pageElement As JsonElement = qpage.Value
                Dim title As String = pageElement.GetProperty("title").GetString
                Dim missing As Boolean = Not IsJsonPropertyPresent(pageElement, "imageinfo")
                If missing Then
                    Return Nothing
                End If
                Dim imageInfo As JsonElement = pageElement.GetProperty("imageinfo")
                Dim info As JsonElement = imageInfo.EnumerateArray(0)
                Dim thumburl As String = info.GetProperty("thumburl").GetString
                Dim url As String = info.GetProperty("thumburl").GetString
                Dim extmetadata As JsonElement = info.GetProperty("extmetadata")
                Dim objectname As JsonElement = extmetadata.GetProperty("ObjectName")
                Dim name As String = objectname.GetProperty("value").GetString
                Dim licenseshortname As JsonElement = extmetadata.GetProperty("LicenseShortName")
                Dim slicenseshortname As String = licenseshortname.GetProperty("value").GetString
                Dim licenseurl As JsonElement = extmetadata.GetProperty("LicenseUrl")
                Dim slicenseurl As String = licenseurl.GetProperty("value").GetString
                Dim artist As JsonElement = extmetadata.GetProperty("Artist")
                Dim sartist As String = artist.GetProperty("value").GetString
                sartist = NormalizeAuthor(sartist)
                Dim img As sImage = PicFromUrl(thumburl)
                Return New Tuple(Of sImage, String())(img, {slicenseshortname, slicenseurl, sartist})
            Next
            Return Nothing
        End Function

        Private Function PicFromUrl(ByVal url As String) As sImage
            Dim img As sImage = Nothing
            Try
                Dim request = WebRequest.Create(url)
                Using response = request.GetResponse()
                    Using stream = response.GetResponseStream()
                        img = sImage.FromStream(stream, True, True)
                    End Using
                End Using
                Return img
            Catch ex As Exception
                EventLogger.EX_Log(ex.Message, "DailyRes")
                img.Dispose()
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Guarda la imagen (en PNG).
        ''' </summary>
        ''' <param name="Path"></param>
        Public Sub Save(ByVal Path As String)
            Dim tex As String() = Path.Split("."c)
            Dim ext As String = "." & tex(tex.Count - 1)
            Dim endname As String = ReplaceLast(Path, ext, ".png")
            Using tstream As New MemoryStream
                _image.Save(endname, Imaging.ImageFormat.Png)
                Dim imageBytes As Byte() = tstream.ToArray
                IO.File.WriteAllBytes(endname, imageBytes)
            End Using
        End Sub

    End Class



End Namespace

