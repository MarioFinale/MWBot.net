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
    Public Class Messages
        
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
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("MWBot.net.Messages", GetType(Messages).Assembly)
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
        '''  Looks up a localized string similar to Filtro de abusos desencadenado en &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property AbuseFilter() As String
            Get
                Return ResourceManager.GetString("AbuseFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to MWBot.net.
        '''</summary>
        Public Shared ReadOnly Property BotEngine() As String
            Get
                Return ResourceManager.GetString("BotEngine", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Verificando al usuario &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property CheckingUser() As String
            Get
                Return ResourceManager.GetString("CheckingUser", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Comprobando usuarios..
        '''</summary>
        Public Shared ReadOnly Property CheckingUsers() As String
            Get
                Return ResourceManager.GetString("CheckingUsers", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Archivo de configuraciones corrupto..
        '''</summary>
        Public Shared ReadOnly Property ConfigError() As String
            Get
                Return ResourceManager.GetString("ConfigError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Edición de bot..
        '''</summary>
        Public Shared ReadOnly Property DefaultSumm() As String
            Get
                Return ResourceManager.GetString("DefaultSumm", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Parte de la información en la página &apos;{0}&apos; fue eliminada u ocultada..
        '''</summary>
        Public Shared ReadOnly Property DeletedInfoMessage() As String
            Get
                Return ResourceManager.GetString("DeletedInfoMessage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Hecho, retornando {0} páginas..
        '''</summary>
        Public Shared ReadOnly Property DoneXPagesReturned() As String
            Get
                Return ResourceManager.GetString("DoneXPagesReturned", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Conflicto de edición en &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property EditConflict() As String
            Get
                Return ResourceManager.GetString("EditConflict", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error al crear el archivo &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property FileCreateErr() As String
            Get
                Return ResourceManager.GetString("FileCreateErr", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error al guardar el archivo &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property FileSaveErr() As String
            Get
                Return ResourceManager.GetString("FileSaveErr", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Obteniendo el ultimo revid de {0} páginas..
        '''</summary>
        Public Shared ReadOnly Property GetLastrevIDs() As String
            Get
                Return ResourceManager.GetString("GetLastrevIDs", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cargando el extracto de la página &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property GetPageExtract() As String
            Get
                Return ResourceManager.GetString("GetPageExtract", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cargando {0} extractos de páginas..
        '''</summary>
        Public Shared ReadOnly Property GetPagesExtract() As String
            Get
                Return ResourceManager.GetString("GetPagesExtract", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to ==================== MWBot.net {0} ====================.
        '''</summary>
        Public Shared ReadOnly Property GreetingMsg() As String
            Get
                Return ResourceManager.GetString("GreetingMsg", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to La url ingresada es inválida.
        '''</summary>
        Public Shared ReadOnly Property InvalidUrl() As String
            Get
                Return ResourceManager.GetString("InvalidUrl", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to El usuario &apos;{0}&apos; es inválido..
        '''</summary>
        Public Shared ReadOnly Property InvalidUser() As String
            Get
                Return ResourceManager.GetString("InvalidUser", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cargando configuraciones.
        '''</summary>
        Public Shared ReadOnly Property LoadingConfig() As String
            Get
                Return ResourceManager.GetString("LoadingConfig", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cargando puntaje ORES del revid {0}..
        '''</summary>
        Public Shared ReadOnly Property LoadingOres() As String
            Get
                Return ResourceManager.GetString("LoadingOres", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Falló el inicio de sesión.
        '''</summary>
        Public Shared ReadOnly Property LoginError() As String
            Get
                Return ResourceManager.GetString("LoginError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to ID de usuario: .
        '''</summary>
        Public Shared ReadOnly Property LoginID() As String
            Get
                Return ResourceManager.GetString("LoginID", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Resultado: .
        '''</summary>
        Public Shared ReadOnly Property LoginResult() As String
            Get
                Return ResourceManager.GetString("LoginResult", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Máximo número de reintentos alcanzado en &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property MaxRetryCount() As String
            Get
                Return ResourceManager.GetString("MaxRetryCount", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error de conexión: .
        '''</summary>
        Public Shared ReadOnly Property NetworkError() As String
            Get
                Return ResourceManager.GetString("NetworkError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de la página de la plantilla de &apos;caja de archivos&apos; (con el espacio de nombres):.
        '''</summary>
        Public Shared ReadOnly Property NewArchiveBoxTemplate() As String
            Get
                Return ResourceManager.GetString("NewArchiveBoxTemplate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de la página de aviso de archivo (con el espacio de nombres):.
        '''</summary>
        Public Shared ReadOnly Property NewArchiveMessageTemplate() As String
            Get
                Return ResourceManager.GetString("NewArchiveMessageTemplate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de la página de la plantilla &apos;no archivar&apos; (con el espacio de nombres):.
        '''</summary>
        Public Shared ReadOnly Property NewAutoArchiveDoNotArchivePageName() As String
            Get
                Return ResourceManager.GetString("NewAutoArchiveDoNotArchivePageName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de la página de la plantilla de autoarchivado programado (con el espacio de nombres):.
        '''</summary>
        Public Shared ReadOnly Property NewAutoArchiveProgrammedArchivePageName() As String
            Get
                Return ResourceManager.GetString("NewAutoArchiveProgrammedArchivePageName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de la página de la plantilla de autoarchivado (con el espacio de nombres):.
        '''</summary>
        Public Shared ReadOnly Property NewAutoArchiveTemplatePagename() As String
            Get
                Return ResourceManager.GetString("NewAutoArchiveTemplatePagename", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de la página de la plantilla de autofirmado (con el espacio de nombres):.
        '''</summary>
        Public Shared ReadOnly Property NewAutoSignatureTemplatePageName() As String
            Get
                Return ResourceManager.GetString("NewAutoSignatureTemplatePageName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre del bot: .
        '''</summary>
        Public Shared ReadOnly Property NewBotName() As String
            Get
                Return ResourceManager.GetString("NewBotName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Contraseña de BOT en la Wiki: .
        '''</summary>
        Public Shared ReadOnly Property NewBotPassword() As String
            Get
                Return ResourceManager.GetString("NewBotPassword", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error al crear un nuevo archivo de configuraciones.
        '''</summary>
        Public Shared ReadOnly Property NewConfigFileError() As String
            Get
                Return ResourceManager.GetString("NewConfigFileError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to El archivo de configuraciones no existe, por favor completa los siguientes campos o cierra el programa y crea el archivo manualmente..
        '''</summary>
        Public Shared ReadOnly Property NewConfigMessage() As String
            Get
                Return ResourceManager.GetString("NewConfigMessage", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de usuario en la Wiki: .
        '''</summary>
        Public Shared ReadOnly Property NewUserName() As String
            Get
                Return ResourceManager.GetString("NewUserName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to URL de la API de la Wiki: .
        '''</summary>
        Public Shared ReadOnly Property NewWikiMainApiUrl() As String
            Get
                Return ResourceManager.GetString("NewWikiMainApiUrl", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to URL principal de la Wiki: .
        '''</summary>
        Public Shared ReadOnly Property NewWikiMainUrl() As String
            Get
                Return ResourceManager.GetString("NewWikiMainUrl", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Este bot no puede editar en &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property NoBots() As String
            Get
                Return ResourceManager.GetString("NoBots", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to No hay cambios por guardar en &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property NoChanges() As String
            Get
                Return ResourceManager.GetString("NoChanges", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to El archivo de configuraciones no existe.
        '''</summary>
        Public Shared ReadOnly Property NoConfigFile() As String
            Get
                Return ResourceManager.GetString("NoConfigFile", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to No se ha encontrado texto con formato de fecha..
        '''</summary>
        Public Shared ReadOnly Property NoDateMatch() As String
            Get
                Return ResourceManager.GetString("NoDateMatch", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to No se pudo obtener el puntaje ORES del revid {0}. EX: {1}.
        '''</summary>
        Public Shared ReadOnly Property OresFailed() As String
            Get
                Return ResourceManager.GetString("OresFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Puntaje ORES del revid {0}:  DMG:{1} /GF:{2}.
        '''</summary>
        Public Shared ReadOnly Property OresLoaded() As String
            Get
                Return ResourceManager.GetString("OresLoaded", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error de servidor en la query del puntaje ORES desde el revid {0} (diff inválido?).
        '''</summary>
        Public Shared ReadOnly Property OresQueryError() As String
            Get
                Return ResourceManager.GetString("OresQueryError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Excepción al obtener el puntaje ORES del revid {0}. EX: {1}.
        '''</summary>
        Public Shared ReadOnly Property OresQueryEx() As String
            Get
                Return ResourceManager.GetString("OresQueryEx", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Query del puntaje ORES del revid {0} completada. GF: {1} DMG: {2}.
        '''</summary>
        Public Shared ReadOnly Property OresQueryResult() As String
            Get
                Return ResourceManager.GetString("OresQueryResult", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cuidado: La página &apos;{0}&apos; no existe aún..
        '''</summary>
        Public Shared ReadOnly Property PageDoesNotExist() As String
            Get
                Return ResourceManager.GetString("PageDoesNotExist", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Página &apos;{0}&apos; cargada..
        '''</summary>
        Public Shared ReadOnly Property PageLoaded() As String
            Get
                Return ResourceManager.GetString("PageLoaded", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Aviso: La página &apos;{0}&apos; no posee una imagen de previsualización..
        '''</summary>
        Public Shared ReadOnly Property PageNoThumb() As String
            Get
                Return ResourceManager.GetString("PageNoThumb", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error en la solicitud POST en &apos;{0}&apos;. EX: {1}.
        '''</summary>
        Public Shared ReadOnly Property POSTEX() As String
            Get
                Return ResourceManager.GetString("POSTEX", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Presione cualquier tecla para salir.
        '''</summary>
        Public Shared ReadOnly Property PressKey() As String
            Get
                Return ResourceManager.GetString("PressKey", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Revid {0} cargado..
        '''</summary>
        Public Shared ReadOnly Property PRevLoaded() As String
            Get
                Return ResourceManager.GetString("PRevLoaded", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Motivo: .
        '''</summary>
        Public Shared ReadOnly Property Reason() As String
            Get
                Return ResourceManager.GetString("Reason", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Eliminando todas las referencias que contengan &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property RemovingRefs() As String
            Get
                Return ResourceManager.GetString("RemovingRefs", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Obteniendo token.
        '''</summary>
        Public Shared ReadOnly Property RequestingToken() As String
            Get
                Return ResourceManager.GetString("RequestingToken", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Error al guardar las configuraciones..
        '''</summary>
        Public Shared ReadOnly Property SaveConfigError() As String
            Get
                Return ResourceManager.GetString("SaveConfigError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Iniciando sesión.
        '''</summary>
        Public Shared ReadOnly Property SigninIn() As String
            Get
                Return ResourceManager.GetString("SigninIn", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Filtro de abusos anti spam desencadenado en &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property SpamBlackList() As String
            Get
                Return ResourceManager.GetString("SpamBlackList", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Edición en &apos;{0}&apos; exitosa..
        '''</summary>
        Public Shared ReadOnly Property SuccessfulEdit() As String
            Get
                Return ResourceManager.GetString("SuccessfulEdit", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Operación exitosa..
        '''</summary>
        Public Shared ReadOnly Property SuccessfulOperation() As String
            Get
                Return ResourceManager.GetString("SuccessfulOperation", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Bot: Reemplazando &apos;{0}&apos; con &apos;{1}&apos; {2}..
        '''</summary>
        Public Shared ReadOnly Property TextReplaced() As String
            Get
                Return ResourceManager.GetString("TextReplaced", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Token obtenido.
        '''</summary>
        Public Shared ReadOnly Property TokenObtained() As String
            Get
                Return ResourceManager.GetString("TokenObtained", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Intentando guardar &apos;{0}&apos;..
        '''</summary>
        Public Shared ReadOnly Property TryingToSave() As String
            Get
                Return ResourceManager.GetString("TryingToSave", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Excepción inesperada: {0}.
        '''</summary>
        Public Shared ReadOnly Property UnexpectedEX() As String
            Get
                Return ResourceManager.GetString("UnexpectedEX", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to La operación ha fallado..
        '''</summary>
        Public Shared ReadOnly Property UnsuccessfulOperation() As String
            Get
                Return ResourceManager.GetString("UnsuccessfulOperation", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to El usuario {0} está bloqueado..
        '''</summary>
        Public Shared ReadOnly Property UserBlocked() As String
            Get
                Return ResourceManager.GetString("UserBlocked", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to El usuario {0} está inactivo..
        '''</summary>
        Public Shared ReadOnly Property UserInactive() As String
            Get
                Return ResourceManager.GetString("UserInactive", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to El usuario {0} no existe..
        '''</summary>
        Public Shared ReadOnly Property UserInexistent() As String
            Get
                Return ResourceManager.GetString("UserInexistent", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to {0} acaba de editar..
        '''</summary>
        Public Shared ReadOnly Property UserJustEdited() As String
            Get
                Return ResourceManager.GetString("UserJustEdited", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nombre de usuario: .
        '''</summary>
        Public Shared ReadOnly Property UserName() As String
            Get
                Return ResourceManager.GetString("UserName", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
