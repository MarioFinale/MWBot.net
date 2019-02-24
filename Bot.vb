﻿Option Strict On
Option Explicit On
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports MWBot.net.GlobalVars

Namespace WikiBot
    Public Class Bot

#Region "Properties"
        Private _botPassword As String
        Private _botUserName As String
        Private _apiUri As Uri
        Private _wikiUri As Uri

        Private Api As ApiHandler
        Private _localName As String
        Private _userName As String

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

        Public ReadOnly Property ApiUri As Uri
            Get
                Return _apiUri
            End Get
        End Property

        Public ReadOnly Property UserName As String
            Get
                Return _userName
            End Get
        End Property

        Public ReadOnly Property LocalName As String
            Get
                Return _localName
            End Get
        End Property

        Public ReadOnly Property WikiUri As Uri
            Get
                Return _wikiUri
            End Get
        End Property
#End Region

#Region "Init"
        Sub New(ByVal configPath As ConfigFile)
            Dim valid As Boolean = LoadConfig(configPath)
            Do Until valid
                valid = LoadConfig(configPath)
            Loop
            Api = New ApiHandler(_botUserName, _botPassword, _apiUri)
            _userName = Api.UserName
        End Sub

        Sub Relogin()
            Api = New ApiHandler(_botUserName, _botPassword, _apiUri)
        End Sub
        ''' <summary>
        ''' Inicializa las configuraciones genereales del programa desde el archivo de configuración.
        ''' Si no existe el archivo, solicita datos al usuario y lo genera.
        ''' </summary>
        ''' <returns></returns>
        Function LoadConfig(ByVal Tfile As ConfigFile) As Boolean
            If Tfile Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            Dim MainBotName As String = String.Empty
            Dim WPSite As String = String.Empty
            Dim WPAPI As String = String.Empty
            Dim WPBotUserName As String = String.Empty
            Dim WPBotPassword As String = String.Empty
            Dim ConfigOK As Boolean = False
            Console.WriteLine(String.Format(Messages.GreetingMsg, MwBotVersion))
            Utils.EventLogger.Debug_Log(Messages.BotEngine & MwBotVersion, Reflection.MethodBase.GetCurrentMethod().Name)
            If System.IO.File.Exists(Tfile.Path) Then
                Utils.EventLogger.Log(Messages.LoadingConfig, Reflection.MethodBase.GetCurrentMethod().Name)
                Dim Configstr As String = System.IO.File.ReadAllText(Tfile.Path)
                Try
                    MainBotName = Utils.TextInBetween(Configstr, "BOTName=""", """")(0)
                    WPBotUserName = Utils.TextInBetween(Configstr, "WPUserName=""", """")(0)
                    WPSite = Utils.TextInBetween(Configstr, "PageURL=""", """")(0)
                    WPBotPassword = Utils.TextInBetween(Configstr, "WPBotPassword=""", """")(0)
                    WPAPI = Utils.TextInBetween(Configstr, "ApiURL=""", """")(0)
                    ConfigOK = True
                Catch ex As IndexOutOfRangeException
                    Utils.EventLogger.Log(Messages.ConfigError, Reflection.MethodBase.GetCurrentMethod().Name)
                End Try
            Else
                Utils.EventLogger.Log(Messages.NoConfigFile, Reflection.MethodBase.GetCurrentMethod().Name)
                Try
                    System.IO.File.Create(Tfile.ToString).Close()
                Catch ex As System.IO.IOException
                    Utils.EventLogger.Log(Messages.NewConfigFileError, Reflection.MethodBase.GetCurrentMethod().Name)
                End Try
            End If

            If Not ConfigOK Then
                Console.Clear()
                Console.WriteLine(Messages.NewConfigMessage)
                Console.WriteLine(Messages.NewBotName)
                MainBotName = Console.ReadLine
                Console.WriteLine(Messages.NewUserName)
                WPBotUserName = Console.ReadLine
                Console.WriteLine(Messages.NewBotPassword)
                WPBotPassword = Console.ReadLine
                Console.WriteLine(Messages.NewWikiMainUrl)
                WPSite = Console.ReadLine
                Console.WriteLine(Messages.NewWikiMainApiUrl)
                WPAPI = Console.ReadLine

                Dim configstr As String = String.Format(SStrings.ConfigTemplate, MainBotName, WPBotUserName, WPBotPassword, WPSite, WPAPI)
                Try
                    System.IO.File.WriteAllText(Tfile.Path, configstr)
                Catch ex As System.IO.IOException
                    Utils.EventLogger.Log(Messages.SaveConfigError, Reflection.MethodBase.GetCurrentMethod().Name)
                End Try
            End If

            _localName = MainBotName
            _botUserName = WPBotUserName
            _botPassword = WPBotPassword
            Try
                _apiUri = New Uri(WPAPI)
                _wikiUri = New Uri(WPSite)
            Catch ex As ArgumentException
                Utils.EventLogger.Log(Messages.InvalidUrl, Reflection.MethodBase.GetCurrentMethod().Name)
                System.IO.File.Delete(Tfile.Path)
                Utils.WaitSeconds(5)
                Return False
            Catch ex2 As UriFormatException
                Utils.EventLogger.Log(Messages.InvalidUrl, Reflection.MethodBase.GetCurrentMethod().Name)
                System.IO.File.Delete(Tfile.Path)
                Utils.PressKeyTimeout(5)
                Return False
            End Try
            Return True
        End Function

#End Region

#Region "ApiFunctions"
        Function POSTQUERY(ByVal postdata As String) As String
            Return Api.Postquery(postdata)
        End Function

        Function GETQUERY(ByVal getdata As String) As String
            Return Api.Getquery(getdata)
        End Function

        Function [GET](ByVal turi As Uri) As String
            Return Api.GET(turi)
        End Function
#End Region

#Region "BotFunctions"
        Public Function GetSpamListregexes(ByVal spamlistPage As Page) As String()
            If spamlistPage Is Nothing Then Throw New ArgumentNullException(Reflection.MethodBase.GetCurrentMethod().Name)
            Dim Lines As String() = Utils.GetLines(spamlistPage.Content, True) 'Extraer las líneas del texto de la página
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
        ''' Retorna el ultimo REVID (como integer) de las paginas indicadas como SortedList (con el formato {Pagename,Revid}), las paginas deben ser distintas. 
        ''' En caso de no existir la pagina, retorna -1 como REVID.
        ''' </summary>
        ''' <param name="pageNames">Array con nombres de paginas unicos.</param>
        ''' <remarks></remarks>
        Function GetLastRevIds(ByVal pageNames As String()) As SortedList(Of String, Integer)
            Utils.EventLogger.Debug_Log(String.Format(Messages.GetLastrevIDs, pageNames.Count), Reflection.MethodBase.GetCurrentMethod().Name)
            Dim PageNamesList As List(Of String) = pageNames.ToList
            PageNamesList.Sort()
            Dim PageList As List(Of List(Of String)) = Utils.SplitStringArrayIntoChunks(PageNamesList.ToArray, 50)
            Dim PagenameAndLastId As New SortedList(Of String, Integer)
            For Each ListInList As List(Of String) In PageList
                Dim Qstring As String = String.Empty
                For Each s As String In ListInList
                    s = Utils.UrlWebEncode(s)
                    Qstring = Qstring & s & "|"
                Next
                Qstring = Qstring.Trim(CType("|", Char))

                Dim QueryResponse As String = GETQUERY((SStrings.GetLastRevIds & Qstring))
                Dim ResponseArray As String() = Utils.TextInBetweenInclusive(QueryResponse, ",""title"":", "}]")

                For Each s As String In ResponseArray

                    Dim pagetitle As String = Utils.TextInBetween(s, ",""title"":""", """,""")(0)

                    If s.Contains(",""missing"":") Then
                        If Not PagenameAndLastId.ContainsKey(pagetitle) Then
                            PagenameAndLastId.Add(Utils.UrlWebDecode(Utils.NormalizeUnicodetext(pagetitle)).Replace(" ", "_"), -1)
                        End If
                    Else
                        If Not PagenameAndLastId.ContainsKey(pagetitle) Then
                            Dim TemplateTitle As String = String.Empty
                            Dim Revid As String = Utils.TextInBetween(s, pagetitle & """,""revisions"":[{""revid"":", ",""parentid"":")(0)
                            Dim Revid_ToInt As Integer = CType(Revid, Integer)

                            Dim modlist As New List(Of String)

                            For Each tx As String In PageNamesList.ToArray
                                Dim tmp As String = tx.ToLower.Replace("_", " ")
                                modlist.Add(tmp)
                            Next

                            Dim normtext As String = Utils.NormalizeUnicodetext(pagetitle)
                            normtext = normtext.ToLower.Replace("_", " ")
                            Dim ItemIndex As Integer = modlist.IndexOf(normtext)
                            TemplateTitle = PageNamesList(ItemIndex)
                            PagenameAndLastId.Add(TemplateTitle, Revid_ToInt)

                        End If
                    End If
                Next
            Next
            Utils.EventLogger.Debug_Log(String.Format(Messages.DoneXPagesReturned, PagenameAndLastId.Count), Reflection.MethodBase.GetCurrentMethod().Name)
            Return PagenameAndLastId
        End Function

        ''' <summary>
        ''' Entrega los títulos de las páginas que coincidan remotamente con el texto entregado como parámetro.
        ''' Usa las mismas sugerencias del cuadro de búsqueda de Wikipedia, pero por medio de la API.
        ''' Si no hay coincidencia, entrega un un string() vacio.
        ''' </summary>
        ''' <param name="PageName">Título aproximado o similar al de una página</param>
        ''' <returns></returns>
        Function SearchForPages(pageName As String) As String()
            Return Utils.GetTitlesFromQueryText(GETQUERY(SStrings.Search & pageName))
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
        ''' En caso de no existir el EDIT ID, retorna 0.
        ''' </summary>
        ''' <param name="revids">Array con EDIT ID's unicos.</param>
        ''' <remarks>Los EDIT ID deben ser distintos</remarks>
        Function GetORESScores(ByVal revids As Integer()) As SortedList(Of Integer, Double())
            Dim Revlist As List(Of List(Of Integer)) = Utils.SplitIntegerArrayIntoChunks(revids, 50)
            Dim EditAndScoreList As New SortedList(Of Integer, Double())
            For Each ListOfList As List(Of Integer) In Revlist

                Dim Qstring As String = String.Empty
                For Each n As Integer In ListOfList
                    Qstring = Qstring & n.ToString & "|"
                Next
                Qstring = Qstring.Trim(CType("|", Char))
                Dim apiuri As Uri = New Uri(SStrings.OresScoresApiQueryUrl & Utils.UrlWebEncode(Qstring))
                Dim s As String = Api.GET(apiuri)

                For Each m As Match In Regex.Matches(s, "({|, )(""[0-9]+"":).+?(}}}})")
                    Dim EditID_str As String = Regex.Match(m.Value, """[0-9]+""").Value
                    EditID_str = EditID_str.Trim(CType("""", Char()))
                    EditID_str = Utils.RemoveAllAlphas(EditID_str)
                    Dim EditID As Integer = Integer.Parse(EditID_str)

                    If m.Value.Contains("error") Then

                        Utils.EventLogger.Debug_Log(String.Format(Messages.OresQueryError, EditID_str), Reflection.MethodBase.GetCurrentMethod().Name)
                        EditAndScoreList.Add(EditID, {0, 0})
                    Else
                        Try
                            Dim DMGScore_str As String = Utils.TextInBetween(m.Value, """true"": ", "}")(0).Replace(".", DecimalSeparator)
                            Dim GoodFaithScore_str As String = Utils.TextInBetween(m.Value, """true"": ", "}")(1).Replace(".", DecimalSeparator)
                            Dim DMGScore As Double = Double.Parse(DMGScore_str) * 100
                            Dim GoodFaithScore As Double = Double.Parse(GoodFaithScore_str) * 100
                            Utils.EventLogger.Debug_Log(String.Format(Messages.OresQueryResult, EditID_str, GoodFaithScore.ToString, DMGScore.ToString), Reflection.MethodBase.GetCurrentMethod().Name)
                            EditAndScoreList.Add(EditID, {DMGScore, GoodFaithScore})
                        Catch ex As IndexOutOfRangeException
                            Utils.EventLogger.Debug_Log(String.Format(Messages.OresQueryEx, EditID_str, ex.Message), Reflection.MethodBase.GetCurrentMethod().Name)
                            EditAndScoreList.Add(EditID, {0, 0})
                        End Try
                    End If
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

            Dim PageList As List(Of List(Of String)) = Utils.SplitStringArrayIntoChunks(PageNamesList.ToArray, 20)
            Dim PagenameAndImage As New SortedList(Of String, String)

            For Each ListInList As List(Of String) In PageList
                Dim Qstring As String = String.Empty

                For Each s As String In ListInList
                    s = Utils.UrlWebEncode(s)
                    Qstring = Qstring & s & "|"
                Next
                Qstring = Qstring.Trim(CType("|", Char))

                Dim QueryResponse As String = GETQUERY(SStrings.GetPagesImage & Qstring)
                Dim ResponseArray As New List(Of String)

                For Each m As Match In Regex.Matches(QueryResponse, "({).+?(})(,|])(?={|})")
                    ResponseArray.Add(m.Value)
                Next

                For Each s As String In ResponseArray.ToArray

                    Dim pagetitle As String = Utils.TextInBetween(s, ",""title"":""", """")(0)
                    Dim PageImage As String = String.Empty
                    If Not s.Contains(",""missing"":") Then

                        If Not PagenameAndImage.ContainsKey(pagetitle) Then
                            Dim PageKey As String = String.Empty
                            Dim modlist As New List(Of String)
                            For Each tx As String In PageNamesList.ToArray
                                modlist.Add(tx.ToLower.Replace("_", " "))
                            Next
                            Dim normtext As String = Utils.NormalizeUnicodetext(pagetitle)
                            normtext = normtext.ToLower.Replace("_", " ")

                            Dim ItemIndex As Integer = modlist.IndexOf(normtext)
                            PageKey = PageNamesList(ItemIndex)

                            If s.Contains("pageimage") Then
                                PageImage = Utils.TextInBetweenInclusive(s, """title"":""" & pagetitle & """", """}")(0)
                                PageImage = Utils.TextInBetween(PageImage, """pageimage"":""", """}")(0)
                            Else
                                PageImage = String.Empty
                            End If
                            PagenameAndImage.Add(PageKey, PageImage)

                        End If
                    End If
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
                        If Not Utils.CountCharacter(TrimmedText, CType("(", Char)) = Utils.CountCharacter(TrimmedText, CType(")", Char)) Then
                            Continue For
                        End If
                    End If
                    If TrimmedText.Contains("<") Then
                        If Not Utils.CountCharacter(TrimmedText, CType("<", Char)) = Utils.CountCharacter(TrimmedText, CType(">", Char)) Then
                            Continue For
                        End If
                    End If
                    If TrimmedText.Contains("«") Then
                        If Not Utils.CountCharacter(TrimmedText, CType("«", Char)) = Utils.CountCharacter(TrimmedText, CType("»", Char)) Then
                            Continue For
                        End If
                    End If
                    If TrimmedText.Contains("{") Then
                        If Not Utils.CountCharacter(TrimmedText, CType("{", Char)) = Utils.CountCharacter(TrimmedText, CType("}", Char)) Then
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
                TrimmedText = Utils.RemoveExcessOfSpaces(TrimmedText)
            End If
            Return TrimmedText
        End Function

        Function GetExtractsFromApiResponse(ByVal queryresponse As String, ByVal charLimit As Integer, ByVal wiki As Boolean) As HashSet(Of WikiExtract)
            Dim ExtractsList As New HashSet(Of WikiExtract)
            Dim ResponseArray As String() = Utils.TextInBetweenInclusive(queryresponse, ",""title"":", """}")
            For Each s As String In ResponseArray
                If Not s.Contains(",""missing"":") Then
                    Dim pagetitle As String = Utils.TextInBetween(s, ",""title"":""", """,""")(0).Replace("_"c, " ")
                    Dim TreatedExtract As String = Utils.TextInBetween(s, pagetitle & """,""extract"":""", """}")(0)
                    TreatedExtract = Utils.NormalizeUnicodetext(TreatedExtract)
                    TreatedExtract = TreatedExtract.Replace("\n", Environment.NewLine)
                    TreatedExtract = TreatedExtract.Replace("\""", """")
                    TreatedExtract = Regex.Replace(TreatedExtract, "\{\\\\.*\}", " ")
                    TreatedExtract = Regex.Replace(TreatedExtract, "\[[0-9]+\]", " ")
                    TreatedExtract = Regex.Replace(TreatedExtract, "\[nota\ [0-9]+\]", " ")
                    TreatedExtract = Utils.RemoveExcessOfSpaces(TreatedExtract)
                    TreatedExtract = Utils.FixResumeNumericExp(TreatedExtract)
                    If TreatedExtract.Contains(""",""missing"":""""}}}}") Then
                        TreatedExtract = Nothing
                    End If
                    If TreatedExtract.Length > charLimit Then
                        TreatedExtract = SafeTrimExtract(TreatedExtract.Substring(0, charLimit + 1), charLimit)
                    End If
                    'Si el título de la página está en el resumen, coloca en negritas la primera ocurrencia
                    If wiki Then
                        Dim regx As New Regex(Regex.Escape(pagetitle), RegexOptions.IgnoreCase)
                        TreatedExtract = regx.Replace(TreatedExtract, "'''" & pagetitle & "'''", 1)
                    End If
                    Dim Extract As New WikiExtract With {
                        .ExtractContent = TreatedExtract,
                        .PageName = Utils.NormalizeUnicodetext(pagetitle)}
                    ExtractsList.Add(Extract)
                End If
            Next
            Return ExtractsList
        End Function


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
        ''' Obtiene la entradilla de varias páginas manteniendo el wikitexto pero eliminando plantillas y referencias.
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
        ''' Obtiene la entradilla de varias páginas manteniendo el wikitexto pero eliminando plantillas y referencias.
        ''' </summary>
        ''' <returns></returns>
        Function GetWikiExtractFromPages(ByVal pages As Page(), ByVal charLimit As Integer) As HashSet(Of WikiExtract)
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
        ''' Obtiene la entradilla de una página manteniendo el wikitexto pero eliminando plantillas y referencias.
        ''' </summary>
        ''' <param name="page"></param>
        ''' <param name="charLimit"></param>
        ''' <returns></returns>
        Function GetWikiExtractFromPage(ByVal page As Page, ByVal charLimit As Integer) As WikiExtract
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
                TreatedExtract = Utils.RemoveExcessOfSpaces(TreatedExtract)
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


        Private Function Removefiles(ByVal str As String) As String
            Dim tstr As String = str
            Do While True
                Dim match As Match = Regex.Match(tstr, "\[\[([Aa]rchivo:|[Ff]ile).+?\]\]")
                If Not match.Success Then
                    Exit Do
                End If
                Do While True
                    Dim tmatch As Match = Regex.Match(tstr, "\[\[([Aa]rchivo:|[Ff]ile).+?\]\]")
                    If (Utils.CountOccurrences(tmatch.Value, "[[") = Utils.CountOccurrences(tmatch.Value, "]]")) Then
                        tstr = tstr.Replace(tmatch.Value, "")
                        Exit Do
                    End If
                    Dim fixedmatch As String = Utils.ReplaceLast(Utils.ReplaceLast(tmatch.Value, "[[", ""), "]]", "")
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
            Utils.EventLogger.Log(String.Format(Messages.GetPagesExtract, pageNames.Count.ToString), Reflection.MethodBase.GetCurrentMethod().Name)
            If pageNames Is Nothing Then Return Nothing
            Dim PageNamesList As List(Of String) = pageNames.ToList
            PageNamesList.Sort()
            Dim PageList As List(Of List(Of String)) = Utils.SplitStringArrayIntoChunks(PageNamesList.ToArray, 20)
            Dim PagenameAndResume As New SortedList(Of String, String)

            For Each ListInList As List(Of String) In PageList
                Dim Qstring As String = String.Empty
                For Each s As String In ListInList
                    s = Utils.UrlWebEncode(s)
                    Qstring = Qstring & s & "|"
                Next
                Qstring = Qstring.Trim(CType("|", Char))
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
            Dim titles As String() = Utils.GetTitlesFromQueryText(GETQUERY(SStrings.SearchForUser & text))
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
            Dim newlist As New List(Of String)
            Dim s As String = String.Empty
            s = POSTQUERY(SStrings.GetPageInclusions & pageName)
            Dim pages As String() = Utils.TextInBetween(s, """title"":""", """}")
            For Each _pag As String In pages
                newlist.Add(Utils.NormalizeUnicodetext(_pag))
            Next
            Return newlist.ToArray
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
            Dim difftext As String = String.Empty
            Try
                difftext = Utils.NormalizeUnicodetext(Utils.TextInBetween(querytext, ",""*"":""", "\n""}}")(0))
            Catch ex As IndexOutOfRangeException
                Return New WikiDiff(fromid, toid, Changedlist)
            End Try
            Dim Rows As String() = Utils.TextInBetween(difftext, "<tr>", "</tr>")
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
        ''' Retorna un elemento Page coincidente al nombre entregado como parámetro.
        ''' </summary>
        ''' <param name="revId">ID de la revisión.</param>
        Function Getpage(ByVal revId As Integer) As Page
            Return New Page(revId, Me)
        End Function

        ''' <summary>
        ''' Verifica si el usuario que se le pase cumple con los requisitos para verificar su actividad
        ''' </summary>
        ''' <param name="user">Usuario de Wiki</param>
        ''' <returns></returns>
        Private Function ValidUser(ByVal user As WikiUser) As Boolean
            Utils.EventLogger.Debug_Log(String.Format(Messages.CheckingUser, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
            'Verificar si el usuario existe
            If Not user.Exists Then
                Utils.EventLogger.Log(String.Format(Messages.UserInexistent, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                Return False
            End If

            'Verificar si el usuario está bloqueado.
            If user.Blocked Then
                Utils.EventLogger.Log(String.Format(Messages.UserBlocked, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                Return False
            End If

            'Verificar si el usuario editó hace al menos 4 días.
            If Date.Now.Subtract(user.LastEdit).Days >= 4 Then
                Utils.EventLogger.Log(String.Format(Messages.UserInactive, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
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
            Dim QueryString As String = SStrings.PrefixSearchQuery & Utils.UrlWebEncode(pagePrefix)
            Dim QueryResult As String = POSTQUERY(QueryString)
            Dim Pages As String() = Utils.TextInBetween(QueryResult, """title"":""", """,""")
            Dim DecodedPages As New List(Of String)
            For Each p As String In Pages
                DecodedPages.Add(Utils.NormalizeUnicodetext(p))
            Next
            Return DecodedPages.ToArray
        End Function
#End Region

#Region "Subs"
        Sub MessageDelivery(ByVal userList As String(), messageTitle As String, messageContent As String, editSummary As String)
            For Each u As String In userList
                Dim user As New WikiUser(Me, u)
                If Not user.Exists Then
                    Utils.EventLogger.Log(String.Format(Messages.UserInexistent, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                    Continue For
                End If
                If user.Blocked Then
                    Utils.EventLogger.Log(String.Format(Messages.UserBlocked, user.UserName), Reflection.MethodBase.GetCurrentMethod().Name)
                    Continue For
                End If
                Dim userTalkPage As Page = user.TalkPage
                userTalkPage.AddSection(messageTitle, messageContent, editSummary, False)
            Next
        End Sub

#End Region

    End Class
End Namespace