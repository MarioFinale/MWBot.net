Option Strict On
Option Explicit On
Imports System.Text.RegularExpressions
Imports MWBot.net.GlobalVars
Imports Utils.Utils
Imports LogEngine
Imports MWBot.net.My.Resources
Imports System.Text.Json
Imports System.Net.Sockets
Imports System.IO
Imports System.Net

Namespace WikiBot
#Disable Warning CA1822
#Disable Warning CA1031
    ''' <summary>
    ''' Clase que media entre el programa y la API MediaWiki.
    ''' </summary>
    Public Class Bot

#Region "Properties"
        Private _botPassword As String
        Private _botUserName As String
        Private Api As ApiHandler

        ''' <summary>
        ''' Ruta del archivo PSV que contiene el registro de eventos.
        ''' </summary>
        ''' <returns></returns>
        Property LogPath As String
            Set(value As String)
                Log_Filepath = value
                EventLogger.LogPath = value
            End Set
            Get
                Return Log_Filepath
            End Get
        End Property

        ''' <summary>
        ''' Indica si la cuenta utilizada posee el permiso "bot".
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Bot As Boolean
            Get
                Dim postdata As String = SStrings.AssertBotData
                Dim postresponse As String = POSTQUERY(postdata)
                If postresponse.Contains(SStrings.AssertBotFailed) Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        ''' <summary>
        ''' Indica si la cuenta utilizada está logueada en la API.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LoggedIn As Boolean
            Get
                Dim postdata As String = SStrings.AssertUserData
                Dim postresponse As String = POSTQUERY(postdata)
                If postresponse.Contains(SStrings.AssertUserFailed) Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        ''' <summary>
        ''' Indica la URI de la API utilizada por el bot.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ApiUri As Uri

        ''' <summary>
        ''' Entrega el usuario del bot según la api.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UserName As String

        ''' <summary>
        ''' Nombre interno del bot.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LocalName As String

        ''' <summary>
        ''' Uri de la wiki.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property WikiUri As Uri

        ''' <summary>
        ''' Entrega el ApiHandler interno del bot.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BotApiHandler As ApiHandler
            Get
                Return Api
            End Get
        End Property

#End Region

#Region "Init"

        ''' <summary>
        ''' Crea una nueva instancia del bot e intenta loguear en la API.
        ''' </summary>
        ''' <param name="configfile">Archivo de configuración.</param>
        ''' <param name="logeng">Archivo de LOG.</param>
        Sub New(ByVal configfile As String, ByRef logeng As LogEngine.LogEngine)
            SetLogEngine(logeng)
            Dim valid As Boolean = LoadConfig(configfile)
            Do Until valid
                valid = LoadConfig(configfile)
            Loop
            Api = New ApiHandler(_botUserName, _botPassword, _ApiUri)
            _UserName = Api.UserName
        End Sub

        Public Sub SetLogEngine(ByRef eng As LogEngine.LogEngine)
            EventLogger = eng
        End Sub

        ''' <summary>
        ''' Intenta loguear de nuevo en la API.
        ''' </summary>
        Public Sub Relogin()
            Api = New ApiHandler(_botUserName, _botPassword, _ApiUri)
        End Sub

        Function SetLogConfig(ByVal Logpath As String, Userpath As String, BotName As String, Verbose As Boolean) As Boolean
            Try
                Dim newLogger As New LogEngine.LogEngine(Logpath, Userpath, BotName, Verbose)
                EventLogger = newLogger
            Catch ex As Exception
                EventLogger.EX_Log(ex.Message, "MWBOT.Net:SetLogConfig", "N/A")
                Return False
            End Try
            Return True
        End Function

        ''' <summary>
        ''' Inicializa las configuraciones genereales del programa desde el archivo de configuración.
        ''' Si no existe el archivo, solicita datos al usuario y lo genera.
        ''' </summary>
        ''' <returns></returns>
        Function LoadConfig(ByVal Tfile As String) As Boolean
            If Tfile Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            Dim MainBotName As String = String.Empty
            Dim WPSite As String = String.Empty
            Dim WPAPI As String = String.Empty
            Dim WPBotUserName As String = String.Empty
            Dim WPBotPassword As String = String.Empty
            Dim ConfigOK As Boolean = False
            Console.WriteLine(String.Format(Messages.GreetingMsg, MwBotVersion))

            EventLogger.Debug_Log(Messages.BotEngine & " " & MwBotVersion, Reflection.MethodBase.GetCurrentMethod().Name)
            If System.IO.File.Exists(Tfile) Then
                EventLogger.Log(Messages.LoadingConfig, Reflection.MethodBase.GetCurrentMethod().Name)
                Dim Configstr As String = System.IO.File.ReadAllText(Tfile)
                Try
                    MainBotName = TextInBetween(Configstr, "BOTName=""", """")(0)
                    WPBotUserName = TextInBetween(Configstr, "WPUserName=""", """")(0)
                    WPSite = TextInBetween(Configstr, "PageURL=""", """")(0)
                    WPBotPassword = TextInBetween(Configstr, "WPBotPassword=""", """")(0)
                    WPAPI = TextInBetween(Configstr, "ApiURL=""", """")(0)
                    ConfigOK = True
                Catch ex As IndexOutOfRangeException
                    EventLogger.Log(Messages.ConfigError, Reflection.MethodBase.GetCurrentMethod().Name)
                End Try
            Else
                EventLogger.Log(Messages.NoConfigFile, Reflection.MethodBase.GetCurrentMethod().Name)
                Try
                    System.IO.File.Create(Tfile.ToString).Close()
                Catch ex As System.IO.IOException
                    EventLogger.Log(Messages.NewConfigFileError, Reflection.MethodBase.GetCurrentMethod().Name)
                End Try
            End If

            If Not ConfigOK Then
                Console.Clear()
                Console.WriteLine(Messages.NewConfigMessage)
                Console.WriteLine(Messages.NewBotName)
                MainBotName = Console.ReadLine
                Console.WriteLine(Messages.NewUserName)
                WPBotUserName = Console.ReadLine
                WPBotUserName &= "@" & WPBotUserName
                Console.WriteLine(Messages.NewBotPassword)
                WPBotPassword = Console.ReadLine
                Console.WriteLine(Messages.NewWikiMainUrl)
                WPSite = Console.ReadLine
                WPAPI = WPSite & "/w/api.php"
                Dim configstr As String = String.Format(SStrings.ConfigTemplate, MainBotName, WPBotUserName, WPBotPassword, WPSite, WPAPI)
                Try
                    System.IO.File.WriteAllText(Tfile, configstr)
                Catch ex As System.IO.IOException
                    EventLogger.Log(Messages.SaveConfigError, Reflection.MethodBase.GetCurrentMethod().Name)
                End Try
            End If

            _LocalName = MainBotName
            _botUserName = WPBotUserName
            _botPassword = WPBotPassword

            Try
                _ApiUri = New Uri(WPAPI)
                _WikiUri = New Uri(WPSite)
            Catch ex As ArgumentException
                EventLogger.Log(Messages.InvalidUrl, Reflection.MethodBase.GetCurrentMethod().Name)
                System.IO.File.Delete(Tfile)
                WaitSeconds(5)
                Return False
            Catch ex2 As UriFormatException
                EventLogger.Log(Messages.InvalidUrl, Reflection.MethodBase.GetCurrentMethod().Name)
                System.IO.File.Delete(Tfile)
                PressKeyTimeout(5)
                Return False
            End Try
            Return True

        End Function

#End Region

#Region "ApiFunctions"
        ''' <summary>
        ''' Envía una solicitud POST a la API.
        ''' </summary>
        ''' <param name="postdata">Datos dentro del cuerpo de la solicitud POST.</param>
        ''' <returns></returns>
        Function POSTQUERY(ByVal postdata As String) As String
            Return Api.Postquery(postdata)
        End Function

        ''' <summary>
        ''' Envía una solicitud GET a la API.
        ''' </summary>
        ''' <param name="getdata">Datos dentro del cuerpo de la solicitud GET. (en bruto, luego del '?')</param>
        ''' <returns></returns>
        Function GETQUERY(ByVal getdata As String) As String
            Return Api.Getquery(getdata)
        End Function

        ''' <summary>
        ''' Envía una solicitud GET a la uri indicada.
        ''' </summary>
        ''' <param name="turi">Uri.</param>
        ''' <returns></returns>
        Function [GET](ByVal turi As Uri) As String
            Return Api.GET(turi)
        End Function

        ''' <summary>
        ''' Envía una solicitud POST a la uri indicada.
        ''' </summary>
        ''' <param name="turi">Uri.</param>
        ''' <param name="postData">Cuerpo de la solicitud POST.</param>
        ''' <returns></returns>
        Function POST(ByVal turi As Uri, postData As String) As String
            Return Api.POST(turi, postData)
        End Function
#End Region

#Region "BotFunctions"

        ''' <summary>
        ''' Realiza varias pruebas para verificar las funciones del bot.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function StartUpCheck() As Boolean
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Loading Enviroment info", "StartUpCheck")

            Try
                Dim OS As String = Utils.Utils.GetOsString
                Dim osdesc As String = Utils.Utils.GetOSDescription
                Dim platform As String = Utils.Utils.GetPlatform
                EventLogger.Log("OS: " & OS, "StartUpCheck")
                EventLogger.Log("OS description: " & osdesc, "StartUpCheck")
                EventLogger.Log("Platform: " & platform, "StartUpCheck")

            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Creating a TCP client", "StartUpCheck")
            Dim tclient As TcpClient
            Try
#Disable Warning IDE0068 ' Use recommended dispose pattern
                tclient = New TcpClient("chat.freenode.net", 6667) With {
                .ReceiveTimeout = 10000,
                .SendTimeout = 10000}
#Enable Warning IDE0068 ' Use recommended dispose pattern
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Creating a network stream", "StartUpCheck")
            Dim netStream As NetworkStream
            Try
                netStream = tclient.GetStream()
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Creating streamreader and streamwriter", "StartUpCheck")
            Dim tReader As StreamReader
            Dim tWriter As StreamWriter
            Try
#Disable Warning IDE0068 ' Use recommended dispose pattern
                tReader = New StreamReader(netStream)
                tWriter = New StreamWriter(netStream)
#Enable Warning IDE0068 ' Use recommended dispose pattern
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Reading and writting to the stream", "StartUpCheck")
            Try
                tReader.ReadLine()
                tWriter.WriteLine("NICK MWBOTTEST")
                tWriter.Flush()
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Disposing Objects", "StartUpCheck")
            Try
                tReader.Dispose()
                tWriter.Dispose()
                netStream.Dispose()
                tclient.Dispose()
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Logging in...", "StartUpCheck")
            Dim tbot As Bot
            Try
                tbot = New Bot("./Config.cfg", New LogEngine.LogEngine("./Log.psv", "./Users.psv", "StartUpCheck", True))
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Loading a page", "StartUpCheck")
            Dim tpage As Page
            Try
                tpage = tbot.Getpage("Sol")
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Loading a user", "StartUpCheck")
            Dim tuser As WikiUser
            Try
                tuser = New WikiUser(tbot, "MarioFinale")
                EventLogger.Log("User loaded", "StartUpCheck")
                EventLogger.Log("User name: " & tuser.UserName, "StartUpCheck")
                EventLogger.Log("Edit count: " & tuser.EditCount.ToString(), "StartUpCheck")
                EventLogger.Log("Is blocked: " & tuser.Blocked.ToString(), "StartUpCheck")
                EventLogger.Log("Last edit: " & tuser.LastEdit.ToShortDateString() & " " & tuser.LastEdit.ToShortTimeString(), "StartUpCheck")

            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Editing a page", "StartUpCheck")
            Dim botusername As String = tbot.UserName
            Dim testpagename As String = "User:" & botusername & "/MWBot-TEST"
            Dim testString As String = "IT WORKS! " & Date.UtcNow.ToShortDateString & " " & Date.UtcNow.ToLongTimeString
            Dim testpage As Page
            Try
                testpage = tbot.Getpage(testpagename)
                testpage.Save(testString, "Test")
                EventLogger.Log("Checking page edit", "StartUpCheck")
                testpage = tbot.Getpage(testpagename)
                If Not testpage.Content = testString Then
                    EventLogger.Log("The page has not been saved correctly", "StartUpCheck")
                    EventLogger.Log("Test failed", "StartUpCheck")
                    Return False
                End If
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Creating a new Func (RecentChanges Stream)", "StartUpCheck")
            Dim tfunc As Func(Of Boolean)
            Try
                tfunc = New Func(Of Boolean)(Function()
                                                 Dim ftclient As WebClient = New WebClient()
                                                 Dim ftstream As Stream = ftclient.OpenRead(New Uri("https://stream.wikimedia.org/v2/stream/recentchange"))
                                                 Dim ftstreamreader As StreamReader = New StreamReader(ftstream)
                                                 For i As Integer = 0 To 20
                                                     Dim currentLine As String = ftstreamreader.ReadLine()
                                                     Console.WriteLine(currentLine)
                                                 Next
                                                 Return True
                                             End Function)
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Creating a new taskadmin", "StartUpCheck")
            Dim taskadmin As TaskAdmin

            Try
                taskadmin = New TaskAdmin()
            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.WriteLine()
            Console.WriteLine("==================================== BEGIN TEST ========================================")
            EventLogger.Log("Adding a func to the taskadmin and running it", "StartUpCheck")
            Console.WriteLine("3...")
            System.Threading.Thread.Sleep(1000)
            Console.WriteLine("2...")
            System.Threading.Thread.Sleep(1000)
            Console.WriteLine("1...")
            System.Threading.Thread.Sleep(1000)
            Try
                taskadmin.NewTask("Test func", "TEST", tfunc, 0, False)

            Catch ex As Exception
                EventLogger.Log("Test failed", "StartUpCheck")
                EventLogger.Log(ex.Source, "StartUpCheck")
                EventLogger.Log(ex.Message, "StartUpCheck")
                EventLogger.Log(ex.StackTrace, "StartUpCheck")
                Return False
            End Try
            While (taskadmin.TaskList.Count > 0)
                System.Threading.Thread.Sleep(1000)
            End While
            Console.WriteLine("===================================== END TEST ==========================================")
            Console.Clear()
            Console.WriteLine("Tests passed.")
            Console.WriteLine("Operation should be normal.")
            Console.WriteLine()
            Console.WriteLine()
            Console.WriteLine("                                                             - Good luck")
            Return True
        End Function

        ''' <summary>
        ''' Entrega una lista con las URL en la lista negra en formato de expresión regular.
        ''' </summary>
        ''' <param name="spamlistPage">Pagina que contiene la lista negra.</param>
        ''' <returns></returns>
        Public Function GetSpamListregexes(ByVal spamlistPage As Page) As String()
            If spamlistPage Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            Dim Lines As String() = GetLines(spamlistPage.Content, True) 'Extraer las líneas del texto de la página
            Dim Regexes As New List(Of String) 'Declarar lista con líneas con expresiones regulares

            For Each l As String In Lines 'Por cada línea...
                Dim tempText As String = l
                If l.Contains("#"c) Then 'Si contiene un comentario
                    tempText = tempText.Split("#"c)(0) 'Obtener el texto antes del comentario
                End If
                tempText = tempText.Trim() 'Eliminar los espacios en blanco
                If Not String.IsNullOrWhiteSpace(tempText) Then 'Verificar que no esté vacio
                    Regexes.Add(tempText) 'Añadir a la lista
                End If
            Next
            Return Regexes.ToArray
        End Function


        ''' <summary>
        ''' Retorna la última revisión de las paginas indicadas, las paginas deben ser distintas. 
        ''' En caso de no existir la pagina, retorna -1 como REVID.
        ''' </summary>
        ''' <param name="pageNames">Hashset de nombres de paginas unicos.</param>
        ''' <remarks></remarks>
        Function GetLastRevisions(ByVal pageNames As HashSet(Of String)) As HashSet(Of WikiRevision)
            EventLogger.Debug_Log(String.Format(Messages.GetLastrevIDs, pageNames.Count), Reflection.MethodBase.GetCurrentMethod().Name)
            Dim PageNamesList As List(Of String) = pageNames.ToList
            PageNamesList.Sort()
            Dim PageList As List(Of List(Of String)) = SplitStringArrayIntoChunks(PageNamesList.ToArray, 50)
            Dim RevisionSet As New HashSet(Of WikiRevision)
            For Each ListInList As List(Of String) In PageList
                Dim Qstring As String = ConcatenateTextArrayWithChar(ListInList.ToArray, "|"c, True)
                Dim queryResponse As String = GETQUERY((SStrings.GetLastRevIds & Qstring))
                Dim response As JsonDocument = GetJsonDocument(queryResponse)
                Dim responseElement As JsonElement = response.RootElement
                Dim query As JsonElement = GetJsonElement(responseElement, "query")
                Dim pages As JsonElement = GetJsonElement(query, "pages")
                Dim ttext As String = pages.ToString

                For Each pageinfo As JsonProperty In pages.EnumerateObject
                    Dim pageElement As JsonElement = pageinfo.Value
                    Dim ns As Integer = pageElement.GetProperty("ns").GetInt32
                    Dim title As String = NormalizeUnicodetext(pageElement.GetProperty("title").GetString).Replace(" ", "_")
                    Dim missing As Boolean = IsJsonPropertyPresent(pageElement, "missing")

                    If missing Then
                        Dim missingRevision As New WikiRevision With {
                            .Title = title,
                            .Missing = missing}
                        RevisionSet.Add(missingRevision)
                        Continue For
                    End If
                    Dim pageid As Integer = pageElement.GetProperty("pageid").GetInt32
                    Dim revisions As JsonElement = GetJsonElement(pageElement, "revisions")
                    revisions = revisions(0)
                    Dim revid As Integer = revisions.GetProperty("revid").GetInt32()
                    Dim parentid As Integer = revisions.GetProperty("parentid").GetInt32()
                    Dim minor As Boolean = IsJsonPropertyPresent(revisions, "minor")
                    Dim user As String = revisions.GetProperty("user").GetString
                    Dim comment As String = revisions.GetProperty("comment").GetString
                    Dim timestamp As Date = GetDateFromMWTimestamp(revisions.GetProperty("timestamp").GetString)
                    Dim revision As New WikiRevision With {
                        .NS = ns,
                        .Comment = comment,
                        .Minor = minor,
                        .Missing = missing,
                        .PageID = pageid,
                        .ParentID = parentid,
                        .RevID = revid,
                        .Timestamp = timestamp,
                        .Title = title,
                        .User = user
                        }
                    RevisionSet.Add(revision)
                Next
            Next
            EventLogger.Debug_Log(String.Format(Messages.DoneXPagesReturned, RevisionSet.Count), Reflection.MethodBase.GetCurrentMethod().Name)
            Return RevisionSet
        End Function

        ''' <summary>
        ''' Entrega los títulos de las páginas que coincidan remotamente con el texto entregado como parámetro.
        ''' Usa las mismas sugerencias del cuadro de búsqueda de Wikipedia, pero por medio de la API.
        ''' Si no hay coincidencia, entrega un un string() vacio.
        ''' </summary>
        ''' <param name="PageName">Título aproximado o similar al de una página</param>
        ''' <returns></returns>
        Function SearchForPages(pageName As String) As String()
            Return GetTitlesFromQueryText(GETQUERY(SStrings.Search & pageName))
        End Function

        ''' <summary>
        ''' Entrega el título de la primera página que coincida remotamente con el texto entregado como parámetro.
        ''' Usa las mismas sugerencias del cuadro de búsqueda de Wikipedia, pero por medio de la API.
        ''' Si no hay coincidencia, entrega una cadena de texto vacía.
        ''' </summary>
        ''' <param name="PageName">Título aproximado o similar al de una página</param>
        ''' <returns></returns>
        Function SearchForPage(pageName As String) As String
            Dim titles As String() = SearchForPages(pageName)
            If titles.Count >= 1 Then
                Return titles(0)
            Else
                Return String.Empty
            End If
        End Function

        ''' <summary>
        ''' Entrega la primera página que coincida remotamente con el texto entregado como parámetro.
        ''' Usa las mismas sugerencias del cuadro de búsqueda de Wikipedia, pero por medio de la API.
        ''' Si no hay coincidencia, no retorna nada.
        ''' </summary>
        ''' <param name="PageName"></param>
        ''' <returns></returns>
        Function GetSearchedPage(pageName As String) As Page
            Dim titles As String() = SearchForPages(pageName)
            If titles.Count >= 1 Then
                Return Getpage(titles(0))
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Retorna el valor ORES (en %) de los EDIT ID (eswiki) indicados como SortedList (con el formato {ID,Score}), los EDIT ID deben ser distintos. 
        ''' En caso de no existir el EDIT ID, retorna {0,0}.
        ''' </summary>
        ''' <param name="revids">Array con EDIT ID's unicos.</param>
        ''' <remarks>Los EDIT ID deben ser distintos</remarks>
        Function GetORESScores(ByVal revids As Integer()) As SortedList(Of Integer, Double())
            Dim Revlist As List(Of List(Of Integer)) = SplitIntegerArrayIntoChunks(revids, 50)
            Dim EditAndScoreList As New SortedList(Of Integer, Double())
            For Each ListOfList As List(Of Integer) In Revlist

                Dim QueryString As String = String.Empty
                For Each n As Integer In ListOfList
                    QueryString = QueryString & n.ToString & "|"
                Next
                QueryString = QueryString.Trim(CType("|", Char))
                Dim apiuri As Uri = New Uri(SStrings.OresScoresApiQueryUrl & UrlWebEncode(QueryString))
                Dim apiRawResponse As String = Api.GET(apiuri)
                Dim response As JsonDocument = GetJsonDocument(apiRawResponse)
                Dim eswiki As JsonElement = GetJsonElement(response, "eswiki")
                Dim scores As JsonElement = eswiki.GetProperty("scores")
                For Each revid As JsonProperty In scores.EnumerateObject
                    Dim editID As Integer = 0
                    Integer.TryParse(revid.Name, editID)
                    Dim element As JsonElement = revid.Value
                    Dim damaging As JsonElement = element.GetProperty("damaging")
                    Dim isError As Boolean = IsJsonPropertyPresent(damaging, "error")
                    If isError Then
                        EditAndScoreList.Add(editID, {0, 0})
                        Continue For
                    End If
                    Dim damagingScore As JsonElement = damaging.GetProperty("score")
                    Dim damagingProbability As JsonElement = damagingScore.GetProperty("probability")
                    Dim damagingProbabilityTrue As Double = Math.Round(damagingProbability.GetProperty("true").GetDouble * 100, 2)

                    Dim goodfaith As JsonElement = element.GetProperty("goodfaith")
                    Dim goodfaithScore As JsonElement = goodfaith.GetProperty("score")
                    Dim goodfaithProbability As JsonElement = goodfaithScore.GetProperty("probability")
                    Dim goodfaithProbabilityTrue As Double = Math.Round(goodfaithProbability.GetProperty("true").GetDouble * 100, 2)
                    EditAndScoreList.Add(editID, {damagingProbabilityTrue, goodfaithProbabilityTrue})
                Next
            Next
            Return EditAndScoreList
        End Function

        ''' <summary>
        ''' Retorna las imagenes de preview de las páginas indicadas en el array de entrada como SortedList (con el formato {Página,Nombre de imagen}), los nombres de página deben ser distintos. 
        ''' En caso de no existir la imagen, retorna string.empty.
        ''' </summary>
        ''' <param name="pageNames">Array con nombres de página unicos.</param>
        Function GetImagesExtract(ByVal pageNames As String()) As SortedList(Of String, String)
            Dim PageNamesList As List(Of String) = pageNames.ToList
            PageNamesList.Sort()
            Dim PageList As List(Of List(Of String)) = SplitStringArrayIntoChunks(PageNamesList.ToArray, 20)
            Dim PagenameAndImage As New SortedList(Of String, String)

            For Each ListInList As List(Of String) In PageList
                Dim queryString As String = ConcatenateTextArrayWithChar(ListInList.ToArray, "|"c, True)
                Dim queryResponse As String = GETQUERY(SStrings.GetPagesImage & queryString)
                Dim response As JsonDocument = GetJsonDocument(queryResponse)
                Dim query As JsonElement = GetJsonElement(response, "query")
                Dim pages As JsonElement = query.GetProperty("pages")

                For Each pageProperty As JsonElement In pages.EnumerateArray
                    Dim title As String = pageProperty.GetProperty("title").GetString
                    Dim hasImage As Boolean = IsJsonPropertyPresent(pageProperty, "pageimage")
                    If Not hasImage Then
                        PagenameAndImage.Add(title, "")
                        Continue For
                    End If
                    Dim pageimage As String = pageProperty.GetProperty("pageimage").GetString
                    PagenameAndImage.Add(title, pageimage)
                Next
            Next
            Return PagenameAndImage
        End Function

        ''' <summary>
        ''' Retorna la entradilla de la página indicada de entrada como string con el límite indicado. 
        ''' En caso de no existir el la página o el resumen, no lo retorna.
        ''' </summary>
        ''' <param name="PageName">Nombre exacto de la página.</param>
        ''' <param name="CharLimit">Cantidad máxima de carácteres.</param>
        Overloads Function GetPageExtract(ByVal pageName As String, charLimit As Integer) As String
            Return GetPagesExtract({pageName}, charLimit).Values(0)
        End Function

        ''' <summary>
        ''' Retorna la entradilla de la página indicada de entrada como string con el límite indicado. 
        ''' En caso de no existir el la página o el resumen, no lo retorna.
        ''' </summary>
        ''' <param name="pageName">Nombre exacto de la página.</param>
        Overloads Function GetPageExtract(ByVal pageName As String) As String
            Return GetPagesExtract({pageName}, 660).Values(0)
        End Function

        ''' <summary>
        ''' Retorna en nombre del archivo de imagen de la página indicada de entrada como string. 
        ''' En caso de no existir el la página o la imagen, no lo retorna.
        ''' </summary>
        ''' <param name="PageName">Nombre exacto de la página.</param>
        Function GetImageExtract(ByVal pageName As String) As String
            Return GetImagesExtract({pageName}).Values(0)
        End Function

        ''' <summary>
        ''' Retorna los resúmenes de las páginas indicadas en el array de entrada como SortedList (con el formato {Página,Resumen}), los nombres de página deben ser distintos. 
        ''' En caso de no existir el la página o el resumen, no lo retorna.
        ''' </summary>
        ''' <param name="pageNames">Array con nombres de página unicos.</param>
        ''' <remarks></remarks>
        Overloads Function GetPagesExtract(ByVal pageNames As String()) As SortedList(Of String, String)
            Return BOTGetPagesExtract(pageNames, 660, False)
        End Function

        ''' <summary>
        ''' Retorna los resúmenes de las páginas indicadas en el array de entrada como SortedList (con el formato {Página,Resumen}), los nombres de página deben ser distintos. 
        ''' En caso de no existir el la página o el resumen, no lo retorna.
        ''' </summary>
        ''' <param name="pageNames">Array con nombres de página unicos.</param>
        ''' <param name="characterLimit">Límite de carácteres en el resumen.</param>
        ''' <remarks></remarks>
        Overloads Function GetPagesExtract(ByVal pageNames As String(), ByVal characterLimit As Integer) As SortedList(Of String, String)
            Return BOTGetPagesExtract(pageNames, characterLimit, False)
        End Function

        ''' <summary>
        ''' Retorna los resúmenes de las páginas indicadas en el array de entrada como SortedList (con el formato {Página,Resumen}), los nombres de página deben ser distintos. 
        ''' En caso de no existir el la página o el resumen, no lo retorna.
        ''' </summary>
        ''' <param name="pageNames">Array con nombres de página unicos.</param>
        ''' <param name="characterLimit">Límite de carácteres en el resumen.</param>
        ''' <remarks></remarks>
        Overloads Function GetPagesExtract(ByVal pageNames As String(), ByVal characterLimit As Integer, ByVal wiki As Boolean) As SortedList(Of String, String)
            Return BOTGetPagesExtract(pageNames, characterLimit, wiki)
        End Function

        ''' <summary>
        ''' Corta de la mejor forma que pueda un extracto para que esté debajo del límite de caracteres especificado.
        ''' </summary>
        ''' <param name="safetext"></param>
        ''' <param name="charlimit"></param>
        ''' <returns></returns>
        Function SafeTrimExtract(ByVal safetext As String, ByVal charlimit As Integer) As String
            Dim TrimmedText As String = safetext
            For a As Integer = charlimit To 0 Step -1
                If (TrimmedText.Chars(a) = ".") Or (TrimmedText.Chars(a) = ";") Then

                    If TrimmedText.Contains("(") Then
                        If Not CountCharacter(TrimmedText, CType("(", Char)) = CountCharacter(TrimmedText, CType(")", Char)) Then
                            Continue For
                        End If
                    End If
                    If TrimmedText.Contains("<") Then
                        If Not CountCharacter(TrimmedText, CType("<", Char)) = CountCharacter(TrimmedText, CType(">", Char)) Then
                            Continue For
                        End If
                    End If
                    If TrimmedText.Contains("«") Then
                        If Not CountCharacter(TrimmedText, CType("«", Char)) = CountCharacter(TrimmedText, CType("»", Char)) Then
                            Continue For
                        End If
                    End If
                    If TrimmedText.Contains("{") Then
                        If Not CountCharacter(TrimmedText, CType("{", Char)) = CountCharacter(TrimmedText, CType("}", Char)) Then
                            Continue For
                        End If
                    End If

                    'Verifica que no este cortando un numero
                    If TrimmedText.Length - 1 >= (a + 1) Then
                        If Regex.Match(TrimmedText.Chars(a + 1), "[0-9]+").Success Then
                            Continue For
                        Else
                            Exit For
                        End If
                    End If
                    'Verifica que no este cortando un n/f
                    If ((TrimmedText.Chars(a - 2) & TrimmedText.Chars(a - 1)).ToString.ToLower = "(n") Or
                    ((TrimmedText.Chars(a - 2) & TrimmedText.Chars(a - 1)).ToString.ToLower = "(f") Then
                        Continue For
                    Else
                        Exit For
                    End If

                End If
                TrimmedText = TrimmedText.Substring(0, a)
            Next
            If Regex.Match(TrimmedText, "{\\.+}").Success Then
                For Each m As Match In Regex.Matches(TrimmedText, "{\\.+}")
                    TrimmedText = TrimmedText.Replace(m.Value, "")
                Next
                TrimmedText = RemoveExcessOfSpaces(TrimmedText)
            End If
            Return TrimmedText
        End Function

        ''' <summary>
        ''' Obtiene los extractos en texto plano que entrega la API (textextracts).
        ''' </summary>
        ''' <param name="queryresponse"></param>
        ''' <param name="charLimit"></param>
        ''' <param name="wiki"></param>
        ''' <returns></returns>
        Function GetExtractsFromApiResponse(ByVal queryresponse As String, ByVal charLimit As Integer, ByVal wiki As Boolean) As HashSet(Of WikiExtract)
            Dim ExtractsList As New HashSet(Of WikiExtract)

            Dim jsonResponse As JsonDocument = GetJsonDocument(queryresponse)
            Dim query As JsonElement = GetJsonElement(jsonResponse, "query")
            Dim pages As JsonElement = query.GetProperty("pages")

            For Each queryPage As JsonProperty In pages.EnumerateObject
                Dim pageElement As JsonElement = queryPage.Value
                Dim title As String = pageElement.GetProperty("title").GetString
                Dim missing As Boolean = IsJsonPropertyPresent(pageElement, "missing")
                If missing Then
                    Dim WExtract As New WikiExtract With {
                        .ExtractContent = "",
                        .PageName = title}
                    ExtractsList.Add(WExtract)
                    Continue For
                End If
                Dim extract As String = pageElement.GetProperty("extract").GetString
                extract = NormalizeUnicodetext(extract)
                extract = extract.Replace("\n", Environment.NewLine)
                extract = Regex.Replace(extract, "\[[0-9]+\]", " ")
                extract = Regex.Replace(extract, "\[nota\ [0-9]+\]", " ")
                extract = Regex.Replace(extract, "\[cita requerida\]", " ")
                extract = RemoveExcessOfSpaces(extract)
                extract = FixResumeNumericExp(extract)
                If extract.Length > charLimit Then
                    extract = SafeTrimExtract(extract.Substring(0, charLimit + 1), charLimit)
                End If
                'Si el título de la página está en el resumen, coloca en negritas la primera ocurrencia
                If wiki Then
                    Dim regx As New Regex(Regex.Escape(extract), RegexOptions.IgnoreCase)
                    extract = regx.Replace(extract, "'''" & extract & "'''", 1)
                End If
                Dim ResultExtract As New WikiExtract With {
                        .ExtractContent = extract,
                        .PageName = title}
                ExtractsList.Add(ResultExtract)
            Next

            Return ExtractsList
        End Function

        ''' <summary>
        ''' Obtiene la entradilla de varias páginas manteniendo el wikitexto pero eliminando plantillas y referencias, mantiene los enlaces.
        ''' </summary>
        ''' <returns></returns>
        Function GetWikiExtractFromPageNames(ByVal pages As String(), ByVal charLimit As Integer) As SortedList(Of String, String)
            Dim tpageres As New SortedList(Of String, String)
            For Each page As String In pages
                Dim tpage As Page = Getpage(page)
                If tpage.Exists Then
                    Dim textract As WikiExtract = GetWikiExtractFromPage(tpage, charLimit)
                    tpageres.Add(page, textract.ExtractContent)
                End If
            Next
            Return tpageres
        End Function

        ''' <summary>
        ''' Obtiene la entradilla de varias páginas manteniendo el wikitexto pero eliminando plantillas y referencias, mantiene los enlaces.
        ''' </summary>
        ''' <returns></returns>
        Function GetWikiExtractFromPages(ByVal pages As String(), ByVal charLimit As Integer) As HashSet(Of WikiExtract)
            Dim tlist As New List(Of Page)
            For Each page As String In pages
                Dim tpage As Page = Getpage(page)
                If tpage.Exists Then
                    tlist.Add(tpage)
                End If
            Next
            Return GetWikiExtractFromPages(tlist.ToArray, charLimit)
        End Function

        ''' <summary>
        ''' Obtiene la entradilla de varias páginas manteniendo el wikitexto pero eliminando plantillas y referencias, mantiene los enlaces.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetWikiExtractFromPages(ByVal pages As Page(), ByVal charLimit As Integer) As HashSet(Of WikiExtract)
            Dim tset As New HashSet(Of WikiExtract)
            For Each page As Page In pages
                Dim textract As WikiExtract = GetWikiExtractFromPage(page, charLimit)
                If Not textract Is Nothing Then
                    tset.Add(textract)
                End If
            Next
            Return tset
        End Function

        ''' <summary>
        ''' Obtiene la entradilla de una página manteniendo el wikitexto pero eliminando plantillas y referencias, mantiene los enlaces.
        ''' </summary>
        ''' <param name="page"></param>
        ''' <param name="charLimit"></param>
        ''' <returns></returns>
        Public Function GetWikiExtractFromPage(ByVal page As Page, ByVal charLimit As Integer) As WikiExtract
            If page.Exists Then
                Dim pagethreads As String() = page.Threads
                Dim TreatedExtract As String = page.Content
                For Each thread As String In pagethreads
                    TreatedExtract = TreatedExtract.Replace(thread, "")
                Next
                Dim templates As String() = Template.GetTemplateTextArray(TreatedExtract).ToArray
                For Each temp As String In templates
                    If Not (temp.ToUpper.StartsWith("{{IPA|") Or
                    temp.ToUpper.StartsWith("{{NR|") Or
                    temp.ToUpper.StartsWith("{{MP|") Or
                    temp.ToUpper.StartsWith("{{NIHONGO|")) Then 'Mantener pantillas de texto comunes
                        TreatedExtract = TreatedExtract.Replace(temp, "").Trim()
                    End If
                Next
                TreatedExtract = Regex.Replace(TreatedExtract, "(\n\{\|)([\s\S]+?)(\n\|\})", "")
                TreatedExtract = Regex.Replace(TreatedExtract, "<[rR]ef ?(|.+)>([\s\S]+?|)<\/[rR]ef>", "")
                TreatedExtract = Regex.Replace(TreatedExtract, "(<[Rr]ef.+?)(\/>)", "")
                TreatedExtract = Regex.Replace(TreatedExtract, "(\[\[[Cc]ategoría:)(.+?)(\]\])", "")
                TreatedExtract = Regex.Replace(TreatedExtract, "\[nota\ [0-9]+\]", "")
                TreatedExtract = RemoveExcessOfSpaces(TreatedExtract)
                TreatedExtract = Removefiles(TreatedExtract)
                TreatedExtract = TreatedExtract.Trim()
                If TreatedExtract.Length > charLimit Then
                    TreatedExtract = SafeTrimExtract(TreatedExtract.Substring(0, charLimit + 1), charLimit)
                End If
                'Si el título de la página está en el resumen, coloca en negritas la primera ocurrencia
                Dim Extract As New WikiExtract With {
                        .ExtractContent = TreatedExtract,
                        .PageName = page.Title}
                Return Extract
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Elimina todos los enlaces de tipo [[Archivo:]] o [[File:]]
        ''' </summary>
        ''' <param name="str">Cadena a limpiar.</param>
        ''' <returns></returns>
        Private Function Removefiles(ByVal str As String) As String
            Dim tstr As String = str
            Do While True
                Dim match As Match = Regex.Match(tstr, "\[\[([Ii]mage:|[Aa]rchivo:|[Ff]ile:).+?\]\]")
                If Not match.Success Then
                    Exit Do
                End If
                Do While True
                    Dim tmatch As Match = Regex.Match(tstr, "\[\[([Ii]mage:|[Aa]rchivo:|[Ff]ile:).+?\]\]")
                    If (CountOccurrences(tmatch.Value, "[[") = CountOccurrences(tmatch.Value, "]]")) Then
                        tstr = tstr.Replace(tmatch.Value, "")
                        Exit Do
                    End If
                    Dim fixedmatch As String = ReplaceLast(ReplaceLast(tmatch.Value, "[[", ""), "]]", "")
                    tstr = tstr.Replace(tmatch.Value, fixedmatch)
                Loop
            Loop
            Return tstr
        End Function

        ''' <summary>
        ''' Retorna los resúmenes de las páginas indicadas en el array de entrada como SortedList (con el formato {Página,Resumen}), los nombres de página deben ser distintos. 
        ''' En caso de no existir el la página o el resumen, no lo retorna.
        ''' </summary>
        ''' <param name="pageNames">Array con nombres de página unicos.</param>
        ''' <remarks></remarks>
        Private Function BOTGetPagesExtract(ByVal pageNames As String(), charLimit As Integer, wiki As Boolean) As SortedList(Of String, String)
            EventLogger.Log(String.Format(Messages.GetPagesExtract, pageNames.Count.ToString), Reflection.MethodBase.GetCurrentMethod().Name)
            If pageNames Is Nothing Then Return Nothing
            Dim PageNamesList As List(Of String) = pageNames.ToList
            PageNamesList.Sort()
            Dim PageList As List(Of List(Of String)) = SplitStringArrayIntoChunks(PageNamesList.ToArray, 20)
            Dim PagenameAndResume As New SortedList(Of String, String)

            For Each ListInList As List(Of String) In PageList
                Dim Qstring As String = ConcatenateTextArrayWithChar(ListInList.ToArray, "|"c, True)
                Dim QueryResponse As String = GETQUERY(SStrings.GetPagesExtract & Qstring)
                Dim ExtractsList As HashSet(Of WikiExtract) = GetExtractsFromApiResponse(QueryResponse, charLimit, wiki)

                Dim NormalizedNames As New List(Of String)
                For Each pageName As String In PageNamesList.ToArray
                    NormalizedNames.Add(pageName.ToLower.Replace("_", " "))
                Next

                For Each Extract As WikiExtract In ExtractsList
                    Dim OriginalNameIndex As Integer = NormalizedNames.IndexOf(Extract.PageName.ToLower)
                    Dim OriginalName As String = PageNamesList(OriginalNameIndex)
                    PagenameAndResume.Add(OriginalName, Extract.ExtractContent)
                Next
            Next

            Return PagenameAndResume
        End Function

        ''' <summary>
        ''' Entrega el título de la primera página en el espacio de nombre usuario que coincida remotamente con el texto entregado como parámetro.
        ''' Usa las mismas sugerencias del cuadro de búsqueda de Wikipedia, pero por medio de la API.
        ''' Si no hay coincidencia, entrega una cadena de texto vacía.
        ''' </summary>
        ''' <param name="text">Título relativo a buscar</param>
        ''' <returns></returns>
        Function UserFirstGuess(text As String) As String
            Dim titles As String() = GetTitlesFromQueryText(GETQUERY(SStrings.SearchForUser & text))
            If titles.Count >= 1 Then
                Return titles(0)
            Else
                Return String.Empty
            End If
        End Function

        ''' <summary>
        ''' Busca un texto exacto en una página y lo reemplaza.
        ''' </summary>
        ''' <param name="Requestedpage">Página a realizar el cambio</param>
        ''' <param name="requestedtext">Texto a reeplazar</param>
        ''' <param name="newtext">Texto que reemplaza</param>
        ''' <param name="reason">Motivo del reemplazo</param>
        ''' <returns></returns>
        Function Replacetext(ByVal requestedpage As Page, requestedtext As String, newtext As String, reason As String) As Boolean
            If requestedpage Is Nothing Then
                Return False
            End If
            Dim PageText As String = requestedpage.Content
            If PageText.Contains(requestedtext) Then
                PageText = PageText.Replace(requestedtext, newtext)
            End If
            requestedpage.CheckAndSave(PageText, String.Format(Messages.TextReplaced, requestedtext, newtext, reason))
            Return True

        End Function

        ''' <summary>
        ''' Retorna un array de tipo string con todas las páginas donde el nombre de la página indicada es llamada (no confundir con "lo que enlaza aquí").
        ''' </summary>
        ''' <param name="pageName">Nombre exacto de la pagina.</param>
        Function GetallInclusions(ByVal pageName As String) As String()
            Return GetallInclusions(pageName, 10)
        End Function

        ''' <summary>
        ''' Retorna un array de tipo string con todas las páginas donde el nombre de la página indicada es llamada (no confundir con "lo que enlaza aquí").
        ''' </summary>
        ''' <param name="pageName">Nombre exacto de la pagina.</param>
        ''' <param name="limit">Limite de iteraciones de 'continue' en la API.</param>
        Function GetallInclusions(ByVal pageName As String, ByVal limit As Integer) As String()
            '===============================================================================
            '                     !!WARNING!!
            '                  SHITTY CODE AHEAD
            '        TRY TO UNDERSTAND IT UNDER YOUR OWN RISK
            '         BLAME THE MINDBENDING MW DOCUMENTATION
            '===============================================================================

            Dim pages As New HashSet(Of String)
            Dim queries As Integer = 1
            Dim queryString As String = SStrings.GetPageInclusions & pageName
            Dim rawQueryResponse As String = POSTQUERY(queryString)
            Dim queryResponse As JsonDocument = GetJsonDocument(rawQueryResponse)
            Dim mustContinue As Boolean = IsJsonPropertyPresent(queryResponse.RootElement, "continue")
            If mustContinue Then
                Dim qcontinue As JsonElement = GetJsonElement(queryResponse, "continue")
                Dim eicontinue = qcontinue.GetProperty("eicontinue").GetString
                queryString = SStrings.GetPageInclusions & pageName & "&eicontinue=" & eicontinue
            End If

            Dim query As JsonElement = GetJsonElement(queryResponse, "query")
            Dim embeddedin As JsonElement = query.GetProperty("embeddedin")

            For Each qresult As JsonElement In embeddedin.EnumerateArray
                Dim title As String = qresult.GetProperty("title").GetString
                pages.Add(title)
            Next

            While mustContinue And queries < limit
                queries += 1
                rawQueryResponse = POSTQUERY(queryString)
                queryResponse = GetJsonDocument(rawQueryResponse)
                mustContinue = IsJsonPropertyPresent(queryResponse.RootElement, "continue")
                If mustContinue Then
                    Dim qcontinue As JsonElement = GetJsonElement(queryResponse, "continue")
                    Dim eicontinue = qcontinue.GetProperty("eicontinue").GetString
                    queryString = SStrings.GetPageInclusions & pageName & "&eicontinue=" & eicontinue
                End If
                For Each qresult As JsonElement In embeddedin.EnumerateArray
                    Dim title As String = qresult.GetProperty("title").GetString
                    pages.Add(title)
                Next
            End While
            Return pages.ToArray()
        End Function

        ''' <summary>
        ''' Obtiene las diferencias de la última edición de la página.
        ''' </summary>
        ''' <param name="thepage">Página a revisar.</param>
        ''' <returns></returns>
        Function GetLastDiff(ByVal thepage As Page) As WikiDiff
            If thepage Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            If Not thepage.Exists Then
                Return Nothing
            End If
            Dim toid As Integer = thepage.CurrentRevId
            Dim fromid As Integer = thepage.ParentRevId
            If fromid = -1 Then
                Return Nothing
            End If
            Return GetDiff(fromid, toid)
        End Function

        ''' <summary>
        ''' Obtiene las diferencias entre dos rev id.
        ''' </summary>
        ''' <param name="fromid">Id base en la comparación.</param>
        ''' <param name="toid">Id a compara.r</param>
        ''' <returns></returns>
        Function GetDiff(ByVal fromid As Integer, ByVal toid As Integer) As WikiDiff

            Dim Changedlist As New List(Of Tuple(Of String, String))
            Dim page1 As Page = Getpage(fromid)
            Dim page2 As Page = Getpage(toid)
            If Not (page1.Exists And page2.Exists) Then
                Return New WikiDiff(fromid, toid, Changedlist)
            End If
            Dim querydata As String = String.Format(SStrings.GetDiffQuery, fromid.ToString, toid.ToString)
            Dim querytext As String = POSTQUERY(querydata)
            Dim difftext As String
            Try
                difftext = NormalizeUnicodetext(TextInBetween(querytext, ",""*"":""", "\n""}}")(0))
            Catch ex As IndexOutOfRangeException
                Return New WikiDiff(fromid, toid, Changedlist)
            End Try
            Dim Rows As String() = TextInBetween(difftext, "<tr>", "</tr>")
            Dim Diffs As New List(Of Tuple(Of String, String))
            For Each row As String In Rows
                Dim matches As MatchCollection = Regex.Matches(row, "<td class=""diff-(addedline|deletedline|context)"">[\S\s]*?<\/td>")
                If matches.Count >= 1 Then
                    Dim cells As New List(Of String)
                    For Each cell As Match In matches
                        Dim TreatedString As String = Regex.Replace(cell.Value, "<td class=""diff-(addedline|deletedline|context)"">", "")
                        TreatedString = Regex.Replace(TreatedString, "(<del class=""diffchange diffchange-inline"">|<div>|</div>|<\/td>|<\/del>|<ins class=""diffchange diffchange-inline"">|<\/ins>)", "")
                        cells.Add(TreatedString)
                    Next
                    If cells.Count = 1 Then
                        Diffs.Add(New Tuple(Of String, String)(String.Empty, cells(0)))
                    ElseIf cells.Count >= 2 Then
                        If Not (cells(0) = cells(1)) Then
                            Diffs.Add(New Tuple(Of String, String)(cells(0), cells(1)))
                        End If
                        Continue For
                    End If
                End If
            Next
            Return New WikiDiff(fromid, toid, Diffs)
        End Function

        ''' <summary>
        ''' Retorna un array de tipo string con todas las páginas donde la página indicada es llamada (no confundir con "lo que enlaza aquí").
        ''' </summary>
        ''' <param name="tpage">Página que se llama.</param>
        Function GetallInclusions(ByVal tpage As Page) As String()
            If tpage Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            Return GetallInclusions(tpage.Title)
        End Function

        ''' <summary>
        ''' Retorna un array con todas las páginas donde la página indicada es llamada (no confundir con "lo que enlaza aquí").
        ''' </summary>
        ''' <param name="pageName">Nombre exacto de la pagina.</param>
        Function GetallInclusionsPages(ByVal pageName As String) As Page()
            Dim pages As String() = GetallInclusions(pageName)
            Dim pagelist As New List(Of Page)
            For Each p As String In pages
                pagelist.Add(Getpage(p))
            Next
            Return pagelist.ToArray
        End Function

        ''' <summary>
        ''' Retorna un array con todas las páginas donde la página indicada es llamada (no confundir con "lo que enlaza aquí").
        ''' </summary>
        ''' <param name="tpage">Página que se llama.</param>
        Function GetallInclusionsPages(ByVal tpage As Page) As Page()
            If tpage Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            Return GetallInclusionsPages(tpage.Title)
        End Function

        ''' <summary>
        ''' Retorna un elemento Page coincidente al nombre entregado como parámetro.
        ''' </summary>
        ''' <param name="pageName">Nombre exacto de la página</param>
        Function Getpage(ByVal pageName As String) As Page
            Return New Page(pageName, Me)
        End Function

        ''' <summary>
        ''' Retorna un elemento Page coincidente al RevID entregado como parámetro.
        ''' </summary>
        ''' <param name="revId">ID de la revisión.</param>
        Function Getpage(ByVal revId As Integer) As Page
            Return New Page(revId, Me)
        End Function

        ''' <summary>
        ''' Retorna una pagina aleatoria.
        ''' </summary>
        ''' <returns></returns>
        Function GetRandomPage() As Page
            Return GetRandomPage(0)
        End Function

        ''' <summary>
        ''' Retorna una pagina aleatoria.
        ''' </summary>
        ''' <param name="pnamespace">Espacio de nombres de la pagina.</param>
        ''' <returns></returns>
        Function GetRandomPage(ByVal pnamespace As Integer) As Page
            Dim tquery As String = POSTQUERY(String.Format(SStrings.RandomPageQuery, pnamespace))
            Dim tpage As String = NormalizeUnicodetext(TextInBetween(tquery, "title"":""", """}").FirstOrDefault())
            Return Getpage(tpage)
        End Function

        ''' <summary>
        ''' Verifica si el usuario que se le pase cumple con los requisitos para verificar su actividad
        ''' </summary>
        ''' <param name="user">Usuario de Wiki</param>
        ''' <returns></returns>
        Public Function UserIsActive(ByVal user As WikiUser) As Boolean
            EventLogger.Debug_Log(String.Format(Messages.CheckingUser, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
            'Verificar si el usuario existe
            If Not user.Exists Then
                EventLogger.Log(String.Format(Messages.UserInexistent, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                Return False
            End If

            'Verificar si el usuario está bloqueado.
            If user.Blocked Then
                EventLogger.Log(String.Format(Messages.UserBlocked, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                Return False
            End If

            'Verificar si el usuario editó hace al menos 4 días.
            If Date.Now.Subtract(user.LastEdit).Days >= 4 Then
                EventLogger.Log(String.Format(Messages.UserInactive, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                Return False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Retorna la cantidad máxima que permita la api de páginas que comienze con el texto.
        ''' </summary>
        ''' <param name="pagePrefix"></param>
        ''' <returns></returns>
        Function PrefixSearch(ByVal pagePrefix As String) As String()
            Dim results As New List(Of String)
            Dim queryString As String = SStrings.PrefixSearchQuery & UrlWebEncode(pagePrefix)
            Dim queryRawResponse As String = POSTQUERY(queryString)
            Dim queryResult As JsonDocument = GetJsonDocument(queryRawResponse)
            Dim queryElement As JsonElement = GetJsonElement(queryResult, "query")
            Dim qprefixsearch As JsonElement = queryElement.GetProperty("prefixsearch")

            For Each searchResult As JsonElement In qprefixsearch.EnumerateArray
                Dim title As String = searchResult.GetProperty("title").GetString
                results.Add(title)
            Next
            Return results.ToArray()
        End Function
#End Region

#Region "Subs"
        Sub MessageDelivery(ByVal userList As String(), messageTitle As String, messageContent As String, editSummary As String)
            For Each u As String In userList
                Dim user As New WikiUser(Me, u)
                If Not user.Exists Then
                    EventLogger.Log(String.Format(Messages.UserInexistent, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                    Continue For
                End If
                If user.Blocked Then
                    EventLogger.Log(String.Format(Messages.UserBlocked, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                    Continue For
                End If
                Dim userTalkPage As Page = user.TalkPage
                userTalkPage.AddSection(messageTitle, messageContent, editSummary, False)
            Next
        End Sub

#End Region

    End Class
End Namespace