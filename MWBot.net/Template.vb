Option Strict On
Option Explicit On
Imports System.Text.RegularExpressions
Imports MWBot.net.Utility
Imports MWBot.net.Utility.Utils

Namespace WikiBot

    Public Class Template
        Public ReadOnly Property Valid As Boolean
        Private _name As String
        Private _parameters As List(Of Tuple(Of String, String))
        Private _text As String
        Private Property Newtemplate As Boolean = True

        ''' <summary>
        ''' Nombre de la plantilla (con el espacio de nombres).
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ''' <summary>
        ''' Texto de la plantilla.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Text As String
            Get
                If Newtemplate Then
                    _text = CreateTemplatetext(_name, _parameters, True)
                    Return _text
                Else
                    Return _text
                End If
            End Get
        End Property

        ''' <summary>
        ''' Contiene los parámetros de la plantilla (con espacios y saltos de línea). Al establecer un valor se crea nuevamente el texto de la plantilla.
        ''' </summary>
        ''' <returns></returns>
        Public Property Parameters As List(Of Tuple(Of String, String))
            Get
                Return _parameters
            End Get
            Set(value As List(Of Tuple(Of String, String)))
                _parameters = value
                _text = CreateTemplatetext(_name, _parameters, True)
            End Set
        End Property

        ''' <summary>
        ''' Crea una nueva plantilla. Si es una nueva se considera el texto como el título, de lo contrario se considera como el contenido de la plantilla y se extrae de este los parámetros.
        ''' El texto de ser inválido, genera una plantilla vacía ("{{}}").
        ''' </summary>
        ''' <param name="Text">Texto a evaluar.</param>
        ''' <param name="newTemplate">¿Es una plantilla nueva?</param>
        Sub New(ByVal text As String, ByVal newTemplate As Boolean)
            Me.Newtemplate = newTemplate
            If newTemplate Then
                _name = text
                _text = MakeSimpleTemplateText(text)
                _parameters = New List(Of Tuple(Of String, String))
            Else
                GetTemplateOfText(text)
            End If
            _Valid = Not String.IsNullOrWhiteSpace(_name)
        End Sub

        ''' <summary>
        ''' Crea una nueva plantilla con los parámetros indicados.
        ''' </summary>
        ''' <param name="Templatename">Nombre de la plantilla.</param>
        ''' <param name="templateparams">Parámetros de la plantilla.</param>
        Sub New(ByVal templateName As String, ByVal templateParams As List(Of Tuple(Of String, String)))
            _name = templateName
            _parameters = templateParams
            _text = MakeTemplateText(templateName, templateParams)
            _Valid = Not String.IsNullOrWhiteSpace(_name)
        End Sub

        ''' <summary>
        ''' Crea una nueva plantilla vacía ("{{}}")
        ''' </summary>
        Sub New()
            _name = String.Empty
            _text = String.Empty
            _parameters = New List(Of Tuple(Of String, String))
            _Valid = False
        End Sub

        ''' <summary>
        ''' Crea el texto de una plantilla simple, que solo contiene el nombre de la misma.
        ''' </summary>
        ''' <param name="tempname">Nombre de la plantilla</param>
        ''' <returns></returns>
        Private Shared Function MakeSimpleTemplateText(ByVal tempname As String) As String
            Return "{{" & tempname & "}}"
        End Function

        ''' <summary>
        ''' Genera la plantilla a partir de los parámetros y el nombre de la misma.
        ''' </summary>
        ''' <param name="templatename"></param>
        ''' <param name="templateparams"></param>
        ''' <param name="newlines"></param>
        ''' <returns></returns>
        Private Shared Function CreateTemplatetext(ByVal templatename As String, ByVal templateparams As List(Of Tuple(Of String, String)), ByVal newlines As Boolean) As String
            Dim linechar As String = String.Empty
            If newlines Then linechar = Environment.NewLine
            Dim text As String = "{{" & templatename & linechar
            For Each t As Tuple(Of String, String) In templateparams
                text = text & "|" & t.Item1 & "=" & t.Item2 & linechar
            Next
            text &= "}}"
            Return text
        End Function

        ''' <summary>
        ''' Genera el texto de la plantilla a partir del nombre y parámetros indicados.
        ''' </summary>
        ''' <param name="tempname">Nombre de la plantilla.</param>
        ''' <param name="tempparams">Parámetros de la plantilla.</param>
        ''' <returns></returns>
        Private Shared Function MakeTemplateText(ByVal tempname As String, ByVal tempparams As List(Of Tuple(Of String, String))) As String
            Dim templatetext As String = String.Empty
            Dim opening As String = "{{"
            Dim closing As String = "}}"

            Dim paramstext As New List(Of String)
            tempparams = tempparams.OrderBy(Function(X) X.Item1).ToList

            For Each parampair As Tuple(Of String, String) In tempparams
                If IsNumeric(parampair.Item1) Then
                    paramstext.Add(parampair.Item2.Trim)
                Else
                    paramstext.Add(parampair.Item1.Trim & " = " & parampair.Item2.Trim)
                End If
            Next
            templatetext = opening & tempname

            For Each s As String In paramstext
                templatetext = templatetext & "|" & s
            Next
            templatetext &= closing
            Return templatetext

        End Function

        ''' <summary>
        ''' Indica si la plantilla contiene el nombre exacto del parámetro indicado.
        ''' </summary>
        ''' <param name="parametername"></param>
        ''' <returns></returns>
        Function ContainsParameter(ByVal parametername As String) As Boolean
            For Each ptup As Tuple(Of String, String) In Parameters
                If ptup.Item1.Trim() = parametername Then Return True
            Next
            Return False
        End Function

        ''' <summary>
        ''' Inicializa la plantilla extrayendo los datos de un texto que debería ser en formato de plantilla.
        ''' </summary>
        ''' <param name="templatetext"></param>
        Sub GetTemplateOfText(ByVal templatetext As String)
            If String.IsNullOrWhiteSpace(templatetext) Then
                Throw New ArgumentException("Empty parameter", NameOf(templatetext))
            End If
            'Verificar si se paso una plantilla
            If Not templatetext.Substring(0, 2) = "{{" Then
                Exit Sub
            End If
            If Not CountCharacter(templatetext, "{"c) = CountCharacter(templatetext, "}"c) Then
                Exit Sub
            End If
            If Not templatetext.Substring(templatetext.Length - 2, 2) = "}}" Then
                Exit Sub
            End If

            _text = templatetext
            _parameters = New List(Of Tuple(Of String, String))


            Dim NewText As String = _text
            Dim ReplacedTemplates As New List(Of String)
            Dim TemplateInnerText As String = NewText.Substring(2, NewText.Length - 4)

            'Reemplazar plantillas internas con texto para reconocer parametros de principal
            Dim temparray As List(Of String) = GetTemplateTextArray(TemplateInnerText)

            For templ As Integer = 0 To temparray.Count - 1
                Dim tempreplace As String = ColoredText("PERIODIBOT:TEMPLATEREPLACE::::" & templ.ToString, 1)
                NewText = NewText.Replace(temparray(templ), tempreplace)
                ReplacedTemplates.Add(temparray(templ))
            Next

            'Reemplazar enlaces dentro de la plantilla para reconocer parametros de principal
            Dim ReplacedLinks As New List(Of String)
            Dim LinkArray As New List(Of String)
            For Each m As Match In Regex.Matches(NewText, "((\[\[)([^\]]+)(\]\]))")
                LinkArray.Add(m.Value)
            Next

            For temp2 As Integer = 0 To LinkArray.Count - 1
                Dim LinkReplace As String = ColoredText("PERIODIBOT:LINKREPLACE::::" & temp2.ToString, 1)
                NewText = NewText.Replace(LinkArray(temp2), LinkReplace)
                ReplacedLinks.Add(LinkArray(temp2))
            Next


            'Reemplazar comentarios y nowiki dentro de la plantilla para reconocer parametros de principal
            Dim ReplacedComments As New List(Of String)
            Dim CommentsArray As New List(Of String)
            For Each m As Match In Regex.Matches(NewText, "((<!--)[\s\S]*?(-->)|(<[nN]owiki>)([\s\S]+?)(<\/[nN]owiki>))")
                CommentsArray.Add(m.Value)
            Next

            For temp3 As Integer = 0 To CommentsArray.Count - 1
                Dim CommentReplace As String = ColoredText("PERIODIBOT:COMMENTSREPLACE::::" & temp3.ToString, 1)
                NewText = NewText.Replace(CommentsArray(temp3), CommentReplace)
                ReplacedComments.Add(CommentsArray(temp3))
            Next

            'Obtener nombre de la plantilla
            Dim tempname As String = String.Empty
            Dim innertext As String = NewText.Substring(2, NewText.Length - 4)
            For cha As Integer = 0 To innertext.Count - 1
                If Not innertext(cha) = "|" Then
                    tempname &= innertext(cha)
                Else
                    Exit For
                End If
            Next

            'Reemplazar plantillas internas en el titulo con texto para reconocer nombre de la principal
            For reptempindex As Integer = 0 To ReplacedTemplates.Count - 1
                Dim tempreplace As String = ColoredText("PERIODIBOT:TEMPLATEREPLACE::::" & reptempindex.ToString, 1)
                tempname = tempname.Replace(tempreplace, ReplacedTemplates(reptempindex)).Trim(CType(" ", Char())).Trim(CType(Environment.NewLine, Char()))
            Next
            _name = tempname

            'Obtener parametros de texto tratado y agregarlos a lista
            Dim params As MatchCollection = Regex.Matches(innertext, "\|[^|]+")
            Dim NamedParams As New List(Of Tuple(Of String, String))
            Dim UnnamedParams As New List(Of String)
            Dim TotalParams As New List(Of Tuple(Of String, String))

            For Each m As Match In params
                Dim ParamNamematch As Match = Regex.Match(m.Value, "\|[^\|=]+=")

                If ParamNamematch.Success Then

                    Dim ParamName As String = ParamNamematch.Value.Substring(1, ParamNamematch.Length - 2)
                    Dim Paramvalue As String = m.Value.Replace(ParamNamematch.Value, String.Empty)
                    NamedParams.Add(New Tuple(Of String, String)(ParamName, Paramvalue))

                Else
                    Dim UnnamedParamValue As String = m.Value.Substring(1, m.Value.Length - 1)
                    UnnamedParams.Add(UnnamedParamValue)

                End If

            Next
            'Los parametros sin nombre son procesados y nombrados segun su posicion
            For param As Integer = 0 To UnnamedParams.Count - 1
                NamedParams.Add(New Tuple(Of String, String)((param + 1).ToString, UnnamedParams(param)))
            Next

            'Restaurar plantillas internas, comentarios y enlaces en parametros, luego agregarlas a lista de parametros
            For Each tup As Tuple(Of String, String) In NamedParams
                Dim ParamName As String = tup.Item1
                Dim ParamValue As String = tup.Item2

                For reptempindex As Integer = 0 To ReplacedTemplates.Count - 1
                    Dim tempreplace As String = ColoredText("PERIODIBOT:TEMPLATEREPLACE::::" & reptempindex.ToString, 1)
                    ParamName = ParamName.Replace(tempreplace, ReplacedTemplates(reptempindex))
                    ParamValue = ParamValue.Replace(tempreplace, ReplacedTemplates(reptempindex))
                Next

                For RepLinkIndex As Integer = 0 To ReplacedLinks.Count - 1
                    Dim LinkReplace As String = ColoredText("PERIODIBOT:LINKREPLACE::::" & RepLinkIndex.ToString, 1)
                    ParamName = ParamName.Replace(LinkReplace, ReplacedLinks(RepLinkIndex))
                    ParamValue = ParamValue.Replace(LinkReplace, ReplacedLinks(RepLinkIndex))
                Next


                For RepCommentsIndex As Integer = 0 To ReplacedComments.Count - 1
                    Dim CommentsReplace As String = ColoredText("PERIODIBOT:COMMENTSREPLACE::::" & RepCommentsIndex.ToString, 1)
                    ParamName = ParamName.Replace(CommentsReplace, ReplacedComments(RepCommentsIndex))
                    ParamValue = ParamValue.Replace(CommentsReplace, ReplacedComments(RepCommentsIndex))
                Next

                TotalParams.Add(New Tuple(Of String, String)(ParamName, ParamValue))

            Next
            'Agregar parametros locales a parametros de clase
            _parameters.AddRange(TotalParams)
        End Sub

        ''' <summary>
        ''' Retorna una lista de plantillas si se le entrega como parámetro un array de tipo string con texto en formato válido de plantilla.
        ''' Si uno de los items del array no tiene formato válido, entregará una plantilla vacia en su lugar ("{{}}").
        ''' </summary>
        ''' <param name="templatearray"></param>
        ''' <returns></returns>
        Public Shared Function GetTemplates(ByVal templatearray As List(Of String)) As List(Of Template)
            If templatearray Is Nothing Then
                Return New List(Of Template)
            End If
            Dim TemplateList As New List(Of Template)
            For Each t As String In templatearray
                TemplateList.Add(New Template(t, False))
            Next
            Return TemplateList
        End Function

        ''' <summary>
        ''' Retorna todas las plantillas que encuentre en una pagina, de no haber entregará una lista vacia.
        ''' </summary>
        ''' <param name="WikiPage"></param>
        ''' <returns></returns>
        Public Shared Function GetTemplates(ByVal wikiPage As Page) As List(Of Template)
            If wikiPage Is Nothing Then
                Return New List(Of Template)
            End If
            Dim TemplateList As New List(Of Template)
            Dim temps As List(Of String) = GetTemplateTextArray(wikiPage.Content)

            For Each t As String In temps
                TemplateList.Add(New Template(t, False))
            Next
            Return TemplateList
        End Function

        ''' <summary>
        ''' Retorna todas las plantillas que encuentre en un texto, de no haber entregará una lista vacia.
        ''' </summary>
        ''' <param name="text">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function GetTemplates(ByVal text As String) As List(Of Template)
            If String.IsNullOrWhiteSpace(text) Then
                Return New List(Of Template)
            End If
            Dim TemplateList As New List(Of Template)
            Dim temps As List(Of String) = Template.GetTemplateTextArray(text)

            For Each t As String In temps
                Dim rectemplate As Template = New Template(t, False)
                If Not (rectemplate.Text Is Nothing AndAlso String.IsNullOrWhiteSpace(rectemplate.Text)) Then
                    TemplateList.Add(New Template(t, False))
                End If
            Next
            Return TemplateList
        End Function

        ''' <summary>
        ''' Test. Inestable, no usar.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function GetTemplates_TEST(ByVal text As String) As List(Of Template)
            Dim te As New Template
            Return te.GetTemplatesTest(text).ToList
        End Function

        ''' <summary>
        ''' Entrega los nombres (normalizados) de las plantillas presentes en el texto entregado
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function GetTemplatesNamesInText(ByVal text As String) As String()
            Dim temps As List(Of String) = GetTemplateTextArray(text)
            Dim templateNames As New HashSet(Of String)
            For Each t As String In temps
                Dim nameMatch As Match = Regex.Match(t, "^{{((plantilla:|template:)*([a-zA-Z\u00C0-\u017F\s]+?))(\||}})", RegexOptions.IgnoreCase)
                Dim name As String = If(nameMatch.Success, nameMatch.Groups(1).Value.Trim, String.Empty)
                name = UppercaseFirstCharacter(name)
                templateNames.Add(name)
            Next
            Return templateNames.ToArray()
        End Function

        ''' <summary>
        ''' Entrega los nombres (normalizados) de las plantillas presentes en el texto entregado con la posibilidad de ignorar el espacio de nombres.
        ''' </summary>
        ''' <param name="text">Texto a analizar.</param>
        ''' <param name="ignoreNamespaces">Ignorar espacio de nombres y entregar solo los nombres.</param>
        ''' <returns></returns>
        Public Shared Function GetTemplatesNamesInText(ByVal text As String, ByVal ignoreNamespaces As Boolean) As String()
            If ignoreNamespaces = False Then
                Return GetTemplatesNamesInText(text)
            End If
            Dim templateNamesInText As String() = GetTemplatesNamesInText(text)
            Dim normalizedTemplateNames As New HashSet(Of String)
            For Each name As String In templateNamesInText
                Dim normalizedName As String = Regex.Replace(name, ".+:", String.Empty).Trim()
                normalizedName = UppercaseFirstCharacter(normalizedName)
                normalizedTemplateNames.Add(normalizedName)
            Next
            Return normalizedTemplateNames.ToArray()
        End Function

        ''' <summary>
        ''' Entrega los nombres (normalizados) de las plantillas presentes en el texto entregado con la posibilidad de ignorar un espacio de nombres específico.
        ''' </summary>
        ''' <param name="text">Texto a analizar.</param>
        ''' <param name="ignoreNamespace">Espacio de nombres a ignorar.</param>
        ''' <returns></returns>
        Public Shared Function GetTemplatesNamesInText(ByVal text As String, ByVal ignoreNamespace As String) As String()
            If String.IsNullOrWhiteSpace(ignoreNamespace) Then
                Return GetTemplatesNamesInText(text)
            End If
            Dim templateNamesInText As String() = GetTemplatesNamesInText(text)
            Dim normalizedTemplateNames As New HashSet(Of String)
            For Each name As String In templateNamesInText
                Dim normalizedName As String = Regex.Replace(name, Regex.Escape(ignoreNamespace) & " *:", String.Empty).Trim()
                normalizedName = UppercaseFirstCharacter(normalizedName)
                normalizedTemplateNames.Add(normalizedName)
            Next
            Return normalizedTemplateNames.ToArray()
        End Function

        ''' <summary>
        ''' Comprueba si en el texto entregado se encuentra una plantilla con el nombre indicado.
        ''' </summary>
        ''' <param name="text">Texto a analizar.</param>
        ''' <param name="templateName">Nombre de la plantilla, con primer carácter en mayúsculas.</param>
        ''' <returns></returns>
        Public Shared Function IsTemplatePresentInText(ByVal text As String, ByVal templateName As String) As Boolean

            Dim a As String() = GetTemplatesNamesInText(text)

            Return a.Contains(templateName.Trim())
        End Function


        ''' <summary>
        ''' Comprueba si en el texto entregado se encuentra una plantilla con el nombre indicado ignorando los espacios de nombre.
        ''' </summary>
        ''' <param name="text">Texto a analizar.</param>
        ''' <param name="templateName">Nombre de la plantilla, con primer carácter en mayúsculas.</param>
        ''' <returns></returns>
        Public Shared Function IsTemplatePresentInText(ByVal text As String, ByVal templateName As String, ignoreNamespaces As Boolean) As Boolean
            If ignoreNamespaces = False Then
                Return IsTemplatePresentInText(text, templateName)
            End If
            Return GetTemplatesNamesInText(text, ignoreNamespaces).Contains(templateName.Trim())
        End Function

        ''' <summary>
        ''' Comprueba si en el texto entregado se encuentra una plantilla con el nombre indicado ignorando un espacio de nombres específico.
        ''' </summary>
        ''' <param name="text">Texto a analizar.</param>
        ''' <param name="templateName">Nombre de la plantilla, con primer carácter en mayúsculas.</param>
        ''' <param name="ignoreNamespace">Espacio de nombres a ignorar.</param>
        ''' <returns></returns>
        Public Shared Function IsTemplatePresentInText(ByVal text As String, ByVal templateName As String, ignoreNamespace As String) As Boolean
            If String.IsNullOrWhiteSpace(ignoreNamespace) Then
                Return IsTemplatePresentInText(text, templateName)
            End If
            Return GetTemplatesNamesInText(text, ignoreNamespace).Contains(templateName.Trim())
        End Function

        Sub New(name As String, parameters As List(Of Tuple(Of String, String)), text As String)
            _name = name
            _parameters = parameters
            _text = text
            Newtemplate = False
            _Valid = Not String.IsNullOrWhiteSpace(_name)
        End Sub

        ''' <summary>
        ''' Test. Inestable, no usar.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Function GetTemplatesTest(ByVal text As String) As Template()
            Dim templatelist As New List(Of Template)
            Dim tokens As String() = {"{{", "}}", "[", "]", "{{{", "}}}", Environment.NewLine & "{|", Environment.NewLine & "|}"}
            Dim tokensList As List(Of Tuple(Of String, Integer())) = GetTokensIndexes(text, tokens)
            Return GetTemplatesTest(text, tokensList)
        End Function

        ''' <summary>
        ''' Test. Inestable, no usar.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="tokensList"></param>
        ''' <returns></returns>
        Function GetTemplatesTest(ByVal text As String, ByVal tokensList As List(Of Tuple(Of String, Integer()))) As Template()
            Dim templatelist As New List(Of Template)

            If Not tokensList(0).Item2.Length > 1 Then Return Nothing

            Dim currentCharacter As Integer = tokensList(0).Item2.First
            Dim analyzedText As String = String.Empty
            For currentCharacter = currentCharacter To text.Length - 1

                analyzedText &= text(currentCharacter)

                If CountString(analyzedText, "{{") = CountString(analyzedText, "}}") Then
                    If CountString(analyzedText, "{{") = 0 Then Continue For
                    templatelist.Add(AnalyzeTemplateParams(analyzedText.Substring(analyzedText.IndexOf("{{"))))
                    analyzedText = ReplaceFirst(analyzedText, analyzedText, Space(analyzedText.Length))

                End If

            Next
            Return templatelist.ToArray
        End Function

        ''' <summary>
        ''' Test. Inestable, no usar.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Private Function AnalyzeTemplateParams(ByVal text As String) As Template
            Dim settingName As Boolean = True
            Dim settingParameter As Boolean = False
            Dim paramlist As New List(Of Tuple(Of String, String))
            Dim templateName As String = String.Empty
            Dim Depth As Integer = 0
            Dim lDepth As Integer = 0
            Dim templateText As String = "{{"
            Dim currentParam As String = String.Empty
            Dim currentParamName As String = String.Empty

            For currentCharacter As Integer = 2 To text.Length - 1
                templateText &= text(currentCharacter)
                Select Case text(currentCharacter)
                    Case "|"c
                        If Not Depth > 0 AndAlso Not lDepth > 0 Then
                            settingName = False
                            If settingParameter Then
                                paramlist.Add(New Tuple(Of String, String)(currentParamName, currentParam))
                                currentParam = String.Empty
                                currentParamName = String.Empty
                            Else
                                settingParameter = True
                            End If
                        Else
                            If settingParameter Then
                                currentParam &= text(currentCharacter)
                            End If
                        End If
                    Case "="c
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If

                        If settingParameter Then
                            If Depth = 0 AndAlso lDepth = 0 Then
                                currentParamName = currentParam
                                currentParam = String.Empty
                            Else
                                currentParam &= text(currentCharacter)
                            End If
                        End If

                    Case "["c
                        lDepth += 1
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If
                        If settingParameter Then
                            currentParam &= text(currentCharacter)
                        End If
                    Case "]"c
                        lDepth -= 1
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If
                        If settingParameter Then
                            currentParam &= text(currentCharacter)
                        End If

                    Case "["c
                        lDepth += 1
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If
                        If settingParameter Then
                            currentParam &= text(currentCharacter)
                        End If
                    Case "]"c
                        lDepth -= 1
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If
                        If settingParameter Then
                            currentParam &= text(currentCharacter)
                        End If
                    Case "{"c
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If
                        If settingParameter Then
                            currentParam += text(currentCharacter)
                        End If
                        If text.Length > currentCharacter + 2 Then
                            currentCharacter += 1
                            templateText &= text(currentCharacter)
                            If text(currentCharacter) = "{"c Then
                                Depth += 1
                            End If
                            If settingName Then
                                templateName &= text(currentCharacter)
                            End If
                            If settingParameter Then
                                currentParam += text(currentCharacter)
                            End If
                        End If
                    Case "}"c
                        If Depth = 0 Then
                            If text.Length >= currentCharacter + 1 Then
                                currentCharacter += 1
                                If text(currentCharacter) = "}"c Then
                                    templateText &= text(currentCharacter)
                                    If settingParameter Then
                                        paramlist.Add(New Tuple(Of String, String)(currentParamName, currentParam))
                                    End If
                                    Exit For
                                Else
                                    If settingName Then
                                        templateName &= text(currentCharacter)
                                    End If
                                    currentCharacter -= 1
                                End If
                                templateText &= text(currentCharacter)
                                If settingName Then
                                    templateName &= text(currentCharacter)
                                End If
                                If settingParameter Then
                                    currentParam += text(currentCharacter)
                                End If
                            End If
                        End If
                        If Depth > 0 Then
                            If settingName Then
                                templateName &= text(currentCharacter)
                            End If
                            If settingParameter Then
                                currentParam += text(currentCharacter)
                            End If
                            If text.Length >= currentCharacter + 1 Then
                                currentCharacter += 1
                                templateText &= text(currentCharacter)
                                If settingName Then
                                    templateName &= text(currentCharacter)
                                End If
                                If settingParameter Then
                                    currentParam += text(currentCharacter)
                                End If
                                If text(currentCharacter) = "}"c Then
                                    Depth -= 1
                                End If

                            End If
                        End If
                    Case Else
                        If text(currentCharacter) = Environment.NewLine Then
                            lDepth = 0
                        End If
                        If settingParameter Then
                            currentParam += text(currentCharacter)
                        End If
                        If settingName Then
                            templateName &= text(currentCharacter)
                        End If
                End Select
            Next
            Dim NumeredParams As New List(Of Tuple(Of String, String, String))
            For i As Integer = 0 To paramlist.Count - 1
                NumeredParams.Add(New Tuple(Of String, String, String)(If(String.IsNullOrWhiteSpace(paramlist(i).Item1), i.ToString, paramlist(i).Item1.Trim()), paramlist(i).Item1, paramlist(i).Item2))
            Next
            Dim temp As New Template(templateName, paramlist, templateText)

            Return temp


        End Function

        Private Function CountString(ByVal text As String, stringToCount As String) As Integer
            Dim newtext As String = text.Replace(stringToCount, String.Empty)
            Dim diff As Integer = text.Length - newtext.Length
            Dim Count As Integer = diff \ stringToCount.Length
            Return Count
        End Function

        Private Function GetTokensIndexes(ByVal text As String, tokens As String()) As List(Of Tuple(Of String, Integer()))
            Dim TokensList As New List(Of Tuple(Of String, Integer()))
            For Each token As String In tokens
                Dim indexList As New List(Of Integer)
                Dim tindex As Integer = 0
                Do Until tindex = -1
                    tindex = text.IndexOf(token)
                    If Not tindex = -1 Then
                        indexList.Add(tindex)
                        text = text.Substring(0, tindex) & Space(token.Length) + text.Substring(tindex + token.Length)
                    End If
                Loop
                TokensList.Add(New Tuple(Of String, Integer())(token, indexList.ToArray))
            Next
            Return TokensList
        End Function

        ''' <summary>
        ''' Retorna un array de string con todas las plantillas contenidas en un texto.
        ''' Pueden repetirse si hay plantillas que contienen otras en su interior.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function GetTemplateTextArray(ByVal text As String) As List(Of String)
            Dim temptext As String
            Dim templist As New List(Of String)
            If String.IsNullOrWhiteSpace(text) Then Return templist
            Dim CharArr As Char() = text.ToArray

            Dim OpenTemplateCount2 As Integer = 0
            Dim CloseTemplateCount2 As Integer = 0

            Dim Flag1 As Boolean = False
            Dim Flag2 As Boolean = False

            Dim beginindex As Integer = 0

            For i As Integer = 0 To CharArr.Length - 1

                If CharArr(i) = "{" Then
                    If Flag1 Then
                        Flag1 = False
                        OpenTemplateCount2 += 1
                    Else
                        Flag1 = True
                    End If
                Else
                    Flag1 = False
                End If

                If CharArr(i) = "}" Then
                    If Flag2 Then
                        Flag2 = False
                        CloseTemplateCount2 += 1
                    Else
                        Flag2 = True
                    End If
                Else
                    Flag2 = False
                End If

                If OpenTemplateCount2 > 0 Then
                    If OpenTemplateCount2 = CloseTemplateCount2 Then
                        temptext = text.Substring(beginindex, (i - beginindex) + 1)
                        If Not temptext.Length = 0 Then
                            Dim BeginPos As Integer = temptext.IndexOf("{{")
                            If Not BeginPos = -1 Then
                                Dim Textbefore As String = temptext.Substring(0, BeginPos)
                                Dim Lenght As Integer = temptext.Length - (Textbefore.Length)
                                Dim TemplateText As String = temptext.Substring(BeginPos, Lenght)
                                If Not TemplateText.Length <= 4 Then
                                    templist.Add(TemplateText)
                                End If
                            End If
                        End If
                        temptext = String.Empty
                        beginindex = i + 1
                        OpenTemplateCount2 = 0
                        CloseTemplateCount2 = 0
                    End If
                End If
            Next
            Dim innertemplates As New List(Of String)
            For Each t As String In templist
                If t.Length >= 4 Then
                    Dim innertext As String = t.Substring(2, t.Length - 4)
                    innertemplates.AddRange(GetTemplateTextArray(innertext))
                End If
            Next
            templist.AddRange(innertemplates)
            Return templist
        End Function

        ''' <summary>
        ''' Elimina los parámetros en blanco de la plantilla.
        ''' </summary>
        ''' <returns></returns>
        Function OptimizeTemplate() As Boolean
            Dim newParameters As New List(Of Tuple(Of String, String))
            For Each parameter As Tuple(Of String, String) In _parameters
                If Not String.IsNullOrWhiteSpace(parameter.Item2.ToString) Then
                    newParameters.Add(parameter)
                End If
            Next
            _parameters = newParameters
            _text = MakeTemplateText(_name, newParameters)
            Return True
        End Function

        ''' <summary>
        ''' Añade un parámetro sin valor al final de la plantilla, no comprueba si el parámetro ya estaba presente en la plantilla.
        ''' </summary>
        ''' <param name="parameterName">Nombre del parámetro.</param>
        ''' <returns></returns>
        Function AppendParameter(ByVal parameterName As String) As Boolean
            If Regex.Match(parameterName, "[=|{}:]").Success Then Throw New ArgumentException(My.Resources.Messages.InvalidParameterNameOnTemplateError)
            _text = Regex.Replace(_text, "\}\}$", " |" & parameterName.Trim() & " }}")
            _parameters.Add(New Tuple(Of String, String)(parameterName, String.Empty))
            Return True
        End Function

        ''' <summary>
        ''' Añade un parámetro y su valor al final de la plantilla, no comprueba si el parámetro ya estaba presente en la plantilla.
        ''' </summary>
        ''' <param name="parameterName">Nombre del parámetro.</param>
        ''' <param name="parameterValue">Valor del parámetro.</param>
        ''' <returns></returns>
        Function AppendParameter(ByVal parameterName As String, ByVal parameterValue As String) As Boolean
            If Regex.Match(parameterName, "[=|{}:]").Success Then Throw New ArgumentException(My.Resources.Messages.InvalidParameterNameOnTemplateError)
            _text = Regex.Replace(_text, "\}\}$", " |" & parameterName.Trim() & " =" & parameterValue & " }}")
            _parameters.Add(New Tuple(Of String, String)(parameterName, parameterValue))
            Return True
        End Function

        ''' <summary>
        ''' Añade un parámetro de la plantilla, si ya existe lo reemplaza. Regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="parameterContent"></param>
        ''' <returns></returns>
        Function AddParameter(ByVal parameterName As String, ByVal parameterContent As String) As Boolean
            If Regex.Match(parameterName, "[=|{}:]").Success Then Throw New ArgumentException(My.Resources.Messages.InvalidParameterNameOnTemplateError)
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    _parameters.Remove(p)
                End If
            Next
            _parameters.Add(New Tuple(Of String, String)(parameterName, parameterContent))
            Return True
            _text = MakeTemplateText(_name, _parameters)
        End Function

        ''' <summary>
        ''' Reemplaza el valor de un parámetro de la plantilla. Puede regenerar el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterContent"></param>
        ''' <param name="regenerateText"></param>
        ''' <returns></returns>
        Function ReplaceParameterContent(ByVal parameterName As String, ByVal newParameterContent As String, ByVal regenerateText As Boolean) As Boolean
            If regenerateText Then
                Return ReplaceParameterContentRegen(parameterName, newParameterContent)
            Else
                Return ReplaceParameterContentNoRegen(parameterName, newParameterContent)
            End If
        End Function

        ''' <summary>
        ''' Reemplaza el contenido de un parámetro de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterContent"></param>
        ''' <returns></returns>
        Function ReplaceParameterContent(ByVal parameterName As String, ByVal newParameterContent As String) As Boolean
            Return ReplaceParameterContentNoRegen(parameterName, newParameterContent)
        End Function

        ''' <summary>
        ''' Reemplaza el contenido de un parámetro de la plantilla. Regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterContent"></param>
        ''' <returns></returns>
        Private Function ReplaceParameterContentRegen(ByVal parameterName As String, ByVal newParameterContent As String) As Boolean
            If Regex.Match(parameterName, "[=|{}:]").Success Then Throw New ArgumentException(My.Resources.Messages.InvalidParameterNameOnTemplateError)
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    If _parameters.Contains(p) Then
                        _parameters.Remove(p)
                        _parameters.Add(New Tuple(Of String, String)(parameterName, newParameterContent))
                    End If
                End If
            Next
            Return True
            _text = MakeTemplateText(_name, _parameters)
        End Function

        ''' <summary>
        ''' Cambia el contenido de un parámetro de la plantilla. Si el nombre del parámetro nuevo ya existía, lo quita. No regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Private Function ReplaceParameterContentNoRegen(ByVal parameterName As String, ByVal newParameterContent As String) As Boolean
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            Dim parameterNameEscaped As String = Regex.Escape(parameterName.Trim())
            Dim parameterString As New List(Of Tuple(Of String, String))
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    _parameters.Remove(p)
                    Dim t As New Tuple(Of String, String)(p.Item1, p.Item2.Replace(p.Item2, newParameterContent))
                    _parameters.Add(t)
                    Dim oldString As String = "|" & p.Item1 & "=" & p.Item2
                    Dim newString As String = "|" & t.Item1 & "=" & t.Item2
                    Dim ps As New Tuple(Of String, String)(oldString, newString)
                    parameterString.Add(ps)
                End If
            Next
            For Each p As Tuple(Of String, String) In parameterString
                _text = _text.Replace(p.Item1, p.Item2)
            Next
            Return True
        End Function

        ''' <summary>
        ''' Añade al contenido de un parámetro nuevo texto. Puede regenerar el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterContent"></param>
        ''' <param name="regenerateText"></param>
        ''' <returns></returns>
        Function AppendParameterContent(ByVal parameterName As String, ByVal newParameterContent As String, ByVal regenerateText As Boolean) As Boolean
            If regenerateText Then
                Return AppendParameterContentRegen(parameterName, newParameterContent)
            Else
                Return AppendParameterContentNoRegen(parameterName, newParameterContent)
            End If
        End Function

        ''' <summary>
        ''' Añade al contenido de un parámetro nuevo texto.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterContent"></param>
        ''' <returns></returns>
        Function AppendParameterContent(ByVal parameterName As String, ByVal newParameterContent As String) As Boolean
            Return AppendParameterContentNoRegen(parameterName, newParameterContent)
        End Function

        ''' <summary>
        ''' Añade al contenido de un parámetro nuevo texto. Regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterContent"></param>
        ''' <returns></returns>
        Private Function AppendParameterContentRegen(ByVal parameterName As String, ByVal newParameterContent As String) As Boolean
            If Regex.Match(parameterName, "[=|{}:]").Success Then Throw New ArgumentException(My.Resources.Messages.InvalidParameterNameOnTemplateError)
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    If _parameters.Contains(p) Then
                        _parameters.Remove(p)
                        _parameters.Add(New Tuple(Of String, String)(parameterName, p.Item2 & newParameterContent))
                    End If
                End If
            Next
            Return True
            _text = MakeTemplateText(_name, _parameters)
        End Function

        ''' <summary>
        ''' Añade al contenido de un parámetro nuevo texto. No regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Private Function AppendParameterContentNoRegen(ByVal parameterName As String, ByVal newParameterContent As String) As Boolean
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            Dim parameterNameEscaped As String = Regex.Escape(parameterName.Trim())
            Dim parameterString As New List(Of Tuple(Of String, String))
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    _parameters.Remove(p)
                    Dim t As New Tuple(Of String, String)(p.Item1, p.Item2 & newParameterContent)
                    _parameters.Add(t)
                    Dim oldString As String = "|" & p.Item1 & "=" & p.Item2
                    Dim newString As String = "|" & t.Item1 & "=" & t.Item2
                    Dim ps As New Tuple(Of String, String)(oldString, newString)
                    parameterString.Add(ps)
                End If
            Next
            For Each p As Tuple(Of String, String) In parameterString
                _text = _text.Replace(p.Item1, p.Item2)
            Next
            Return True
        End Function

        ''' <summary>
        ''' Cambia el nombre de un parámetro de la plantilla manteniendo el contenido. Si el nombre del parámetro nuevo ya existía, lo quita.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterName"></param>
        ''' <returns></returns>
        Function ChangeNameOfParameter(ByVal parameterName As String, ByVal newParameterName As String) As Boolean
            Return ChangeNameOfParameterNoRegen(parameterName, newParameterName)
        End Function

        ''' <summary>
        ''' Cambia el nombre de un parámetro de la plantilla manteniendo el contenido. Si el nombre del parámetro nuevo ya existía, lo quita. Puede regenerar el texto completo de la plantilla
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterName"></param>
        ''' <param name="regenerateText"></param>
        ''' <returns></returns>
        Function ChangeNameOfParameter(ByVal parameterName As String, ByVal newParameterName As String, ByVal regenerateText As Boolean) As Boolean
            If regenerateText Then
                Return ChangeNameOfParameterRegen(parameterName, newParameterName)
            Else
                Return ChangeNameOfParameterNoRegen(parameterName, newParameterName)
            End If
        End Function

        ''' <summary>
        ''' Cambia el nombre de un parámetro de la plantilla manteniendo el contenido. Si el nombre del parámetro nuevo ya existía, lo quita. Regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="newParameterName"></param>
        ''' <returns></returns>
        Private Function ChangeNameOfParameterRegen(ByVal parameterName As String, ByVal newParameterName As String) As Boolean
            If ContainsParameter(parameterName) Then
                If ContainsParameter(newParameterName) Then
                    RemoveParameterNoRegen(newParameterName)
                End If
            End If
            If Regex.Match(parameterName, "[=|{}:]").Success Then Throw New ArgumentException(My.Resources.Messages.InvalidParameterNameOnTemplateError)
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    If _parameters.Contains(p) Then
                        _parameters.Remove(p)
                        _parameters.Add(New Tuple(Of String, String)(newParameterName, p.Item2))
                    End If
                End If
            Next
            Return True
            _text = MakeTemplateText(_name, _parameters)
        End Function

        ''' <summary>
        ''' Cambia el nombre de un parámetro de la plantilla manteniendo el contenido. Si el nombre del parámetro nuevo ya existía, lo quita. No regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Private Function ChangeNameOfParameterNoRegen(ByVal parameterName As String, ByVal newParameterName As String) As Boolean
            If ContainsParameter(parameterName) Then
                If ContainsParameter(newParameterName) Then
                    RemoveParameterNoRegen(newParameterName)
                End If
            End If
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            Dim parameterNameEscaped As String = Regex.Escape(parameterName.Trim())
            Dim parameterString As New List(Of Tuple(Of String, String))
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    _parameters.Remove(p)
                    Dim t As New Tuple(Of String, String)(p.Item1.Replace(parameterName, newParameterName), p.Item2)
                    _parameters.Add(t)
                    Dim oldString As String = "|" & p.Item1 & "=" & p.Item2
                    Dim newString As String = "|" & t.Item1 & "=" & t.Item2
                    Dim ps As New Tuple(Of String, String)(oldString, newString)
                    parameterString.Add(ps)
                End If
            Next
            For Each p As Tuple(Of String, String) In parameterString
                _text = _text.Replace(p.Item1, p.Item2)
            Next
            Return True
        End Function

        ''' <summary>
        ''' Quita un parámetro y su contenido de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Function RemoveParameter(ByVal parameterName As String) As Boolean
            Return RemoveParameterNoRegen(parameterName)
        End Function

        ''' <summary>
        ''' Quita un parámetro y su contenido de la plantilla. Opcionalmente puede regenerar el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <param name="regenText"></param>
        ''' <returns></returns>
        Function RemoveParameter(ByVal parameterName As String, ByVal regenText As Boolean) As Boolean
            If regenText Then
                Return RemoveParameterRegen(parameterName)
            Else
                Return RemoveParameterNoRegen(parameterName)
            End If
        End Function

        ''' <summary>
        ''' Quita un parámetro y su contenido de la plantilla. Regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Private Function RemoveParameterRegen(ByVal parameterName As String) As Boolean
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    _parameters.Remove(p)
                End If
            Next
            Return True
            _text = MakeTemplateText(_name, _parameters)
        End Function

        ''' <summary>
        ''' Quita un parámetro y su contenido de la plantilla. No regenera el texto completo de la plantilla.
        ''' </summary>
        ''' <param name="parameterName"></param>
        ''' <returns></returns>
        Private Function RemoveParameterNoRegen(ByVal parameterName As String) As Boolean
            Dim oldParameters As List(Of Tuple(Of String, String)) = New List(Of Tuple(Of String, String))(_parameters)
            Dim parameterNameEscaped As String = Regex.Escape(parameterName.Trim())
            Dim parameterString As New List(Of String)
            For Each p As Tuple(Of String, String) In oldParameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    _parameters.Remove(p)
                    parameterString.Add("|" & p.Item1 & "=" & p.Item2)
                End If
            Next
            For Each p As String In parameterString
                _text = _text.Replace(p, String.Empty)
            Next
            Return True
        End Function

        ''' <summary>
        ''' Indica si la mayoría de parámetros comienzan con un espacio que los separa del caráter 'pipe' (|).
        ''' </summary>
        ''' <returns></returns>
        Function MostParametersAreSeparatedFromPipes() As Boolean
            Dim separated As Integer = 0
            Dim notSeparated As Integer = 0
            For Each p As Tuple(Of String, String) In _parameters
                If p.Item1.StartsWith(" ") Then
                    separated += 1
                Else
                    notSeparated += 1
                End If
            Next
            Return separated > notSeparated
        End Function

        Function GetParameterContent(ByVal parameterName As String) As String
            Dim content As String = String.Empty
            For Each p As Tuple(Of String, String) In _parameters
                If p.Item1.Trim().Equals(parameterName.Trim()) Then
                    content = p.Item2
                End If
            Next
            Return content
        End Function


    End Class



    Public Class WikiLink
        Property Text As String
        Property Type As WikiLinkType
        Property Name As String
        Property Content As String
    End Class

    Public Enum WikiLinkType
        Internal
        External
    End Enum

End Namespace