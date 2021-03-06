﻿Option Strict On
Option Explicit On
Imports System.Globalization
Imports MWBot.net.Utility
Imports MWBot.net.Utility.Utils
Imports MWBot.net.GlobalVars
Imports MWBot.net.My.Resources

Namespace WikiBot

    Public Class WikiUser
        Private Property WorkerBot As Bot

#Region "Properties"
        Public ReadOnly Property UserName As String
        Public ReadOnly Property EditCount As Integer
        Public ReadOnly Property Registration As Date
        Public ReadOnly Property FirstEdit As Date
        Public ReadOnly Property Groups As List(Of String)
        Public ReadOnly Property Blocked As Boolean
        Public ReadOnly Property BlockedBy As String
        Public ReadOnly Property BlockReason As String
        Public ReadOnly Property BlockedTimestamp As String
        Public ReadOnly Property BlockExpiry As String
        Public ReadOnly Property BlockId As Integer
        Public ReadOnly Property Exists As Boolean
        Public ReadOnly Property BlockedById As Integer
        Public ReadOnly Property LastEdit As Date
        Public ReadOnly Property UserId As Integer
        Public ReadOnly Property Gender As String
        Public ReadOnly Property TalkPage As Page
            Get
                Return WorkerBot.Getpage("User talk:" & _UserName)
            End Get
        End Property
        Public ReadOnly Property UserPage As Page
            Get
                Return WorkerBot.Getpage("User:" & _UserName)
            End Get
        End Property
        Public ReadOnly Property IsBot As Boolean
            Get
                If (_Groups IsNot Nothing) AndAlso _Groups.Contains("bot") Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property
#End Region

        Sub New(ByRef wikiBot As Bot, userName As String)
            _UserName = userName
            WorkerBot = wikiBot
            LoadInfo()
            _LastEdit = GetLastEditTimestampUser(_UserName)
        End Sub

        Sub New()
            Exists = False
        End Sub

        Sub LoadInfo()
            Dim queryresponse As String = WorkerBot.POSTQUERY(SStrings.LoadUserQuery & _UserName)
            Dim fequeryresponse As String = WorkerBot.POSTQUERY(SStrings.UserFirstEditQuery & _UserName)
            Try
                _UserName = NormalizeUnicodetext(TextInBetween(queryresponse, """name"":""", """")(0))

                If queryresponse.Contains("""missing"":""""") Or queryresponse.Contains(",""invalid"":""""}") Then
                    _Exists = False
                    Exit Sub
                Else
                    _Exists = True
                End If

                _UserId = Integer.Parse(TextInBetween(queryresponse, """userid"":", ",")(0))
                _EditCount = Integer.Parse(TextInBetween(queryresponse, """editcount"":", ",")(0))

                Try
                    Dim registrationString As String = TextInBetween(queryresponse, """registration"":""", """")(0).Replace("-"c, "").Replace("T"c, "").Replace("Z"c, "").Replace(":"c, "")
                    _Registration = Date.ParseExact(registrationString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                Catch ex As IndexOutOfRangeException
                    'En caso de usuarios tan antiguos que la API no regresa la fecha de ingreso.
                    _Registration = New Date(2004, 1, 1, 0, 0, 0)
                End Try

                Try
                    _FirstEdit = Date.ParseExact(TextInBetween(fequeryresponse, """timestamp"":""", """")(0).Replace("-"c, "").Replace("T"c, "").Replace("Z"c, "").Replace(":"c, ""), "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                Catch ex As IndexOutOfRangeException
                    _FirstEdit = Nothing
                End Try


                _Groups = TextInBetween(queryresponse, """groups"":[", "],")(0).Split(","c).Select(Of String)(New Func(Of String, String)(Function(x) x.Replace("""", ""))).ToList
                _Gender = TextInBetween(queryresponse, """gender"":""", """")(0)

                If queryresponse.Contains("blockid") Then
                    _Blocked = True
                    _BlockId = Integer.Parse(TextInBetween(queryresponse, """blockid"":", ",")(0))
                    _BlockedTimestamp = TextInBetween(queryresponse, """blockedtimestamp"":""", """")(0)
                    _BlockedBy = NormalizeUnicodetext(TextInBetween(queryresponse, """blockedby"":""", """")(0))
                    _BlockedById = Integer.Parse(TextInBetween(queryresponse, """blockedbyid"":", ",")(0))
                    _BlockReason = NormalizeUnicodetext(TextInBetween(queryresponse, """blockreason"":""", """")(0))
                    _BlockExpiry = TextInBetween(queryresponse, """blockexpiry"":""", """")(0)
                End If

            Catch ex As IndexOutOfRangeException
                EventLogger.EX_Log(ex.Message, Reflection.MethodBase.GetCurrentMethod().Name)
            End Try

        End Sub

        ''' <summary>
        ''' Entrega como Date la fecha de la última edición del usuario entregado como parámetro.
        ''' </summary>
        ''' <param name="user">Nombre exacto del usuario</param>
        ''' <returns></returns>
        Private Function GetLastEditTimestampUser(ByVal user As String) As Date
            user = UrlWebEncode(user)
            Dim qtest As String = WorkerBot.POSTQUERY(SStrings.LastUserEditQuery & user)
            If qtest.Contains("""usercontribs"":[]") Then
                Return Nothing
            Else
                Try
                    Dim timestring As String = TextInBetween(qtest, """timestamp"":""", """,")(0).Replace("T", "|").Replace("Z", String.Empty)
                    Dim fec As Date = Date.ParseExact(timestring, "yyyy-MM-dd|HH:mm:ss", CultureInfo.InvariantCulture)
                    Return fec
                Catch ex As IndexOutOfRangeException
                    Return Nothing
                End Try

            End If

        End Function

    End Class

End Namespace