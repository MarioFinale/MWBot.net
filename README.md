# MWBot.net
Base para crear herramientas para sitios basados en el software Mediawiki.

# Ejemplos simples de uso

Cargar una página, editarla y guardarla.

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
         ConfigFile cfg = new ConfigFile(GlobalVars.ConfigFilePath); 
         Bot Workerbot = new Bot(cfg);
         Page examplepage = Workerbot.GetPage("Example page");
         string newtext = examplepage.Content.Replace("Text", "NewText");
         //Do something
         examplepage.Save(newtext, "Test edit"); 
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
         Dim cfg As ConfigFile = New ConfigFile(GlobalVars.ConfigFilePath)
         Dim Workerbot as Bot = New Bot(cfg)
         Dim examplepage as Page = Workerbot.Getpage("Example page")
         Dim newtext as String = examplepage.Content.Replace("Text", "Newtext")
         'Do something
         examplepage.Save(newtext, "Test edit")
      End Sub      
   End Class   
End Namespace
```
