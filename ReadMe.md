# Celeste 64: Fragments of the Mountain
This is the source code and general information for *Celeste 64: Fragments of the Mountain*, a game made by the original Celeste developers in under 2 weeks for Celeste's 6th Anniversary. We haven't done a lot of 3D development so much of this is not very optimized, it's coded in libraries mostly intended for 2D games, and we put it all together very, very quickly. Consider this similar to a weekend jam game, so experiment and learn at your own risk!  

You can find prebuilt version of the game on [itch.io](https://maddymakesgamesinc.itch.io/celeste64).

### Installation
 - You need [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
 - Clone this repo, make sure NuGet packages are found with `dotnet restore`
 - Run `Celeste64.csproj` with `dotnet run` or `dotnet build`

### Libraries Used
 - [Foster](https://github.com/FosterFramework/Foster) + [SDL2](https://github.com/libsdl-org/sdl): Input/Windowing/Rendering
 - [SledgeFormats](https://github.com/LogicAndTrick/sledge-formats): Parsing TrenchBroom level formats
 - [SharpGLTF](https://github.com/vpenades/SharpGLTF): Parsing and Animating glTF2 models
 - [FMOD](https://www.fmod.com): For Music and Sound Effects

### Tools Used
 - [TrenchBroom](https://trenchbroom.github.io/): For Level Editing
 - [Blender](https://www.blender.org/): For creating 3D Models
 - [Aseprite](https://www.aseprite.org/): For drawing Textures

### Resources Used
 - [khronos glTF Tutorials](https://github.khronos.org/glTF-Tutorials/gltfTutorial/gltfTutorial_020_Skins.html#the-joint-matrices): To figure out how Mesh Skins/Bones work
 - [LearnOpenGL](https://learnopengl.com/Advanced-OpenGL/Depth-testing): For general rendering concepts / normalizing Depth
 - [Kenny's Input Prompts](https://kenney.nl/assets/input-prompts): For UI Button Prompts
 - [Renogare](https://www.dafont.com/renogare.font): Main font

### Created By ...
 - [Maddy Thorson](http://maddymakesgames.com/)
 - [Noel Berry](https://noelberry.ca)
 - [Amora B.](https://amorabettany.com)
 - [Pedro "Saint11" Medeiros](http://saint11.org/)
 - [Power Up Audio](https://powerupaudio.com/)
 - [Lena Raine](https://lena.fyi/)
 - [Heidy Motta](https://www.heidy.page/).

### License
 - The Celeste IP and everything in the `Content` folder are owned by [Maddy Makes Games, Inc](https://www.maddymakesgames.com/).
 - The `Source` folder, with exceptions where noted, is [licensed under MIT](Source/License.txt).
 - The `Source/Audio/FMOD` folder contains bindings and binaries from FMOD.
 - We're fine with non-commercial Mods / Levels / Fan Games using assets from the `Content` folder as long as it's clear it is not made by the Celeste team or endorsed by us.
