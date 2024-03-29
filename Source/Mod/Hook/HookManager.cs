using MonoMod.RuntimeDetour;

namespace Celeste64.Mod;

public sealed class HookManager
{
	private HookManager() { }

	public static HookManager Instance => instance ??= new HookManager();
	private static HookManager? instance = null;

	private readonly List<Hook> hooks = [];
	private readonly List<ILHook> ilHooks = [];

	public void RegisterHook(Hook? hook)
	{
		if (hook != null)
		{
			Log.Info($"{Settings.EnableAdditionalLogging}");
			if (Settings.EnableAdditionalLogging)
				Log.Info($"Registering On-hook for method '{hook.Source}' in type '{hook.Source.DeclaringType}' with hook method '{hook.Target}' in type '{hook.Target.DeclaringType}'");
			
			hooks.Add(hook);
		}
	}

	public void RegisterILHook(ILHook? ilHook)
	{
		if (ilHook != null)
		{
			if (Settings.EnableAdditionalLogging)
				Log.Info($"Registering IL-hook for method '{ilHook.Method}' in type '{ilHook.Method.DeclaringType}' with hook method '{ilHook.Manipulator.Method}' in type '{ilHook.Manipulator.Method.DeclaringType}'");
			
			ilHooks.Add(ilHook);
		}
	}

	public void RemoveHook(Hook? hook)
	{
		if (hook != null)
		{
			hooks.Remove(hook);
		}
	}

	public void RemoveILHook(ILHook? ilHook)
	{
		if (ilHook != null)
		{
			ilHooks.Remove(ilHook);
		}
	}

	internal void ClearHooks()
	{
		foreach (var hook in hooks)
		{
			hook.Dispose();
		}
		foreach (var hook in ilHooks)
		{
			hook.Dispose();
		}
		hooks.Clear();
		ilHooks.Clear();
	}
}
