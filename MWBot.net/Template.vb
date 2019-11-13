Option Strict On
Option Explicit On
Imports System.Text.RegularExpressions
Imports Utils.Utils

Namespace WikiBot

    Public Class Template
        Private _name As String
        Private _parameters As List(Of Tuple(Of String, String))
        Private _text As String
        Private _newtemplate As Boolean = True
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
                If _newtemplate Then
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
            _newtemplate = newTemplate
            If newTemplate Then
                _name = text
                _text = MakeSimpleTemplateText(text)
                _parameters = New List(Of Tuple(Of String, String))
            Else
                GetTemplateOfText(text)
            End If
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
        End Sub

        ''' <summary>
        ''' Crea una nueva plantilla vacía ("{{}}")
        ''' </summary>
        Sub New()
            _name = String.Empty
            _text = String.Empty
            _parameters = New List(Of Tuple(Of String, String))
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
            Dim linechar As String = ""
            If newlines Then linechar = Environment.NewLine
            Dim text As String = "{{" & templatename & linechar
            For Each t As Tuple(Of String, String) In templateparams
                text = text & "|" & t.Item1 & "=" & t.Item2 & linechar
            Next
            text = text & "}}"
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
            templatetext = templatetext & closing
            Return templatetext

        End Function

        ''' <summary>
        ''' Inicializa la plantilla extrayendo los datos de un texto que debería ser en formato de plantilla.
        ''' </summary>
        ''' <param name="templatetext"></param>
        Sub GetTemplateOfText(ByVal templatetext As String)
            If String.IsNullOrWhiteSpace(templatetext) Then
                Throw New ArgumentException("Empty parameter", "templatetext")
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
                    tempname = tempname & innertext(cha)
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
                    Dim Paramvalue As String = m.Value.Replace(ParamNamematch.Value, "")
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
            Dim temps As List(Of String) = Template.GetTemplateTextArray(wikiPage.Content)

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


        Sub New(name As String, parameters As List(Of Tuple(Of String, String)), text As String)
            _name = name
            _parameters = parameters
            _text = text
            _newtemplate = False
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
        Function AnalyzeTemplateParams(ByVal text As String) As Template
            Dim settingName As Boolean = True
            Dim settingParameter As Boolean = False
            Dim paramlist As New List(Of Tuple(Of String, String))
            Dim templateName As String = String.Empty
            Dim Depth As Integer = 0
            Dim lDepth As Integer = 0
            Dim htmDepth As Integer = 0
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
                                currentParam = ""
                                currentParamName = ""
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
                                currentParam = ""
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

        Function CountString(ByVal text As String, stringToCount As String) As Integer
            Dim newtext As String = text.Replace(stringToCount, "")
            Dim diff As Integer = text.Length - newtext.Length
            Dim Count As Integer = diff \ stringToCount.Length
            Return Count
        End Function

        Function GetTokensIndexes(ByVal text As String, tokens As String()) As List(Of Tuple(Of String, Integer()))
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
            Dim temptext As String = String.Empty
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