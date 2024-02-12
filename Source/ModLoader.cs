using System.Reflection;
using System.Text.Json;

namespace Celeste64
{
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
			// Mod Infos are required now, so make a dummy mod info for the valilla game too. This shouldn't really be used for anything.
			ModInfo vanillaModInfo = new ModInfo()
			{
				Id = "Celeste64Vanilla",
				Name = "Celese 64: Fragments of the Mountains",
				Version = "1.0.0"
			};
			VanillaGameMod vanillaMod = new VanillaGameMod()
			{
				ModInfo = vanillaModInfo,
				Filesystem = new FolderModFilesystem(Assets.ContentPath)
			};

			List<GameMod> mods =
			[
				// Load vanilla as a mod, to unify all asset loading code
				vanillaMod
			];
			ModManager.Instance.VanillaGameMod = vanillaMod;

			// Find all mods in directories:
			foreach (var modDir in Directory.EnumerateDirectories(ModFolderPath))
			{
				var modName = Path.GetFileNameWithoutExtension(modDir)!; // Todo: read from some metadata file
				var fs = new FolderModFilesystem(modDir);

				mods.Add(LoadGameMod(modName, fs));
				Log.Info($"Loaded mod from directory: {modName}");
			}

			// Find all mods in zips:
			foreach (var modZip in Directory.EnumerateFiles(ModFolderPath, "*.zip"))
			{
				var modName = Path.GetFileNameWithoutExtension(modZip)!; // Todo: read from some metadata file
				var fs = new ZipModFilesystem(modZip);

				mods.Add(LoadGameMod(modName, fs));
				Log.Info($"Loaded mod from zip: {modName}");
			}

			ModManager.Instance.Unload();
			// We've collected all the mods now, time to initialize them
			foreach (var mod in mods)
			{
				mod.Filesystem?.AssociateWithMod(mod);
				ModManager.Instance.RegisterMod(mod);
			}

			ModManager.Instance.InitializeFilesystemBackgroundCleanup();
		}

		private static GameMod LoadGameMod(string modFolder, IModFilesystem fs)
		{
			GameMod? loadedMod = null;
			var anyDllFile = false;

			string fujiJsonFileName = "";
			var candidateFiles = fs.FindFilesInDirectoryRecursive("", "json");

			foreach (var jsonFileName in candidateFiles) {
				if (jsonFileName.ToLower() == "fuji.json") {
					fujiJsonFileName = jsonFileName;
					break;
				}
			}

			ModInfo modInfo;
			if (fs.TryOpenFile(fujiJsonFileName,
				stream => JsonSerializer.Deserialize(stream, ModInfoContext.Default.ModInfo),
				out var info))
			{
				modInfo = info;
				if (!modInfo.IsValid())
				{
					throw new Exception($"Fuji Exception: Invalid Fuji.json file for {modFolder}");
				}
			}
			else
			{
				throw new Exception($"Fuji Exception: Tried to load mod {modFolder} but could not find a valid Fuji.json file");
			}

			foreach (var dllFile in fs.FindFilesInDirectoryRecursive("DLLs", "dll"))
			{
				if (!fs.TryOpenFile(dllFile, stream => Assembly.Load(stream.ReadAllToByteArray()), out var asm))
				{
					continue;
				}
				
				Log.Info($"Loaded mod .dll file: {dllFile}");

				anyDllFile = true;
				
				foreach (var type in asm.GetExportedTypes())
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
					instance.ModInfo = modInfo;
				}
			}

			if (loadedMod is not { })
			{
				if (anyDllFile)
				{
					Log.Warning($"Mod at {fs.Root} has dll files, but none of them contain a type extending from {typeof(GameMod)}.");
				}
			
				// No GameMod, create a dummy one
				loadedMod = new DummyGameMod()
				{
					ModInfo = modInfo,
					Filesystem = fs
				};
			}
			
			return loadedMod;
		}
	}
}
