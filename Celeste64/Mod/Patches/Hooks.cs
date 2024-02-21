using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonoMod.Cil;
using MonoMod.Core;
using MonoMod.RuntimeDetour;

namespace Celeste64.Mod.Patches;

/// <summary>
/// <li>Prevent <see cref="Hook"/>s / <see cref="ILHook"/>s on types outside of the "Celeste64" namespace.</li>
/// </summary>
internal static class Hooks
{
	private static ILHook? hook_Hook_ctor;
	private static ILHook? hook_ILHook_ctor;
	
	public static void Load()
	{
		// Using MonoMod to defeat MonoMod.
		hook_Hook_ctor = new ILHook(typeof(Hook).GetConstructor(BindingFlags.Public | BindingFlags.Instance, [typeof(MethodBase), typeof(MethodInfo), typeof(object), typeof(IDetourFactory), typeof(DetourConfig), typeof(bool)])!, IL_CheckHook, applyByDefault: false);
		hook_ILHook_ctor = new ILHook(typeof(ILHook).GetConstructor(BindingFlags.Public | BindingFlags.Instance, [typeof(MethodBase), typeof(ILContext.Manipulator), typeof(IDetourFactory), typeof(DetourConfig), typeof(bool)])!, IL_CheckHook, applyByDefault: false);
	}
	
	public static void AfterLoad()
	{
		hook_Hook_ctor?.Apply();
		hook_ILHook_ctor?.Apply();
	}

	public static void Unload()
	{
		hook_Hook_ctor?.Dispose();
		hook_ILHook_ctor?.Dispose();
	}
	
	private static void IL_CheckHook(ILContext il)
	{
		var cur = new ILCursor(il);
		cur.EmitLdarg1(); // 'MethodBase method'
		cur.EmitDelegate(ThrowIfInvalidHook);
	}
	
	private static readonly Dictionary<Assembly, GameMod> _assemblyLookup = [];
	private static readonly Assembly _monomodAsm = typeof(Hook).Assembly;
	
	private static void ThrowIfInvalidHook(MethodBase method)
	{
		var stacktrace = new StackTrace();
		var asm = stacktrace
			.GetFrames()
			.Select(frame => frame.GetMethod()?.DeclaringType?.Assembly)
			.FirstOrDefault(asm => asm != _monomodAsm);

		if (asm != null)
		{
			if (!_assemblyLookup.TryGetValue(asm, out var gameMod))
			{
				gameMod = ModManager.Instance.Mods.Find(mod => mod.GetType().Assembly == asm);
				_assemblyLookup.Add(asm, gameMod!);
			}
		
			if (gameMod == null)
				Log.Warning($"Registering hook from non-mod assembly '{asm}'");
			// Mods can opt-out of this, but then they are on their own.
			else if (method.DeclaringType?.Namespace is { } ns && gameMod.PreventHookProtectionYesIKnowThisIsDangerousAndCanBreak.Contains(ns))
				return;
		}
		
		if (method.DeclaringType != null && method.DeclaringType.Namespace != "Celeste64")
			throw new InvalidOperationException("Hooking methods outside of the 'Celeste64' namespace is not allowed! " +
			                                    "Those methods might change their implementation, causing the hook to break!" +
			                                    "Please consider reaching out to the authors first, before trying to avoid this protection." +
			                                    "If you are aware of the risks but need to do it anyway, you can enable the 'PreventHookProtectionYesIKnowThisIsDangerousAndCanBreak' property inside your 'GameMod'.");
	}
}