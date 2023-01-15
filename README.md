# ** Announcement ** - Chromely CefSharp is no longer being maintained! 

For those who would like to continue working on the project via forks or alternative approaches, please send mattkol the appropriate links and they will be added here.

Thanks to all the contributors over the years for making the platform a great learning place.

Thank you all.
------------------------------------------------------------------------------------------------------------------------------------------

<p align="center"><img src="https://github.com/chromelyapps/Chromely/blob/master/nugets/chromely.ico?raw=true" /></p>
<h1 align="center">Chromely CefSharp</h1>

Chromely is a lightweight alternative to <a href="https://github.com/ElectronNET/Electron.NET">Electron.NET</a>, <a href="https://github.com/electron/electron">Electron</a> for .NET/.NET Core developers.

Chromely is a .NET/.NET Core HTML5 Chromium desktop framework. It is focused on building apps based on [Xilium.CefGlue](https://gitlab.com/xiliumhq/chromiumembedded/cefglue), [CefSharp](https://github.com/cefsharp/CefSharp) implementations of  embedded Chromium ([CEF](https://bitbucket.org/chromiumembedded/cef)) **without WinForms or WPF**, but can be extended to use WinForms or WPF. Chromely uses **Windows**, **Linux** and **MacOS** native GUI API as "thin" chromium hosts.

With Chromely you can build Single Page Application (SPA) HTML5 desktop apps with or without Node/npm. Building SPA apps using Blazor or javascript frameworks like Angular, React, Vue or similar is easy. You can use Visual Studio Code or any IDE you are familiar with as long as Chromely knows the entry html file from the compiled/bundled files. For more info please see - [Blazor-Demos](https://github.com/chromelyapps/demo-projects/tree/master/blazor) and [Chromely-Apps](https://github.com/chromelyapps/demo-projects/tree/master/angular-react-vue).

##### If you like Chromely, please give it a star - it helps! #####

Have a quick question? Wanna chat? Connect on  [![Join the chat at https://gitter.im/chromely_/Lobby](https://badges.gitter.im/chromely_/Lobby.svg)](https://gitter.im/chromely_/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Have an app/project/tool using Chromely - [please share!](https://github.com/chromelyapps/Chromely/issues/63)

[![Chromely.Core](http://img.shields.io/nuget/vpre/Chromely.Core.svg?style=flat&label=Chromely.Core)](https://www.nuget.org/packages/Chromely.Core)
[![Chromely.CefSharp](http://img.shields.io/nuget/vpre/Chromely.CefSharp.svg?style=flat&label=Chromely.CefSharp)](https://www.nuget.org/packages/Chromely.CefSharp)

A basic Chromely project requires: - [Demos](https://github.com/chromelyapps/demo-projects/tree/master/regular-chromely/CefSharpDemo)

````csharp
class Program
{
   [STAThread]
   static void Main(string[] args)
   {
       AppBuilder
       .Create()
       .UseApp<ChromelyBasicApp>()
       .Build()
       .Run(args);
    }
}
````

### Chromely Demos 
Get started with our [demos](https://github.com/chromelyapps/demo-projects/tree/master/regular-chromely/CefSharpDemo). 
![](https://github.com/chromelyapps/Chromely/blob/master/Screenshots/chromely_screens_n3.gif)

### References
* CEF - https://bitbucket.org/chromiumembedded/cef
* CefSharp - https://github.com/cefsharp/CefSharp

Contributing
---
Contributions are always welcome, via PRs, issues raised, or any other means. To become a dedicated contributor, please [contact the Chromely team](https://github.com/orgs/chromelyapps/people) or [raise an issue](https://github.com/chromelyapps/Chromely/issues) mentioning your intent.

License
---
Chromely is MIT licensed. For dependency licenses [please see](https://github.com/chromelyapps/Chromely/blob/master/LICENSE.md).

Credits
---
Thanks to [JetBrains](https://www.jetbrains.com) for the OSS license of Resharper Ultimate.

Improved and optimized using:

<a href="https://www.jetbrains.com/resharper/
"><img src="https://blog.jetbrains.com/wp-content/uploads/2014/04/logo_resharper.gif" alt="Resharper logo" width="100" /></a>
