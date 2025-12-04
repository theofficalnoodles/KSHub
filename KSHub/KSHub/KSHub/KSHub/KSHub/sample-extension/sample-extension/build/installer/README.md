# KSHub (CefSharp starter)

This repository contains a starter KSHub browser (WPF + CefSharp), a sample unpacked extension, and packaging scripts.

## Quick dev steps (Windows)
1. Install Visual Studio 2022 with .NET Desktop workload and .NET 6 SDK.
2. Install Inno Setup (if you want the installer).
3. Clone the repo and open `KSHub.sln` in Visual Studio.
4. Restore NuGet packages (CefSharp).
5. Build & run the `KSHub` project (Start or `dotnet run` from project dir).
6. From the running app: click ✱ button to load `sample-extension` folder and test extension script.
7. To package: run `build\build-and-package.ps1` from a Developer PowerShell (it runs publish + Inno Setup).

## Extension testing
- The sample extension adds a purple bar on top of pages and logs to the console.
- For more complex extensions, test API availability; some Chrome-specific APIs may not be available in CEF.

## Notes on CefSharp native files
CefSharp requires native CEF binaries (DLLs). NuGet usually copies runtime files for publish; if missing, follow CefSharp docs:
https://github.com/cefsharp/CefSharp/wiki/General-Usage

## Legal / licensing
Chromium/CEF/CefSharp and many third-party components have their own licenses. Do not claim this is "Google Chrome". Include proper attributions when distributing.
Do NOT copy this without my PERMISSION/Paying Me
