using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Celeste64.Mod;

public static class ModLoader
{
	public const string ModsFolder = "Mods";

	private static string[]? modsFolderPaths = null;

	internal static List<string> FailedToLoadMods = [];

	public static string[] ModFolderPaths
	{
		get
		{
			if (modsFolderPaths == null)
			{
				var baseFolder = AppContext.BaseDirectory;
				var searchUpPath = "";
				int up = 0;
				while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModsFolder)) && up++ < 6)
					searchUpPath = Path.Join(searchUpPath, "..");
				if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModsFolder)))
					throw new Exception($"Unable to find {ModsFolder} Directory from '{baseFolder}'");
				var modsFolderPath = Path.Join(baseFolder, searchUpPath, ModsFolder);
				var userModsFolderPath = Path.Join(App.UserPath, ModsFolder);
				try
				{
					Directory.CreateDirectory(userModsFolderPath);
					modsFolderPaths = [modsFolderPath, userModsFolderPath];
				}
				catch
				{
					modsFolderPaths = [modsFolderPath];
				}
			}

			return modsFolderPaths;
		}
	}

	internal static void RegisterAllMods()
	{
		FailedToLoadMods.Clear();
		ModManager.Instance.VanillaGameMod = new VanillaGameMod
		{
			// Mod Infos are required now, so make a dummy mod info for the valilla game too. This shouldn't really be used for anything.
			ModInfo = new ModInfo
			{
				Id = "Celeste64Vanilla",
				Name = "Celeste 64: Fragments of the Mountain",
				VersionString = "1.1.1",
			},
			Filesystem = new FolderModFilesystem(Assets.ContentPath)
		};

		Log.Info($"Loading mods from: \n- {String.Join("\n- ", ModFolderPaths)}");

		List<(ModInfo ModInfo, IModFilesystem ModFs)> modInfos = [];

		// Find all mods in directories:
		foreach (var modDir in ModFolderPaths.SelectMany(path => Directory.EnumerateDirectories(path)))
		{
			var modName = Path.GetFileNameWithoutExtension(modDir)!; // Todo: read from some metadata file
			var fs = new FolderModFilesystem(modDir);

			var info = LoadModInfo(modName, fs);
			if (info != null)
			{
				if (info.Id == "Celeste64Vanilla" || modInfos.Any(data => data.Item1.Id == info.Id))
				{
					FailedToLoadMods.Add(modName);
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
		foreach (var modZip in ModFolderPaths.SelectMany(path => Directory.EnumerateFiles(path, "*.zip")))
		{
			var modName = Path.GetFileNameWithoutExtension(modZip)!; // Todo: read from some metadata file
			var fs = new ZipModFilesystem(modZip);

			var info = LoadModInfo(modName, fs);
			if (info != null)
			{
				if (info.Id == "Celeste64Vanilla" || modInfos.Any(data => data.Item1.Id == info.Id))
				{
					FailedToLoadMods.Add(modName);
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

		// Sort the mods by their ID alphabetically before loading.
		// This helps us ensure some level of consistency/determinism to hopefully avoid quirks in behaviour.
		modInfos = [.. modInfos.OrderBy(mod => mod.ModInfo.Id)];
		modInfos.Reverse(); // Reverse alphabetical -> alphabetical

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

				try
				{
					var mod = LoadGameMod(info, fs);
					mod.Filesystem?.AssociateWithMod(mod);
					
					try
					{
						ModManager.Instance.RegisterMod(mod);

						// Load hooks after the mod has been registered
						foreach (var type in mod.GetType().Assembly.GetTypes())
						{
							FindAndRegisterHooks(info, type);
						}
					}
					catch
					{
						// Perform cleanup
						ModManager.Instance.DeregisterMod(mod);
						HookManager.Instance.ClearHooksOfMod(info);
						throw;
					}
					
					loaded.Add(info);
					loadedModInIteration = true;
				}
				catch (Exception ex)
				{
					FailedToLoadMods.Add(info.Id);
					Log.Error($"Fuji Error: An error occurred while trying to load mod: {info.Id}");
					Log.Error(ex.ToString());
				}

				modInfos.RemoveAt(i);
			}

			if (!loadedModInIteration)
			{
				// This means that all infos left don't have their dependencies met
				// Handle this by adding them to the FailedToLoadMods list and logging an error.
				// Then break out of the loop so we can continue.
				foreach (var (info, _) in modInfos)
				{
					FailedToLoadMods.Add(info.Id);
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
				break;
			}
		}

		ModManager.Instance.InitializeFilesystemBackgroundCleanup();

		// Finally, log all loaded mods to the console
		StringBuilder modListString = new();

		modListString.Append("Mods:\n\n");

		foreach (GameMod mod in ModManager.Instance.Mods)
		{
			modListString.Append($"- [{(mod.Enabled ? "X" : " ")}] {mod.ModInfo.Id}, v{mod.ModInfo.Version}\n");
		}

		Log.Info(modListString);
	}

	private static ModInfo? LoadModInfo(string modFolder, IModFilesystem fs)
	{
		if (!fs.TryOpenFile(Assets.FujiJSON, stream => JsonSerializer.Deserialize(stream, ModInfoContext.Default.ModInfo), out var info))
		{
			FailedToLoadMods.Add(modFolder);
			Log.Error($"Fuji Error: Tried to load mod from {modFolder} but could not find a {Assets.FujiJSON} file");
			return null;
		}
		if (info != null && !info.IsValid())
		{
			FailedToLoadMods.Add(modFolder);
			Log.Error($"Fuji Error: Invalid Fuji.json file for {Assets.FujiJSON} in {modFolder}");
			return null;
		}

		return info;
	}

	private static GameMod LoadGameMod(ModInfo info, IModFilesystem fs)
	{
		bool modEnabled = ModSettings.GetOrMakeModSettings(info.Id).Enabled;

		GameMod? loadedMod = null;
		GameModSettings? loadedModSettings = null;
		Type? loadedModSettingsType = null;
		var anyDllFile = false;

		info.AssemblyContext = new ModAssemblyLoadContext(info, fs);
		foreach (var assembly in info.AssemblyContext.Assemblies)
		{
			Log.Info($"Loaded assembly file '{assembly}' for mod {info.Id}");
			anyDllFile = true;

			foreach (var type in assembly.GetExportedTypes())
			{
				if (type.BaseType == typeof(GameMod))
				{
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
				else if (type.BaseType == typeof(GameModSettings))
				{
					if (loadedModSettings is { })
					{
						Log.Error($"Mod at {fs.Root} contains multiple classes extending from {typeof(GameModSettings)} " +
								  $"[{loadedModSettings.GetType().FullName} vs {type.FullName}]! Only the first one will be used!");
						continue;
					}

					if (Activator.CreateInstance(type) is not GameModSettings instance)
						continue;

					loadedModSettings = instance;
					loadedModSettingsType = type;
				}
			}
		}

		if (loadedMod is null || !modEnabled)
		{
			if (loadedMod is null && anyDllFile)
			{
				Log.Warning($"Mod at {fs.Root} has assemblies, but none of them contain a public type extending from {typeof(GameMod)}.");
			}

			// Either no GameMod found or mod was disabled, so make a dummy one
			loadedMod = new DummyGameMod
			{
				ModInfo = info,
				Filesystem = fs
			};
		}

		if (loadedModSettings != null && loadedModSettingsType != null)
		{
			loadedMod.SettingsType = loadedModSettingsType;
			loadedMod.Settings = loadedModSettings;
			loadedMod.LoadSettings();
		}

		return loadedMod;
	}

	private static void FindAndRegisterHooks(ModInfo modInfo, Type type)
	{
		List<IDisposable> hooks = [];
		
		try
		{
			// On. hooks
			var onHookMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Select(m => (m, m.GetCustomAttribute<InternalOnHookGenTargetAttribute>()))
				.Where(t => t.Item2 != null)
				.Cast<(MethodInfo, InternalOnHookGenTargetAttribute)>();
			
			foreach (var (info, attr) in onHookMethods)
			{
				var onHook = new Hook(attr.Target, info);
				hooks.Add(onHook);
				HookManager.Instance.RegisterHook(onHook, modInfo);
			}
			
			// IL. hooks
			var ilHookMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Select(m => (m, m.GetCustomAttribute<InternalILHookGenTargetAttribute>()))
				.Where(t => t.Item2 != null)
				.Cast<(MethodInfo, InternalILHookGenTargetAttribute)>();
			
			foreach (var (info, attr) in ilHookMethods)
			{
				var ilHook = new ILHook(attr.Target, info.CreateDelegate<ILContext.Manipulator>());
				hooks.Add(ilHook);
				HookManager.Instance.RegisterILHook(ilHook, modInfo);
			}
		}
		catch
		{
			// Some hook failed. Need to dispose all previous ones
			foreach (var hook in hooks)
				hook.Dispose();
			
			throw;
		}
	}
}
