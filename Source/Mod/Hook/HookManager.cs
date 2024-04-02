using MonoMod.RuntimeDetour;

namespace Celeste64.Mod;

public sealed class HookManager
{
	private HookManager() { }

	public static HookManager Instance => instance ??= new HookManager();
	private static HookManager? instance = null;

	private readonly Dictionary<string, List<Hook>> hooksByMod = [];
	private readonly Dictionary<string, List<ILHook>> ilHooksByMod = [];

	public void RegisterHook(Hook hook)
	{
		var mod = ModAssemblyLoadContext.GetInfoForCallingAssembly();
		if (mod == null)
		{
			if (hook != null)
				Log.Warning($"Failed to identify mod trying to register On-hook for method '{hook.Source}' in type '{hook.Source.DeclaringType}' with hook method '{hook.Target}' in type '{hook.Target.DeclaringType}'");
			else
				Log.Warning($"Failed to identify mod trying to register a null On-hook");
			return;
		}
		if (hook == null)
		{
			Log.Warning($"Mod '{mod.Id}' tried to register a null On-hook");
			return;
		}
			
		RegisterHook(hook, mod);
	}
	public void RegisterILHook(ILHook ilHook)
	{
		var mod = ModAssemblyLoadContext.GetInfoForCallingAssembly();
		if (mod == null)
		{
			if (ilHook != null)
				Log.Warning($"Failed to identify mod trying to register IL-hook for method '{ilHook.Method}' in type '{ilHook.Method.DeclaringType}' with hook method '{ilHook.Manipulator.Method}' in type '{ilHook.Manipulator.Method.DeclaringType}'");
			else
				Log.Warning($"Failed to identify mod trying to register a null IL-hook");
			return;
		}
		if (ilHook == null)
		{
			Log.Warning($"Mod '{mod.Id}' tried to register a null IL-hook");
			return;
		}
		
		RegisterILHook(ilHook, mod);
	}
	
	public void RegisterHook(Hook hook, ModInfo mod)
	{
		if (Settings.EnableAdditionalLogging)
			Log.Info($"Registering On-hook for method '{hook.Source}' in type '{hook.Source.DeclaringType}' with hook method '{hook.Target}' in type '{hook.Target.DeclaringType}' for mod '{mod.Id}'");
		
		if (!hooksByMod.TryGetValue(mod.Id, out var hooks))
		{
			hooks = [];
			hooksByMod.Add(mod.Id, hooks);
		}
		hooks.Add(hook);
	}
	public void RegisterILHook(ILHook ilHook, ModInfo mod)
	{
		if (Settings.EnableAdditionalLogging)
			Log.Info($"Registering IL-hook for method '{ilHook.Method}' in type '{ilHook.Method.DeclaringType}' with hook method '{ilHook.Manipulator.Method}' in type '{ilHook.Manipulator.Method.DeclaringType}' for mod '{mod.Id}'");
			
		if (!ilHooksByMod.TryGetValue(mod.Id, out var ilHooks))
		{
			ilHooks = [];
			ilHooksByMod.Add(mod.Id, ilHooks);
		}
		ilHooks.Add(ilHook);
	}

	public void DeregisterHook(Hook hook)
	{
		var mod = ModAssemblyLoadContext.GetInfoForCallingAssembly();
		if (mod == null)
		{
			if (hook != null)
				Log.Warning($"Failed to identify mod trying to deregister On-hook for method '{hook.Source}' in type '{hook.Source.DeclaringType}' with hook method '{hook.Target}' in type '{hook.Target.DeclaringType}'");
			else
				Log.Warning($"Failed to identify mod trying to deregister a null On-hook");
			return;
		}
		if (hook == null)
		{
			Log.Warning($"Mod '{mod.Id}' tried to deregister a null On-hook");
			return;
		}
		
		DeregisterHook(hook, mod);
	}
	public void DeregisterILHook(ILHook ilHook)
	{
		var mod = ModAssemblyLoadContext.GetInfoForCallingAssembly();
		if (mod == null)
		{
			if (ilHook != null)
				Log.Warning($"Failed to identify mod trying to deregister IL-hook for method '{ilHook.Method}' in type '{ilHook.Method.DeclaringType}' with hook method '{ilHook.Manipulator.Method}' in type '{ilHook.Manipulator.Method.DeclaringType}'");
			else
				Log.Warning($"Failed to identify mod trying to deregister a null IL-hook");
			return;
		}
		if (ilHook == null)
		{
			Log.Warning($"Mod '{mod.Id}' tried to deregister a null IL-hook");
			return;
		}
		
		DeregisterILHook(ilHook, mod);
	}
	
	public void DeregisterHook(Hook hook, ModInfo mod)
	{
		hook.Dispose();
		if (hooksByMod.TryGetValue(mod.Id, out var hooks))
			hooks.Remove(hook);
	}
	public void DeregisterILHook(ILHook ilHook, ModInfo mod)
	{
		ilHook.Dispose();
		if (ilHooksByMod.TryGetValue(mod.Id, out var ilHooks))
			ilHooks.Remove(ilHook);
	}
	
	internal void ClearHooksOfMod(ModInfo mod)
	{
		if (hooksByMod.Remove(mod.Id, out var hooks))
		{
			foreach (var hook in hooks)
			{
				hook.Dispose();
			}
		}
		
		if (ilHooksByMod.Remove(mod.Id, out var ilHooks))
		{
			foreach (var ilHook in ilHooks)
			{
				ilHook.Dispose();
			}
		}
	}

	internal void ClearHooks()
	{
		foreach (var hook in hooksByMod.Values.SelectMany(static hooks => hooks))
		{
			hook.Dispose();
		}
		foreach (var ilHook in ilHooksByMod.Values.SelectMany(static ilHooks => ilHooks))
		{
			ilHook.Dispose();
		}
		hooksByMod.Clear();
		ilHooksByMod.Clear();
	}
}
