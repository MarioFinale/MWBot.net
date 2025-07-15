Option Strict On
Option Explicit On
Imports System.Net
Imports System.Runtime.InteropServices
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports MWBot.net.My.Resources
Imports MWBot.net.WikiBot

Namespace Utility
    Public NotInheritable Class Utils

#Region "Properties"
        Public Shared Codename As String = "Utils"
        Public Shared DirSeparator As String = IO.Path.DirectorySeparatorChar
        ''' <summary>
        ''' El separador de decimales varia segun SO y configuracion regional, eso puede afectar los calculos.
        ''' </summary>
        Public Shared DecimalSeparator As String = String.Format(CType(1.1, String)).Substring(1, 1)
        Public Shared OS As String = GetOsString()
        Public Shared Exepath As String = ReplaceLast(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, Process.GetCurrentProcess().MainModule.ModuleName, "")

#End Region

#Region "Prevent init"
        Private Sub New()
        End Sub
#End Region

#Region "Text Functions"
        Public Shared signpattern As String = "([0-9]{2}):([0-9]{2}) ([0-9]{2}|[0-9]) ([A-z]{3})([\.,])* [0-9]{4}( \([A-z]{3,4}\))*"

        Public Shared Function GetJsonDocument(ByVal jsonString As String) As JsonDocument
            Return JsonDocument.Parse(jsonString)
        End Function

        Public Shared Function GetJsonElement(ByVal jsonDocument As JsonDocument, ByVal propertyName As String) As JsonElement
            Dim element As JsonElement
            Dim text As String = jsonDocument.RootElement.ToString
            element = jsonDocument.RootElement.GetProperty(propertyName)
            Return element
        End Function


        Public Shared Function GetJsonElement(ByVal jsonElement As JsonElement, ByVal propertyName As String) As JsonElement
            Dim text As String = jsonElement.ToString
            jsonElement = jsonElement.GetProperty(propertyName)
            Return jsonElement
        End Function

        Public Shared Function IsJsonPropertyPresent(ByVal jsonElement As JsonElement, ByVal properyName As String) As Boolean
            For Each elementProperty As JsonProperty In jsonElement.EnumerateObject
                If elementProperty.Name = properyName Then Return True
            Next
            Return False
        End Function

        ''' <summary>
        ''' Crea una cadena de texto concatenado según los parámetros entregados
        ''' </summary>
        ''' <param name="array">Array de origen</param>
        ''' <param name="separator">Separador</param>
        ''' <returns></returns>
        Public Shared Function ConcatenateTextArrayWithChar(ByVal array As String(), ByVal separator As Char) As String
            Dim result As String = ""
            For Each s As String In array
                result &= s & separator
            Next
            Return result.Trim(separator)
        End Function

        ''' <summary>
        ''' Crea una cadena de texto concatenado según los parámetros entregados y la codifica según urlencode
        ''' </summary>
        ''' <param name="array">Array de origen</param>
        ''' <param name="separator">Separador</param>
        ''' <param name="urlEncode">Codificar el texto en urlencode, se codificará aunque se establesca en 'false'</param>
        ''' <returns></returns>
        <CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="Overload")>
        Public Shared Function ConcatenateTextArrayWithChar(ByVal array As String(), ByVal separator As Char, ByVal urlEncode As Boolean) As String
            Dim result As String = ""
            For Each s As String In array
                result &= s & separator
            Next
            Return UrlWebEncode(ReplaceLast(result, separator, ""))
        End Function

        ''' <summary>
        ''' Elimina los tags html de una cadena de texto dada.
        ''' </summary>
        ''' <param name="html">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function StripTags(ByVal html As String) As String
            Return Regex.Replace(html, "<.*?>", String.Empty)
        End Function
        ''' <summary>
        ''' Elimina las líneas en blanco.
        ''' </summary>
        ''' <param name="Str">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function Removewhitelines(ByVal Str As String) As String
            Return Regex.Replace(Str, "^\s+$[\r\n]*", "", RegexOptions.Multiline)
        End Function
        ''' <summary>
        ''' Elimina los excesos de espacios (consecutivos) en una cadena de texto.
        ''' </summary>
        ''' <param name="text">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function RemoveExcessOfSpaces(ByVal text As String) As String
            Return Regex.Replace(text, "\s{2,}", " ")
        End Function

        ''' <summary>
        ''' Cuenta las veces que un carácter aparece en una cadena de texto dada.
        ''' </summary>
        ''' <param name="value">Texto a evaluar.</param>
        ''' <param name="ch">Carácter a buscar.</param>
        ''' <returns></returns>
        Public Shared Function CountCharacter(ByVal value As String, ByVal ch As Char) As Integer
            Return value.Count(Function(c As Char) c = ch)
        End Function

        ''' <summary>
        ''' Regresa la misma cadena de texto, pero con la primera letra mayúscula.
        ''' </summary>
        ''' <param name="val">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function UppercaseFirstCharacter(ByVal val As String) As String
            If String.IsNullOrEmpty(val) Then
                Return val
            End If
            Dim array() As Char = val.ToCharArray
            array(0) = Char.ToUpper(array(0))
            Return New String(array)
        End Function

        ''' <summary>
        ''' Codifica texto para ser guardado en el LOG.
        ''' </summary>
        ''' <param name="text">Texto a codificar</param>
        ''' <returns></returns>
        Public Shared Function PsvSafeEncode(ByVal text As String) As String
            Return text.Replace("|"c, "%CHAR:U+007C%")
        End Function

        ''' <summary>
        ''' Decodifica texto guardado en el LOG.
        ''' </summary>
        ''' <param name="text">Texto a decodificar.</param>
        ''' <returns></returns>
        Public Shared Function PsvSafeDecode(ByVal text As String) As String
            Return text.Replace("%CHAR:U+007C%", "|")
        End Function

        ''' <summary>
        ''' Convierte el texto completo a minúsculas y luego coloca en mayúsculas la primera letra.
        ''' </summary>
        ''' <param name="text">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function NormalizeText(ByVal text As String) As String
            Dim s As String = text.ToLower
            Return UppercaseFirstCharacter(s)
        End Function

        ''' <summary>
        ''' Verifica si una cadena de texto es numérica.
        ''' </summary>
        ''' <param name="number">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function IsNumeric(ByVal number As String) As Boolean
            If Regex.IsMatch(number, "^[0-9 ]+$") Then
                Return True
            Else
                Return False
            End If
        End Function


        ''' <summary>
        ''' Obtiene el último hilo que coincida con el título entregado.
        ''' </summary>
        ''' <param name="threads"></param>
        ''' <returns></returns>
        Public Shared Function GetLastThreadByTitle(ByVal threads As String(), title As String) As String
            Dim matchingthread As String = String.Empty
            For Each t As String In threads
                If GetTitleFromThread(t) = title Then
                    matchingthread = t
                End If
            Next
            Return matchingthread
        End Function

        ''' <summary>
        ''' Entrega el primer título del hilo en formato wikitexto que se le pase como parámetro. Si no tiene título entregará una cadena vacía.
        ''' </summary>
        ''' <param name="thread"></param>
        ''' <returns></returns>
        Public Shared Function GetTitleFromThread(ByVal thread As String) As String
            Dim TitlesList As New List(Of String)
            Dim temptext As String = thread
            Dim commentMatch As MatchCollection = Regex.Matches(temptext, "(<!--)[\s\S]*?(-->)")
            Dim CommentsList As New List(Of String)
            For i As Integer = 0 To commentMatch.Count - 1
                CommentsList.Add(commentMatch(i).Value)
                temptext = temptext.Replace(commentMatch(i).Value, ColoredText("PERIODIBOT::::COMMENTREPLACE::::" & i, 4))
            Next
            Dim mc As MatchCollection = Regex.Matches(temptext, "([\n\r]|^)((==(?!=)).+?(==(?!=)))")
            For Each m As Match In mc
                TitlesList.Add(m.Value)
            Next
            If TitlesList.Count > 0 Then
                Return TitlesList.First
            Else
                Return String.Empty
            End If
        End Function


        ''' <summary>
        ''' Entrega únicamente los títulos de los hilos en formato wikitexto que se les pase como parámetro. Si uno de los parámetros no tiene título entregará una cadena vacía en su lugar.
        ''' </summary>
        ''' <param name="threads"></param>
        ''' <returns></returns>
        Public Shared Function GetTitlesFromThreads(ByVal threads As String()) As String()
            Dim TitlesList As New List(Of String)
            For Each threadtext As String In threads
                TitlesList.Add(GetTitleFromThread(threadtext))
            Next
            Return TitlesList.ToArray
        End Function

        ''' <summary>
        ''' Entrega los elementos en el segundo array que no estén presentes en el primero
        ''' </summary>
        ''' <param name="Arr1">Array base</param>
        ''' <param name="arr2">Array a comparar</param>
        ''' <returns></returns>
        Public Shared Function GetSecondArrayAddedDiff(ByVal arr1 As String(), arr2 As String()) As String()
            Dim Difflist As New List(Of String)
            For i As Integer = 0 To arr2.Count - 1
                If Not arr1.Contains(arr2(i)) Then
                    Difflist.Add(arr2(i))
                End If
            Next
            Return Difflist.ToArray
        End Function

        ''' <summary>
        ''' Entrega los titulos de los hilos en el segundo array que sean distintos al primero
        ''' </summary>
        ''' <param name="threadlist1">Array base</param>
        ''' <param name="threadlist2">Array a comparar</param>
        ''' <returns></returns>
        Public Shared Function GetChangedThreadsTitle(ByVal threadlist1 As String(), threadlist2 As String()) As String()
            Dim Difflist As New List(Of String)
            Dim thread1 As List(Of String) = threadlist1.ToList
            Dim thread2 As List(Of String) = threadlist2.ToList
            thread1.Sort()
            thread2.Sort()

            If thread1.Count = thread2.Count Then
                For i As Integer = 0 To thread1.Count - 1
                    If Not thread1(i) = thread2(i) Then
                        Difflist.Add(GetTitleFromThread(thread1(i)))
                    End If
                Next
            ElseIf thread2.Count > thread1.Count - 1 Then
                Difflist.AddRange(GetSecondArrayAddedDiff(GetTitlesFromThreads(thread1.ToArray), GetTitlesFromThreads(thread2.ToArray)))
            End If
            Return Difflist.ToArray
        End Function


        ''' <summary>
        ''' Entrega los titulos de los hilos en el segundo array que sean distintos al primero. Los array deben tener los mismos elementos o se retornara un array vacio.
        ''' </summary>
        ''' <param name="threadlist1">Array base</param>
        ''' <param name="threadlist2">Array a comparar</param>
        ''' <returns></returns>
        Public Shared Function GetChangedThreads(ByVal threadlist1 As String(), threadlist2 As String()) As String()
            Dim Difflist As New List(Of String)
            Dim thread1 As List(Of String) = threadlist1.ToList
            Dim thread2 As List(Of String) = threadlist2.ToList
            thread1.Sort()
            thread2.Sort()
            If thread1.Count = thread2.Count Then
                For i As Integer = 0 To thread1.Count - 1
                    If Not thread1(i) = thread2(i) Then
                        Difflist.Add(thread2(i))
                    End If
                Next
            End If
            Return Difflist.ToArray
        End Function

        ''' <summary>
        ''' Entrega el último hilo en los hilos entregados que coincida con el nombre
        ''' </summary>
        ''' <param name="threads"></param>
        ''' <param name="title"></param>
        ''' <returns></returns>
        Public Shared Function GetThreadByTitle(ByVal threads As String(), title As String) As String
            Dim thread As String = String.Empty
            For Each threadtext As String In threads
                Dim currentthreadtitle As String = GetTitleFromThread(threadtext)
                If title = currentthreadtitle Then
                    thread = currentthreadtitle
                End If
            Next
            Return thread
        End Function


        ''' <summary>
        ''' Convierte un array de tipo string numérico a integer. Si uno de los elementos no es numérico retorna 0 en su lugar.
        ''' </summary>
        ''' <param name="arr">Array a evaluar</param>
        ''' <returns></returns>
        Public Shared Function StringArrayToInt(ByVal arr As String()) As Integer()
            Dim intlist As New List(Of Integer)
            For i As Integer = 0 To arr.Count - 1
                If IsNumeric(arr(i)) Then
                    intlist.Add(Integer.Parse(arr(i)))
                Else
                    intlist.Add(0)
                End If
            Next
            Return intlist.ToArray
        End Function

        ''' <summary>
        ''' Realiza los escapes para usar una cadena de texto dentro de una expresión regular.
        ''' </summary>
        ''' <param name="s">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function RegexParser(ByVal s As String) As String
            Return Regex.Escape(s)
        End Function

        ''' <summary>
        ''' Realiza los escapes para usar una cadena de texto dentro de una expresión regular, exceptuando el "pipe" (|).
        ''' </summary>
        ''' <param name="s">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function SpamListParser(ByVal s As String) As String
            s = s.Replace("\", "\\")
            Return s.Replace("[", "\[").Replace("/", "\/").Replace("^", "\^").Replace("$", "\$").Replace(".", "\.") _
                .Replace("?", "\?").Replace("*", "\*").Replace("+", "\+").Replace("(", "\(").Replace(")", "\)") _
                .Replace("{", "\{").Replace("}", "\}")
        End Function
        ''' <summary>
        ''' Entrega el título de la página en la pseudoplantilla de resúmenes de página.
        ''' </summary>
        ''' <param name="SourceString">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function GetTitlesOfTemplate(ByVal SourceString As String) As String()
            Return TextInBetween(SourceString, "|" & Environment.NewLine & "|", "=")
        End Function

        ''' <summary>
        ''' Reeplaza la primera ocurrencia de una cadena dada en la cadena de entrada.
        ''' </summary>
        ''' <param name="text">Cadena de texto a modificar.</param>
        ''' <param name="search">Cadena texto a buscar.</param>
        ''' <param name="replace">Cadena texto de reemplazo.</param>
        ''' <returns></returns>
        Public Shared Function ReplaceFirst(ByVal text As String, ByVal search As String, ByVal replace As String) As String
            Dim pos As Integer = text.IndexOf(search)
            If pos < 0 Then
                Return text
            End If
            Return text.Substring(0, pos) & replace + text.Substring(pos + search.Length)
        End Function

        ''' <summary>
        ''' Reeplaza la todas las ocurrencia de una cadena dada en la cadena de entrada menos la primera.
        ''' </summary>
        ''' <param name="text">Cadena de texto a modificar.</param>
        ''' <param name="search">Cadena texto a buscar.</param>
        ''' <param name="replace">Cadena texto de reemplazo.</param>
        ''' <returns></returns>
        Public Shared Function ReplaceEveryOneButFirst(ByVal text As String, ByVal search As String, ByVal replace As String) As String
            Dim placeholder As String = ColoredText("#$^&&*!?.::++!+_@(!@PLACEHOLDER:::PLACEHOLDER:$^&&*!?.::++!+:PLACEHOLDER::PLACEHOLDER:$^&&*!?.::++!+:PLACEHOLDER::PLACEHOLDER$^&&*!?.::++!+", 1, 1)
            text = ReplaceFirst(text, search, placeholder)
            text = text.Replace(search, replace)
            text = text.Replace(placeholder, search)
            Return text
        End Function

        ''' <summary>
        ''' Reeplaza la todas las ocurrencia de una cadena dada en la cadena de entrada menos la última.
        ''' </summary>
        ''' <param name="text">Cadena de texto a modificar.</param>
        ''' <param name="search">Cadena texto a buscar.</param>
        ''' <param name="replace">Cadena texto de reemplazo.</param>
        ''' <returns></returns>
        Public Shared Function ReplaceEveryOneButLast(ByVal text As String, ByVal search As String, ByVal replace As String) As String
            Dim placeholder As String = ColoredText("#$^&&*!?.::++!+_@(!@PLACEHOLDER:::PLACEHOLDER:$^&&*!?.::++!+:PLACEHOLDER::PLACEHOLDER:$^&&*!?.::++!+:PLACEHOLDER::PLACEHOLDER$^&&*!?.::++!+", 1, 1)
            text = ReplaceLast(text, search, placeholder)
            text = text.Replace(search, replace)
            text = text.Replace(placeholder, search)
            Return text
        End Function


        ''' <summary>
        ''' Reeplaza la última ocurrencia de una cadena dada en la cadena de entrada.
        ''' </summary>
        ''' <param name="text">Cadena de texto a modificar.</param>
        ''' <param name="search">Cadena texto a buscar.</param>
        ''' <param name="replace">Cadena texto de reemplazo.</param>
        ''' <returns></returns>
        Public Shared Function ReplaceLast(ByVal text As String, ByVal search As String, ByVal replace As String) As String
            Dim pos As Integer = text.LastIndexOf(search)
            If pos < 0 Then
                Return text
            End If
            Return text.Substring(0, pos) & replace & text.Substring(pos + search.Length)
        End Function

        ''' <summary>
        ''' Escribe una línea en la salida del programa a modo de registro siguiendo un formato estándar 
        ''' (en realidad es completamente arbitrario pero está ordenado y bonito :) ).
        ''' </summary>
        ''' <param name="type">Tipo de registro</param>
        ''' <param name="source">Origen del registro</param>
        ''' <param name="message">Mensaje de salida</param>
        Public Shared Sub WriteLine(ByVal type As String, ByVal source As String, message As String)
            Dim msgstr As String = "[" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "]" & " [" & source & " " & type & "] " & message
            Console.WriteLine(msgstr)
        End Sub

        ''' <summary>
        ''' Retorna las líneas contenidas en una cadena de texto.
        ''' </summary>
        ''' <param name="text">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function GetLines(ByVal text As String) As String()
            Return GetLines(text, False)
        End Function

        ''' <summary>
        ''' Retorna las líneas contenidas en una cadena de texto.
        ''' </summary>
        ''' <param name="text">Texto a evaluar.</param>
        ''' <param name="removeemptylines">Eliminar las líneas vacías</param>
        ''' <returns></returns>
        Public Shared Function GetLines(ByVal text As String, ByVal removeEmptyLines As Boolean) As String()
            Dim thelines As List(Of String) = text.Split({vbCrLf, vbCr, vbLf, Environment.NewLine}, StringSplitOptions.None).ToList
            If removeEmptyLines Then
                thelines.RemoveAll(Function(x) String.IsNullOrWhiteSpace(x))
            End If
            Return thelines.ToArray
        End Function

        ''' <summary>
        ''' Soluciona un error de la api en los resúmenes, donde cuertas plantillas los números los entrega repetidos con varios símbolos en medio.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function FixResumeNumericExp(ByVal text As String) As String
            Dim newtext As String = text
            For Each m As Match In Regex.Matches(text, "&+[0-9]+\.&+[0-9 ]+")
                Dim num As Integer = Integer.Parse(TextInBetween(m.Value, "&&&&&&&&", ".&&&&&")(0))
                Dim numsrt As String = num.ToString & " "
                newtext = newtext.Replace(m.Value, numsrt)
            Next
            Return newtext
        End Function
        ''' <summary>
        ''' Normaliza los títulos de la plantilla de resúmenes.
        ''' </summary>
        ''' <param name="SourceString">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function GetNormalizedTitlesOfTemplate(ByVal SourceString As String) As String()
            Dim mlist As New List(Of String)
            For Each m As Match In Regex.Matches(SourceString, "(\n\|)[\s\S]*?(=\n)")
                mlist.Add(NormalizeText(m.Value.Replace("|", String.Empty).Replace("=", String.Empty).Replace(vbLf, String.Empty).Replace("_", " ")))
            Next
            Return mlist.ToArray
        End Function
        ''' <summary>
        ''' Entrega un array con todas las coincidencias donde se encuentre un texto en medio de dos cadenas de texto
        ''' </summary>
        ''' <param name="SourceString">Texto a evaluar.</param>
        ''' <param name="string1">Texto a la izquierda.</param>
        ''' <param name="string2">Texto a la derecha.</param>
        ''' <returns></returns>
        Public Shared Function TextInBetween(ByVal SourceString As String, string1 As String, string2 As String) As String()
            Dim mlist As New List(Of String)
            For Each m As Match In Regex.Matches(SourceString, "(" & RegexParser(string1) & ")[\s\S]*?(" & RegexParser(string2) & ")")
                mlist.Add(m.Value.Replace(string1, String.Empty).Replace(string2, String.Empty))
            Next
            Return mlist.ToArray
        End Function
        ''' <summary>
        ''' Similar a TextInBetween pero incluye además las las cadenas a la izquierda y derecha del texto.
        ''' </summary>
        ''' <param name="SourceString">Texto a evaluar.</param>
        ''' <param name="string1">Texto a la izquierda.</param>
        ''' <param name="string2">Texto a la derecha</param>
        ''' <returns></returns>
        Public Shared Function TextInBetweenInclusive(ByVal SourceString As String, string1 As String, string2 As String) As String()
            Dim mlist As New List(Of String)
            For Each m As Match In Regex.Matches(SourceString, "(" & RegexParser(string1) & ")[\s\S]*?(" & RegexParser(string2) & ")")
                mlist.Add(m.Value)
            Next
            Return mlist.ToArray
        End Function
        ''' <summary>
        ''' Entrega un array con todos los números (integer) que estén entre comillas.
        ''' </summary>
        ''' <param name="SourceString">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function MatchQuotedIntegers(ByVal SourceString As String) As String()
            Dim mlist As New List(Of String)
            For Each m As Match In Regex.Matches(SourceString, "(""[0-9]+"")")
                mlist.Add(m.Value.Replace("""", ""))
            Next
            Return mlist.ToArray
        End Function
        ''' <summary>
        ''' Entrega los títulos en una cadena de texto con el formato de respuesta de la Api de wikipedia.
        ''' </summary>
        ''' <param name="sourcestring">Texto a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function GetTitlesFromQueryText(ByVal sourcestring As String) As String()
            Return TextInBetween(sourcestring, ",""title"":""", """,")
        End Function

        ''' <summary>
        ''' Normaliza el texto ASCII con códigos unicodes escapados con el formato \\u(número)
        ''' </summary>
        ''' <param name="stringToNormalize"></param>
        ''' <returns></returns>
        Public Shared Function NormalizeUnicodetext(ByVal stringToNormalize As String) As String
            Dim temptext As String = Regex.Replace(stringToNormalize, "\\u([\dA-Fa-f]{4})", Function(v) ChrW(Convert.ToInt32(v.Groups(1).Value, 16)))
            temptext = Regex.Replace(temptext, "(?<!\\)\\n", Environment.NewLine)
            temptext = Regex.Replace(temptext, "(?<!\\)\\t", ControlChars.Tab)
            temptext = temptext.Replace("\""", """")
            temptext = temptext.Replace("\\", "\")
            temptext = temptext.Replace("\\n", "\n")
            temptext = temptext.Replace("\\t", "\t")
            Return temptext
        End Function

        ''' <summary>
        ''' Normaliza el texto ASCII con códigos unicodes escapados con el formato \\u(número)
        ''' </summary>
        ''' <param name="stringToNormalize"></param>
        ''' <returns></returns>
        Public Shared Function NormalizeHexScapedString(ByVal stringToNormalize As String) As String
            Dim temptext As String = Regex.Replace(stringToNormalize, "\\u([\dA-Fa-f]{4})", Function(v) ChrW(Convert.ToInt32(v.Groups(1).Value, 16)))
            Return temptext
        End Function

        ''' <summary>
        ''' Codifica una cadena de texto en URLENCODE.
        ''' </summary>
        ''' <param name="textToEncode">Texto a codificar.</param>
        ''' <returns></returns>
        Public Shared Function UrlWebEncode(ByVal textToEncode As String) As String
            Dim PreTreatedText As String = WebUtility.UrlEncode(textToEncode)
            Dim TreatedText As String = Regex.Replace(PreTreatedText, "%\w{2}", Function(x) x.Value.ToUpper)
            Return TreatedText
        End Function

        ''' <summary>
        ''' Decodifica una cadena de texto en URLENCODE.
        ''' </summary>
        ''' <param name="text">Texto a decodificar.</param>
        ''' <returns></returns>
        Public Shared Function UrlWebDecode(ByVal text As String) As String
            Return WebUtility.UrlDecode(text)
        End Function

        ''' <summary>
        ''' Evalua una línea de texto (formato IRC según la RFC) y entrega el usuario que emitió el mensaje.
        ''' </summary>
        ''' <param name="response">Mensaje a evaluar.</param>
        ''' <returns></returns>
        Public Shared Function GetUserFromChatresponse(ByVal response As String) As String
            Return response.Split("!"c)(0).Replace(":", "")
        End Function

        ''' <summary>
        ''' Elimina todas las letras dejando únicamente números
        ''' </summary>
        ''' <param name="text">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function RemoveAllAlphas(ByVal text As String) As String
            Return Regex.Replace(text, "[^0-9]", "")
        End Function

        ''' <summary>
        ''' Retorna una cadena de texto formateada de tal forma que muestra colores, para mas info ver https://github.com/myano/jenni/wiki/IRC-String-Formatting
        ''' </summary>
        ''' <param name="ForegroundColor">Color del texto</param>
        ''' <param name="BackgroundColor">Color del fondo, si se omite se usa el color por defecto del cliente irc</param>
        Public Shared Function ColoredText(ByVal text As String, ForegroundColor As Integer, Optional BackgroundColor As Integer = 99) As String
            Dim _foregroundColor As String = ForegroundColor.ToString("00")
            Dim _backgroundColor As String = BackgroundColor.ToString("00")
            If _backgroundColor = "99" Then
                Return Chr(3) & _foregroundColor & text & Chr(3) & Chr(15)
            Else
                Return Chr(3) & _foregroundColor & "," & _backgroundColor & text & Chr(3) & Chr(15)
            End If
        End Function

        ''' <summary>
        ''' Cuenta las veces que se repite una cadena de texto en otra cadena de texto.
        ''' </summary>
        ''' <param name="StToSearch"></param>
        ''' <param name="StToLookFor"></param>
        ''' <returns></returns>
        Public Shared Function CountOccurrences(ByVal StToSearch As String, StToLookFor As String) As Integer
            Dim txtlen As Integer = StToSearch.Length
            Dim strlen As Integer = StToLookFor.Length
            Dim newstring As String = StToSearch.Replace(StToLookFor, String.Empty)
            Dim newtxtlen As Integer = newstring.Length
            Dim lenghtdiff As Integer = txtlen - newtxtlen
            Dim occurences As Integer = CInt(lenghtdiff / strlen)
            Return occurences
        End Function

        ''' <summary>
        ''' Entrega un valor que simboliza el nivel de aparición de las palabras indicadas
        ''' </summary>
        ''' <param name="Phrase">Frase a evaluar</param>
        ''' <param name="words">Palabras a buscar</param>
        ''' <returns></returns>
        Public Shared Function LvlOfAppereance(ByVal phrase As String, words As String()) As Double
            If (phrase Is Nothing) Or (words Is Nothing) Then
                Return 0
            End If
            Dim PhraseString As String() = phrase.Split(Chr(32))
            Dim NOWords As Integer = PhraseString.Count
            Dim NOAppeareances As Integer = 0
            For a As Integer = 0 To NOWords - 1
                For Each s As String In words
                    If PhraseString(a).ToLower.Contains(s.ToLower) Then
                        NOAppeareances += 1
                    End If
                Next
            Next
            Return ((CType(NOAppeareances, Double) * 100) / CType(NOWords, Double))
        End Function

        ''' <summary>
        ''' Separa un array de string segun lo especificado. Retorna una lista con listas de texto.
        ''' </summary>
        ''' <param name="StrArray">Lista a partir</param>
        ''' <param name="chunkSize">En cuantos items se parte</param>
        ''' <returns></returns>
        Public Shared Function SplitStringArrayIntoChunks(strArray As String(), chunkSize As Integer) As List(Of List(Of String))
            Return strArray.
                    Select(Function(x, i) New With {Key .Index = i, Key .Value = x}).
                    GroupBy(Function(x) (x.Index \ chunkSize)).
                    Select(Function(x) x.Select(Function(v) v.Value).ToList()).
                    ToList()
        End Function

        ''' <summary>
        ''' Retorna una cadena de texto que interpreta los bytes entregados como Byte, Kbyte o Mbyte respectivamente.
        ''' Importante: El valor debe ser en Bytes, no Bits. La función redondea en la segunda posición decimal.
        ''' </summary>
        ''' <param name="bytes"></param>
        ''' <returns></returns>
        Public Shared Function GetSizeAsString(ByVal bytes As Integer) As String
            If bytes < 999 Then
                Return bytes.ToString & " Bytes"
            ElseIf bytes < 999999 Then
                Return System.Math.Round((bytes / 1000), 2).ToString & " KB"
            Else
                Return System.Math.Round((bytes / 1000000), 2).ToString & " MB"
            End If
        End Function

        ''' <summary>
        ''' Retorna el destino del enlace interno y el texto del mismo. Debe pasarse como parámetro una cadena de texto con formato de enlace wiki. Debe cumplir con la expresión regular "(\[\[)(.+?)(\]\]))".
        ''' </summary>
        ''' <param name="tlink"></param>
        ''' <returns></returns>
        Public Shared Function GetLinkText(ByVal tlink As String) As Tuple(Of String, String)
            Dim tstr As String = tlink.Replace("[[", "").Replace("]]", "")
            If tstr.Contains("|") Then
                Dim tval As Tuple(Of String, String) = New Tuple(Of String, String)(tstr.Split("|"c)(0).Trim(), tstr.Split("|"c)(1).Trim())
                Return tval
            End If
            Return New Tuple(Of String, String)(tstr, tstr)
        End Function

#End Region

#Region "Math Functions"
        ''' <summary>
        ''' Retorna true si un numero es par.
        ''' </summary>
        ''' <param name="Number"></param>
        ''' <returns></returns>
        Public Shared Function IsODD(ByVal number As Integer) As Boolean
            Return (number Mod 2 = 0)
        End Function

        ''' <summary>
        ''' Separa un array de integer segun lo especificado. Retorna una lista con listas de integer.
        ''' </summary>
        ''' <param name="IntArray">Lista a partir</param>
        ''' <param name="chunkSize">En cuantos items se parte</param>
        ''' <returns></returns>
        Public Shared Function SplitIntegerArrayIntoChunks(intArray As Integer(), chunkSize As Integer) As List(Of List(Of Integer))
            Return intArray.
                    Select(Function(x, i) New With {Key .Index = i, Key .Value = x}).
                    GroupBy(Function(x) (x.Index \ chunkSize)).
                    Select(Function(x) x.Select(Function(v) v.Value).ToList()).
                    ToList()
        End Function

        ''' <summary>
        ''' Convierte una cadena de texto a un array de integer donde cada valor equivale a la representación numérica del cada caracter en la cadena.
        ''' </summary>
        ''' <param name="tstring"></param>
        ''' <returns></returns>
        Public Shared Function StringToIntArray(ByVal tstring As String) As Integer()
            Return tstring.ToCharArray.[Select](Function(c) Convert.ToInt32(c.ToString())).ToArray()
        End Function


#End Region

#Region "Program Subs"
        ''' <summary>
        ''' Finaliza el programa correctamente.
        ''' </summary>
        Public Shared Sub ExitProgram()
            Environment.Exit(0)
        End Sub

#End Region

#Region "Program Functions"

        ''' <summary>
        ''' Entrega la plataforma de ejecución.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetPlatform() As String
            Dim isFreebsd As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
            Dim isLinux As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            Dim isWindows As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            Dim isOSx As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            Return If(isFreebsd, "FreeBDS", "") & If(isLinux, "Linux", "") & If(isWindows, "Windows", "") & If(isOSx, "OSX", "")
        End Function

        Public Shared Function GetOSDescription() As String
            Return RuntimeInformation.OSDescription
        End Function

        Public Shared Function GetExecMode() As String
            Dim processname As String = Process.GetCurrentProcess().ProcessName
            Dim dotnet As Boolean = processname.Contains("dotnet")
            Return If(dotnet, "dotnet (JIT)", "native")
        End Function

        Public Shared Function GetOsString() As String
            Return GetOSDescription() & " (" & GetPlatform() & ", " & GetExecMode() & ")"

        End Function

        ''' <summary>
        ''' Intercambia dos objetos del mismo tipo
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="arg1"></param>
        ''' <param name="arg2"></param>;
        Sub Swap(Of T)(ByRef arg1 As T, ByRef arg2 As T)
            Dim temp As T = arg1
            arg1 = arg2
            arg2 = temp
        End Sub

        ''' <summary>
        ''' Establece un tiempo de espera (en segundos)
        ''' </summary>
        ''' <param name="seconds"></param>
        ''' <returns></returns>
        Public Shared Function WaitSeconds(ByVal seconds As Integer) As Boolean
            System.Threading.Thread.Sleep(seconds * 1000)
            Return True
        End Function

        ''' <summary>
        ''' Convierte una cadena de texto con una hora en formato unix a DateTime
        ''' </summary>
        ''' <param name="strUnixTime"></param>
        ''' <returns></returns>
        Public Shared Function UnixToTime(ByVal strUnixTime As String) As Date
            UnixToTime = DateAdd(DateInterval.Second, Val(strUnixTime), #1/1/1970#)
            If UnixToTime.IsDaylightSavingTime = True Then
                UnixToTime = DateAdd(DateInterval.Hour, 1, UnixToTime)
            End If
        End Function

        ''' <summary>
        ''' Convierte una fecha a un numero entero que representa la hora en formato unix
        ''' </summary>
        ''' <param name="dteDate"></param>
        ''' <returns></returns>
        Public Shared Function TimeToUnix(ByVal dteDate As Date) As Integer
            If dteDate.IsDaylightSavingTime = True Then
                dteDate = DateAdd(DateInterval.Hour, -1, dteDate)
            End If
            TimeToUnix = CInt(DateDiff(DateInterval.Second, #1/1/1970#, dteDate))
        End Function

        ''' <summary>
        ''' Convierte una cadena de texto con formato especial a segundos
        ''' </summary>
        ''' <param name="time"></param>
        ''' <returns></returns>
        Public Shared Function TimeStringToSeconds(ByVal time As String) As Integer
            Try
                Dim Str1 As String() = time.Split(CType(":", Char))
                Dim Str2 As String() = Str1(0).Split(CType(".", Char))

                Dim Str_Days As String = Str2(0)
                Dim Str_Hours As String = Str2(1)
                Dim Str_Minutes As String = Str1(1)

                Dim Days As Integer = Convert.ToInt32(Str_Days) * 86400
                Dim Hours As Integer = Convert.ToInt32(Str_Hours) * 3600
                Dim Minutes As Integer = Convert.ToInt32(Str_Minutes) * 60

                Dim total As Integer = (Days + Hours + Minutes)
                Return total
            Catch ex As IndexOutOfRangeException
                Return 0
            End Try
        End Function

        ''' <summary>
        ''' Retorna los hilos hijos del programa, puede variar según el framework. No es fiable como indicador de tareas en ejecución.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetCurrentThreads() As Integer
            Return Process.GetCurrentProcess().Threads.Count
        End Function

        ''' <summary>
        ''' Entrega la cantidad de memoria reservada para el proceso, depende del framework y no tiene relación directa con el rendimiento real del programa.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function PrivateMemory() As Long
            Return CLng(Process.GetCurrentProcess().PrivateMemorySize64 / 1024)
        End Function

        ''' <summary>
        ''' Entrega la cantidad de memoria que el programa está usando efectivamente.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function UsedMemory() As Long
            Return CLng(Process.GetCurrentProcess().WorkingSet64 / 1024)
        End Function

        ''' <summary>
        ''' Entrega Verdadero si el usuario presiona cualquier tecla dentro del tiempo indicado
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function PressKeyTimeout(ByVal Timeout As Integer) As Boolean
            Dim exitloop As Boolean = False
            Console.Write(SStrings.PressKey)
            For ttime As Integer = 0 To Timeout
                Console.Write(".")
                If Console.KeyAvailable Then
                    exitloop = True
                    Exit For
                End If
                System.Threading.Thread.Sleep(1000)
            Next
            Console.Write(Environment.NewLine)
            Return exitloop
        End Function


#End Region

#Region "Wikipedia exclusive"
        ''' <summary>
        ''' Entrega como array de DateTime todas las fechas (Formato de firma Wikipedia) en el texto dado.
        ''' </summary>
        ''' <param name="text">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function AllDateTimes(ByVal text As String) As DateTime()
            Dim Datelist As New List(Of DateTime)
            For Each m As Match In Regex.Matches(text, signpattern)
                Dim TheDate As DateTime = ESWikiDatetime(m.Value)
                Datelist.Add(TheDate)
            Next
            Return Datelist.ToArray
        End Function

        ''' <summary>
        ''' Evalua texto (wikicódigo) y regresa un array de string con cada uno de los hilos del mismo (los que comienzan con = ejemplo = y terminan en otro comienzo o el final de la página).
        ''' </summary>
        ''' <param name="pagetext">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function GetPageMainThreads(ByVal pagetext As String) As String()
            Dim tutil As Utils = New Utils
            Return tutil.GetLvlThread(pagetext, 1)
        End Function

        ''' <summary>
        ''' Evalua texto (wikicódigo) y regresa un array de string con cada uno de los hilos del mismo (los que comienzan con == ejemplo == y terminan en otro comienzo o el final de la página).
        ''' </summary>
        ''' <param name="pagetext">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function GetPageThreads(ByVal pagetext As String) As String()
            Dim tutil As Utils = New Utils
            Return tutil.GetLvlThread(pagetext, 2)
        End Function

        ''' <summary>
        ''' Evalua texto (wikicódigo) y regresa un array de string con cada uno de los subhilos del mismo (los que comienzan con === ejemplo === y terminan en otro subhilo o el final de la página).
        ''' </summary>
        ''' <param name="pagetext">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function GetPageSubThreads(ByVal pagetext As String) As String()
            Dim tutil As Utils = New Utils
            Return tutil.GetLvlThread(pagetext, 3)
        End Function

        ''' <summary>
        ''' Función base que obtiene los secciones de una página según la expresión regular que defina las secciones.
        ''' </summary>
        ''' <param name="pagetext">Texto</param>
        ''' <param name="lvl">Nivel (= 1 = | == 2 == | === 3 === | ==== 4 ==== ), cualquier otro valor será ignorado y se trabajará con Nivel 2</param>
        ''' <returns></returns>
        Private Function GetLvlThread(ByVal pagetext As String, ByVal lvl As Integer) As String()
            Dim ThreadList As New List(Of String)
            Dim temptext As String = pagetext
            Dim commentMatch As MatchCollection = Regex.Matches(temptext, "(<!--)[\s\S]*?(-->)")
            Dim NowikiMatch As MatchCollection = Regex.Matches(temptext, "(<[nN]owiki>)([\s\S]+?)(<\/[nN]owiki>)")
            Dim CodeMatch As MatchCollection = Regex.Matches(temptext, "(<[cC]ode>)([\s\S]+?)(<\/[cC]ode>)")
            Dim PreMatch As MatchCollection = Regex.Matches(temptext, "(<pre>)([\s\S]+?)(<\/pre>)", RegexOptions.IgnoreCase)
            Dim SrcMatch As MatchCollection = Regex.Matches(temptext, "(<source( [^>]*)?>)([\s\S]+?)(<\/source>)", RegexOptions.IgnoreCase)
            Dim SyntaxHlMatch As MatchCollection = Regex.Matches(temptext, "(<syntaxhighlight( [^>]*)?>)([\s\S]+?)(<\/syntaxhighlight>)", RegexOptions.IgnoreCase)

            'Comentarios
            Dim CommentsList As New List(Of String)
            'Nowiki
            Dim NowikiList As New List(Of String)
            'Code
            Dim CodeList As New List(Of String)
            'Pre
            Dim Prelist As New List(Of String)
            'Src
            Dim SrcList As New List(Of String)
            'SynHL
            Dim SyntaxHlList As New List(Of String)

            'Comentarios
            For i As Integer = 0 To commentMatch.Count - 1
                CommentsList.Add(commentMatch(i).Value)
                temptext = temptext.Replace(commentMatch(i).Value, ColoredText("PERIODIBOT::::COMMENTREPLACE::::" & i, 4))
            Next
            'Nowiki
            For i As Integer = 0 To NowikiMatch.Count - 1
                NowikiList.Add(NowikiMatch(i).Value)
                temptext = temptext.Replace(NowikiMatch(i).Value, ColoredText("PERIODIBOT::::NOWIKIREPLACE::::" & i, 4))
            Next
            'Code
            For i As Integer = 0 To CodeMatch.Count - 1
                CodeList.Add(CodeMatch(i).Value)
                temptext = temptext.Replace(CodeMatch(i).Value, ColoredText("PERIODIBOT::::CODEREPLACE::::" & i, 4))
            Next
            'Pre
            For i As Integer = 0 To PreMatch.Count - 1
                Prelist.Add(PreMatch(i).Value)
                temptext = temptext.Replace(PreMatch(i).Value, ColoredText("PERIODIBOT::::PREREPLACE::::" & i, 4))
            Next
            'Src
            For i As Integer = 0 To SrcMatch.Count - 1
                SrcList.Add(SrcMatch(i).Value)
                temptext = temptext.Replace(SrcMatch(i).Value, ColoredText("PERIODIBOT::::SRCREPLACE::::" & i, 4))
            Next
            'SynHL
            For i As Integer = 0 To SyntaxHlMatch.Count - 1
                SyntaxHlList.Add(SyntaxHlMatch(i).Value)
                temptext = temptext.Replace(SyntaxHlMatch(i).Value, ColoredText("PERIODIBOT::::SYNREPLACE::::" & i, 4))
            Next

            Dim RegexExpression As String

            Select Case lvl
                Case 1
                    RegexExpression = "(((\n|\r|^)(=(?!=))(.+)(=(?!=))[ \t]*(\n|\r))([\s\S]*?))(?=(((\n|\r)(=(?!=))(.+)(=(?!=))[ \t]*(\n|\r))([\s\S]*?))|$)"
                Case 2
                    RegexExpression = "(((\n|\r|^)(==(?!=))(.+)(==(?!=))[ \t]*(\n|\r))([\s\S]*?))(?=(((\n|\r)(==(?!=))(.+)(==(?!=))[ \t]*(\n|\r))([\s\S]*?))|$)"
                Case 3
                    RegexExpression = "(((\n|\r|^)(===(?!=))(.+)(===(?!=))[ \t]*(\n|\r))[\s\S]*?)(?=(((\n|\r)(=){1,4}(.+)(=){1,4}[ \t]*(\n|\r))[\s\S]*?)|$)"
                Case 4
                    RegexExpression = "(((\n|\r|^)(====(?!=))(.+)(====(?!=))[ \t]*(\n|\r))[\s\S]*?)(?=(((\n|\r)(=){1,4}(.+)(=){1,4}[ \t]*(\n|\r))[\s\S]*?)|$)"
                Case Else
                    RegexExpression = "(((\n|\r|^)(==(?!=))(.+)(==(?!=))[ \t]*(\n|\r))([\s\S]*?))(?=(((\n|\r)(==(?!=))(.+)(==(?!=))[ \t]*(\n|\r))([\s\S]*?))|$)"
            End Select

            Dim threads As MatchCollection = Regex.Matches(temptext, RegexExpression)

            For Each t As Match In threads
                ThreadList.Add(t.Groups(1).Value)
            Next

            Dim EndThreadList As New List(Of String)
            For Each t As String In ThreadList
                Dim nthreadtext As String = t
                'Comentarios
                For i As Integer = 0 To commentMatch.Count - 1
                    Dim commenttext As String = ColoredText("PERIODIBOT::::COMMENTREPLACE::::" & i, 4)
                    nthreadtext = nthreadtext.Replace(commenttext, CommentsList(i))
                Next
                'Nowiki
                For i As Integer = 0 To NowikiMatch.Count - 1
                    Dim codetext As String = ColoredText("PERIODIBOT::::NOWIKIREPLACE::::" & i, 4)
                    nthreadtext = nthreadtext.Replace(codetext, NowikiList(i))
                Next
                'Code
                For i As Integer = 0 To CodeMatch.Count - 1
                    Dim codetext As String = ColoredText("PERIODIBOT::::CODEREPLACE::::" & i, 4)
                    nthreadtext = nthreadtext.Replace(codetext, CodeList(i))
                Next
                'Pre
                For i As Integer = 0 To PreMatch.Count - 1
                    Dim codetext As String = ColoredText("PERIODIBOT::::PREREPLACE::::" & i, 4)
                    nthreadtext = nthreadtext.Replace(codetext, Prelist(i))
                Next
                'Src
                For i As Integer = 0 To SrcMatch.Count - 1
                    Dim codetext As String = ColoredText("PERIODIBOT::::SRCREPLACE::::" & i, 4)
                    nthreadtext = nthreadtext.Replace(codetext, SrcList(i))
                Next
                'SynHL
                For i As Integer = 0 To SyntaxHlMatch.Count - 1
                    Dim codetext As String = ColoredText("PERIODIBOT::::SYNREPLACE::::" & i, 4)
                    nthreadtext = nthreadtext.Replace(codetext, SyntaxHlList(i))
                Next
                EndThreadList.Add(nthreadtext)
            Next

            Return EndThreadList.ToArray
        End Function

        ''' <summary>
        ''' Entrega como DateTime la última fecha (formato firma Wikipedia) en el último parrafo. Si no encuentra firma retorna 31/12/9999.
        ''' </summary>
        ''' <param name="text">Texto a evaluar</param>
        ''' <returns></returns>
        Public Shared Function LastParagraphDateTime(ByVal text As String) As DateTime
            If String.IsNullOrEmpty(text) Then
                Throw New ArgumentException("Empty parameter", NameOf(text))
            End If
            text = text.Trim(CType(vbCrLf, Char())) & " "
            Dim lastparagraph As String = Regex.Match(text, ".+(?=(([\n\r])==.+==)|$)").Value
            Dim matchc As MatchCollection = Regex.Matches(lastparagraph, signpattern)

            If matchc.Count = 0 And Not (((lastparagraph(0) = ";"c) Or (lastparagraph(0) = ":"c) Or (lastparagraph(0) = "*"c) Or (lastparagraph(0) = "#"c))) Then
                Dim mlines As MatchCollection = Regex.Matches(text, ".+\n")
                For i As Integer = mlines.Count - 1 To 0 Step -1

                    If i = (mlines.Count - 1) Then
                        If Not ((mlines(i).Value(0) = ";"c) Or (mlines(i).Value(0) = ":"c) Or (mlines(i).Value(0) = "*"c) Or (mlines(i).Value(0) = "#"c)) Then
                            If Regex.Match(mlines(i).Value, signpattern).Success Then
                                lastparagraph = mlines(i).Value
                                Exit For
                            End If
                        Else
                            Exit For
                        End If
                    Else
                        If Not ((mlines(i).Value(0) = ";"c) Or (mlines(i).Value(0) = ":"c) Or (mlines(i).Value(0) = "*"c) Or (mlines(i).Value(0) = "#"c)) Then
                            If Regex.Match(mlines(i).Value, signpattern).Success Then
                                lastparagraph = mlines(i).Value
                                Exit For
                            End If
                        Else
                            Exit For
                        End If
                    End If
                Next
            End If
            Dim TheDate As DateTime = ESWikiDatetime(lastparagraph)
            Return TheDate
        End Function

        ''' <summary>
        ''' Entrega la ultima fecha, que aparezca en un texto dado (si la fecha tiene formato de firma wikipedia).
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function ESWikiDatetime(ByVal text As String) As DateTime
            Dim TheDate As DateTime = Nothing
            Dim matchc As MatchCollection = Regex.Matches(text, signpattern)

            If matchc.Count = 0 Then
                Return New Date(9999, 12, 31, 23, 59, 59)
            End If

            For Each m As Match In matchc
                Try
                    Dim parsedtxt As String = m.Value.Replace(" "c, "/"c)
                    parsedtxt = parsedtxt.Replace(":"c, "/"c)
                    parsedtxt = parsedtxt.ToLower.Replace("ene", "01").Replace("feb", "02") _
                .Replace("mar", "03").Replace("abr", "04").Replace("may", "05") _
                .Replace("jun", "06").Replace("jul", "07").Replace("ago", "08") _
                .Replace("sep", "09").Replace("oct", "10").Replace("nov", "11") _
                .Replace("dic", "12")

                    parsedtxt = Regex.Replace(parsedtxt, "([^0-9/])", "")
                    Dim dates As New List(Of Integer)
                    For Each s As String In parsedtxt.Split("/"c)
                        If Not String.IsNullOrWhiteSpace(s) Then
                            dates.Add(Integer.Parse(RemoveAllAlphas(s)))
                        End If
                    Next
                    If Not (dates.Count = 5) Then Return New Date(9999, 12, 31, 23, 59, 59)
                    Dim dat As New DateTime(dates(4), dates(3), dates(2), dates(0), dates(1), 0)
                    TheDate = dat
                Catch ex As System.FormatException
                    Return New Date(9999, 12, 31, 23, 59, 59)
                End Try
            Next
            Return TheDate
        End Function


        ''' <summary>
        ''' Entrega como DateTime la fecha más reciente en el texto dado (en formato de firma wikipedia).
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function MostRecentDate(ByVal text As String) As DateTime
            Dim dates As New List(Of DateTime)
            Dim matchc As MatchCollection = Regex.Matches(text, signpattern)

            If matchc.Count = 0 Then
                Return New DateTime(9999, 12, 31, 23, 59, 59)
            End If

            For Each m As Match In matchc
                Try
                    Dim parsedtxt As String = m.Value.Replace(" "c, "/"c)
                    parsedtxt = parsedtxt.Replace(":"c, "/"c)
                    parsedtxt = parsedtxt.ToLower.Replace("ene", "01").Replace("feb", "02") _
                .Replace("mar", "03").Replace("abr", "04").Replace("may", "05") _
                .Replace("jun", "06").Replace("jul", "07").Replace("ago", "08") _
                .Replace("sep", "09").Replace("oct", "10").Replace("nov", "11") _
                .Replace("dic", "12").Replace("jan", "01").Replace("apr", "04") _
                .Replace("aug", "08").Replace("dec", "12")

                    parsedtxt = Regex.Replace(parsedtxt, "([^0-9/])", "")
                    Dim datesInt As New List(Of Integer)
                    For Each s As String In parsedtxt.Split("/"c)
                        If Not String.IsNullOrWhiteSpace(s) Then
                            datesInt.Add(Integer.Parse(s))
                        End If
                    Next
                    Dim dat As New DateTime(datesInt(4), datesInt(3), datesInt(2), datesInt(0), datesInt(1), 0)
                    dates.Add(dat)
                Catch ex As System.FormatException
                Catch ex2 As System.ArgumentOutOfRangeException
                End Try
            Next
            dates.Sort()
            Return dates.Last
        End Function

        ''' <summary>
        ''' Entrega como DateTime la primera fecha que aparece en el hilo.
        ''' </summary>
        ''' <param name="text"></param>
        ''' <returns></returns>
        Public Shared Function FirstDate(ByVal text As String) As DateTime
            Dim matchc As MatchCollection = Regex.Matches(text, signpattern)
            Dim tdat As New DateTime(9999, 12, 31, 23, 59, 59)
            If matchc.Count = 0 Then
                Return tdat
            End If

            For Each m As Match In matchc
                Try
                    Dim parsedtxt As String = m.Value.Replace(" "c, "/"c)
                    parsedtxt = parsedtxt.Replace(":"c, "/"c)
                    parsedtxt = parsedtxt.ToLower.Replace("ene", "01").Replace("feb", "02") _
                    .Replace("mar", "03").Replace("abr", "04").Replace("may", "05") _
                    .Replace("jun", "06").Replace("jul", "07").Replace("ago", "08") _
                    .Replace("sep", "09").Replace("oct", "10").Replace("nov", "11") _
                    .Replace("dic", "12")

                    parsedtxt = Regex.Replace(parsedtxt, "([^0-9/])", "")
                    Dim datesInt As New List(Of Integer)
                    For Each s As String In parsedtxt.Split("/"c)
                        If Not String.IsNullOrWhiteSpace(s) Then
                            datesInt.Add(Integer.Parse(s))
                        End If
                    Next
                    tdat = New DateTime(datesInt(4), datesInt(3), datesInt(2), datesInt(0), datesInt(1), 0)
                Catch ex As System.FormatException
                Catch ex2 As System.ArgumentOutOfRangeException
                End Try
                Return tdat
            Next
            Return tdat
        End Function

        Public Shared Function GetSpanishTimeString(ByVal tDate As Date) As String
            Dim cinfo As Globalization.CultureInfo = New System.Globalization.CultureInfo("es-ES")
            Dim mstring As String = cinfo.DateTimeFormat.GetAbbreviatedMonthName(tDate.Month)
            If mstring.Length > 3 Then
                mstring = mstring.Substring(0, 3)
            End If
            Dim dstring As String = tDate.Hour.ToString("00") & ":" & tDate.Minute.ToString("00") & " " & tDate.Day.ToString & " " & mstring & " " & tDate.Year.ToString & " (UTC)"
            Return dstring
        End Function

        Public Shared Function GetSubArray(ByVal str As String(), index As Integer) As String()
            If str.Count = 0 Then Return str
            Dim l As New List(Of String)
            For i As Integer = index To str.Count - 1
                l.Add(str(i))
            Next
            Return l.ToArray
        End Function

        Public Shared Function SplitStringIntoChunks(ByVal str As String, size As Integer) As String()
            Return Enumerable.Range(0, CInt(Math.Round(str.Length / size))).Select(Function(x) str.Substring(x * size, size)).ToArray()
        End Function

        ''' <summary>
        ''' Convierte una cadena de texto ed fecha con el formato que entrega MediaWiki a un objeto date
        ''' </summary>
        ''' <param name="timestamp"></param>
        ''' <returns></returns>
        Public Shared Function GetDateFromMWTimestamp(ByVal timestamp As String) As Date
            timestamp = timestamp.Replace("-"c, " "c).Replace("T"c, " "c).Replace(":"c, " "c).Replace("Z", "")
            Dim timeArray As String() = timestamp.Split(" "c)
            Dim year As Integer = Integer.Parse(timeArray(0))
            Dim month As Integer = Integer.Parse(timeArray(1))
            Dim day As Integer = Integer.Parse(timeArray(2))
            Dim hour As Integer = Integer.Parse(timeArray(3))
            Dim minute As Integer = Integer.Parse(timeArray(4))
            Dim second As Integer = Integer.Parse(timeArray(5))
            Dim parsedDate As Date = New Date(year, month, day, hour, minute, second)
            Return parsedDate
        End Function

        Public Shared Function GetTemplate(ByVal text As String, templatename As String, removenamespace As Boolean) As Template
            If removenamespace Then
                If templatename.Contains(":"c) Then
                    templatename = templatename.Split(":"c)(1)
                End If
            End If
            Return GetTemplate(text, templatename)
        End Function

        Public Shared Function GetTemplate(ByVal text As String, templatename As String) As Template
            Dim tlist As List(Of Template) = Template.GetTemplates(text)
            For Each t As Template In tlist
                If t.Valid AndAlso (t.Name.Trim.Substring(0, 1).ToUpper & t.Name.Trim.Substring(1).ToLower) = (templatename.Trim.Substring(0, 1).ToUpper & templatename.Trim.Substring(1).ToLower) Then
                    Return t
                End If
            Next
            Return New Template
        End Function

        Public Shared Function IsTemplatePresent(ByVal text As String, templatename As String) As Boolean
            Return IsTemplatePresent(text, templatename, True)
        End Function

        Public Shared Function IsTemplatePresent(ByVal text As String, templatename As String, removenamespace As Boolean) As Boolean
            If removenamespace Then
                If templatename.Contains(":") Then
                    templatename = templatename.Split(":"c)(1).Trim
                End If
            End If
            Dim tlist As List(Of Template) = Template.GetTemplates(text)
            For Each t As Template In tlist
                If t.Valid AndAlso (t.Name.Trim.Substring(0, 1).ToUpper & t.Name.Trim.Substring(1).ToLower) = (templatename.Trim.Substring(0, 1).ToUpper & templatename.Trim.Substring(1).ToLower) Then
                    Return True
                End If
            Next
            Return False
        End Function

        ''' <summary>
        ''' Entrega el comienzo de la plantilla (sin su espacio de nombres) si se encuentra en el texto
        ''' </summary>
        ''' <param name="text">Texto a analizar</param>
        ''' <param name="PageName">Nombre de la plantilla (con su espacio de nombres, para funcionar correctamente debe estar en el espacio de nombres "Template" o su equivalente en la wiki.</param>
        ''' <returns></returns>
        Public Shared Function GetTemplateBeggining(ByVal text As String, PageName As String) As String
            Dim templatelist As List(Of Template) = Template.GetTemplates(text)
            For Each temp As Template In templatelist
                Dim PageNameWithoutNamespace As String = PageName.Split(":"c)(1).Trim
                Dim PageNameRegex As String = "[" & PageNameWithoutNamespace.Substring(0, 1).ToUpper & PageNameWithoutNamespace.Substring(0, 1).ToLower & "]" & PageNameWithoutNamespace.Substring(1)
                Dim templateregex As String = "{{ *" & PageNameRegex & " *"
                Dim IsPresent As Boolean = Regex.Match(temp.Text, templateregex).Success
                If IsPresent Then
                    Return Regex.Match(temp.Text, templateregex).Value
                End If
            Next
            Return String.Empty
        End Function

#End Region

    End Class
End Namespace
