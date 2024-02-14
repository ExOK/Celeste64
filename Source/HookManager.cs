using MonoMod.RuntimeDetour;

namespace Celeste64;

public sealed class HookManager
{
	private HookManager() { }

	private static HookManager? instance = null;
	public static HookManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new HookManager();
			}
			return instance;
		}
	}

	private List<Hook> Hooks { get; set; } = new List<Hook>();
	private List<ILHook> ILHooks { get; set; } = new List<ILHook>();

	public void RegisterHook(Hook? hook)
	{
		if (hook != null)
		{
			Hooks.Add(hook);
		}
	}

	public void RegisterILHook(ILHook? ilHook)
	{
		if (ilHook != null)
		{
			ILHooks.Add(ilHook);
		}
	}

	public void RemoveHook(Hook? hook)
	{
		if (hook != null)
		{
			Hooks.Remove(hook);
		}
	}

	public void RemoveILHook(ILHook? ilHook)
	{
		if (ilHook != null)
		{
			ILHooks.Remove(ilHook);
		}
	}

	internal void ClearHooks()
	{
		foreach(var hook in Hooks)
		{
			hook.Dispose();
		}
		foreach(var hook in ILHooks)
		{
			hook.Dispose();
		}
		Hooks.Clear();
		ILHooks.Clear();
	}
}