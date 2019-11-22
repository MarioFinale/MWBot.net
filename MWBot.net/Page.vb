Option Strict On
Option Explicit On
Imports System.Net
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports MWBot.net.GlobalVars
Imports MWBot.net.My.Resources
Imports Utils.Utils

Namespace WikiBot
    Public Class Page

        Private Property WorkerBot As Bot


#Region "Properties"

        Private Property Username As String
        Private Property SiteUri As Uri

        ''' <summary>
        ''' Entrega el puntaje ORES {damaging,goodfaith} de la página.
        ''' </summary>
        ''' <returns></returns>
        Public Function ORESScore() As Double()
            Return GetORESScore(_CurrentRevId)
        End Function
        ''' <summary>
        ''' Entrega el contenido (wikitexto) de la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Content As String

        ''' <summary>
        ''' Entrega el revid actual de la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CurrentRevId As Integer

        ''' <summary>
        ''' entrega el último usuario en editar la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Lastuser As String

        ''' <summary>
        ''' Entrega el título de la página (con el namespace).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Title As String

        ''' <summary>
        ''' Entrega la marca de tiempo de la edición actual de la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Timestamp As String

        ''' <summary>
        ''' Entrega el comentario de la última edición en la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Comment As String

        Private _Threads As String()
        ''' <summary>
        ''' Entrega las secciones de la página.
        ''' </summary>
        ''' <returns></returns>
        Public Function Threads() As String()
            Return _Threads
        End Function

        Private _Categories As String()
        ''' <summary>
        ''' Entrega las primeras 10 categorías de la página.
        ''' </summary>
        ''' <returns></returns>
        Public Function Categories() As String()
            Return _Categories
        End Function

        ''' <summary>
        ''' Indica si la edición ha sido ocultada u ocultada parcialmente.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasHiddenInfo As Boolean

        ''' <summary>
        ''' Entrega el promedio de visitas diarias de la página en los últimos 2 meses.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PageViews As Integer
            Get
                Return GetPageViewsAvg(_Title)
            End Get
        End Property
        ''' <summary>
        ''' Tamaño de la página en bytes.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer

        ''' <summary>
        ''' Número del espacio de nombres al cual pertenece la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PageNamespace As Integer

        ''' <summary>
        ''' Extracto de la intro de la pagina (segun wikipedia, largo completo).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Extract As String

        ''' <summary>
        ''' Imagen de miniatura de la pagina.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Thumbnail As String

        Public ReadOnly Property RootPage As String

        ''' <summary>
        ''' Obtiene el revid de la edición anterior de la página (si existe).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ParentRevId As Integer

        ''' <summary>
        ''' Indica si la pagina existe.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Exists As Boolean

        ''' <summary>
        ''' Indica si la pagina puede ser editada por el bot.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BotEditable As Boolean
            Get
                Return BotCanEdit(Me.Content, WorkerBot.LocalName)
            End Get
        End Property

        ''' <summary>
        ''' Entrega la fecha de la última edición en la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LastEdit As Date


        Public ReadOnly Property URL As String
            Get
                Return WorkerBot.WikiUri.OriginalString & "wiki/" & Title.Replace(" ", "_")
            End Get
        End Property

        ''' <summary>
        ''' Entrega el ID de la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PageID As Integer
#End Region
        ''' <summary>
        ''' Inicializa una nueva página, por lo general no se llama de forma directa. Se puede obtener una página creandola con Bot.Getpage.
        ''' </summary>
        ''' <param name="PageTitle">Título exacto de la página</param>
        ''' <param name="wbot">Bot logueado a la wiki</param>
        Public Sub New(ByVal pageTitle As String, ByRef wbot As Bot)
            If wbot Is Nothing Then Throw New ArgumentNullException(NameOf(wbot), "Worker bot is is nothing")
            WorkerBot = wbot
            Username = WorkerBot.UserName
            Loadpage(pageTitle, WorkerBot.WikiUri)
        End Sub
        ''' <summary>
        ''' Inicializa una nueva página, por lo general no se llama de forma directa. Se puede obtener una página creandola con Bot.Getpage.
        ''' </summary>
        ''' <param name="revid">Revision ID.</param>/param>
        ''' <param name="wbot">Bot logueado a la wiki</param>
        Public Sub New(ByVal revid As Integer, ByRef wbot As Bot)
            If wbot Is Nothing Then Throw New ArgumentNullException(NameOf(wbot), "Worker bot is is nothing")
            WorkerBot = wbot
            Username = WorkerBot.UserName
            Loadpage(revid, WorkerBot.WikiUri)
        End Sub

        ''' <summary>
        ''' Inicializa de nuevo la página (al crear una página esta ya está inicializada).
        ''' </summary>
        Public Sub Load()
            Loadpage(_Title, SiteUri)
        End Sub

        ''' <summary>
        ''' Inicializa la página, esta función no se llama de forma directa
        ''' </summary>
        ''' <param name="PageTitle">Título exacto de la página</param>
        ''' <param name="site">Sitio de la página</param>
        ''' <returns></returns>
        Private Overloads Function Loadpage(ByVal PageTitle As String, ByVal site As Uri) As Boolean
            If site Is Nothing Then
                Throw New ArgumentNullException(NameOf(site), "Empty parameter")
            End If

            If String.IsNullOrEmpty(PageTitle) Then
                Throw New ArgumentNullException(NameOf(PageTitle), "Empty parameter")
            End If
            SiteUri = site
            PageInfoData(PageTitle)
            If Exists AndAlso Not String.IsNullOrWhiteSpace(_Content) Then
                _Threads = GetPageThreads(_Content)
            End If
            EventLogger.Debug_Log(String.Format(Messages.PageLoaded, PageTitle), Reflection.MethodBase.GetCurrentMethod().Name, Username)
            Return True
        End Function

        ''' <summary>
        ''' Inicializa la página, esta función no se llama de forma directa
        ''' </summary>
        ''' <param name="Revid">ID de revisión.</param>
        ''' <param name="site">Sitio de la página.</param>
        ''' <returns></returns>
        Private Overloads Function Loadpage(ByVal Revid As Integer, ByVal site As Uri) As Boolean
            If site Is Nothing Then
                Throw New ArgumentNullException(NameOf(site), "Site is nothing.")
            End If
            If Revid <= 0 Then
                Throw New ArgumentNullException(NameOf(Revid), "Invalid revid.")
            End If
            SiteUri = site
            PageInfoData(Revid)
            _Threads = GetPageThreads(_Content)
            EventLogger.Debug_Log(String.Format(Messages.PRevLoaded, Revid.ToString), Reflection.MethodBase.GetCurrentMethod().Name, Username)
            Return True
        End Function

        ''' <summary>
        ''' Retorna el valor ORES (en %) de un EDIT ID (eswiki) indicados como porcentaje en double. 
        ''' En caso de no existir el EDIT ID, retorna 0.
        ''' </summary>  
        ''' <param name="revid">EDIT ID de la edicion a revisar</param>
        ''' <remarks>Los EDIT ID deben ser distintos</remarks>
        Private Function GetORESScore(ByVal revid As Integer) As Double()
            Return WorkerBot.GetORESScores({revid})(0)
        End Function


        ''' <summary>
        ''' Guarda la página en la wiki. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="summary">Resumen de la edición</param>
        ''' <param name="IsMinor">¿Marcar como menor?</param>
        ''' <param name="spam">¿Reemplazar los link marcados como spam?</param>
        ''' <returns></returns>
        Overloads Function Save(ByVal pageContent As String, ByVal summary As String, ByVal isMinor As Boolean, ByVal isBot As Boolean, ByVal spam As Boolean) As EditResults
            Return SavePage(pageContent, summary, isMinor, isBot, spam, 0)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="EditSummary">Resumen de la edición</param>
        ''' <param name="IsMinor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Private Function SavePage(ByVal pageContent As String, ByVal EditSummary As String, ByVal IsMinor As Boolean, ByVal IsBot As Boolean, ByVal Spamreplace As Boolean, ByRef RetryCount As Integer) As EditResults
            If String.IsNullOrWhiteSpace(pageContent) Then
                Throw New ArgumentNullException(NameOf(pageContent), "Empty parameter")
            End If

            If pageContent = Content Then
                EventLogger.Debug_Log(String.Format(Messages.NoChanges, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Edit_successful
            End If

            Dim EditToken As String = String.Empty
            Try
                EditToken = GetEditToken()
            Catch ex As WebException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _Title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.POST_error
            End Try

            Dim ntimestamp As String = GetCurrentTimestamp()
            If Not ntimestamp = _Timestamp Then
                EventLogger.Log(String.Format(Messages.EditConflict, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Edit_conflict
            End If

            Dim additionalParams As String = String.Empty
            If IsMinor Then
                additionalParams = "&minor="
            Else
                additionalParams = "&notminor="
            End If

            If IsBot Then
                additionalParams &= "&bot="
            End If

            Dim postdata As String = String.Format(SStrings.SavePage, additionalParams, _Title, UrlWebEncode(EditSummary), UrlWebEncode(pageContent), UrlWebEncode(EditToken))
            Dim postresult As String = String.Empty

            Try
                postresult = WorkerBot.POSTQUERY(postdata)
                Threading.Thread.Sleep(300) 'Some time for the server to process the data
                Load() 'Update page data
            Catch ex As IO.IOException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _Title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.POST_error
            Catch ex As WebException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _Title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.POST_error
            End Try

            If String.IsNullOrWhiteSpace(postresult) Then
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _Title, PsvSafeEncode(postresult)), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.POST_error
            End If

            If postresult.Contains("""result"":""Success""") Then
                EventLogger.Log(String.Format(Messages.SuccessfulEdit, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Edit_successful
            End If

            If Not postresult.ToLower.Contains("editconflict") Then
                EventLogger.Log(String.Format(Messages.EditConflict, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Edit_conflict
            End If

            If postresult.ToLower.Contains("abusefilter") Then
                EventLogger.Log(String.Format(Messages.AbuseFilter, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                EventLogger.Debug_Log("ABUSEFILTER: " & postresult, Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.AbuseFilter
            End If

            If postresult.ToLower.Contains("spamblacklist") Then
                EventLogger.Log(String.Format(Messages.SpamBlackList, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                EventLogger.Debug_Log("SPAMBLACKLIST: " & postresult, Reflection.MethodBase.GetCurrentMethod().Name, Username)
                If Spamreplace Then
                    Dim spamlinkRegex As String = TextInBetween(postresult, """spamblacklist"":""", """")(0)
                    Dim newtext As String = Regex.Replace(pageContent, SpamListParser(spamlinkRegex), Function(x) "<nowiki>" & x.Value & "</nowiki>") 'Reeplazar links con el Nowiki
                    If Not RetryCount > MaxRetry Then
                        Return SavePage(newtext, EditSummary, IsMinor, IsBot, True, RetryCount + 1)
                    Else
                        EventLogger.Log(String.Format(Messages.MaxRetryCount, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                        Return EditResults.Max_retry_count
                    End If
                Else
                    Return EditResults.SpamBlacklist
                End If
            End If

            If postresult.ToLower.Contains("protectedpage") Then
                Return EditResults.ProtectedPage
            End If

            'Unexpected result, log and retry
            EventLogger.EX_Log(PsvSafeEncode(postresult), Reflection.MethodBase.GetCurrentMethod().Name, Username)

            If Not RetryCount > MaxRetry Then
                'Refresh credentials, retry
                WorkerBot.Relogin()
                Return SavePage(pageContent, EditSummary, IsMinor, IsBot, True, RetryCount + 1)
            Else
                EventLogger.Log(String.Format(Messages.SpamBlackList, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Max_retry_count
            End If
            Return EditResults.Unexpected_Result
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Comprueba si la página tiene la plantilla {{nobots}}. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="Summary">Resumen de la edición</param>
        ''' <param name="Minor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Overloads Function CheckAndSave(ByVal pageContent As String, ByVal summary As String, ByVal minor As Boolean, ByVal bot As Boolean, ByVal spam As Boolean) As EditResults
            If Not BotCanEdit(_Content, Username) Then
                EventLogger.Log(String.Format(Messages.NoBots, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.No_bots
            End If
            Return SavePage(pageContent, summary, minor, bot, spam, 0)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Comprueba si la página tiene la plantilla {{nobots}}. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="Summary">Resumen de la edición</param>
        ''' <param name="Minor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Overloads Function CheckAndSave(ByVal pageContent As String, ByVal summary As String, ByVal minor As Boolean, ByVal bot As Boolean) As EditResults
            Return CheckAndSave(pageContent, summary, minor, bot, False)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Comprueba si la página tiene la plantilla {{nobots}}. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="Summary">Resumen de la edición</param>
        ''' <param name="IsMinor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Overloads Function CheckAndSave(ByVal pageContent As String, ByVal summary As String, ByVal isMinor As Boolean) As EditResults
            Return CheckAndSave(pageContent, summary, isMinor, False, False)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Comprueba si la página tiene la plantilla {{nobots}}. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="Summary">Resumen de la edición</param>
        ''' <returns></returns>
        Overloads Function CheckAndSave(ByVal pageContent As String, ByVal summary As String) As EditResults
            Return CheckAndSave(pageContent, summary, False, False, False)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Comprueba si la página tiene la plantilla {{nobots}}. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <returns></returns>
        Overloads Function CheckAndSave(ByVal pageContent As String) As EditResults
            Return CheckAndSave(pageContent, Messages.DefaultSumm, False, False, False)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="Summary">Resumen de la edición</param>
        ''' <param name="Minor">¿Marcar como menor?</param>
        ''' <param name="bot">¿Marcar como edición de bot?</param>
        ''' <returns></returns>
        Overloads Function Save(ByVal pageContent As String, ByVal summary As String, ByVal minor As Boolean, ByVal bot As Boolean) As EditResults
            Return SavePage(pageContent, summary, minor, bot, False, 0)
        End Function


        ''' <summary>
        ''' Guarda la página en la wiki. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="summary">Resumen de la edición</param>
        ''' <param name="minor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Overloads Function Save(ByVal pageContent As String, ByVal summary As String, ByVal minor As Boolean) As EditResults
            Return SavePage(pageContent, summary, minor, False, False, 0)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Texto (wikicódigo) de la página</param>
        ''' <param name="Summary">Resumen de la edición</param>
        ''' <returns></returns>
        Overloads Function Save(ByVal pageContent As String, ByVal summary As String) As EditResults
            Return SavePage(pageContent, summary, False, False, False, 0)
        End Function

        ''' <summary>
        ''' Guarda la página en la wiki. Si la página no existe, la crea.
        ''' </summary>
        ''' <param name="pageContent">Contenido como texto (wikicódigo) de la página</param>
        ''' <returns></returns>
        Overloads Function Save(ByVal pageContent As String) As EditResults
            Return SavePage(pageContent, Messages.DefaultSumm, False, False, False, 0)
        End Function

        ''' <summary>
        ''' Añade una sección nueva a una página dada. Útil en casos como messagedelivery.
        ''' </summary>
        ''' <param name="sectionTitle">Título de la sección nueva</param>
        ''' <param name="sectionContent">Texto de la sección</param>
        ''' <param name="editSummary">Resumen de edición</param>
        ''' <param name="isMinor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Private Function AddSectionPage(ByVal sectionTitle As String, ByVal sectionContent As String, ByVal editSummary As String, ByVal isMinor As Boolean) As EditResults
            Dim additionalParameters As String = String.Empty
            If String.IsNullOrEmpty(sectionContent) Or String.IsNullOrEmpty(sectionTitle) Then
                Throw New ArgumentNullException(System.Reflection.MethodBase.GetCurrentMethod().Name)
            End If

            If Not GetCurrentTimestamp() = _Timestamp Then
                EventLogger.Log(String.Format(Messages.EditConflict, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Edit_conflict
            End If

            If Not BotCanEdit(_Content, Username) Then
                EventLogger.Log(String.Format(Messages.NoBots, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.No_bots
            End If

            If isMinor Then
                additionalParameters &= "&minor=true"
            End If

            Dim postdata As String = String.Format(SStrings.AddThread, additionalParameters, UrlWebEncode(_Title), UrlWebEncode(editSummary), UrlWebEncode(sectionTitle), UrlWebEncode(sectionContent), UrlWebEncode(GetEditToken()))
            Dim postresult As String
            Try
                postresult = WorkerBot.POSTQUERY(postdata)
                Threading.Thread.Sleep(300) 'Some time to the server to process the data
                Load() 'Update page data
            Catch ex As WebException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _Title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.POST_error
            End Try

            If postresult.Contains("""result"":""Success""") Then
                EventLogger.Log(String.Format(Messages.SuccessfulEdit, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.Edit_successful
            End If

            If postresult.Contains("abusefilter") Then
                EventLogger.Log(String.Format(Messages.AbuseFilter, _Title), Reflection.MethodBase.GetCurrentMethod().Name, Username)
                Return EditResults.AbuseFilter
            End If

            Return EditResults.Unexpected_Result
        End Function

        ''' <summary>
        ''' Añade una sección nueva a una página dada. Útil en casos como messagedelivery.
        ''' </summary>
        ''' <param name="sectionTitle">Título de la sección nueva</param>
        ''' <param name="sectionContent">Texto de la sección</param>
        ''' <param name="editSummary">Resumen de edición</param>
        ''' <param name="IsMinor">¿Marcar como menor?</param>
        ''' <returns></returns>
        Overloads Function AddSection(ByVal sectionTitle As String, ByVal sectionContent As String, ByVal editSummary As String, ByVal isMinor As Boolean) As EditResults
            Return AddSectionPage(sectionTitle, sectionContent, editSummary, isMinor)
        End Function

        ''' <summary>
        ''' Añade una sección nueva a una página dada. Útil en casos como messagedelivery.
        ''' </summary>
        ''' <param name="sectionTitle">Título de la sección nueva</param>
        ''' <param name="sectionContent">Texto de la sección</param>
        ''' <param name="editSummary">Resumen de edición</param>
        ''' <returns></returns>
        Overloads Function AddSection(ByVal sectionTitle As String, ByVal sectionContent As String, ByVal editSummary As String) As EditResults
            Return AddSectionPage(sectionTitle, sectionContent, editSummary, False)
        End Function

        ''' <summary>
        ''' Añade una sección nueva a una página dada. Útil en casos como messagedelivery.
        ''' </summary>
        ''' <param name="SectionTitle">Título de la sección nueva</param>
        ''' <param name="sectionContent">Texto de la sección</param>
        ''' <returns></returns>
        Overloads Function AddSection(ByVal sectionTitle As String, ByVal sectionContent As String) As EditResults
            Return AddSectionPage(sectionTitle, sectionContent, Messages.DefaultSumm, False)
        End Function

        ''' <summary>
        ''' Función que verifica si la página puede ser editada por bots (se llama desde Save())
        ''' </summary>
        ''' <param name="pageContent">Texto de la página</param>
        ''' <param name="userName">Usuario que edita</param>
        ''' <returns></returns>
        Private Shared Function BotCanEdit(ByVal pageContent As String, ByVal userName As String) As Boolean
            If String.IsNullOrWhiteSpace(pageContent) Then
                'Página en blanco, por lo tanto pueden editarla bots
                Return True
            End If
            If String.IsNullOrWhiteSpace(userName) Then
                Throw New ArgumentException(NameOf(userName), "Empty username.")
            End If
            userName = userName.Normalize
            Return Not Regex.IsMatch(pageContent, "\{\{(nobots|bots\|(allow=none|deny=(?!none).*(" & userName & "|all)|optout=all))\}\}", RegexOptions.IgnoreCase)
        End Function

        ''' <summary>
        ''' Obtiene un Token de edición desde la API de MediaWiki
        ''' </summary>
        ''' <returns></returns>
        Private Function GetEditToken() As String
            Dim querytext As String = SStrings.EditToken
            Dim queryresult As String = WorkerBot.POSTQUERY(querytext)
            Dim token As String = TextInBetween(queryresult, """csrftoken"":""", """}}")(0).Replace("\\", "\")
            Return token
        End Function

        ''' <summary>
        ''' Hace una solicitud a la API respecto a una página y actualiza los datos
        ''' </summary>
        ''' <param name="Pagename">Título exacto de la página</param>
        Private Overloads Sub PageInfoData(ByVal pageName As String)
            Dim querystring As String = String.Format(SStrings.PageInfo, UrlWebEncode(pageName))
            Dim QueryText As String = WorkerBot.GETQUERY(querystring)
            RetryLoad(querystring, 3)
        End Sub

        ''' <summary>
        ''' Hace una solicitud a la API respecto a una página y actualiza los datos
        ''' </summary>
        ''' <param name="Revid">Revision ID de la página</param>
        Private Overloads Sub PageInfoData(ByVal Revid As Integer)
            Dim querystring As String = String.Format(SStrings.PageInfoRevid, Revid.ToString)
            RetryLoad(querystring, 3)
        End Sub

        Private Sub RetryLoad(ByVal querystring As String, ByVal maxRetries As Integer)
            For i As Integer = 0 To maxRetries
                If LoadPageInfo(querystring) Then Return
            Next
            Throw New MaxRetriesExeption
        End Sub

        Private Function LoadPageInfo(ByVal querystring As String) As Boolean
            Dim queryresponse As String = WorkerBot.GETQUERY(querystring)
            Dim jsonResponse As JsonDocument = GetJsonDocument(queryresponse)
            Dim iserror As Boolean = IsJsonPropertyPresent(jsonResponse.RootElement, "error")
            If iserror Then
                Return False
            End If
            Dim query As JsonElement = GetJsonElement(jsonResponse, "query")
            Dim pages As JsonElement = query.GetProperty("pages")

            '===Prop===
            Dim qpageTitle As String
            Dim qpageNS As Integer
            Dim qpageLastEdit As Date
            Dim qpageLastTimestamp As String
            Dim qpageRevID As Integer
            Dim qpageParentRevID As Integer
            Dim qpageLastUser As String = String.Empty
            Dim qpageComment As String = String.Empty
            Dim qpageThumbnail As String = String.Empty
            Dim qpageCategories As New List(Of String)
            Dim qpageSize As Integer
            Dim qpageContent As String
            Dim qpageDeletedInfo As Boolean
            Dim qpageRoot As String
            Dim qpageExtract As String
            Dim qpageID As Integer

            Dim pageProperty As JsonProperty = pages.EnumerateObject(0)
            Dim pageElement As JsonElement = pageProperty.Value

            qpageTitle = pageElement.GetProperty("title").GetString
            qpageRoot = qpageTitle.Split("/"c)(0).Trim()
            qpageNS = pageElement.GetProperty("ns").GetInt32
            Dim missing As Boolean = IsJsonPropertyPresent(pageElement, "missing")
            If missing Then
                _Title = qpageTitle
                _PageNamespace = qpageNS
                _RootPage = qpageRoot
                _Exists = False
                Return True
            End If
            If IsJsonPropertyPresent(pageElement, "pageimage") Then
                qpageThumbnail = pageElement.GetProperty("pageimage").GetString
            End If

            qpageID = pageElement.GetProperty("pageid").GetInt32
            qpageExtract = pageElement.GetProperty("extract").GetString

            If IsJsonPropertyPresent(pageElement, "categories") Then
                Dim categories As JsonElement = pageElement.GetProperty("categories")
                For Each category As JsonElement In categories.EnumerateArray
                    Dim categoryname As String = category.GetProperty("title").GetString
                    qpageCategories.Add(categoryname)
                Next
            End If

            Dim revisions As JsonElement = pageElement.GetProperty("revisions")
            Dim currentrevision As JsonElement = revisions.EnumerateArray(0)
            qpageRevID = currentrevision.GetProperty("revid").GetInt32
            qpageParentRevID = currentrevision.GetProperty("parentid").GetInt32
            qpageLastEdit = GetDateFromMWTimestamp(currentrevision.GetProperty("timestamp").GetString)
            qpageLastTimestamp = currentrevision.GetProperty("timestamp").GetString
            qpageSize = currentrevision.GetProperty("size").GetInt32

            If IsJsonPropertyPresent(currentrevision, "comment") Then
                qpageComment = currentrevision.GetProperty("comment").GetString
            Else
                qpageDeletedInfo = True
            End If
            If IsJsonPropertyPresent(currentrevision, "user") Then
                qpageLastUser = currentrevision.GetProperty("user").GetString
            Else
                qpageDeletedInfo = True
            End If

            Dim slots As JsonElement = currentrevision.GetProperty("slots")
            Dim mainslot As JsonElement = slots.GetProperty("main")
            qpageContent = mainslot.GetProperty("*").GetString

            _Title = qpageTitle
            _PageNamespace = qpageNS
            _LastEdit = qpageLastEdit
            _Timestamp = qpageLastTimestamp
            _CurrentRevId = qpageRevID
            _ParentRevId = qpageParentRevID
            _Lastuser = qpageLastUser
            _Comment = qpageComment
            _Thumbnail = qpageThumbnail
            _Categories = qpageCategories.ToArray
            _Size = qpageSize
            _Content = qpageContent
            _Exists = True
            _HasHiddenInfo = qpageDeletedInfo
            _RootPage = qpageRoot
            _Extract = qpageExtract
            _PageID = qpageID
            Return True
        End Function

        Private Function GetCurrentTimestamp() As String
            Dim querystring As String = String.Format(SStrings.GetLastTimestamp, Title)
            Dim queryresponse As String = WorkerBot.GETQUERY(querystring)
            Dim jsonResponse As JsonDocument = GetJsonDocument(queryresponse)
            Dim iserror As Boolean = IsJsonPropertyPresent(jsonResponse.RootElement, "error")
            If iserror Then
                Return ""
            End If
            Dim query As JsonElement = GetJsonElement(jsonResponse, "query")
            Dim pages As JsonElement = query.GetProperty("pages")
            Dim pageProperty As JsonProperty = pages.EnumerateObject(0)
            Dim pageElement As JsonElement = pageProperty.Value
            If IsJsonPropertyPresent(pageElement, "missing") Then
                Return ""
            End If
            Dim revisions As JsonElement = pageElement.GetProperty("revisions")
            Dim currentrevision As JsonElement = revisions.EnumerateArray(0)
            Return currentrevision.GetProperty("timestamp").GetString
        End Function

        ''' <summary>
        ''' Elimina una referencia que contenga una cadena exacta.
        ''' (No usar a menos de que se esté absolutamente seguro de lo que se hace).
        ''' </summary>
        ''' <param name="RequestedPage">Página a revisar</param>
        ''' <param name="RequestedRef">Texto que determina que referencia se elimina</param>
        ''' <returns></returns>
        Function RemoveRef(ByVal requestedPage As Page, requestedRef As String) As Boolean
            If String.IsNullOrWhiteSpace(requestedRef) Then
                Return False
            End If
            If requestedPage Is Nothing Then
                Return False
            End If
            Dim pageregex As String = String.Empty
            Dim PageText As String = requestedPage.Content
            For Each c As Char In requestedRef
                pageregex = pageregex & "[" & c.ToString.ToUpper & c.ToString.ToLower & "]"
            Next

            If requestedPage.PageNamespace = 0 Then
                For Each m As Match In Regex.Matches(PageText, "(<[REFref]+>)([^<]+?)" & pageregex & ".+?(<[/REFref]+>)")
                    PageText = PageText.Replace(m.Value, "")
                Next
                requestedPage.CheckAndSave(PageText, String.Format(Messages.RemovingRefs, requestedRef), False, True)
                Return True
            Else
                Return False
            End If

        End Function

        ''' <summary>
        ''' Retorna el promedio de los últimos dos meses de la página entregada (solo para páginas wikimedia)
        ''' El proyecto se deduce extrayendo el texto entre "https://" y ".org"
        ''' </summary>
        ''' <param name="Page">Nombre exacto de la página a evaluar</param>
        ''' <returns></returns>
        Private Function GetPageViewsAvg(ByVal page As String) As Integer
            Try
                Dim Project As String = TextInBetween(SiteUri.OriginalString, "https://", ".org")(0)
                Dim currentDate As DateTime = DateTime.Now
                Dim Month As Integer = currentDate.Month - 1
                Dim CurrentMonth As Integer = currentDate.Month
                Dim Year As Integer = currentDate.Year
                Dim Currentyear As Integer = currentDate.Year
                Dim FirstDay As String = "01"
                Dim LastDay As Integer = System.DateTime.DaysInMonth(Year, CurrentMonth)
                Dim Views As New List(Of Integer)
                Dim ViewAverage As Integer
                Dim totalviews As Integer

                If Month = 0 Then
                    Month = 12
                    Year -= 1
                End If

                Dim Url As Uri = New Uri(String.Format(SStrings.GetPageViews, Project, page, Year, Currentyear, Month.ToString("00"), CurrentMonth.ToString("00"), FirstDay, LastDay))
                Dim response As String = WorkerBot.GET(Url)

                For Each view As String In TextInBetween(response, """views"":", "}")
                    Views.Add(Integer.Parse(view))
                Next
                For Each i As Integer In Views
                    totalviews += i
                Next
                ViewAverage = CInt((totalviews / (Views.Count - 1)))

                Return ViewAverage
            Catch ex As IndexOutOfRangeException
                Return 0
            End Try

        End Function
    End Class
End Namespace