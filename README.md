# MWBot.net

**Base library to create tools for sites based on MediaWiki software.**  

[![License: LGPL-2.0](https://img.shields.io/badge/License-LGPL%202.0-blue.svg)](https://www.gnu.org/licenses/old-licenses/lgpl-2.0.en.html)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-MWBot.net-blue.svg)](https://www.nuget.org/packages/MWBot.net)

A .NET library (VB.NET) for interacting with MediaWiki APIs. Supports page loading/editing, template manipulation, logging, task management, and more. Designed as a foundation for MediaWiki bots and tools.

## Installation

```bash
dotnet add package MWBot.net
```

Or via Package Manager Console:

```powershell
Install-Package MWBot.net
```

- **Target framework:** `net10.0`
- **License:** LGPL-2.0-only
- **Dependencies:** Magick.NET (for image handling)

## Simple usage examples

### Loading a page, editing it and saving it 

#### C#

```csharp
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
            
            Page examplepage = Workerbot.Getpage("Example page");
            string newtext = examplepage.Content.Replace("Text", "NewText");
            // Do something
            examplepage.Save(newtext, "Test edit using C#!"); 
        }
    }
}
```

#### Visual Basic

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
        
         Dim examplepage As Page = Workerbot.Getpage("Example page")
         Dim newtext As String = examplepage.Content.Replace("Text", "Newtext")
         
         ' Do something
         examplepage.Save(newtext, "Test edit using VB!")
      End Sub      
   End Class   
End Namespace
```

### Template manipulation

#### Visual Basic

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

        ' Load page
        Dim examplePage As Page = Workerbot.Getpage("Example page")
        Dim pageContent As String = examplePage.Content

        ' Do something
        Dim templateName As String = "Cool template name"
        Dim oldParameterName As String = "Old parameter name"
        Dim newParameterName As String = "New and cool parameter name"

        If Template.IsTemplatePresentInText(pageContent, templateName) Then
            Dim templates As List(Of Template) = Template.GetTemplates(pageContent)
            For Each t As Template In templates
                Dim originalTemplateText As String = t.Text
                If t.Name.Trim.Equals(templateName.Trim()) Then
                    If t.ContainsParameter(oldParameterName) Then
                        t.ChangeNameOfParameter(oldParameterName, newParameterName) ' This will update the template Text property
                    End If
                End If
                pageContent = pageContent.Replace(originalTemplateText, t.Text) ' Replace the old template text with the new template text
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

#### C#

```csharp
using System;
using System.Collections.Generic;
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
                    if (t.Name.Trim().Equals(templateName.Trim()))
                    {
                        if (t.ContainsParameter(oldParameterName))
                            t.ChangeNameOfParameter(oldParameterName, newParameterName); // This will update the template Text property
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

## Configuration

The library uses a simple config file (path passed to the `Bot` constructor). On first run, if the file is missing or invalid, it will prompt for the required values (bot name, username, password, wiki URL) and create it.

## License

This project is licensed under the **LGPL-2.0-only** license. See the [LICENSE](LICENSE) file for details.

## Author

**MarioFinale**  
GitHub: [@MarioFinale](https://github.com/MarioFinale)

## Related Projects

- [PeriodiBOT](https://github.com/MarioFinale/PeriodiBOT) — Multipurpose MediaWiki bot built on this library.
- [IRCCLIENT](https://github.com/MarioFinale/IRCCLIENT) — IRC client library that integrates with MWBot.net for bot control.

---

*Package version: 1.3.*  
*See the [commit history](https://github.com/MarioFinale/MWBot.net/commits/master/) for the full changelog.*
