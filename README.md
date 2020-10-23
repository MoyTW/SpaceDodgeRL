# Setup Notes

This isn't a comprehensive setup list, because really who's gonna set this up other than me?

1. DL, install Godot /w C# support Visual Studio Code
2. Install .NET deps if not present
  * .NET SDK 3.1 https://dotnet.microsoft.com/download/dotnet-core/sdk-for-vs-code
  * .NET 4.7.2 Developer Pack https://dotnet.microsoft.com/download/dotnet-framework/net472
3. Godot settings
  * Editor Settings -> Mono -> Editor -> External Editor = Visual Studio Code
  * Editor Settings -> Mono -> Builds -> dotnet CLI
4. Visual Studio Code settings
  * Install C# Plugin
  * Install C# Tools for Godot Plugin

This should let you run the project from Godot, probably.