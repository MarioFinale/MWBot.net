Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Resources
Imports System.Text.RegularExpressions
Imports System.Threading
Imports MWBot.net.GlobalVars
Imports Utils.Utils
Imports MWBot.net.My.Resources
Imports System.Net.Http
Imports System.Text.Json

Namespace WikiBot

    Public Class ApiHandler
        Private ApiCookies As CookieContainer
        Private _userName As String = String.Empty

        Private _botUsername As String = String.Empty
        Private _botPassword As String = String.Empty
        Private _apiUri As Uri
        Private _userAgent As String = "MWBot.net/" & MwBotVersion & " (http://es.wikipedia.org/wiki/User_talk:MarioFinale) .NET/MONO"

#Region "Properties"
        Public ReadOnly Property UserName As String
            Get
                Return _userName
            End Get
        End Property

        Public ReadOnly Property ApiUri As Uri
            Get
                Return _apiUri
            End Get
        End Property

        Public Property UserAgent As String
            Get
                Return _userAgent
            End Get
            Set(value As String)
                _userAgent = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Inicializa una nueva instancia del bot.
        ''' </summary>
        ''' <param name="BotUsername">Nombre de usuario del bot</param>
        ''' <param name="BotPassword">Contraseña del bot (solo botpassword), más información ver https://www.mediawiki.org/wiki/Manual:Bot_passwords </param>
        ''' <param name="tUri">Direccion de la API</param>
        Sub New(ByVal botUserName As String, botPassword As String, tUri As Uri)
            If String.IsNullOrWhiteSpace(botUserName) Then
                Throw New ArgumentException("No username")
            End If
            If String.IsNullOrWhiteSpace(botPassword) Then
                Throw New ArgumentException("No BotPassword")
            End If
            If tUri Is Nothing Then
                Throw New ArgumentException("No Api Uri")
            End If
            Init(botUserName, botPassword, tUri)
        End Sub


        Private Sub Init(ByVal botUserName As String, botPassword As String, apiUri As Uri)
            _botUsername = botUserName
            _botPassword = botPassword
            _apiUri = apiUri
            ApiCookies = New CookieContainer
            LogOn()
            _userName = botUserName.Split("@"c)(0).Trim()
        End Sub

        ''' <summary>
        ''' Obtiene un Token y cookies de ingreso, establece las cookies de la clase y retorna el token como string.
        ''' </summary>
        Private Function GetWikiToken() As String
            EventLogger.Log(Messages.RequestingToken, SStrings.LocalSource)
            Dim postdata As String = SStrings.GetWikiToken
            Dim postresponse As String = PostDataAndGetResult(_apiUri, postdata, ApiCookies)
            Dim queryResponse As JsonElement = GetJsonElement(GetJsonDocument(postresponse), "query")
            Dim tokens As JsonElement = GetJsonElement(queryResponse, "tokens")
            Dim logintoken As JsonElement = GetJsonElement(tokens, "logintoken")
            Dim token As String = logintoken.GetString
            EventLogger.Log(Messages.TokenObtained, SStrings.LocalSource)
            Return token
        End Function

        ''' <summary>
        ''' Luego de obtener un Token y cookies de ingreso, envía estos al servidor para loguear y guarda las cookies de sesión.
        ''' </summary>
        Public Function LogOn() As String
            EventLogger.Log(Messages.SigninIn, SStrings.LocalSource)
            Dim token As String
            Dim turi As Uri = _apiUri
            Dim postdata As String
            Dim postresponse As String
            Dim login As JsonElement
            Dim result As String = String.Empty
            Dim exitloop As Boolean = False

            Do Until exitloop
                Try
                    token = GetWikiToken()
                    postdata = String.Format(SStrings.Login, _botUsername, _botPassword, UrlWebEncode(token))
                    postresponse = PostDataAndGetResult(turi, postdata, ApiCookies)

                    Dim jsonResponse As JsonDocument = GetJsonDocument(postresponse)
                    login = GetJsonElement(jsonResponse, "login")
                    result = GetJsonElement(login, "result").GetString
                    EventLogger.Log(Messages.LoginResult & result, SStrings.LocalSource)

                    Dim lguserid As Integer = GetJsonElement(login, "lguserid").GetInt32
                    EventLogger.Log(Messages.LoginID & lguserid, SStrings.LocalSource)

                    Dim lgusername As String = GetJsonElement(login, "lgusername").GetString
                    EventLogger.Log(Messages.UserName & lgusername, SStrings.LocalSource)
                    Return result
                Catch ex1 As WebException
                    EventLogger.Log(Messages.NetworkError & ex1.Message, SStrings.LocalSource)
                Catch ex2 As IOException
                    EventLogger.Log(Messages.NetworkError & ex2.Message, SStrings.LocalSource)
#Disable Warning CA1031 ' Generic exceptions needs to be catched
                Catch ex3 As Exception
                    EventLogger.Log(Messages.LoginError, SStrings.LocalSource)
                    If result.ToLower(Globalization.CultureInfo.InvariantCulture) = "failed" Then
                        Dim reason As String = GetJsonElement(login, "reason").GetString
                        Console.WriteLine(Environment.NewLine & Environment.NewLine)
                        Console.WriteLine(Messages.Reason & reason)
                        Console.WriteLine(Environment.NewLine & Environment.NewLine)
                        Console.Write(Messages.PressKey)
                        Console.ReadKey()
                        ExitProgram()
                        Return result
                    End If
                    Return result
#Enable Warning CA1031
                End Try
                Console.WriteLine(Environment.NewLine)
                exitloop = PressKeyTimeout(5)
            Loop
            ExitProgram()
            Return result
        End Function

        ''' <summary>
        ''' Envía un POST a la url de la API con los datos indicados
        ''' </summary>
        ''' <param name="postData"></param>
        ''' <returns></returns>
        Public Function Postquery(ByVal postData As String) As String
            Dim postresponse As String = PostDataAndGetResult(_apiUri, postData, ApiCookies)
            Return postresponse
        End Function

        ''' <summary>
        ''' Envía una solicitud GET a la api con los datos solicitados (luego del "?")
        ''' </summary>
        ''' <param name="getData"></param>
        ''' <returns></returns>
        Public Function Getquery(ByVal getData As String) As String
            Dim turi As Uri = New Uri(_apiUri.OriginalString & "?" & getData)
            Dim getresponse As String = GetDataAndResult(turi, ApiCookies)
            Return getresponse
        End Function

        Public Overloads Function [GET](ByVal address As String) As String
            Return [GET](New Uri(address))
        End Function

        Public Overloads Function [GET](ByVal pageUri As Uri) As String
            If pageUri Is Nothing Then
                Return String.Empty
            End If
            Dim getresponse As String = GetDataAndResult(pageUri, ApiCookies)
            Return getresponse
        End Function


        ''' <summary>
        ''' Envía una solicitud POST a la uri indicada
        ''' </summary>
        ''' <param name="pageUri">Recurso al cual enviar la solicitud</param>
        ''' <param name="postData">Datos en la solicitud POST como application/x-www-form-urlencoded</param>
        ''' <returns></returns>
        Public Overloads Function POST(ByVal pageUri As Uri, ByVal postData As String) As String
            If pageUri Is Nothing Then
                Return String.Empty
            End If
            If String.IsNullOrWhiteSpace(postData) Then
                Return String.Empty
            End If
            Dim postresponse As String = PostDataAndGetResult(pageUri, postData, ApiCookies)
            Return postresponse
        End Function

        ''' <summary>
        ''' Limpia todas las cookies, retorna "true" si finaliza correctamente.
        ''' </summary>
        Public Function CleanCookies() As Boolean
            ApiCookies = New CookieContainer
            Return True
        End Function

        ''' <summary>Realiza una solicitud de tipo GET a un recurso web y retorna el texto.</summary>
        ''' <param name="pageURI">URI absoluta del recurso web.</param>
        Public Function GetDataAndResult(ByVal pageUri As Uri) As String
            Return GetDataAndResult(pageUri, New CookieContainer)
        End Function

        ''' <summary>Realiza una solicitud de tipo GET a un recurso web y retorna el texto.</summary>
        ''' <param name="pageUri">URL absoluta del recurso web.</param>
        ''' <param name="Cookies">Cookies sobre los que se trabaja.</param>
        Public Function GetDataAndResult(ByVal pageUri As Uri, ByRef cookies As CookieContainer) As String
            Dim tryCount As Integer = 0

            Do Until tryCount = MaxRetry

                If pageUri Is Nothing Then
                    Throw New ArgumentNullException(NameOf(pageUri), "Null uri")
                End If

                If cookies Is Nothing Then
                    cookies = New CookieContainer
                End If

                Dim tempcookies As CookieContainer = cookies

                Dim encoding As New Text.UTF8Encoding
                Dim handler As HttpClientHandler = New HttpClientHandler With {
                    .CookieContainer = cookies,
                    .UseCookies = True
                }
                Dim client As HttpClient = New HttpClient(handler)
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent)
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive")
                client.DefaultRequestHeaders.Add("Method", "GET")
                Dim response As Byte() = Nothing

                Try
                    response = client.GetByteArrayAsync(pageUri).Result
                    tempcookies.Add(cookies.GetCookies(pageUri))
                Catch ex As System.Net.WebException
                    tryCount += 1
#Disable Warning CA1031
                Catch ex2 As Exception
                    tryCount += 1
#Enable Warning CA1031
                Finally
                    client.Dispose()
                End Try
                If Not response Is Nothing Then
                    cookies = tempcookies
                    Return AdaptEncoding(response)
                End If
                Return Nothing
            Loop
            Throw New MaxRetriesExeption
        End Function

        Private Function AdaptEncoding(ByVal responseBytes As Byte()) As String
            Dim text As String
            Using reader As New StreamReader(New MemoryStream(responseBytes), True)
                text = reader.ReadToEnd
                If text.Contains("�") Then
                    If Regex.Match(text, "<!doctype html[\s\S]+?<head>[\s\S]+?<meta .+; charset=iso-8859-1"" *\/>[\s\S]+?<\/head>", RegexOptions.IgnoreCase).Success Then
                        Dim iso As Text.Encoding = System.Text.Encoding.GetEncoding(28591)
                        text = iso.GetString(responseBytes)
                    End If
                    If Regex.Match(text, "<!doctype html[\s\S]+?<head>[\s\S]+?<meta .+; charset=iso-8859-9"" *\/>[\s\S]+?<\/head>", RegexOptions.IgnoreCase).Success Then
                        Dim iso As Text.Encoding = System.Text.Encoding.GetEncoding(28599)
                        text = iso.GetString(responseBytes)
                    End If
                End If
            End Using
            Return text
        End Function

        ''' <summary>Realiza una solicitud de tipo POST a un recurso web y retorna el texto.</summary>
        ''' <param name="pageURI">URI absoluta del recurso web.</param>
        ''' <param name="postData">Cadena de texto que se envia en el POST.</param>
        Public Function PostDataAndGetResult(pageUri As Uri, postData As String) As String
            Return PostDataAndGetResult(pageUri, postData, New CookieContainer)
        End Function

        ''' <summary>Realiza una solicitud de tipo POST a un recurso web y retorna el texto.</summary>
        ''' <param name="pageUri">URL absoluta del recurso web.</param>
        ''' <param name="postData">Cadena de texto que se envia en el POST.</param>
        Public Function PostDataAndGetResult(pageUri As Uri, postData As String, ByRef cookies As CookieContainer, Optional retrycount As Integer = 0) As String

            If pageUri Is Nothing Then
                Throw New ArgumentNullException(NameOf(pageUri), "Empty uri.")
            End If

            If cookies Is Nothing Then
                cookies = New CookieContainer
            End If

            Dim tempcookies As CookieContainer = cookies

            Dim encoding As New Text.UTF8Encoding
            Dim byteData As Byte() = encoding.GetBytes(postData)
            Dim handler As HttpClientHandler = New HttpClientHandler With {
                .CookieContainer = cookies,
                .UseCookies = True
            }

            Dim client As HttpClient = New HttpClient(handler)
            Dim content As StringContent = New StringContent(postData)
            content.Headers.Add("Method", "POST")
            content.Headers.ContentType = Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
            content.Headers.ContentLength = byteData.Length
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent)
            client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive")
            client.DefaultRequestHeaders.Add("Method", "POST")
            client.Timeout = New TimeSpan(0, 0, 30)
            Dim response As Byte() = Nothing
            Try
                response = client.PostAsync(pageUri, content).Result.Content.ReadAsByteArrayAsync.Result
                tempcookies.Add(cookies.GetCookies(pageUri))
            Catch ex As System.Net.WebException
                If retrycount < 3 Then
                    Return PostDataAndGetResult(pageUri, postData, cookies, retrycount + 1)
                End If
#Disable Warning CA1031
            Catch ex2 As Exception
                If retrycount < 3 Then
                    Return PostDataAndGetResult(pageUri, postData, cookies, retrycount + 1)
                End If
#Enable Warning CA1031
            Finally
                client.Dispose()
                content.Dispose()
            End Try
            If Not response Is Nothing Then
                cookies = tempcookies
                Return AdaptEncoding(response)
            End If
            Return Nothing
        End Function
    End Class

End Namespace