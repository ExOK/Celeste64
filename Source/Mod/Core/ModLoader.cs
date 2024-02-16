using System.Reflection;
using System.Text.Json;

namespace Celeste64.Mod;

public static class ModLoader
{
	public const string ModsFolder = "Mods";

	private static string? modsFolderPath = null;

	public static string ModFolderPath
	{
		get
		{
			if (modsFolderPath == null)
			{
				var baseFolder = AppContext.BaseDirectory;
				var searchUpPath = "";
				int up = 0;
				while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModsFolder)) && up++ < 6)
					searchUpPath = Path.Join(searchUpPath, "..");
				if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModsFolder)))
					throw new Exception($"Unable to find {ModsFolder} Directory from '{baseFolder}'");
				modsFolderPath = Path.Join(baseFolder, searchUpPath, ModsFolder);
			}

			return modsFolderPath;
		}
	}

	internal static void RegisterAllMods()
	{
		ModManager.VanillaGameMod = new VanillaGameMod
		{
			// Mod Infos are required now, so make a dummy mod info for the vanilla game too. This shouldn't really be used for anything.
			ModInfo = new ModInfo
			{
				Id = "Celeste64Vanilla",
				Name = "Celeste 64: Fragments of the Mountains",
				Version = "1.1.1"
			},
			Filesystem = new FolderModFilesystem(Assets.ContentPath)
		};

		List<(ModInfo, IModFilesystem)> modInfos =
		[
			// Load vanilla as a mod, to unify all asset loading code
			(ModManager.VanillaGameMod.ModInfo, ModManager.VanillaGameMod.Filesystem),
		];

		// Find all mods in directories:
		foreach (var modDir in Directory.EnumerateDirectories(ModFolderPath))
		{
			var modName = Path.GetFileNameWithoutExtension(modDir)!; // Todo: read from some metadata file
			var fs = new FolderModFilesystem(modDir);

			modInfos.Add((LoadModInfo(modName, fs), fs));
			Log.Info($"Loaded mod from directory: {modName}");
		}

		// Find all mods in zips:
		foreach (var modZip in Directory.EnumerateFiles(ModFolderPath, "*.zip"))
		{
			var modName = Path.GetFileNameWithoutExtension(modZip)!; // Todo: read from some metadata file
			var fs = new ZipModFilesystem(modZip);

			modInfos.Add((LoadModInfo(modName, fs), fs));
			Log.Info($"Loaded mod from zip: {modName}");
		}
		
		ModManager.Unload();
		
		// We use an slightly silly approach to load all dependencies first:
		// Load all mods which have their dependencies met and repeat until we're done.
		bool loadedModInIteration = false;
		HashSet<ModInfo> loaded = [];
		
		while (modInfos.Count > 0)
		{
			for (int i = modInfos.Count - 1; i >= 0; i--)
			{
				var (info, fs) = modInfos[i];
				
				bool dependenciesSatisfied = true;
				if (info.Dependencies is { } deps)
				{
					foreach (var (modID, version) in deps)
					{
						if (loaded.FirstOrDefault(loadedInfo => loadedInfo.Id == modID) is { } dep) continue; // TODO: Check dependency version

						dependenciesSatisfied = false;
						break;
					}	
				}

				if (!dependenciesSatisfied) continue;

				var mod = LoadGameMod(info, fs);
				mod.Filesystem?.AssociateWithMod(mod);
				ModManager.RegisterMod(mod);		

				modInfos.RemoveAt(i);
				loaded.Add(info);
				loadedModInIteration = true;
			}
			
			if (!loadedModInIteration)
			{
				// This means that all infos left infos don't have their dependencies met
				// TODO: Gracefully handle this case
				foreach (var (info, _) in modInfos)
				{
					Log.Error($"Mod '{info.Id} is missing following dependencies:");

					var missingDependencies = info.Dependencies!.Where(dep =>
					{
						var (modID, version) = dep;
						return loaded.FirstOrDefault(loadedInfo => loadedInfo.Id == modID) == null;
					});
					foreach (var (modID, version) in missingDependencies)
					{
						Log.Error($" - ModID: '{modID}' Version: '{version}' ");
					}
				}
			}
		}

		ModManager.InitializeFilesystemBackgroundCleanup();
	}
	
	private static ModInfo LoadModInfo(string modFolder, IModFilesystem fs)
	{
		if (!fs.TryOpenFile("Fuji.json", stream => JsonSerializer.Deserialize(stream, ModInfoContext.Default.ModInfo), out var info))
			throw new Exception($"Fuji Exception: Tried to load mod {modFolder} but could not find a valid Fuji.json file");
		if (!info.IsValid())
			throw new Exception($"Fuji Exception: Invalid Fuji.json file for {modFolder}/Fuji.json");

		return info;
	}

	private static GameMod LoadGameMod(ModInfo info, IModFilesystem fs)
	{
		// If the mod is not enabled, don't load the assemblies
		if (!Save.Instance.GetOrMakeMod(info.Id).Enabled)
		{
			return new DummyGameMod
			{
				ModInfo = info,
				Filesystem = fs
			};
		}
		
		GameMod? loadedMod = null;
		var anyDllFile = false;
		foreach (var assemblyPath in fs.FindFilesInDirectoryRecursive(Assets.LibraryFolder, Assets.LibraryExtension))
		{
			var symbolPath = Path.ChangeExtension(assemblyPath, $".{Assets.LibrarySymbolExtension}");

			using var assemblyStream = fs.OpenFile(assemblyPath);
			using var symbolStream = fs.FileExists(symbolPath) ? fs.OpenFile(symbolPath) : null;
			
			var assembly = Assembly.Load(assemblyStream.ReadAllToByteArray(), symbolStream?.ReadAllToByteArray());
			
			Log.Info($"Loaded assembly file '{assemblyPath}' for mod {info.Id}");
			anyDllFile = true;
				
			foreach (var type in assembly.GetExportedTypes())
			{
				if (type.BaseType != typeof(GameMod))
					continue;
			
				if (loadedMod is { })
				{
					Log.Error($"Mod at {fs.Root} contains multiple classes extending from {typeof(GameMod)} " +
					          $"[{loadedMod.GetType().FullName} vs {type.FullName}]! Only the first one will be used!");
					continue;
				}
					
				if (Activator.CreateInstance(type) is not GameMod instance)
					continue;
			
				loadedMod = instance;
				instance.Filesystem = fs;
				instance.ModInfo = info;
			}
		}

		if (loadedMod is not { })
		{
			if (anyDllFile)
			{
				Log.Warning($"Mod at {fs.Root} has dll files, but none of them contain a type extending from {typeof(GameMod)}.");
			}
			
			// No GameMod, create a dummy one
			loadedMod = new DummyGameMod
			{
				ModInfo = info,
				Filesystem = fs
			};
		}
			
		return loadedMod;
	}
}