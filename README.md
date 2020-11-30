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
5. Set up tests
  * Install .Net Core Test Explorer Plugin
  * `cp ./.mono/assemblies/Debug/GodotSharp.dll .mono/temp/bin/Debug/`

This should let you run the project from Godot, probably.

# Pages

It's playable online at https://moytw.github.io/SpaceDodgeRL/SpaceDodgeRL.html -
however, I'm only going to push specific commits because the game's .pck file is
apparently ~21MB? which is suboptimal. It's cool that I got it to run online
though! The online version is *significantly* slower than even my laptop (and my
laptop is not a snappy machine), to the extent that I'd have to actually worry
about optimization if I want browser to be the intended experience.

Still it's really sweet that Godot makes it so easy!

See https://posworkshop.space/posts/godot-deploy-web-export-to-github/ as well.