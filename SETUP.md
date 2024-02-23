## Welcome to Celeste 64 Modding! - How to setup and get Fuji running on your system.
#### Note: Fuji is still in its early phases and things are changing rapidly

Fuji is the modding tool used for Celeste 64 modding, you can find it [here](https://github.com/FujiAPI/Fuji). Instead of using the original Celeste 64 files downloaded from Itch or Github, Fuji is a drop-in replacement for Celeste64.

Currently only Windows builds are provided on the [releases page](https://github.com/FujiAPI/Fuji/releases).

We do not currently officially support Linux or MacOS as we are focusing on the Windows version for now, but if you'd still like to use it on one of those platforms, you can try building it yourself. The steps are as follows:

Prerequisites: You must have [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

- Clone Fuji to your computer with `git clone https://github.com/FujiAPI/Fuji && cd Fuji && git checkout $(git tag | tail -1)`
    - this clones the latest stable release to your machine.

- Figure out your system's RID via the [.NET wiki](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids)
  - Shortlist:
    1. M series Macs - `osx-arm64`
    2. Most Linux systems - `linux-x64`

- Run `dotnet restore && dotnet publish Celeste64.Launcher/Celeste64.Launcher.csproj -c Release -r RID -o build && cp -r Content Mods build`
    - Replace the underlined section with your system's RID.

- In the generated build folder run the `Celeste64.Launcher` executable and have fun!
Note: you may rename and move the `build` folder to wherever you like.

### Installing and Using Mods:
Fuji comes with a few pre-installed mods. They exist in the `Mods` directory inside `build`. Place downloaded mods alongside those. If you would like to switch skins you first must load into a level then pause and select your desired skin. Your skin will update once you reload (exiting the level, dying, or selecting retry)

Further instructions are available at the [Fuji wiki](https://github.com/FujiAPI/Fuji/wiki) or on the [Discord](https://discord.gg/9NJcbSyuae).
