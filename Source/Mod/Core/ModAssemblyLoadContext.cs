using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using Mono.Cecil;

namespace Celeste64.Mod;

internal sealed class ModAssemblyLoadContext : AssemblyLoadContext
{
	private static readonly ReaderWriterLockSlim _allContextsLock = new();
	private static readonly LinkedList<ModAssemblyLoadContext> _allContexts = [];
	private static readonly Dictionary<string, ModAssemblyLoadContext> _contextsByModID = new();

	// All modules loaded by the context.
	private readonly Dictionary<string, ModuleDefinition> _assemblyModules = new();
	
	// Cache for this mod including dependencies.
	private readonly ConcurrentDictionary<string, Assembly> _assemblyLoadCache = new();
	private readonly ConcurrentDictionary<string, IntPtr> _assemblyUnmanagedLoadCache = new();
	
	// Cache for this mod specifically. Also used when loading from this mod as a dependency.
	private readonly ConcurrentDictionary<string, Assembly> _localLoadCache = new();
	private readonly ConcurrentDictionary<string, IntPtr> _localUnmanagedLoadCache = new();
	
	private readonly object LOCK = new();
	
	private readonly ModInfo Info;
	private readonly List<ModAssemblyLoadContext> DependencyContexts = [];
	
	// Our node in the all ALCs list.
	private LinkedListNode<ModAssemblyLoadContext>? listNode;
	private bool isDisposed = false;

	internal ModAssemblyLoadContext(ModInfo info, IModFilesystem fs) : base(info.Id, isCollectible: true)
	{
		Info = info;
		
		// Resolve dependencies
		if (info.Dependencies is { } deps)
		{
			foreach (var (modId, version) in deps)
			{
				if (_contextsByModID.TryGetValue(modId, out var alc))
					DependencyContexts.Add(alc);
			}
		}
		
		// Load all assemblies
		foreach (var assemblyPath in fs.FindFilesInDirectoryRecursive(Assets.LibraryFolder, Assets.LibraryExtension))
		{
			var symbolPath = Path.ChangeExtension(assemblyPath, $".{Assets.LibrarySymbolExtension}");

			using var assemblyStream = fs.OpenFile(assemblyPath);
			using var symbolStream = fs.FileExists(symbolPath) ? fs.OpenFile(symbolPath) : null;
		
			var module = ModuleDefinition.ReadModule(assemblyStream);
			Log.Info($"Loading module: {module.Assembly.Name.Name}");

			// Reset stream back to beginning
			assemblyStream.Position = 0;
			
			var assembly = LoadFromStream(assemblyStream, symbolStream);
			var asmName = assembly.GetName().Name!;
			
			if (_assemblyModules.TryAdd(asmName, module))
			{
				_assemblyLoadCache.TryAdd(asmName, assembly);
				_localLoadCache.TryAdd(asmName, assembly);
			} 
			else
			{
				Log.Warning($"Assembly name conflict for name '{asmName}' in mod '{info.Id}'!");
			}
		}
	}
	
	public void Dispose()
	{
		lock (LOCK) {
			if (isDisposed)
				return;
			isDisposed = true;

			// Remove from mod ALC list
			_allContextsLock.EnterWriteLock();
			try {
				_allContexts.Remove(listNode!);
				_contextsByModID.Remove(Info.Id);
				listNode = null;
			} finally {
				_allContextsLock.ExitWriteLock();
			}

			// Unload all assemblies loaded in the context
			foreach (ModuleDefinition module in _assemblyModules.Values)
				module.Dispose();
			_assemblyModules.Clear();

			_assemblyLoadCache.Clear();
			_localLoadCache.Clear();
		}
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		Log.Info($"Assembly load request for: {assemblyName}");

		if (_assemblyLoadCache.TryGetValue(assemblyName.Name!, out var cachedAsm))
			return cachedAsm;
		
		return base.Load(assemblyName);
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		Log.Info($"Unmanaged load request for: {unmanagedDllName}");
		
		if (_assemblyUnmanagedLoadCache.TryGetValue(unmanagedDllName, out var cachedHandle))
			return cachedHandle;
		
		return base.LoadUnmanagedDll(unmanagedDllName);
	}
}