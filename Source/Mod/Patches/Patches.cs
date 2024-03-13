namespace Celeste64.Mod.Patches;

internal static class Patches
{
	public static void Load()
	{
		Keyboard.Load();
		Hooks.Load();
		SledgeHooks.Load();

		// Needs to be done last, to prevent our patches from being rejected
		Hooks.AfterLoad();
	}

	public static void Unload()
	{
		Keyboard.Unload();
		Hooks.Unload();
		SledgeHooks.Unload();
	}
}
