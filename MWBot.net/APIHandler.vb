Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Collections.Generic
Imports MWBot.net.GlobalVars
Imports MWBot.net.Utility.Utils
Imports MWBot.net.My.Resources
Imports System.Net.Http
Imports System.Text.Json
Imports System.Text

Namespace WikiBot

    Public Class ApiHandler
        Private ApiCookies As CookieContainer
        Private _userName As String = String.Empty
        Private _botUsername As String = String.Empty
        Private _botPassword As String = String.Empty
        Private _apiUri As Uri
        Private _userAgent As String = "MWBot.net/" & MwBotVersion & " (http://es.wikipedia.org/wiki/User_talk:MarioFinale) .NET/MONO"
        Private _requestDelay As Double = 100
        Private _exponentialBackOffDelayMs As Integer = 3000

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

        Public Property RequestDelay As Double
            Get
                Return _requestDelay
            End Get
            Set(value As Double)
                _requestDelay = value
            End Set
        End Property

        Public Property ExponentialBackOffDelayMs As Integer
            Get
                Return _exponentialBackOffDelayMs
            End Get
            Set(value As Integer)
                _exponentialBackOffDelayMs = value
            End Set
        End Property
#End Region

        ''' <summary>
        ''' Inicializa una nueva instancia del bot.
        ''' </summary>
        ''' <param name="BotUsername">Nombre de usuario del bot.</param>
        ''' <param name="BotPassword">Contraseña del bot (solo botpassword), más información ver https://www.mediawiki.org/wiki/Manual:Bot_passwords </param>
        ''' <param name="tUri">Direccion de la API.</param>
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
        ''' Envía un POST a la url de la API con los datos indicados.
        ''' </summary>
        ''' <param name="postData"></param>
        ''' <returns></returns>
        Public Function Postquery(ByVal postData As String) As String
            Dim postresponse As String = PostDataAndGetResult(_apiUri, postData, ApiCookies)
            Return postresponse
        End Function

        ''' <summary>
        ''' Envía una solicitud GET a la api con los datos solicitados (luego del "?").
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
        ''' Performs a GET request to the specified web address and returns the response as a MemoryStream.
        ''' </summary>
        ''' <param name="address">The URL as a string to which the GET request will be made.</param>
        ''' <returns>A MemoryStream containing the response data.</returns>
        Public Overloads Function GetAsStream(ByVal address As String) As MemoryStream
            Return GetAsStream(New Uri(address))
        End Function

        ''' <summary>
        ''' Performs a GET request to the specified web address and returns the response as a MemoryStream.
        ''' </summary>
        ''' <param name="pageUri">The URI to which the GET request will be made.</param>
        ''' <returns>A MemoryStream containing the response data or Nothing if the URI is null.</returns>
        Public Overloads Function GetAsStream(ByVal pageUri As Uri) As MemoryStream
            If pageUri Is Nothing Then
                Return Nothing
            End If
            Dim getResponse As MemoryStream = GetDataAsStream(pageUri, ApiCookies)
            Return getResponse
        End Function


        ''' <summary>
        ''' Envía una solicitud POST a la uri indicada.
        ''' </summary>
        ''' <param name="pageUri">Recurso al cual enviar la solicitud.</param>
        ''' <param name="postData">Datos en la solicitud POST como application/x-www-form-urlencoded.</param>
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
        ''' Envía una solicitud POST con cabeceras personalizadas a la URI indicada, sin utilizar cookies persistentes.
        ''' </summary>
        ''' <param name="pageUri">URI del recurso al cual enviar la solicitud.</param>
        ''' <param name="postData">Datos del cuerpo de la solicitud POST (application/x-www-form-urlencoded por defecto).</param>
        ''' <param name="customHeaders">Diccionario de cabeceras HTTP personalizadas a incluir en la solicitud. Puede ser Nothing.</param>
        ''' <returns>El cuerpo de la respuesta como cadena de texto, o Nothing si la solicitud falla.</returns>
        Public Function POSTWithHeaders(pageUri As Uri, postData As String, customHeaders As Dictionary(Of String, String)) As String
            Dim tempCookies As CookieContainer = New CookieContainer
            Return POSTWithHeaders(pageUri, postData, customHeaders, tempCookies)
        End Function

        ''' <summary>
        ''' Envía una solicitud POST con cabeceras personalizadas a la URI indicada, utilizando y actualizando el contenedor de cookies proporcionado.
        ''' </summary>
        ''' <param name="pageUri">URI del recurso al cual enviar la solicitud.</param>
        ''' <param name="postData">Datos del cuerpo de la solicitud POST (application/x-www-form-urlencoded por defecto).</param>
        ''' <param name="customHeaders">Diccionario de cabeceras HTTP personalizadas a incluir en la solicitud. Puede ser Nothing.</param>
        ''' <param name="cookies">Contenedor de cookies que se utiliza en la solicitud y se actualiza con las cookies recibidas en la respuesta.</param>
        ''' <returns>El cuerpo de la respuesta como cadena de texto, o Nothing si la solicitud falla.</returns>
        Public Function POSTWithHeaders(pageUri As Uri, postData As String, customHeaders As Dictionary(Of String, String), ByRef cookies As CookieContainer) As String
            Return PostDataAndGetResultInternal(pageUri, postData, customHeaders, cookies, 0)
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
            Dim delay As Integer = _exponentialBackOffDelayMs

            Do Until tryCount = MaxRetry

                If pageUri Is Nothing Then
                    Throw New ArgumentNullException(NameOf(pageUri), "Null uri")
                End If

                If cookies Is Nothing Then
                    cookies = New CookieContainer
                End If

                Dim RequestDelayInMS As Double = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                While (RequestDelayInMS < _requestDelay) 'Limit post requests per second
                    Thread.Sleep(1)
                    SyncLock RequestLock
                        RequestDelayInMS = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                    End SyncLock
                End While
                SyncLock RequestLock
                    LastRequestTimestamp = Date.UtcNow
                End SyncLock

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
                Dim response As String = Nothing

                Try
                    Dim message As Task(Of HttpResponseMessage) = client.GetAsync(pageUri)
                    Dim res As HttpResponseMessage = message.Result
                    Dim theaders As Headers.HttpResponseHeaders = res.Headers
                    response = res.Content.ReadAsStringAsync.Result()
                    tempcookies.Add(cookies.GetCookies(pageUri))

                Catch ex As System.Net.WebException
                    tryCount += 1
                    delay = delay * 2 ' exponential backoff
                    Thread.Sleep(delay)
#Disable Warning CA1031
                Catch ex2 As Exception
                    tryCount += 1
                    delay = delay * 2 ' exponential backoff
                    Thread.Sleep(delay)
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

        ''' <summary>
        ''' Realiza una solicitud de tipo GET a un recurso web y retorna el contenido como un MemoryStream.
        ''' </summary>
        ''' <param name="pageUri">URL absoluta del recurso web.</param>
        ''' <param name="Cookies">Cookies sobre los que se trabaja.</param>
        ''' <returns>Un MemoryStream con el contenido del recurso web.</returns>
        Public Function GetDataAsStream(ByVal pageUri As Uri, ByRef cookies As CookieContainer) As MemoryStream
            Dim tryCount As Integer = 0
            Dim delay As Integer = _exponentialBackOffDelayMs

            Do Until tryCount = MaxRetry

                If pageUri Is Nothing Then
                    Throw New ArgumentNullException(NameOf(pageUri), "Null uri")
                End If

                If cookies Is Nothing Then
                    cookies = New CookieContainer
                End If

                Dim RequestDelayInMS As Double = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                While (RequestDelayInMS < _requestDelay) 'Limit post requests per second
                    Thread.Sleep(1)
                    SyncLock RequestLock
                        RequestDelayInMS = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                    End SyncLock
                End While
                SyncLock RequestLock
                    LastRequestTimestamp = Date.UtcNow
                End SyncLock

                Dim tempcookies As CookieContainer = cookies

                Dim handler As HttpClientHandler = New HttpClientHandler With {
            .CookieContainer = cookies,
            .UseCookies = True
        }
                Dim client As HttpClient = New HttpClient(handler)
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent)
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive")
                client.DefaultRequestHeaders.Add("Method", "GET")

                Dim response As MemoryStream = Nothing

                Try
                    Dim message As Task(Of HttpResponseMessage) = client.GetAsync(pageUri)
                    Dim res As HttpResponseMessage = message.Result
                    Dim theaders As Headers.HttpResponseHeaders = res.Headers

                    ' Read the content as a byte array and then convert to MemoryStream
                    Dim contentBytes As Byte() = res.Content.ReadAsByteArrayAsync().Result
                    response = New MemoryStream(contentBytes)

                    tempcookies.Add(cookies.GetCookies(pageUri))

                Catch ex As System.Net.WebException
                    tryCount += 1
                    delay = delay * 2 ' exponential backoff
                    Thread.Sleep(delay)
#Disable Warning CA1031
                Catch ex2 As Exception
                    tryCount += 1
                    delay = delay * 2 ' exponential backoff
                    Thread.Sleep(delay)
#Enable Warning CA1031
                Finally
                    client.Dispose()
                End Try

                If Not response Is Nothing Then
                    cookies = tempcookies
                    Return response
                End If

                ' If we get here, it means the response was Nothing, which shouldn't happen with MemoryStream
                ' but we'll handle it just in case
                Return Nothing
            Loop
            Throw New MaxRetriesExeption
        End Function

        Private Function AdaptEncoding(ByVal proposedtext As String) As String
            'Convierte el texto a un array de bytes usando UTF-8
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(proposedtext)

            'Lista de Encoding a probar
            Dim encodings As Encoding() = {
                Encoding.UTF8,
                Encoding.GetEncoding(28591),
                Encoding.Unicode,
                Encoding.BigEndianUnicode,
                Encoding.UTF32
            }

            For Each encoding As Encoding In encodings
                Try
                    Return encoding.GetString(bytes)
                Catch ex As DecoderFallbackException
                End Try
            Next

            'Rendirse! retornar el texto original
            Return proposedtext
        End Function

        ''' <summary>Realiza una solicitud de tipo POST a un recurso web y retorna el texto.</summary>
        ''' <param name="pageURI">URI absoluta del recurso web.</param>
        ''' <param name="postData">Cadena de texto que se envia en el POST.</param>
        Public Function PostDataAndGetResult(pageUri As Uri, postData As String) As String
            Return PostDataAndGetResult(pageUri, postData, New CookieContainer)
        End Function

        ''' <summary>Realiza una solicitud de tipo POST a un recurso web y retorna el texto uando las cookies de sesión.</summary>
        ''' <param name="pageURI">URI absoluta del recurso web.</param>
        ''' <param name="postData">Cadena de texto que se envia en el POST.</param>
        Public Function PostDataAndGetResultWithCookies(pageUri As Uri, postData As String) As String
            Return PostDataAndGetResult(pageUri, postData, ApiCookies)
        End Function

        ''' <summary>Realiza una solicitud de tipo POST a un recurso web y retorna el texto.</summary>
        ''' <param name="pageUri">URL absoluta del recurso web.</param>
        ''' <param name="postData">Cadena de texto que se envia en el POST.</param>
        Public Function PostDataAndGetResult(pageUri As Uri, postData As String, ByRef cookies As CookieContainer, Optional retrycount As Integer = 0) As String
            Return PostDataAndGetResultInternal(pageUri, postData, Nothing, cookies, retrycount)
        End Function

        ''' <summary>
        ''' Implementación interna común para todas las solicitudes POST (con o sin cabeceras personalizadas).
        ''' Usa el mismo patrón de reintentos con backoff exponencial que los métodos GET de la clase.
        ''' </summary>
        ''' <param name="pageUri">URI del recurso.</param>
        ''' <param name="postData">Datos del cuerpo de la solicitud.</param>
        ''' <param name="customHeaders">Cabeceras personalizadas o Nothing.</param>
        ''' <param name="cookies">Contenedor de cookies (se actualiza).</param>
        ''' <param name="retrycount">Reservado por compatibilidad de firma (el control de reintentos es interno).</param>
        ''' <returns>Cuerpo de la respuesta o Nothing.</returns>
        Private Function PostDataAndGetResultInternal(pageUri As Uri, postData As String, customHeaders As Dictionary(Of String, String), ByRef cookies As CookieContainer, retrycount As Integer) As String
            Dim tryCount As Integer = 0
            Dim delay As Integer = _exponentialBackOffDelayMs

            Do Until tryCount = MaxRetry
                If pageUri Is Nothing Then
                    Throw New ArgumentNullException(NameOf(pageUri), "Empty uri.")
                End If

                If cookies Is Nothing Then
                    cookies = New CookieContainer
                End If

                Dim RequestDelayInMS As Double = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                While (RequestDelayInMS < _requestDelay)
                    Thread.Sleep(1)
                    SyncLock RequestLock
                        RequestDelayInMS = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                    End SyncLock
                End While
                SyncLock RequestLock
                    LastRequestTimestamp = Date.UtcNow
                End SyncLock

                Dim tempcookies As CookieContainer = cookies

                Dim encoding As New Text.UTF8Encoding
                Dim byteData As Byte() = encoding.GetBytes(If(postData, String.Empty))
                Dim handler As HttpClientHandler = New HttpClientHandler With {
                    .CookieContainer = cookies,
                    .UseCookies = True
                }

                Dim client As HttpClient = New HttpClient(handler)
                Dim content As StringContent = New StringContent(If(postData, String.Empty))
                content.Headers.ContentType = Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
                content.Headers.ContentLength = byteData.Length
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent)
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive")
                client.DefaultRequestHeaders.Add("Method", "POST")
                client.Timeout = New TimeSpan(0, 0, 30)

                If customHeaders IsNot Nothing Then
                    For Each header As KeyValuePair(Of String, String) In customHeaders
                        If String.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase) Then
                            content.Headers.ContentType = Headers.MediaTypeHeaderValue.Parse(header.Value)
                        ElseIf Not String.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase) Then
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value)
                        End If
                    Next
                End If

                Dim response As String = Nothing
                Try
                    Dim message As Task(Of HttpResponseMessage) = client.PostAsync(pageUri, content)
                    Dim res As HttpResponseMessage = message.Result
                    response = res.Content.ReadAsStringAsync.Result()
                    tempcookies.Add(cookies.GetCookies(pageUri))
                Catch ex As System.Net.WebException
                    tryCount += 1
                    delay = delay * 2
                    Thread.Sleep(delay)
#Disable Warning CA1031
                Catch ex2 As Exception
                    tryCount += 1
                    delay = delay * 2
                    Thread.Sleep(delay)
#Enable Warning CA1031
                Finally
                    client.Dispose()
                    content.Dispose()
                End Try

                If response IsNot Nothing Then
                    cookies = tempcookies
                    Return AdaptEncoding(response)
                End If
            Loop

            Return Nothing
        End Function

        ''' <summary>
        ''' Extrae los headers de la respuesta HTTP en un diccionario (valores múltiples unidos por coma).
        ''' </summary>
        Private Function ExtractResponseHeaders(response As HttpResponseMessage) As Dictionary(Of String, String)
            Dim headers As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            If response Is Nothing Then
                Return headers
            End If

            For Each header As KeyValuePair(Of String, IEnumerable(Of String)) In response.Headers
                headers(header.Key) = String.Join(", ", header.Value)
            Next

            If response.Content IsNot Nothing Then
                For Each header As KeyValuePair(Of String, IEnumerable(Of String)) In response.Content.Headers
                    headers(header.Key) = String.Join(", ", header.Value)
                Next
            End If

            Return headers
        End Function

        ''' <summary>
        ''' Realiza una solicitud GET y retorna el contenido junto con la URI final (tras redirecciones), el código de estado y los headers.
        ''' Usa las cookies de sesión actuales del bot (ApiCookies). Si se requiere una solicitud sin cookies de sesión, use la sobrecarga que recibe un CookieContainer.
        ''' </summary>
        Public Function GetWithDetails(ByVal address As String) As HttpResult
            If String.IsNullOrWhiteSpace(address) Then
                Throw New ArgumentException("Address cannot be null or empty", NameOf(address))
            End If
            Return GetWithDetails(New Uri(address))
        End Function

        ''' <summary>
        ''' Realiza una solicitud GET y retorna el contenido junto con la URI final (tras redirecciones), el código de estado y los headers.
        ''' Usa las cookies de sesión actuales del bot (ApiCookies). Si se requiere una solicitud sin cookies de sesión, use la sobrecarga que recibe un CookieContainer.
        ''' </summary>
        Public Function GetWithDetails(ByVal pageUri As Uri) As HttpResult
            Return GetWithDetails(pageUri, ApiCookies)
        End Function

        ''' <summary>
        ''' Realiza una solicitud GET y retorna el contenido junto con la URI final (tras redirecciones), el código de estado y los headers.
        ''' </summary>
        ''' <param name="pageUri">URI absoluta del recurso.</param>
        ''' <param name="cookies">Cookies (se actualizan con la respuesta).</param>
        Public Function GetWithDetails(ByVal pageUri As Uri, ByRef cookies As CookieContainer) As HttpResult
            Dim tryCount As Integer = 0
            Dim delay As Integer = _exponentialBackOffDelayMs

            Do Until tryCount = MaxRetry
                If pageUri Is Nothing Then
                    Throw New ArgumentNullException(NameOf(pageUri), "Null uri")
                End If

                If cookies Is Nothing Then
                    cookies = New CookieContainer
                End If

                Dim RequestDelayInMS As Double = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                While (RequestDelayInMS < _requestDelay)
                    Thread.Sleep(1)
                    SyncLock RequestLock
                        RequestDelayInMS = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                    End SyncLock
                End While
                SyncLock RequestLock
                    LastRequestTimestamp = Date.UtcNow
                End SyncLock

                Dim tempcookies As CookieContainer = cookies
                Dim handler As HttpClientHandler = New HttpClientHandler With {
                    .CookieContainer = cookies,
                    .UseCookies = True
                }
                Dim client As HttpClient = New HttpClient(handler)
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent)
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive")
                client.DefaultRequestHeaders.Add("Method", "GET")

                Dim responseContent As String = Nothing
                Dim finalUri As Uri = pageUri
                Dim statusCode As HttpStatusCode = HttpStatusCode.OK
                Dim headersDict As Dictionary(Of String, String) = Nothing

                Try
                    Dim message As Task(Of HttpResponseMessage) = client.GetAsync(pageUri)
                    Dim res As HttpResponseMessage = message.Result
                    If res.RequestMessage IsNot Nothing AndAlso res.RequestMessage.RequestUri IsNot Nothing Then
                        finalUri = res.RequestMessage.RequestUri
                    End If
                    statusCode = res.StatusCode
                    headersDict = ExtractResponseHeaders(res)
                    responseContent = res.Content.ReadAsStringAsync().Result()
                    tempcookies.Add(cookies.GetCookies(pageUri))
                Catch ex As System.Net.WebException
                    tryCount += 1
                    delay = delay * 2
                    Thread.Sleep(delay)
#Disable Warning CA1031
                Catch ex2 As Exception
                    tryCount += 1
                    delay = delay * 2
                    Thread.Sleep(delay)
#Enable Warning CA1031
                Finally
                    client.Dispose()
                End Try

                If responseContent IsNot Nothing Then
                    cookies = tempcookies
                    Return New HttpResult(AdaptEncoding(responseContent), finalUri, statusCode, headersDict)
                End If
            Loop

            Throw New MaxRetriesExeption
        End Function

        ''' <summary>
        ''' Realiza una solicitud POST y retorna el contenido junto con la URI final (tras redirecciones), el código de estado y los headers.
        ''' Usa las cookies de sesión actuales del bot (ApiCookies). Si se requiere una solicitud sin cookies de sesión, use la sobrecarga que recibe un CookieContainer.
        ''' </summary>
        Public Function PostWithDetails(pageUri As Uri, postData As String) As HttpResult
            Return PostWithDetails(pageUri, postData, ApiCookies)
        End Function

        ''' <summary>
        ''' Realiza una solicitud POST y retorna el contenido junto con la URI final (tras redirecciones), el código de estado y los headers.
        ''' </summary>
        ''' <param name="pageUri">URI absoluta del recurso.</param>
        ''' <param name="postData">Datos del cuerpo.</param>
        ''' <param name="cookies">Cookies (se actualizan con la respuesta).</param>
        Public Function PostWithDetails(pageUri As Uri, postData As String, ByRef cookies As CookieContainer) As HttpResult
            Return PostWithDetails(pageUri, postData, Nothing, cookies)
        End Function

        ''' <summary>
        ''' Realiza una solicitud POST (con cabeceras personalizadas opcionales) y retorna el contenido junto con la URI final, el código de estado y los headers.
        ''' Usa las cookies de sesión actuales del bot (ApiCookies). Si se requiere una solicitud sin cookies de sesión, use la sobrecarga que recibe un CookieContainer.
        ''' </summary>
        ''' <param name="pageUri">URI absoluta del recurso.</param>
        ''' <param name="postData">Datos del cuerpo.</param>
        ''' <param name="customHeaders">Cabeceras personalizadas o Nothing.</param>
        Public Function PostWithDetails(pageUri As Uri, postData As String, customHeaders As Dictionary(Of String, String)) As HttpResult
            Return PostWithDetails(pageUri, postData, customHeaders, ApiCookies)
        End Function

        ''' <summary>
        ''' Realiza una solicitud POST (con cabeceras personalizadas opcionales) y retorna el contenido junto con la URI final, el código de estado y los headers.
        ''' </summary>
        ''' <param name="pageUri">URI absoluta del recurso.</param>
        ''' <param name="postData">Datos del cuerpo.</param>
        ''' <param name="customHeaders">Cabeceras personalizadas o Nothing.</param>
        ''' <param name="cookies">Cookies (se actualizan con la respuesta).</param>
        Public Function PostWithDetails(pageUri As Uri, postData As String, customHeaders As Dictionary(Of String, String), ByRef cookies As CookieContainer) As HttpResult
            Dim tryCount As Integer = 0
            Dim delay As Integer = _exponentialBackOffDelayMs

            Do Until tryCount = MaxRetry
                If pageUri Is Nothing Then
                    Throw New ArgumentNullException(NameOf(pageUri), "Null uri")
                End If

                If cookies Is Nothing Then
                    cookies = New CookieContainer
                End If

                Dim RequestDelayInMS As Double = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                While (RequestDelayInMS < _requestDelay)
                    Thread.Sleep(1)
                    SyncLock RequestLock
                        RequestDelayInMS = (Date.UtcNow - LastRequestTimestamp).TotalMilliseconds
                    End SyncLock
                End While
                SyncLock RequestLock
                    LastRequestTimestamp = Date.UtcNow
                End SyncLock

                Dim tempcookies As CookieContainer = cookies
                Dim encoding As New Text.UTF8Encoding
                Dim byteData As Byte() = encoding.GetBytes(If(postData, String.Empty))
                Dim handler As HttpClientHandler = New HttpClientHandler With {
                    .CookieContainer = cookies,
                    .UseCookies = True
                }
                Dim client As HttpClient = New HttpClient(handler)
                Dim content As StringContent = New StringContent(If(postData, String.Empty))
                content.Headers.ContentType = Headers.MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded")
                content.Headers.ContentLength = byteData.Length
                client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent)
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive")
                client.DefaultRequestHeaders.Add("Method", "POST")
                client.Timeout = New TimeSpan(0, 0, 30)

                If customHeaders IsNot Nothing Then
                    For Each header As KeyValuePair(Of String, String) In customHeaders
                        If String.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase) Then
                            content.Headers.ContentType = Headers.MediaTypeHeaderValue.Parse(header.Value)
                        ElseIf Not String.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase) Then
                            client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value)
                        End If
                    Next
                End If

                Dim responseContent As String = Nothing
                Dim finalUri As Uri = pageUri
                Dim statusCode As HttpStatusCode = HttpStatusCode.OK
                Dim headersDict As Dictionary(Of String, String) = Nothing

                Try
                    Dim message As Task(Of HttpResponseMessage) = client.PostAsync(pageUri, content)
                    Dim res As HttpResponseMessage = message.Result
                    If res.RequestMessage IsNot Nothing AndAlso res.RequestMessage.RequestUri IsNot Nothing Then
                        finalUri = res.RequestMessage.RequestUri
                    End If
                    statusCode = res.StatusCode
                    headersDict = ExtractResponseHeaders(res)
                    responseContent = res.Content.ReadAsStringAsync().Result()
                    tempcookies.Add(cookies.GetCookies(pageUri))
                Catch ex As System.Net.WebException
                    tryCount += 1
                    delay = delay * 2
                    Thread.Sleep(delay)
#Disable Warning CA1031
                Catch ex2 As Exception
                    tryCount += 1
                    delay = delay * 2
                    Thread.Sleep(delay)
#Enable Warning CA1031
                Finally
                    client.Dispose()
                    content.Dispose()
                End Try

                If responseContent IsNot Nothing Then
                    cookies = tempcookies
                    Return New HttpResult(AdaptEncoding(responseContent), finalUri, statusCode, headersDict)
                End If
            Loop

            Throw New MaxRetriesExeption
        End Function
    End Class

End Namespace