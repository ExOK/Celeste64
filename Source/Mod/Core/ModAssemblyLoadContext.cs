using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Mono.Cecil;

namespace Celeste64.Mod;

/// <summary>
/// A mod's assembly context, which handles resolving/loading mod assemblies
/// </summary>
internal sealed class ModAssemblyLoadContext : AssemblyLoadContext
{
	/// <summary>
	/// A list of assembly names which must not be loaded by a mod. The list will be initialized upon first access (which is before any mods will have loaded).
	/// </summary>
	private static string[] AssemblyLoadBlackList => _assemblyLoadBlackList ??= AssemblyLoadContext.Default.Assemblies.Select(asm => asm.GetName().Name)
		.Append("Mono.Cecil.Pdb").Append("Mono.Cecil.Mdb") // These two aren't picked up by default for some reason
		.ToArray()!;
	private static string[]? _assemblyLoadBlackList = null;
	
	/// <summary>
	/// The folder name where mod unmanaged assemblies will be loaded from.
	/// </summary>
	private static string UnmanagedLibraryFolder => _unmanagedLibraryFolder ??= (
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "lib-win-x64" :
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? RuntimeInformation.OSArchitecture switch
			{
				Architecture.X64 => "lib-linux-x64",
				Architecture.Arm => "lib-linux-arm",
				Architecture.Arm64 => "lib-linux-arm64",
				_ => throw new PlatformNotSupportedException(),
			} :
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "lib-osx-x64" :
			throw new PlatformNotSupportedException());
	private static string? _unmanagedLibraryFolder = null;

	
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
	
	private readonly ModInfo _info;
	private readonly IModFilesystem _fs;
	private readonly List<ModAssemblyLoadContext> _dependencyContexts = [];
	
	// Our node in the all ALCs list.
	private LinkedListNode<ModAssemblyLoadContext>? listNode;
	private bool isDisposed = false;

	internal ModAssemblyLoadContext(ModInfo info, IModFilesystem fs) : base(info.Id, isCollectible: true)
	{
		_info = info;
		_fs = fs;
		
		// Resolve dependencies
		foreach (var (modId, _) in info.Dependencies)
		{
			if (_contextsByModID.TryGetValue(modId, out var alc))
				_dependencyContexts.Add(alc);
		}
		_contextsByModID.TryAdd(info.Id, this);
		
		// Load all assemblies
		foreach (var assemblyPath in fs.FindFilesInDirectoryRecursive(Assets.LibraryFolder, Assets.LibraryExtension))
		{
			LoadAssemblyFromModPath(assemblyPath);
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
				_contextsByModID.Remove(_info.Id);
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

	protected override Assembly? Load(AssemblyName asmName)
	{
		// Lookup in the cache
		if (_assemblyLoadCache.TryGetValue(asmName.Name!, out var cachedAsm))
			return cachedAsm;
		
		// Try to load the assembly locally (from this or dependency ALCs)
		// // If that fails, try to load the assembly globally (game assemblies)
		var asm = LoadManagedLocal(asmName) ?? LoadManagedGlobal(asmName);
		if (asm != null)
		{
			_assemblyLoadCache.TryAdd(asmName.Name!, asm);
			return asm;
		}

		Log.Warning($"Failed to load assembly '{asmName.FullName}' for mod '{_info.Id}'");
		return null;
	}

	protected override IntPtr LoadUnmanagedDll(string name)
	{
		// Lookup in the cache
		if (_assemblyUnmanagedLoadCache.TryGetValue(name, out var cachedHandle))
			return cachedHandle;
		
		// Try to load the unmanaged assembly locally (from this or dependency ALCs)
		// If that fails, don't fallback to loading it globally - unmanaged dependencies have to be explicitly specified
		var handle = LoadUnmanaged(name);
		if (handle.HasValue && handle.Value != IntPtr.Zero)
		{
			_assemblyUnmanagedLoadCache.TryAdd(name, handle.Value);
			return handle.Value;
		}

		Log.Warning($"Failed to load native library '{name}' for mod '{_info.Id}'");
		return IntPtr.Zero;
	}
	
	private Assembly? LoadManagedLocal(AssemblyName asmName)
	{
		// Try to load the assembly from this mod
		if (LoadManagedFromThisMod(asmName) is { } asm)
			return asm;
		
		// Try to load the assembly from dependency assembly contexts
		foreach (var depCtx in _dependencyContexts)
		{
			if (depCtx.LoadManagedFromThisMod(asmName) is { } depAsm)
				return depAsm;
		}
		
		return null;
	}
	
	private Assembly? LoadManagedGlobal(AssemblyName asmName)
	{
		try 
		{
			// Try to load the assembly from the default assembly load context
			if (AssemblyLoadContext.Default.LoadFromAssemblyName(asmName) is { } globalAsm)
				return globalAsm;
		}
		catch
		{
			// ignored
		}
		
		return null;
	}
	
	private IntPtr? LoadUnmanaged(string name)
	{
		// Try to load the assembly from this mod
		if (LoadUnmanagedFromThisMod(name) is { } handle)
			return handle;
		
		// Try to load the assembly from dependency assembly contexts
		foreach (var depCtx in _dependencyContexts)
		{
			if (depCtx.LoadUnmanagedFromThisMod(name) is { } depHandle)
				return depHandle;
		}
		
		return null;
	}
	
	private Assembly? LoadManagedFromThisMod(AssemblyName asmName)
	{
		// Lookup in the cache
		if (_localLoadCache.TryGetValue(asmName.Name!, out var asm))
			return asm;
		
		// Try to load the assembly from the same library directory
		return LoadAssemblyFromModPath(Path.Combine(Assets.LibraryFolder, $"{asmName.Name!}.{Assets.LibraryExtension}"));
	}
	
	private IntPtr? LoadUnmanagedFromThisMod(string name)
	{
		// Lookup in the cache
		if (_localUnmanagedLoadCache.TryGetValue(name, out var handle))
			return handle;
		
		// Determine the OS-specific name of the assembly
		string osName =
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{name}.dll" :
			RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? $"lib{name}.so" :
			RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"lib{name}.dylib" :
			name;

		// Try multiple paths to load the library from
        foreach (string libName in new[] { name, osName }) {
	        string libraryPath = Path.Combine(Assets.LibraryFolder, UnmanagedLibraryFolder, libName);
	        
	        if (_fs is FolderModFilesystem folderFs)
	        {
		        // We can load the library directly in this case
		        if (NativeLibrary.TryLoad(Path.Combine(folderFs.Root, libraryPath), out handle))
		        {
			        _localUnmanagedLoadCache.TryAdd(name, handle);
			        return handle;
		        }
	        }
	        
	        // Otherwise, we need to extract the library into a temporary file
	        // TODO: Store this in a consistent cache file inside the install?
	        var tempFilePath = Path.GetTempFileName();
	        using (var tempFile = File.OpenWrite(tempFilePath))
	        {
		        try
		        {
					using var libraryStream = _fs.OpenFile(libraryPath);
					libraryStream.CopyTo(tempFile);
		        } 
		        catch
		        {
			        // Not found
			        continue;
		        }
	        }

            // Try to load the native library from the temporary file
            if (NativeLibrary.TryLoad(tempFilePath, out handle))
            {
	            _localUnmanagedLoadCache.TryAdd(name, handle);
                return handle;
            }
        }
		
		return null;
	}
	
	private Assembly? LoadAssemblyFromModPath(string assemblyPath)
	{
		try
		{
			var symbolPath = Path.ChangeExtension(assemblyPath, $".{Assets.LibrarySymbolExtension}");

			using var assemblyStream = _fs.OpenFile(assemblyPath);
			using var symbolStream = _fs.FileExists(symbolPath) ? _fs.OpenFile(symbolPath) : null;
		
			var module = ModuleDefinition.ReadModule(assemblyStream);
			if (AssemblyLoadBlackList.Contains(module.Assembly.Name.Name, StringComparer.OrdinalIgnoreCase))
				throw new Exception($"Attempted load of blacklisted assembly {module.Assembly.Name} from mod '{_info.Id}'");

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
				Log.Warning($"Assembly name conflict for name '{asmName}' in mod '{_info.Id}'!");
			}
		
			return assembly;
		}
		catch
		{
			return null;
		}
	}
}