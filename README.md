# MWBot.net
Base library to create tools for sites based on Mediawiki software. - Base para crear herramientas para sitios basados en el software Mediawiki.


# Simple usage examples - Ejemplos simples de uso

Loading a page, editing it and saving it. - Cargar una página, editarla y guardarla.

* C#

```C#
using System;
using MWBot.net;
using MWBot.net.WikiBot;

namespace MyProgram
{
    class Program
    {
        static void TestMethod()
        {
            string configPath = @"c:\Config.cfg";
            string logPath = @"c:\log.psv";
            string usersPath = @"c:\users.psv";
            string botName = "MyCoolBotName";

            SimpleLogger EventLogger = new SimpleLogger(logPath, usersPath, botName, true);
            Bot Workerbot = new Bot(configPath, EventLogger);
            
            Page examplepage = Workerbot.GetPage("Example page");
            string newtext = examplepage.Content.Replace("Text", "NewText");
            //Do something
            examplepage.Save(newtext, "Test edit using C#!"); 
        }
    }
}
```

* Visual Basic

```vbnet
Imports System
Imports MWBot.net
Imports MWBot.net.WikiBot

Namespace MyProgram
   Class Program   
      Public Shared Sub TestMethod()      
         Dim configPath As String = "c:\Config.cfg"
         Dim logPath As String = "c:\log.psv"
         Dim usersPath As String = "c:\users.psv"
         Dim botName As String = "MyCoolBotName"

         Dim EventLogger As New SimpleLogger(logPath, usersPath, botName, True)
         Dim Workerbot As Bot = New Bot(configPath, EventLogger)
        
         Dim examplepage as Page = Workerbot.Getpage("Example page")
         Dim newtext as String = examplepage.Content.Replace("Text", "Newtext")
         
         'Do something
         examplepage.Save(newtext, "Test edit using VB!")
      End Sub      
   End Class   
End Namespace
```
* Template manipulation on VB

```vbnet
Imports System
Imports MWBot.net
Imports MWBot.net.WikiBot

Namespace MyProgram
   Class Program   
      Public Shared Sub TestMethod()      
        Dim configPath As String = "c:\Config.cfg"
        Dim logPath As String = "c:\log.psv"
        Dim usersPath As String = "c:\users.psv"
        Dim botName As String = "MyCoolBotName"

        Dim EventLogger As New SimpleLogger(logPath, usersPath, botName, True)
        Dim Workerbot As Bot = New Bot(configPath, EventLogger)

        'Load page
        Dim examplePage As Page = Workerbot.Getpage("Example page")
        Dim pageContent As String = examplePage.Content

        'Do something
        Dim templateName As String = "Cool template name"
        Dim oldParameterName As String = "Old parameter name"
        Dim newParameterName As String = "New and cool parameter name"

        If Template.IsTemplatePresentInText(pageContent, templateName) Then
            Dim templates As List(Of Template) = Template.GetTemplates(pageContent)
            For Each t As Template In templates
                Dim originalTemplateText As String = t.Text
                If t.Name.Trim.Equals(templateName.Trim) Then
                    If t.ContainsParameter(oldParameterName) Then
                        t.ChangeNameOfParameter(oldParameterName, newParameterName) 'This will remove the parameter from the template object changing the Text property of it.
                    End If
                End If
                pageContent = pageContent.Replace(originalTemplateText, t.Text) 'Replace the old template text with the new template text
            Next
        End If

        Dim minorEdit As Boolean = False
        Dim botEdit As Boolean = True
        Dim replaceBlacklistedLinks As Boolean = True
        Dim editSummary As String = "Cool template manipulation"

        examplePage.Save(pageContent, editSummary, minorEdit, botEdit, replaceBlacklistedLinks)
      End Sub      
   End Class   
End Namespace
```

* Template Manipulation on C#

```C#
using System;
using MWBot.net;
using MWBot.net.WikiBot;
namespace MyProgram
{
    class Program
    {
        public static void TestMethod()
        {
            string configPath = @"c:\Config.cfg";
            string logPath = @"c:\log.psv";
            string usersPath = @"c:\users.psv";
            string botName = "MyCoolBotName";

            SimpleLogger EventLogger = new SimpleLogger(logPath, usersPath, botName, true);
            Bot Workerbot = new Bot(configPath, EventLogger);

            // Load page
            Page examplePage = Workerbot.Getpage("Example page");
            string pageContent = examplePage.Content;

            // Do something
            string templateName = "Cool template name";
            string oldParameterName = "Old parameter name";
            string newParameterName = "New and cool parameter name";

            if (Template.IsTemplatePresentInText(pageContent, templateName))
            {
                List<Template> templates = Template.GetTemplates(pageContent);
                foreach (Template t in templates)
                {
                    string originalTemplateText = t.Text;
                    if (t.Name.Trim.Equals(templateName.Trim()))
                    {
                        if (t.ContainsParameter(oldParameterName))
                            t.ChangeNameOfParameter(oldParameterName, newParameterName);// This will remove the parameter from the template object changing the Text property of it.
                    }
                    pageContent = pageContent.Replace(originalTemplateText, t.Text); // Replace the old template text with the new template text
                }
            }

            bool minorEdit = false;
            bool botEdit = true;
            bool replaceBlacklistedLinks = true;
            string editSummary = "Cool template manipulation";

            examplePage.Save(pageContent, editSummary, minorEdit, botEdit, replaceBlacklistedLinks);
        }
    }
}
```



