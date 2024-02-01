using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static Celeste64.Assets;

namespace Celeste64
{
	public static class ModLoader
	{
		public const string ModFolder = "Mods";

		private static string? modFolderPath = null;

		public static string ModFolderPath
		{
			get
			{
				if (modFolderPath == null)
				{
					var baseFolder = AppContext.BaseDirectory;
					var searchUpPath = "";
					int up = 0;
					while (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModFolder)) && up++ < 6)
						searchUpPath = Path.Join(searchUpPath, "..");
					if (!Directory.Exists(Path.Join(baseFolder, searchUpPath, ModFolder)))
						throw new Exception($"Unable to find {ModFolder} Directory from '{baseFolder}'");
					modFolderPath = Path.Join(baseFolder, searchUpPath, ModFolder);
				}

				return modFolderPath;
			}
		}

        [RequiresUnreferencedCode("Uses Reflection to load mod DLLs")]
        internal static void RegisterAllMods()
		{
			List<GameMod> mods =
			[
				// Load vanilla as a mod, to unify all asset loading code
				new VanillaGameMod
				{
					Filesystem = new FolderModFilesystem(ContentPath),
				}
			];

			// Find all mods in directories:
			foreach (var modDir in Directory.EnumerateDirectories(ModFolderPath))
			{
				var modName = Path.GetFileNameWithoutExtension(modDir)!; // Todo: read from some metadata file
				var fs = new FolderModFilesystem(modDir);

				mods.Add(LoadMod(modName, fs));
				Log.Info($"Loaded mod from directory: {modName}");
			}

			// Find all mods in zips:
			foreach (var modZip in Directory.EnumerateFiles(ModFolderPath, "*.zip"))
			{
				var modName = Path.GetFileNameWithoutExtension(modZip)!; // Todo: read from some metadata file
				var fs = new ZipModFilesystem(modZip);

				mods.Add(LoadMod(modName, fs));
				Log.Info($"Loaded mod from zip: {modName}");
			}

			ModManager.Instance.Unload();
			// We've collected all the mods now, time to initialize them
			foreach (var mod in mods)
			{
				mod.Filesystem.AssociateWithMod(mod);
				ModManager.Instance.RegisterMod(mod);
			}

			ModManager.Instance.InitializeFilesystemBackgroundCleanup();
		}

		[RequiresUnreferencedCode("Uses Reflection to load mod DLLs")]
		private static GameMod LoadMod(string modName, IModFilesystem fs)
		{
			GameMod? loadedMod = null;
			var anyDllFile = false;
				
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
				}
			}

			if (loadedMod is not { })
			{
				if (anyDllFile)
				{
					Log.Warning($"Mod at {fs.Root} has dll files, but none of them contain a type extending from {typeof(GameMod)}.");
				}
			
				// No GameMod, create a dummy one
				loadedMod = new DummyGameMod();
			}

			loadedMod.Filesystem = fs;
			loadedMod.modName = modName;
			
			return loadedMod;
		}
	}
}
