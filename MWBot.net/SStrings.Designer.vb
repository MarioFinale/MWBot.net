﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Public Class SStrings
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Public Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("MWBot.net.SStrings", GetType(SStrings).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Public Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=edit{0}&amp;title={1}&amp;summary={2}&amp;section=new&amp;sectiontitle={3}&amp;text={4}&amp;token={5}.
        '''</summary>
        Public Shared ReadOnly Property AddThread() As String
            Get
                Return ResourceManager.GetString("AddThread", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Destino.
        '''</summary>
        Public Shared ReadOnly Property ArchiveDestiny() As String
            Get
                Return ResourceManager.GetString("ArchiveDestiny", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to /Archivo-00-índice.
        '''</summary>
        Public Shared ReadOnly Property ArchiveIndex() As String
            Get
                Return ResourceManager.GetString("ArchiveIndex", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to action=query&amp;assert=bot&amp;format=json.
        '''</summary>
        Public Shared ReadOnly Property AssertBotData() As String
            Get
                Return ResourceManager.GetString("AssertBotData", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to assertbotfailed.
        '''</summary>
        Public Shared ReadOnly Property AssertBotFailed() As String
            Get
                Return ResourceManager.GetString("AssertBotFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to action=query&amp;assert=user&amp;format=json.
        '''</summary>
        Public Shared ReadOnly Property AssertUserData() As String
            Get
                Return ResourceManager.GetString("AssertUserData", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to assertuserfailed.
        '''</summary>
        Public Shared ReadOnly Property AssertUserFailed() As String
            Get
                Return ResourceManager.GetString("AssertUserFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to  *[Ff]irma automática.
        '''</summary>
        Public Shared ReadOnly Property AutoSignatureTemplateInsideRegex() As String
            Get
                Return ResourceManager.GetString("AutoSignatureTemplateInsideRegex", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to &lt;!-- Caja generada por PeriodiBOT, puedes editarla cuanto quieras, pero los nuevos enlaces siempre se añadirán al final. --&gt;.
        '''</summary>
        Public Shared ReadOnly Property BoxMessage() As String
            Get
                Return ResourceManager.GetString("BoxMessage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to ======================CONFIG======================
        '''BOTName=&quot;{0}&quot;
        '''WPUserName=&quot;{1}&quot;
        '''WPBotPassword=&quot;{2}&quot;
        '''PageURL=&quot;{3}&quot;
        '''ApiURL=&quot;{4}&quot;.
        '''</summary>
        Public Shared ReadOnly Property ConfigTemplate() As String
            Get
                Return ResourceManager.GetString("ConfigTemplate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Días a mantener.
        '''</summary>
        Public Shared ReadOnly Property DaysTokeep() As String
            Get
                Return ResourceManager.GetString("DaysTokeep", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to PeriodiBOT/Sabias que.
        '''</summary>
        Public Shared ReadOnly Property DidYouKnowPageName() As String
            Get
                Return ResourceManager.GetString("DidYouKnowPageName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;meta=tokens.
        '''</summary>
        Public Shared ReadOnly Property EditToken() As String
            Get
                Return ResourceManager.GetString("EditToken", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=compare&amp;fromrev={0}&amp;torev={1}.
        '''</summary>
        Public Shared ReadOnly Property GetDiffQuery() As String
            Get
                Return ResourceManager.GetString("GetDiffQuery", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;prop=revisions&amp;titles=.
        '''</summary>
        Public Shared ReadOnly Property GetLastRevIds() As String
            Get
                Return ResourceManager.GetString("GetLastRevIds", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;maxlag=5&amp;action=query&amp;prop=revisions&amp;rvprop=timestamp&amp;titles={0}.
        '''</summary>
        Public Shared ReadOnly Property GetLastTimestamp() As String
            Get
                Return ResourceManager.GetString("GetLastTimestamp", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to https://ores.wikimedia.org/v3/scores/eswiki/?models=damaging|goodfaith&amp;format=json&amp;revids=.
        '''</summary>
        Public Shared ReadOnly Property GetOresScore() As String
            Get
                Return ResourceManager.GetString("GetOresScore", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;list=embeddedin&amp;eilimit=1000&amp;eititle=.
        '''</summary>
        Public Shared ReadOnly Property GetPageInclusions() As String
            Get
                Return ResourceManager.GetString("GetPageInclusions", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;prop=extracts&amp;exintro=&amp;explaintext=&amp;titles=.
        '''</summary>
        Public Shared ReadOnly Property GetPagesExtract() As String
            Get
                Return ResourceManager.GetString("GetPagesExtract", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;formatversion=2&amp;prop=pageimages&amp;titles=.
        '''</summary>
        Public Shared ReadOnly Property GetPagesImage() As String
            Get
                Return ResourceManager.GetString("GetPagesImage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;list=categorymembers&amp;cmlimit=500&amp;cmtitle=.
        '''</summary>
        Public Shared ReadOnly Property GetPagesInCategory() As String
            Get
                Return ResourceManager.GetString("GetPagesInCategory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to https://wikimedia.org/api/rest_v1/metrics/pageviews/per-article/{0}/all-access/all-agents/{1}/daily/{2}{4}{6}00/{3}{5}{7}00.
        '''</summary>
        Public Shared ReadOnly Property GetPageViews() As String
            Get
                Return ResourceManager.GetString("GetPageViews", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;list=search&amp;srwhat=text&amp;srlimit=500&amp;srsearch=.
        '''</summary>
        Public Shared ReadOnly Property GetTextInclusions() As String
            Get
                Return ResourceManager.GetString("GetTextInclusions", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;meta=tokens&amp;type=login.
        '''</summary>
        Public Shared ReadOnly Property GetWikiToken() As String
            Get
                Return ResourceManager.GetString("GetWikiToken", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to ¡Hola! Este mensaje es un aviso automático a todos los miembros activos de [[Wikipedia:Mediación informal/Participantes/Lista|Mediación informal]] para informar de una [[Wikipedia:Mediación informal/Solicitudes|nueva solicitud]]. Por favor, considera participar en la discusión.
        '''
        '''¡Gracias por tu atención! ~~~~.
        '''</summary>
        Public Shared ReadOnly Property InfMedMsg() As String
            Get
                Return ResourceManager.GetString("InfMedMsg", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Wikipedia:Mediación informal/Solicitudes.
        '''</summary>
        Public Shared ReadOnly Property InfMedPage() As String
            Get
                Return ResourceManager.GetString("InfMedPage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to InformalMediationLastThreadCount.
        '''</summary>
        Public Shared ReadOnly Property InfMedSettingsName() As String
            Get
                Return ResourceManager.GetString("InfMedSettingsName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to (Bot) Aviso automático de una nueva solicitud en [[Wikipedia:Mediación informal/Participantes/Lista|Mediación informal]]..
        '''</summary>
        Public Shared ReadOnly Property InfMedSumm() As String
            Get
                Return ResourceManager.GetString("InfMedSumm", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Atención en [[Wikipedia:Mediación informal/Solicitudes|Mediación informal]].
        '''</summary>
        Public Shared ReadOnly Property InfMedTitle() As String
            Get
                Return ResourceManager.GetString("InfMedTitle", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Wikipedia:Mediación informal/Participantes/Lista.
        '''</summary>
        Public Shared ReadOnly Property InformalMediationMembers() As String
            Get
                Return ResourceManager.GetString("InformalMediationMembers", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to IRC.
        '''</summary>
        Public Shared ReadOnly Property IrcSource() As String
            Get
                Return ResourceManager.GetString("IrcSource", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to MantenerCajaDeArchivos.
        '''</summary>
        Public Shared ReadOnly Property KeepFileBox() As String
            Get
                Return ResourceManager.GetString("KeepFileBox", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to FirmaEnÚltimoPárrafo.
        '''</summary>
        Public Shared ReadOnly Property LastPSignature() As String
            Get
                Return ResourceManager.GetString("LastPSignature", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to action=query&amp;list=usercontribs&amp;uclimit=1&amp;format=json&amp;ucuser=.
        '''</summary>
        Public Shared ReadOnly Property LastUserEditQuery() As String
            Get
                Return ResourceManager.GetString("LastUserEditQuery", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to action=query&amp;format=json&amp;list=users&amp;usprop=blockinfo|groups|editcount|registration|gender&amp;ususers=.
        '''</summary>
        Public Shared ReadOnly Property LoadUserQuery() As String
            Get
                Return ResourceManager.GetString("LoadUserQuery", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to LOCAL.
        '''</summary>
        Public Shared ReadOnly Property LocalSource() As String
            Get
                Return ResourceManager.GetString("LocalSource", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=login&amp;lgname={0}&amp;lgpassword={1}&amp;lgdomain=&amp;lgtoken={2}.
        '''</summary>
        Public Shared ReadOnly Property Login() As String
            Get
                Return ResourceManager.GetString("Login", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Usuario:PeriodiBOT/Curiosidades/Hilos más largos en la historia del café.
        '''</summary>
        Public Shared ReadOnly Property LongestThreads() As String
            Get
                Return ResourceManager.GetString("LongestThreads", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to FirmaMásRecienteEnLaSección.
        '''</summary>
        Public Shared ReadOnly Property MostRecentSignature() As String
            Get
                Return ResourceManager.GetString("MostRecentSignature", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to https://ores.wikimedia.org/v3/scores/eswiki/?models=damaging|goodfaith&amp;format=json&amp;revids=.
        '''</summary>
        Public Shared ReadOnly Property OresScoresApiQueryUrl() As String
            Get
                Return ResourceManager.GetString("OresScoresApiQueryUrl", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;maxlag=5&amp;action=query&amp;prop=revisions|pageimages|categories|extracts&amp;rvprop=user|timestamp|size|comment|content|ids&amp;rvslots=main&amp;exlimit=1&amp;explaintext&amp;exintro&amp;titles={0}.
        '''</summary>
        Public Shared ReadOnly Property PageInfo() As String
            Get
                Return ResourceManager.GetString("PageInfo", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;maxlag=5&amp;action=query&amp;prop=revisions|pageimages|categories|extracts&amp;rvprop=user|timestamp|size|comment|content|ids&amp;rvslots=main&amp;exlimit=1&amp;explaintext&amp;exintro&amp;revids={0}.
        '''</summary>
        Public Shared ReadOnly Property PageInfoRevid() As String
            Get
                Return ResourceManager.GetString("PageInfoRevid", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;pslimit=max&amp;list=prefixsearch&amp;pssearch=.
        '''</summary>
        Public Shared ReadOnly Property PrefixSearchQuery() As String
            Get
                Return ResourceManager.GetString("PrefixSearchQuery", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;generator=random&amp;grnnamespace={0}&amp;grnlimit=1.
        '''</summary>
        Public Shared ReadOnly Property RandomPageQuery() As String
            Get
                Return ResourceManager.GetString("RandomPageQuery", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Usuario:PeriodiBOT/Resumen página.
        '''</summary>
        Public Shared ReadOnly Property ResumePageName() As String
            Get
                Return ResourceManager.GetString("ResumePageName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;assert=user&amp;action=edit{0}&amp;title={1}&amp;summary={2}&amp;text={3}&amp;token={4}.
        '''</summary>
        Public Shared ReadOnly Property SavePage() As String
            Get
                Return ResourceManager.GetString("SavePage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;list=search&amp;utf8=1&amp;srsearch=.
        '''</summary>
        Public Shared ReadOnly Property Search() As String
            Get
                Return ResourceManager.GetString("Search", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;list=search&amp;utf8=1&amp;srnamespace=2&amp;srsearch=.
        '''</summary>
        Public Shared ReadOnly Property SearchForUser() As String
            Get
                Return ResourceManager.GetString("SearchForUser", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Estrategia.
        '''</summary>
        Public Shared ReadOnly Property Strategy() As String
            Get
                Return ResourceManager.GetString("Strategy", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Wikipedia:Café por tema/Grupos.
        '''</summary>
        Public Shared ReadOnly Property TopicGroupsPage() As String
            Get
                Return ResourceManager.GetString("TopicGroupsPage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Wikipedia:Café por tema.
        '''</summary>
        Public Shared ReadOnly Property TopicPageName() As String
            Get
                Return ResourceManager.GetString("TopicPageName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Plantilla:Tema.
        '''</summary>
        Public Shared ReadOnly Property TopicTemplate() As String
            Get
                Return ResourceManager.GetString("TopicTemplate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to format=json&amp;action=query&amp;uclimit=1&amp;ucdir=newer&amp;list=usercontribs&amp;ucuser=.
        '''</summary>
        Public Shared ReadOnly Property UserFirstEditQuery() As String
            Get
                Return ResourceManager.GetString("UserFirstEditQuery", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Avisar al archivar.
        '''</summary>
        Public Shared ReadOnly Property WarnArchiving() As String
            Get
                Return ResourceManager.GetString("WarnArchiving", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to si.
        '''</summary>
        Public Shared ReadOnly Property YES() As String
            Get
                Return ResourceManager.GetString("YES", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to sí.
        '''</summary>
        Public Shared ReadOnly Property YES2() As String
            Get
                Return ResourceManager.GetString("YES2", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
