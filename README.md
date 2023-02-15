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

## IDE setup for Unity in Windows
1. Install **Unity 2019.4.39** from the [Unity Download Archive](https://unity.com/releases/editor/archive). You must install this specific version!
1. Install the [.NET Framework 4.7.1 Developer Pack](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net471-developer-pack-offline-installer).
1. Open the UnityProject folder in Unity. Unity will set up the necessary dependencies which may take a few minutes.
1. Modify Unity's settings to make VS Code the default editor (this is important!).
    1. Click Edit > Preferences
    1. On the left, click External Tools
    1. In the External Tools menu, change **External Script Editor** to **Visual Studio Code**. Exit the menu.
1. Modify the Omnisharp VS Code plugin to use a .NET version that is compatible with Unity:
    1. In the Project tab of Unity, open any script in the Assets > Scripts folder. This should open in VS Code.
    1. In VS Code, Select File > Preferences > Settings. 
    1. Click the "Workspace" tab (instead of "User") in the settings menu.
    1. In the Search Settings editbox at the top, type `omnisharp.usemodernnet`.
    1. You should get a checkbox for **Omnisharp: Use Modern Net**. Uncheck the box and close Visual Studio.

## Making changes to the Unity project
### Making changes to assets/prefabs
1. Make whatever changes to the assets/prefabs you want.
1. If you have introduced new assets/prefabs, you need to make them part of the asset bundle in order to make them available to Outer Wilds. To do this:
    1. Single-click the asset you want to include in the bundle.
    1. On the bottom-right corner of the screen under the "Asset Labels" section, set **AssetBundle** to **spawnpointselector**.
1. To rebuild the asset bundle, select Assets > Build AssetBundles. Wait for the progress bar to complete.
1. To include the new assets in Outer Wilds, in the main project in VS Code, select Terminal > Run Build Task. This will copy the updated asset bundle to the mod folder under %APPDATA%.

### Making changes to scripts
**NOTE:** These instructions apply only to making changes to scripts in the UnityProjects folder and its subfolders. You can build code in the OuterWildsRandomSpeedrun folder as you normally would.
1. If you are introducing a new script that needs to be used in Outer Wilds, create it in the Assets > Scripts folder.
1. Edit the script by double-clicking it in the Unity Editor. This will open it in VS Code.
1. Edit the script as you desire within VS Code and save it. **Don't attempt to compile the code within VS Code**.
1. Return to the Unity Editor. The editor will freeze for a moment while it compiles the code. Look at the footer of the Unity Editor for any red text indicating a compile error. (There may be orange warnings, too.) Upon successful compilation, Unity will produce an updated SpawnPointSelectorAssembly.dll in the UnityProject/Library/ScriptAssemblies folder.
1. Open the main project in VS Code (like you would for a change not related to the Unity project).
1. Select Terminal > Run Build Task. This will copy the updated DLL file to the mod folder under %APPDATA%.