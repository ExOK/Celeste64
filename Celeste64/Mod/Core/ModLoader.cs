using System.Reflection;
using System.Text.Json;
using MonoMod.RuntimeDetour;

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
		ModManager.Instance.VanillaGameMod = new VanillaGameMod
		{
			// Mod Infos are required now, so make a dummy mod info for the valilla game too. This shouldn't really be used for anything.
			ModInfo = new ModInfo
			{
				Id = "Celeste64Vanilla",
				Name = "Celese 64: Fragments of the Mountains",
				VersionString = "1.0.0",
			},
			Filesystem = new FolderModFilesystem(Assets.ContentPath)
		};

		List<(ModInfo, IModFilesystem)> modInfos = [];

		// Find all mods in directories:
		foreach (var modDir in Directory.EnumerateDirectories(ModFolderPath))
		{
			var modName = Path.GetFileNameWithoutExtension(modDir)!; // Todo: read from some metadata file
			var fs = new FolderModFilesystem(modDir);

			ModInfo? info = LoadModInfo(modName, fs);
			if(info != null)
			{
				if(info.Id == "Celeste64Vanilla" || modInfos.Any(data => data.Item1.Id == info.Id))
				{
					Log.Error($"Fuji Error: Could not load mod from directory: {modName}, because a mod with that id already exists");
				}
				else
				{
					modInfos.Add((info, fs));
					Log.Info($"Loaded mod from directory: {modName}");
				}
			}
		}

		// Find all mods in zips:
		foreach (var modZip in Directory.EnumerateFiles(ModFolderPath, "*.zip"))
		{
			var modName = Path.GetFileNameWithoutExtension(modZip)!; // Todo: read from some metadata file
			var fs = new ZipModFilesystem(modZip);

			ModInfo? info = LoadModInfo(modName, fs);
			if (info != null)
			{
				if (info.Id == "Celeste64Vanilla" || modInfos.Any(data => data.Item1.Id == info.Id))
				{
					Log.Error($"Fuji Error: Could not load mod from zip: {modName}, because a mod with that id already exists");
				}
				else
				{
					modInfos.Add((info, fs));
					Log.Info($"Loaded mod from zip: {modName}");
				}
			}
		}

		ModManager.Instance.Unload();

		// Load vanilla as a mod, to unify all asset loading code
		ModManager.Instance.RegisterMod(ModManager.Instance.VanillaGameMod);

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
				foreach (var (modID, versionString) in info.Dependencies)
				{
					var version = new Version(versionString);

					if (loaded.FirstOrDefault(loadedInfo => loadedInfo.Id == modID) is { } dep &&
					    dep.Version.Major == version.Major &&
					    (dep.Version.Minor > version.Minor ||
					     dep.Version.Minor == version.Minor && dep.Version.Build >= version.Build))
					{
						continue;
					}

					dependenciesSatisfied = false;
					break;
				}

				if (!dependenciesSatisfied) continue;

				var mod = LoadGameMod(info, fs);
				mod.Filesystem?.AssociateWithMod(mod);
				ModManager.Instance.RegisterMod(mod);

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

					var missingDependencies = info.Dependencies.Where(dep =>
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

		ModManager.Instance.InitializeFilesystemBackgroundCleanup();
	}

	private static ModInfo? LoadModInfo(string modFolder, IModFilesystem fs)
	{
		if (!fs.TryOpenFile(Assets.FujiJSON, stream => JsonSerializer.Deserialize(stream, ModInfoContext.Default.ModInfo), out var info))
		{
			Log.Error($"Fuji Error: Tried to load mod from {modFolder} but could not find a {Assets.FujiJSON} file");
			return null;
		}
		if (info != null && !info.IsValid())
		{
			Log.Error($"Fuji Error: Invalid Fuji.json file for {Assets.FujiJSON} in {modFolder}");
			return null;
		}

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

		var assemblyContext = new ModAssemblyLoadContext(info, fs);
		foreach (var assembly in assemblyContext.Assemblies)
		{
			Log.Info($"Loaded assembly file '{assembly}' for mod {info.Id}");
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

		if (loadedMod is null)
		{
			if (anyDllFile)
			{
				Log.Warning($"Mod at {fs.Root} has assemblies, but none of them contain a public type extending from {typeof(GameMod)}.");
			}

			// No GameMod, create a dummy one
			return new DummyGameMod
			{
				ModInfo = info,
				Filesystem = fs
			};
		}
		
		// Load hooks
		foreach (var type in loadedMod.GetType().Assembly.GetTypes())
		{
			FindAndRegisterHooks(type);
		}

		return loadedMod;
	}
	
	private static void FindAndRegisterHooks(Type type)
	{
		// On. hooks
		var onHookMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			.Select(m => (m, m.GetCustomAttribute<InternalOnHookGenTargetAttribute>()))
			.Where(t => t.Item2 != null)
			.Cast<(MethodInfo, InternalOnHookGenTargetAttribute)>();

		foreach (var (info, attr) in onHookMethods)
		{
			Log.Info($"Registering On-hook for method '{attr.Target}' in type '{attr.Target.DeclaringType}' with hook method '{info}'");
			HookManager.Instance.RegisterHook(new Hook(attr.Target, info));
		}
		
		// IL. hooks
		var ilHookMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			.Select(m => (m, m.GetCustomAttribute<InternalILHookGenTargetAttribute>()))
			.Where(t => t.Item2 != null)
			.Cast<(MethodInfo, InternalILHookGenTargetAttribute)>();

		foreach (var (info, attr) in ilHookMethods)
		{
			Log.Info($"Registering IL-hook for method '{attr.Target}' in type '{attr.Target.DeclaringType}' with hook method '{info}'");
			HookManager.Instance.RegisterHook(new Hook(attr.Target, info));
		}
	}
}