﻿Option Strict On
Option Explicit On
Imports System.Globalization

Namespace WikiBot

    Public Class WikiUser
        Private _workerBot As Bot

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
        Public ReadOnly Property UserPage As Page


        Public ReadOnly Property IsBot As Boolean
            Get
                If _groups.Contains("bot") Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property
#End Region

        Sub New(ByRef wikiBot As Bot, userName As String)
            _userName = userName
            _workerBot = wikiBot
            LoadInfo()
            _lastEdit = GetLastEditTimestampUser(_userName)
        End Sub

        Sub LoadInfo()
            Dim queryresponse As String = _workerBot.POSTQUERY(SStrings.LoadUserQuery & _userName)
            Dim fequeryresponse As String = _workerBot.POSTQUERY(SStrings.UserFirstEditQuery & _userName)
            Try
                _userName = Utils.NormalizeUnicodetext(Utils.TextInBetween(queryresponse, """name"":""", """")(0))

                If queryresponse.Contains("""missing"":""""") Then
                    _exists = False
                    Exit Sub
                Else
                    _exists = True
                End If

                _talkPage = _workerBot.Getpage("Usuario discusión:" & _userName)
                _userPage = _workerBot.Getpage("Usuario:" & _userName)
                _userId = Integer.Parse(Utils.TextInBetween(queryresponse, """userid"":", ",")(0))
                _editCount = Integer.Parse(Utils.TextInBetween(queryresponse, """editcount"":", ",")(0))

                Try
                    Dim registrationString As String = Utils.TextInBetween(queryresponse, """registration"":""", """")(0).Replace("-"c, "").Replace("T"c, "").Replace("Z"c, "").Replace(":"c, "")
                    _FirstEdit = Date.ParseExact(Utils.TextInBetween(fequeryresponse, """timestamp"":""", """")(0).Replace("-"c, "").Replace("T"c, "").Replace("Z"c, "").Replace(":"c, ""), "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                    _registration = Date.ParseExact(registrationString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                Catch ex As IndexOutOfRangeException
                    'En caso de usuarios tan antiguos que la API no regresa la fecha de ingreso.
                    _registration = New Date(2004, 1, 1, 0, 0, 0)
                    _FirstEdit = New Date(2004, 1, 1, 0, 0, 0)
                End Try
                _groups.AddRange(Utils.TextInBetween(queryresponse, """groups"":[", "],")(0).Split(","c).Select(Of String)(New Func(Of String, String)(Function(x) x.Replace("""", ""))).ToArray)
                _gender = Utils.TextInBetween(queryresponse, """gender"":""", """")(0)



                If queryresponse.Contains("blockid") Then
                    _blocked = True
                    _blockID = Integer.Parse(Utils.TextInBetween(queryresponse, """blockid"":", ",")(0))
                    _blockedTimestamp = Utils.TextInBetween(queryresponse, """blockedtimestamp"":""", """")(0)
                    _blockedBy = Utils.NormalizeUnicodetext(Utils.TextInBetween(queryresponse, """blockedby"":""", """")(0))
                    _blockedbyId = Integer.Parse(Utils.TextInBetween(queryresponse, """blockedbyid"":", ",")(0))
                    _blockReason = Utils.NormalizeUnicodetext(Utils.TextInBetween(queryresponse, """blockreason"":""", """")(0))
                    _blockExpiry = Utils.TextInBetween(queryresponse, """blockexpiry"":""", """")(0)
                End If

            Catch ex As IndexOutOfRangeException
                Utils.EventLogger.EX_Log(ex.Message, Reflection.MethodBase.GetCurrentMethod().Name)
            End Try

        End Sub

        ''' <summary>
        ''' Entrega como DateTime la fecha de la última edición del usuario entregado como parámetro.
        ''' </summary>
        ''' <param name="user">Nombre exacto del usuario</param>
        ''' <returns></returns>
        Function GetLastEditTimestampUser(ByVal user As String) As DateTime
            user = Utils.UrlWebEncode(user)
            Dim qtest As String = _workerBot.POSTQUERY(SStrings.LastUserEditQuery & user)

            If qtest.Contains("""usercontribs"":[]") Then
                Dim fec As DateTime = DateTime.ParseExact("1111-11-11|11:11:11", "yyyy-MM-dd|HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                Return fec
            Else
                Try
                    Dim timestring As String = Utils.TextInBetween(qtest, """timestamp"":""", """,")(0).Replace("T", "|").Replace("Z", String.Empty)
                    Dim fec As DateTime = DateTime.ParseExact(timestring, "yyyy-MM-dd|HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                    Return fec
                Catch ex As IndexOutOfRangeException
                    Dim fec As DateTime = DateTime.ParseExact("1111-11-11|11:11:11", "yyyy-MM-dd|HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                    Return fec
                End Try

            End If

        End Function

    End Class

End Namespace