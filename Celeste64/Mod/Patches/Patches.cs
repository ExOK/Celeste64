namespace Celeste64.Mod.Patches;

internal static class Patches
{
    public static void Load()
    {
        Keyboard.Load();
    }
    
    public static void Unload()
    {
        Keyboard.Unload();
    }
}