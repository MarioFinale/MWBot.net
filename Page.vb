Option Strict On
Option Explicit On
Imports System.Net
Imports System.Text.RegularExpressions
Imports MWBot.net.GlobalVars
Imports Utils.Utils

Namespace WikiBot
    Public Class Page

        Private _bot As Bot


#Region "Properties"

        Private _ID As Integer
        Private _username As String
        Private _siteuri As Uri

        ''' <summary>
        ''' Entrega el puntaje ORES {damaging,goodfaith} de la página.
        ''' </summary>
        ''' <returns></returns>
        Public Function ORESScores() As Double()
            Return GetORESScores(_currentRevID)
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

        ''' <summary>
        ''' Entrega las secciones de la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Threads As String()

        ''' <summary>
        ''' Entrega las primeras 10 categorías de la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Categories As String()

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
                Return GetPageViewsAvg(_title)
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
        ''' Entrega la fecha de la edición en la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EditDate As Date

        ''' <summary>
        ''' Indica si la pagina existe.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Exists As Boolean
            Get
                If _ID = -1 Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        ''' <summary>
        ''' Indica si la pagina puede ser editada por el bot.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property BotEditable As Boolean
            Get
                Return BotCanEdit(Me.Content, _bot.LocalName)
            End Get
        End Property

        ''' <summary>
        ''' Entrega la fecha de la última edición en la página.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LastEdit As Date
            Get
                Return GetLastEdit()
            End Get
        End Property


        Public ReadOnly Property LastTimeStamp As String
            Get
                Return GetLastTimeStamp()
            End Get
        End Property

        Public ReadOnly Property URL As String
            Get
                Return _bot.WikiUri.OriginalString & "wiki/" & Title.Replace(" ", "_")
            End Get
        End Property
#End Region
        ''' <summary>
        ''' Inicializa una nueva página, por lo general no se llama de forma directa. Se puede obtener una página creandola con Bot.Getpage.
        ''' </summary>
        ''' <param name="PageTitle">Título exacto de la página</param>
        ''' <param name="wbot">Bot logueado a la wiki</param>
        Public Sub New(ByVal pageTitle As String, ByRef wbot As Bot)
            If wbot Is Nothing Then Throw New ArgumentNullException("wbot")
            _bot = wbot
            _username = _bot.UserName
            Loadpage(pageTitle, _bot.WikiUri)
        End Sub
        ''' <summary>
        ''' Inicializa una nueva página, por lo general no se llama de forma directa. Se puede obtener una página creandola con Bot.Getpage.
        ''' </summary>
        ''' <param name="revid">Revision ID.</param>/param>
        ''' <param name="wbot">Bot logueado a la wiki</param>
        Public Sub New(ByVal revid As Integer, ByRef wbot As Bot)
            If wbot Is Nothing Then Throw New ArgumentNullException("wbot")
            _bot = wbot
            _username = _bot.UserName
            Loadpage(revid, _bot.WikiUri)
        End Sub

        ''' <summary>
        ''' Inicializa de nuevo la página (al crear una página esta ya está inicializada).
        ''' </summary>
        Public Sub Load()
            Loadpage(_title, _siteuri)
        End Sub

        ''' <summary>
        ''' Inicializa la página, esta función no se llama de forma directa
        ''' </summary>
        ''' <param name="PageTitle">Título exacto de la página</param>
        ''' <param name="site">Sitio de la página</param>
        ''' <returns></returns>
        Private Overloads Function Loadpage(ByVal PageTitle As String, ByVal site As Uri) As Boolean
            If site Is Nothing Then
                Throw New ArgumentNullException("site", "Empty parameter")
            End If

            If String.IsNullOrEmpty(PageTitle) Then
                Throw New ArgumentNullException("PageTitle", "Empty parameter")
            End If
            _siteuri = site
            PageInfoData(PageTitle)
            _Threads = GetPageThreads(_Content)
            EventLogger.Debug_Log(String.Format(Messages.PageLoaded, PageTitle), Reflection.MethodBase.GetCurrentMethod().Name, _username)
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
                Throw New ArgumentNullException("site")
            End If
            If Revid <= 0 Then
                Throw New ArgumentNullException("Revid")
            End If
            _siteuri = site
            PageInfoData(Revid)
            _Threads = GetPageThreads(_Content)
            EventLogger.Debug_Log(String.Format(Messages.PRevLoaded, Revid.ToString), Reflection.MethodBase.GetCurrentMethod().Name, _username)
            Return True
        End Function

        ''' <summary>
        ''' Retorna el valor ORES (en %) de un EDIT ID (eswiki) indicados como porcentaje en double. 
        ''' En caso de no existir el EDIT ID, retorna 0.
        ''' </summary>  
        ''' <param name="revid">EDIT ID de la edicion a revisar</param>
        ''' <remarks>Los EDIT ID deben ser distintos</remarks>
        Private Function GetORESScores(ByVal revid As Integer) As Double()
            EventLogger.Debug_Log(String.Format(Messages.LoadingOres, revid.ToString), Reflection.MethodBase.GetCurrentMethod().Name, _username)
            Try
                Dim turi As Uri = New Uri(SStrings.GetOresScore & revid)
                Dim s As String = _bot.GET(turi)

                Dim DMGScore_str As String = TextInBetween(TextInBetweenInclusive(s, "{""damaging"": {""score"":", "}}}")(0), """true"": ", "}}}")(0).Replace(".", DecimalSeparator)
                Dim GoodFaithScore_str As String = TextInBetween(TextInBetweenInclusive(s, """goodfaith"": {""score"":", "}}}")(0), """true"": ", "}}}")(0).Replace(".", DecimalSeparator)
                Dim DMGScore As Double = Math.Round((Double.Parse(DMGScore_str) * 100), 2)
                Dim GoodFaithScore As Double = Math.Round((Double.Parse(GoodFaithScore_str) * 100), 2)
                EventLogger.Debug_Log(String.Format(Messages.OresLoaded, revid.ToString, DMGScore_str, GoodFaithScore_str), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return {DMGScore, GoodFaithScore}
            Catch ex As IndexOutOfRangeException
                EventLogger.Debug_Log(String.Format(Messages.OresFailed, revid, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return Nothing
            End Try
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
                Throw New ArgumentNullException("pageContent", "Empty parameter")
            End If

            If pageContent = Content Then
                EventLogger.Debug_Log(String.Format(Messages.NoChanges, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.Edit_successful
            End If

            Dim EditToken As String = String.Empty
            Try
                EditToken = GetEditToken()
            Catch ex As WebException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.POST_error
            End Try

            Dim ntimestamp As String = GetLastTimeStamp()
            If Not ntimestamp = _timestamp Then
                EventLogger.Log(String.Format(Messages.EditConflict, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.Edit_conflict
            End If

            Dim additionalParams As String = String.Empty
            If IsMinor Then
                additionalParams = "&minor="
            Else
                additionalParams = "&notminor="
            End If

            If IsBot Then
                additionalParams = additionalParams & "&bot="
            End If

            Dim postdata As String = String.Format(SStrings.SavePage, additionalParams, _title, UrlWebEncode(EditSummary), UrlWebEncode(pageContent), UrlWebEncode(EditToken))
            Dim postresult As String = String.Empty

            Try
                postresult = _bot.POSTQUERY(postdata)
                Threading.Thread.Sleep(1000) 'Some time for the server to process the data
                Load() 'Update page data
            Catch ex As IO.IOException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.POST_error
            Catch ex As WebException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.POST_error
            End Try

            If String.IsNullOrWhiteSpace(postresult) Then
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _title, PsvSafeEncode(postresult)), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.POST_error
            End If

            If postresult.Contains("""result"":""Success""") Then
                EventLogger.Log(String.Format(Messages.SuccessfulEdit, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.Edit_successful
            End If

            If postresult.ToLower.Contains("abusefilter") Then
                EventLogger.Log(String.Format(Messages.AbuseFilter, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                EventLogger.Debug_Log("ABUSEFILTER: " & postresult, Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.AbuseFilter
            End If

            If postresult.ToLower.Contains("spamblacklist") Then
                EventLogger.Log(String.Format(Messages.SpamBlackList, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                EventLogger.Debug_Log("SPAMBLACKLIST: " & postresult, Reflection.MethodBase.GetCurrentMethod().Name, _username)
                If Spamreplace Then
                    Dim spamlinkRegex As String = TextInBetween(postresult, """spamblacklist"":""", """")(0)
                    Dim newtext As String = Regex.Replace(pageContent, SpamListParser(spamlinkRegex), Function(x) "<nowiki>" & x.Value & "</nowiki>") 'Reeplazar links con el Nowiki
                    If Not RetryCount > MaxRetry Then
                        Return SavePage(newtext, EditSummary, IsMinor, IsBot, True, RetryCount + 1)
                    Else
                        EventLogger.Log(String.Format(Messages.MaxRetryCount, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
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
            EventLogger.EX_Log(PsvSafeEncode(postresult), Reflection.MethodBase.GetCurrentMethod().Name, _username)

            If Not RetryCount > MaxRetry Then
                'Refresh credentials, retry
                _bot.Relogin()
                Return SavePage(pageContent, EditSummary, IsMinor, IsBot, True, RetryCount + 1)
            Else
                EventLogger.Log(String.Format(Messages.SpamBlackList, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
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
            If Not BotCanEdit(_content, _username) Then
                EventLogger.Log(String.Format(Messages.NoBots, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
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
            Return SavePage(Content, Messages.DefaultSumm, False, False, False, 0)
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

            If Not GetLastTimeStamp() = _timestamp Then
                EventLogger.Log(String.Format(Messages.EditConflict, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.Edit_conflict
            End If

            If Not BotCanEdit(_content, _username) Then
                EventLogger.Log(String.Format(Messages.NoBots, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.No_bots
            End If

            If isMinor Then
                additionalParameters = additionalParameters & "&minor=true"
            End If

            Dim postdata As String = String.Format(SStrings.AddThread, additionalParameters, UrlWebEncode(_title), UrlWebEncode(editSummary), UrlWebEncode(sectionTitle), UrlWebEncode(sectionContent), UrlWebEncode(GetEditToken()))
            Dim postresult As String = String.Empty
            Try
                postresult = _bot.POSTQUERY(postdata)
                Threading.Thread.Sleep(1000) 'Some time to the server to process the data
                Load() 'Update page data
            Catch ex As WebException
                EventLogger.EX_Log(String.Format(Messages.POSTEX, _title, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.POST_error
            End Try

            If postresult.Contains("""result"":""Success""") Then
                EventLogger.Log(String.Format(Messages.SuccessfulEdit, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                Return EditResults.Edit_successful
            End If

            If postresult.Contains("abusefilter") Then
                EventLogger.Log(String.Format(Messages.AbuseFilter, _title), Reflection.MethodBase.GetCurrentMethod().Name, _username)
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
                Throw New ArgumentException("Parameter empty", "userName")
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
            Dim queryresult As String = _bot.POSTQUERY(querytext)
            Dim token As String = TextInBetween(queryresult, """csrftoken"":""", """}}")(0).Replace("\\", "\")
            Return token
        End Function

        ''' <summary>
        ''' Hace una solicitud a la API respecto a una página y actualiza los datos
        ''' </summary>
        ''' <param name="Pagename">Título exacto de la página</param>
        Private Overloads Sub PageInfoData(ByVal pageName As String)
            Dim querystring As String = String.Format(SStrings.PageInfo, UrlWebEncode(pageName))
            Dim QueryText As String = _bot.GETQUERY(querystring)
            LoadPageInfo(querystring, pageName)
        End Sub

        ''' <summary>
        ''' Hace una solicitud a la API respecto a una página y actualiza los datos
        ''' </summary>
        ''' <param name="Revid">Revision ID de la página</param>
        Private Overloads Sub PageInfoData(ByVal Revid As Integer)
            Dim querystring As String = String.Format(SStrings.PageInfoRevid, Revid.ToString)
            LoadPageInfo(querystring, Revid.ToString)
        End Sub

        Private Sub LoadPageInfo(ByVal querystring As String, ByVal PagenameOrID As String)
            Dim QueryText As String = _bot.GETQUERY(querystring)
            Dim Def As String = "-1"

            Dim PTitle As String = NormalizeUnicodetext(TextInBetween(QueryText, """title"":""", """,").DefaultIfEmpty("").FirstOrDefault())
            Dim WNamespace As String = TextInBetween(QueryText, """ns"":", ",").DefaultIfEmpty(Def).FirstOrDefault()
            Dim PageID As String = TextInBetween(QueryText, "{""pageid"":", ",""ns").DefaultIfEmpty(Def).FirstOrDefault()
            Dim PaRevID As String = TextInBetween(QueryText, """parentid"":", ",""").DefaultIfEmpty(Def).FirstOrDefault()
            Dim PRevID As String = TextInBetween(QueryText, """revid"":", ",""").DefaultIfEmpty(Def).FirstOrDefault()
            Dim User As String = NormalizeUnicodetext(TextInBetween(QueryText, """user"":""", """,").DefaultIfEmpty("").FirstOrDefault())
            Dim Timestamp As String = TextInBetween(QueryText, """timestamp"":""", """,").DefaultIfEmpty("").FirstOrDefault()
            Dim Size As String = NormalizeUnicodetext(TextInBetween(QueryText, ",""size"":", ",""").DefaultIfEmpty("0").FirstOrDefault())
            Dim pComment As String = NormalizeUnicodetext(TextInBetween(QueryText, """comment"":""", """,").DefaultIfEmpty("").FirstOrDefault())
            Dim Wikitext As String = If(Regex.Match(QueryText, "(wikitext"","".+?\*"":"")([\s\S]+?[^\\])(""}})", RegexOptions.IgnoreCase).Groups(2).Value, "")
            Wikitext = NormalizeUnicodetext(Wikitext)
            Dim PExtract As String = NormalizeUnicodetext(TextInBetween(QueryText, """extract"":""", """}").DefaultIfEmpty("").FirstOrDefault())
            Dim PageImage As String = NormalizeUnicodetext(TextInBetween(QueryText, """pageimage"":""", """").DefaultIfEmpty("").FirstOrDefault())
            Dim PCategories As New List(Of String)

            If PageID = "-1" Then
                EventLogger.Debug_Log(String.Format(Messages.PageDoesNotExist, PagenameOrID), Reflection.MethodBase.GetCurrentMethod().Name, _username)
            End If

            Dim deletedinfo As Func(Of Boolean) = Function()
                                                      Dim vars As String() = {User, Wikitext}
                                                      For Each v As String In vars
                                                          If String.IsNullOrWhiteSpace(v) Then Return True
                                                      Next
                                                      Return False
                                                  End Function

            If deletedinfo() Then
                EventLogger.Debug_Log(String.Format(Messages.DeletedInfoMessage, PagenameOrID), Reflection.MethodBase.GetCurrentMethod().Name, _username)
                _HasHiddenInfo = True
            End If

            'Inutil
            'If String.IsNullOrWhiteSpace(PageImage) Then
            '    EventLogger.Debug_Log(String.Format(Messages.PageNoThumb, PagenameOrID), Reflection.MethodBase.GetCurrentMethod().Name, _username)
            'End If

            For Each m As Match In Regex.Matches(QueryText, "title"":""[Cc][a][t][\S\s]+?(?=""})")
                PCategories.Add(NormalizeUnicodetext(m.Value.Replace("title"":""", "")))
            Next

            Dim Rootp As String
            If Regex.Match(PTitle, "\/.+").Success Then
                Rootp = PTitle.Split("/"c)(0)
            Else
                Rootp = PTitle
            End If

            _RootPage = Rootp
            _Title = PTitle
            _ID = Integer.Parse(PageID)
            _Lastuser = User
            _Timestamp = Timestamp
            _Content = Wikitext
            _Size = Integer.Parse(Size)
            _PageNamespace = Integer.Parse(WNamespace)
            _Categories = PCategories.ToArray
            _CurrentRevId = Integer.Parse(PRevID)
            _ParentRevId = Integer.Parse(PaRevID)
            _Extract = PExtract
            _Thumbnail = PageImage
            _Comment = pComment
            _EditDate = ConvertTimestampToDate(Timestamp)
        End Sub




        ''' <summary>
        ''' Entrega la ultima marca de tiempo de la pagina.
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLastTimeStamp() As String
            Dim querystring As String = String.Format(SStrings.GetLastTimestamp, _title)
            Try
                Dim QueryText As String = _bot.POSTQUERY(querystring)
                Return TextInBetween(QueryText, """timestamp"":""", """")(0)
            Catch ex As IndexOutOfRangeException
                Return ""
            Catch ex As WebException
                Return ""
            End Try
        End Function

        ''' <summary>
        ''' Entrega la ultima marca de tiempo de la pagina.
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLastEdit() As Date
            Dim timestamp As String = GetLastTimeStamp()
            Return ConvertTimestampToDate(timestamp)
        End Function

        ''' <summary>
        ''' Convierte un timestamp de mediawiki a un objeto Date
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        Private Function ConvertTimestampToDate(ByVal timestamp As String) As Date
            Dim timestringarray As String() = timestamp.Replace("T"c, " "c).Replace("Z"c, "").Replace("-"c, " "c).Replace(":"c, " "c).Split(" "c)
            Dim timeintarray As Integer() = StringArrayToInt(timestringarray)
            Dim editdate As Date = New Date(timeintarray(0), timeintarray(1), timeintarray(2), timeintarray(3), timeintarray(4), timeintarray(5), DateTimeKind.Utc)
            Return editdate
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
                Dim Project As String = TextInBetween(_siteuri.OriginalString, "https://", ".org")(0)
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
                    Year = Year - 1
                End If

                Dim Url As Uri = New Uri(String.Format(SStrings.GetPageViews, Project, page, Year, Currentyear, Month.ToString("00"), CurrentMonth.ToString("00"), FirstDay, LastDay))
                Dim response As String = _bot.GET(Url)

                For Each view As String In TextInBetween(response, """views"":", "}")
                    Views.Add(Integer.Parse(view))
                Next
                For Each i As Integer In Views
                    totalviews = totalviews + i
                Next
                ViewAverage = CInt((totalviews / (Views.Count - 1)))

                Return ViewAverage
            Catch ex As IndexOutOfRangeException
                Return 0
            End Try

        End Function
    End Class
End Namespace