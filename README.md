# OWRandomSpeedrun
random shipless speedrun generator for Outer Wilds

## IDE setup for VS Code in Windows
1. Install [VS Code](https://code.visualstudio.com/download).
1. Open VS Code, and open the folder where you cloned the repository. You will probably get some build errors; don't worry about them yet.
1. Install the [C# VS Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp).
1. Install [VS Build Tools 2022](https://aka.ms/vs/17/release/vs_BuildTools.exe).
    1. When installing, include the **.NET desktop build tools** option.
1. Install [winget](https://www.microsoft.com/p/app-installer/9nblggh4nns1#activetab=pivot:overviewtab).
1. Open a Command Prompt **with administrator privileges** and execute `winget install Microsoft.NuGet` to install NuGet.
1. Log out of Windows & log back in so that your PATH variables are correctly configured.
1. Select Terminal > Run build task. The mod should correctly compile (possibly with some warnings).
1. To verify that it compiled correctly, open an Explorer window and navigate to `%APPDATA%\OuterWildsModManager\OWML\Mods\ZacBauermeister.OuterWildsRandomSpeedrun`. If you see OuterWildsRandomSpeedrun.dll in the directory, it compiled successfully.
