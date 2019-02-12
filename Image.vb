Imports System.Drawing
Imports System.Net
Imports System.Text.RegularExpressions

Namespace WikiBot
    Public Class Image
        Property Name As String
        Property Image As Drawing.Image
        Property Author As String
        Property Uri As Uri
        Property License As String
        Property LicenseUrl As String

        ''' <summary>
        ''' Obtiene una imagen de la wiki dado el nombre exacto (Máximo 1000px de ancho). Como el proyecto eswiki obtiene las imágenes de Commons se consulta a la wiki misma aunque se retorne una URL a Commons.
        ''' </summary>
        ''' <param name="CommonsFileName"></param>
        ''' <param name="Workerbot"></param>
        Public Sub New(ByVal CommonsFileName As String, ByRef Workerbot As Bot)
            If Not (CommonsFileName.ToUpper.EndsWith(".PNG") Or CommonsFileName.ToUpper.EndsWith(".JPG") Or CommonsFileName.ToUpper.EndsWith(".SVG") Or CommonsFileName.ToUpper.EndsWith(".GIF") Or CommonsFileName.ToUpper.EndsWith(".JPEG")) Then
                Throw New ArgumentException("The file must be a image.", "CommonsFileName")
            End If
            Dim timg As Tuple(Of Drawing.Image, String()) = GetCommonsFile(CommonsFileName, Workerbot)
            Image = timg.Item1
            Name = CommonsFileName
            Author = timg.Item2(2)
            License = timg.Item2(0)
            If Not String.IsNullOrWhiteSpace(timg.Item2(1)) Then
                LicenseUrl = timg.Item2(1)
            End If
            Uri = New Uri("https://commons.wikimedia.org/wiki/File:" & CommonsFileName.Replace(" ", "_"))
        End Sub

        Private Function GetCommonsFile(ByVal CommonsFilename As String, ByVal Workerbot As Bot) As Tuple(Of Drawing.Image, String())
            Dim responsestring As String = Utils.NormalizeUnicodetext(Workerbot.GETQUERY("action=query&format=json&titles=File:" & Utils.UrlWebEncode(CommonsFilename) & "&prop=imageinfo&iiprop=extmetadata|url&iiurlwidth=1000"))
            Dim thumburlmatches As String() = Utils.TextInBetween(responsestring, """thumburl"":""", """,")
            Dim licencematches As String() = Utils.TextInBetween(responsestring, """LicenseShortName"":{""value"":""", """,")
            Dim licenceurlmatches As String() = Utils.TextInBetween(responsestring, """LicenseUrl"":{""value"":""", """,")
            Dim authormatches As String() = Utils.TextInBetween(responsestring, """Artist"":{""value"":""", """,")
            Dim matchstring As String = "<[\S\s]+?>"
            Dim matchstring2 As String = "\([\S\s]+?\)"
            Dim licence As String = String.Empty
            Dim licenceurl As String = String.Empty
            Dim author As String = String.Empty
            If licencematches.Count() > 0 Then licence = Regex.Replace(licencematches(0), matchstring, "")
            If licenceurlmatches.Count() > 0 Then licenceurl = Regex.Replace(licenceurlmatches(0), matchstring, "")

            If authormatches.Count() > 0 Then
                author = Regex.Replace(authormatches(0), matchstring, "")
                author = Regex.Replace(author, matchstring2, "").Trim()
                Dim authors As String() = author.Split(Environment.NewLine.ToCharArray())

                If authors.Count() > 1 Then

                    For i As Integer = 0 To authors.Count() - 1
                        If Not String.IsNullOrWhiteSpace(authors(i)) Then author = authors(i)
                    Next
                End If

                If author.Contains(":") Then author = author.Split(":"c)(1).Trim()
            End If

            Dim img As Drawing.Image = New Bitmap(1, 1)
            If thumburlmatches.Count() > 0 Then img = PicFromUrl(thumburlmatches(0))
            If String.IsNullOrWhiteSpace(author) Or (author.ToLower().Contains("unknown")) Then author = "Desconocido"
            Return New Tuple(Of Drawing.Image, String())(img, New String() {licence, licenceurl, author})
        End Function

        Private Function PicFromUrl(ByVal url As String) As Drawing.Image
            Dim img As Drawing.Image = New Bitmap(1, 1)
            Try
                Dim request = WebRequest.Create(url)
                Using response = request.GetResponse()
                    Using stream = response.GetResponseStream()
                        img = CType(Drawing.Image.FromStream(stream).Clone(), Drawing.Image)
                    End Using
                End Using
                Return img
            Catch ex As Exception
                Utils.EventLogger.EX_Log(ex.Message, "DailyRes")
                img.Dispose()
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Guarda la imagen (en png ignorando la extensión).
        ''' </summary>
        ''' <param name="Path"></param>
        Public Sub Save(ByVal Path As String)
            Dim tex As String() = Path.Split("."c)
            Dim ext As String = "." & tex(tex.Count - 1)
            Dim endname As String = Utils.ReplaceLast(Path, ext, ".png")
            Image.Save(endname, Imaging.ImageFormat.Png)
        End Sub

    End Class



End Namespace

