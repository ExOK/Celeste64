using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Celeste64.Mod;

public abstract class GameMod
{
	#region Internally Used Data
	internal ModRecord_V01 ModSaveData => Save.GetOrMakeMod(ModInfo.Id);

	// They get set as part of the Mod Loading step, not the constructor.
	internal IModFilesystem Filesystem { get; set; } = null!;
	internal ModInfo ModInfo { get; set; } = null!;

	// Used for storing the assets loaded for this mod specifically.
	internal readonly Dictionary<string, Map> Maps = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Shader> Shaders = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Texture> Textures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Subtexture> Subtextures = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, SkinnedTemplate> Models = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Font> Fonts = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, FMOD.Sound> Sounds = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, FMOD.Sound> Music = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Dictionary<string, string>> Strings = new(StringComparer.OrdinalIgnoreCase);
	internal readonly Dictionary<string, Dictionary<string, List<Language.Line>>> DialogLines = new(StringComparer.OrdinalIgnoreCase);
	internal readonly List<LevelInfo> Levels = new();
	internal bool Loaded = false;

	/// <summary>
	/// Cleanup tasks that have to be performed when this mod gets unloaded.
	/// Called after <see cref="OnModUnloaded"/>, not inside of it,
	/// to make sure that a mod can't accidentally skip calling this due to not calling base.OnModUnloaded.
	/// </summary>
	internal Action? OnUnloadedCleanup { get; private set; }
	#endregion

	// These provide readonly properties to access assets associated with this mod.
	// Modders should not be manually changing these at runtime, which is why they are readonly.
	// This lets you bypass going through the assets system which might be more efficient, and will prioritize loading from this mod.
	// It will also bypass any asset replacements.
	public IReadOnlyDictionary<string, Map> ModMaps => Maps;
	public IReadOnlyDictionary<string, Shader> ModShaders => Shaders;
	public IReadOnlyDictionary<string, Texture> ModTextures => Textures;
	public IReadOnlyDictionary<string, Subtexture> ModSubTextures => Subtextures;
	public IReadOnlyDictionary<string, SkinnedTemplate> ModModels => Models;
	public IReadOnlyDictionary<string, Font> ModFonts => Fonts;
	public IReadOnlyDictionary<string, FMOD.Sound> ModSounds => Sounds;
	public IReadOnlyDictionary<string, FMOD.Sound> ModMusic => Music;
	public IReadOnlyDictionary<string, Dictionary<string, string>> ModStrings => Strings;
	public IReadOnlyDictionary<string, Dictionary<string, List<Language.Line>>> ModDialogLines => DialogLines;
	public IReadOnlyList<LevelInfo> ModLevels => Levels;

	// This is here to give mods easier access to these objects, so they don't have to get them themselves
	// Warning, these may be null if they haven't been initialized yet, so you should always do a null check before using them.
	public Game? Game => Game.Instance;
	public World? World => Game?.World;
	public Map? Map => World?.Map;
	public Player? Player => World?.Get<Player>();

	// Common Metadata about this mod.
	public bool Enabled => this is VanillaGameMod || ModSaveData.Enabled;
	public virtual Type? SettingsType { get; set; }
	public virtual GameModSettings? Settings { get; set; }

	/// <summary>
	/// List of currently used <see cref="ImGuiHandler"/>s by this mod.
	/// </summary>
	public List<ImGuiHandler> ImGuiHandlers = [];

	/// <summary>
	/// List of skins added by this mod.
	/// </summary>
	public readonly List<SkinInfo> Skins = [];

	/// <summary>
	/// List of namespaces for which hook protections should be disabled.
	/// Hooking Fuji or other mods can break and cause issues when they update.
	/// Please consider reaching out to the authors first, so they can provide a stable public API.
	/// By enabling this property, you understand those risks and are aware that your mod might break. 
	/// </summary>
	public readonly List<string> PreventHookProtectionYesIKnowThisIsDangerousAndCanBreak = [];

	#region Save Functions
	// These functions allow modders to save data and get save data from the save file.
	// These are done as wrapper functions mostly to make it harder to accidentally mess up the save data in an unexpected way
	// And so we can change how they work later if needed.

	public string SaveString(string key, string value)
	{
		return ModSaveData.SetString(key, value);
	}
	public string GetString(string key, string defaultValue = "")
	{
		return ModSaveData.GetString(key, defaultValue);
	}
	public int SaveInt(string key, int value)
	{
		return ModSaveData.SetInt(key, value);
	}
	public int GetInt(string key, int defaultValue = 0)
	{
		return ModSaveData.GetInt(key, defaultValue);
	}
	public float SaveFloat(string key, float value)
	{
		return ModSaveData.SetFloat(key, value);
	}
	public float GetFloat(string key, float defaultValue = 0.0f)
	{
		return ModSaveData.GetFloat(key, defaultValue);
	}
	public bool SaveBool(string key, bool value)
	{
		return ModSaveData.SetBool(key, value);
	}
	public bool GetBool(string key, bool defaultValue = false)
	{
		return ModSaveData.GetBool(key, defaultValue);
	}
	#endregion

	#region Mod Settings

	/// <summary>
	/// Save this Mod's Settings.
	/// </summary>
	/// <returns>Whether the settings were saved or not.</returns>
	public bool SaveSettings()
	{
		if (SettingsType == null || Settings == null)
			return false;

		try
		{
			return SaveSettingsForType("Settings.", SettingsType, Settings);
		}
		catch (Exception e)
		{
			Log.Error($"Failed to save the settings of {ModInfo.Id}!");
			Log.Error(e.Message);
			return false;
		}
	}

	public bool SaveSettingsForType(string settingKey, Type type, object instance)
	{
		if (type == null || instance == null)
			return false;

		var props = type.GetProperties();
		foreach (var prop in props)
		{
			object? propValue = prop.GetValue(instance);
			if (propValue is int propInt)
			{
				ModSaveData.SettingsSetInt($"{settingKey}{prop.Name}", propInt);
			}
			else if (propValue is bool propBool)
			{
				ModSaveData.SettingsSetBool($"{settingKey}{prop.Name}", propBool);
			}
			else if (propValue is string propString)
			{
				ModSaveData.SettingsSetString($"{settingKey}{prop.Name}", propString);
			}
			else if (propValue is float propFloat)
			{
				ModSaveData.SettingsSetFloat($"{settingKey}{prop.Name}", propFloat);
			}
			else if (prop.PropertyType.IsEnum)
			{
				int intVal = propValue != null ? (int)propValue : 0;
				ModSaveData.SettingsSetInt($"{settingKey}{prop.Name}", intVal);
			}
			else if (propValue != null && prop.PropertyType.GetCustomAttribute<SettingSubMenuAttribute>() != null)
			{
				SaveSettingsForType($"{settingKey}{prop.Name}.", prop.PropertyType, propValue);
			}
		}
		return true;
	}

	/// <summary>
	/// Load this Mod's Settings.
	/// </summary>
	/// <returns>Whether the settings were loaded or not.</returns>
	public bool LoadSettings()
	{
		if (SettingsType == null || Settings == null)
			return false;

		try
		{
			return LoadSettingsForType("Settings.", SettingsType, Settings);
		}
		catch (Exception e)
		{
			Log.Error($"Failed to save the settings of {ModInfo.Id}!");
			Log.Error(e.Message);
			return false;
		}
	}

	public bool LoadSettingsForType(string settingKey, Type type, object instance)
	{
		if (type == null || instance == null)
			return false;

		var props = type.GetProperties();
		foreach (var prop in props)
		{
			object? propValue = prop.GetValue(instance);
			if (propValue is int propInt)
			{
				prop.SetValue(instance, ModSaveData.SettingsGetInt($"{settingKey}{prop.Name}", propInt));
			}
			else if (propValue is bool propBool)
			{
				prop.SetValue(instance, ModSaveData.SettingsGetBool($"{settingKey}{prop.Name}", propBool));
			}
			else if (propValue is string propString)
			{
				prop.SetValue(instance, ModSaveData.SettingsGetString($"{settingKey}{prop.Name}", propString));
			}
			else if (propValue is float propFloat)
			{
				prop.SetValue(instance, ModSaveData.SettingsGetFloat($"{settingKey}{prop.Name}", propFloat));
			}
			else if (prop.PropertyType.IsEnum)
			{
				int intVal = propValue != null ? (int)propValue : 0;
				prop.SetValue(instance, prop.PropertyType.GetEnumValues().GetValue(ModSaveData.SettingsGetInt($"{settingKey}{prop.Name}", intVal)));
			}
			else if (propValue != null && prop.PropertyType.GetCustomAttribute<SettingSubMenuAttribute>() != null)
			{
				LoadSettingsForType($"{settingKey}{prop.Name}.", prop.PropertyType, propValue);
			}
		}
		return true;
	}

	/// <summary>
	/// This function is responsible for creating mod settings menus from the settings object
	/// It works with AddMenuSettingsForType which is broken out because that calls itself recursively to make submenus.
	/// This can be overloaded by mods to manually initialize the menu instead.
	/// </summary>
	/// <param name="settingsMenu">The menu we are adding settings to.</param>
	public virtual void AddModSettings(ModOptionsMenu settingsMenu)
	{
		if (SettingsType == null || Settings == null)
			return;

		AddMenuSettingsForType(settingsMenu, SettingsType, Settings);
	}

	/// <summary>
	/// Initialize Menu settings for a given type.
	/// This will go through the properties of an object, and automatically convert them to menu items.
	/// </summary>
	/// <param name="menu">The menu we will add the menu items to.</param>
	/// <param name="type">The type of the object we are getting the properties for.</param>
	/// <param name="instance">The instance of the object we want to get the properties from.</param>
	public virtual void AddMenuSettingsForType(Menu menu, Type type, object instance)
	{
		if (type == null || instance == null)
			return;

		var props = type.GetProperties();
		foreach (var prop in props)
		{
			var propType = prop.PropertyType;

			if (prop.GetCustomAttribute<SettingIgnoreAttribute>() != null)
				continue;


			string propName = prop.Name;
			string? nameAttibute = prop.GetCustomAttribute<SettingNameAttribute>()?.Name;
			if (!string.IsNullOrEmpty(nameAttibute))
			{
				// We go through the Mod strings directly to avoid possible naming conflicts with other mods or the vanilla game
				// If the property name can be localized, use that, otherwise, just use the attribute name to make localization optional
				propName = Loc.TryGetModString(this, nameAttibute, out string localizedName) ?
					localizedName :
					nameAttibute;
			}

			Menu.Item? newItem = null;

			if (prop.GetCustomAttribute<SettingSpacerAttribute>() != null)
			{
				menu.Add(new Menu.Spacer());
			}

			bool changingNeedsReload = prop.GetCustomAttribute<SettingNeedsReloadAttribute>() != null;

			string? subheader = prop.GetCustomAttribute<SettingSubHeaderAttribute>()?.SubHeader;
			if (!string.IsNullOrEmpty(subheader))
			{
				string subHeader = Loc.TryGetModString(this, subheader, out string localizedSubHeader) ?
					localizedSubHeader :
					subheader;
				menu.Add(new Menu.SubHeader((Loc.Unlocalized)subHeader));
			}

			if (propType.IsEnum)
			{
				newItem = new Menu.MultiSelect(
					(Loc.Unlocalized)propName,
					propType.GetEnumNames().ToList(),
					() =>
					{
						object? val = prop.GetValue(instance);
						int intVal = val != null ? (int)val : 0;
						return intVal;
					},
					(int value) =>
					{
						object? newValue = propType.GetEnumValues().GetValue(value);
						prop.SetValue(instance, propType.GetEnumValues().GetValue(value));
						OnModSettingChanged(prop.Name, newValue, changingNeedsReload);
					}
				);
			}
			else if (propType == typeof(int))
			{
				int min = 0;
				int max = 10;
				var settingRangeAttribute = prop.GetCustomAttribute<SettingRangeAttribute>();
				if (settingRangeAttribute != null && settingRangeAttribute.Max > settingRangeAttribute.Min)
				{
					min = settingRangeAttribute.Min;
					max = settingRangeAttribute.Max;
				}
				newItem = new Menu.Slider(
					(Loc.Unlocalized)propName,
					min,
					max,
					() => prop.GetValue(instance) as int? ?? 0,
					(int value) =>
					{
						prop.SetValue(instance, value);
						OnModSettingChanged(prop.Name, value, changingNeedsReload);
					}
				);
			}
			else if (propType == typeof(bool))
			{
				newItem = new Menu.Toggle(
					(Loc.Unlocalized)propName,
					() =>
					{
						bool newValue = !(prop.GetValue(instance) as bool? ?? false);
						prop.SetValue(instance, newValue);
						OnModSettingChanged(prop.Name, newValue, changingNeedsReload);
					},
					() => prop.GetValue(instance) as bool? ?? false
				);
			}
			else if (prop.PropertyType.GetCustomAttribute<SettingSubMenuAttribute>() != null)
			{
				object? value = prop.GetValue(instance);
				if (value != null)
				{
					var subMenu = new Menu(menu.RootMenu) { Title = propName };
					AddMenuSettingsForType(subMenu, prop.PropertyType, value);
					subMenu.Add(new Menu.Option((Loc.Unlocalized)"Back", () =>
						{
							if (menu != null)
							{
								menu.PopRootSubMenu();
							}
						}));
					newItem = new Menu.Submenu(
						(Loc.Unlocalized)propName,
						menu.RootMenu,
						subMenu
					);
				}
			}

			if (newItem != null)
			{
				string? propDescription = prop.GetCustomAttribute<SettingDescriptionAttribute>()?.Description;
				if (!string.IsNullOrEmpty(propDescription))
				{
					propDescription = Loc.TryGetModString(this, propDescription, out string localizedDescription) ?
						localizedDescription :
						propDescription;
					newItem.Describe((Loc.Unlocalized)propDescription);
				}
				menu.Add(newItem);
			}
		}
	}

	/// <summary>
	/// This gets called when a mod setting is changed.
	/// </summary>
	/// <param name="settingName"> The name of the setting that changed</param>
	/// <param name="value"> The value the setting is changing to.</param>
	/// <param name="needsReload"> Whether this setting should relauch the game</param>
	public virtual void OnModSettingChanged(string settingName, object? newValue, bool needsReload)
	{
		if (needsReload)
		{
			Game.Instance.NeedsReload = true;
		}
	}

	#endregion

	/// <summary>
	/// Get all mods which depend on this mod.
	/// </summary>
	public List<GameMod> GetDependents()
	{
		var depMods = new List<GameMod>();

		foreach (var mod in ModManager.Instance.Mods)
		{
			if (mod.ModInfo.Dependencies.ContainsKey(ModInfo.Id) && mod.Enabled)
			{
				depMods.Add(mod);
			}
		}

		return depMods;
	}

	/// <summary>
	/// Disables the mod "safely" (accounts for dependent mods, etc.)
	/// If it returns true, this means it is not safe to disable the mod.
	/// You should first simulate the operation with DisableSafe(true).
	/// If it is not safe to disable the mod (if the function returns true), it's recommended that you don't go through with it.
	/// </summary>
	/// <param name="simulate"></param>
	public bool DisableSafe(bool simulate)
	{
		bool shouldEvac = false;

		foreach (var dependent in this.GetDependents())
		{
			if (!simulate)
			{
				Save.GetOrMakeMod(dependent.ModInfo.Id).Enabled = false;
			}

			if (dependent == ModManager.Instance.CurrentLevelMod)
			{
				shouldEvac = true;
			} // We'll want to adjust behaviour if the current level's parent mod must be disabled.
		}

		if (shouldEvac && !simulate)
		{
			Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Replace,
				Scene = () => new Titlescreen(),
				ToPause = true,
				ToBlack = new AngledWipe(),
				PerformAssetReload = true
			});
		} // If necessary, evacuate to main menu!!

		return shouldEvac;
	}

	/// <summary>
	/// Enables the mod's dependencies.
	/// </summary>
	public void EnableDependencies()
	{
		foreach (var dep in ModInfo.Dependencies.Keys.ToList())
		{
			Save.GetOrMakeMod(dep).Enabled = true;
		}
	}

	/// <summary>
	/// This allows modders to add their own actors to the actor factory system.
	/// This can also be used to replace existing actors, but be warned that only one mod can replace something at a time.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="factory"></param>
	public void AddActorFactory(string name, Map.ActorFactory factory)
	{
		if (Map.ModActorFactories.TryAdd(name, factory))
		{
			OnUnloadedCleanup += () => Map.ModActorFactories.Remove(name);
		}
		else
		{
			Log.Warning($"An actor factory with the name {name} was already loaded. Factory won't be loaded.");
		}
	}


	// Passthrough functions to simplify adding Hooks to the Hook Manager.
	public static void RegisterHook(Hook hook) => HookManager.Instance.RegisterHook(hook);
	public static void RegisterILHook(ILHook iLHook) => HookManager.Instance.RegisterILHook(iLHook);
	public static void RemoveHook(Hook hook) => HookManager.Instance.RemoveHook(hook);
	public static void RemoveILHook(ILHook iLHook) => HookManager.Instance.RemoveILHook(iLHook);

	/// <summary>
	/// Registers the provided custom player state,
	/// and ensures it will be deregistered once the mod unloads.
	/// </summary>
	public void AddPlayerState<T>() where T : CustomPlayerState, new()
	{
		CustomPlayerStateRegistry.Register<T>();
		OnUnloadedCleanup += CustomPlayerStateRegistry.Deregister<T>;
	}

	#region Game Events


	// Game Event Functions. These are used to provide an "interface" of sorts that mods can easily override.
	// They will not be called if the mod is disabled.

	/// <summary>
	/// Called when the Mod is first loaded, or when it becomes enabled
	/// </summary>
	public virtual void OnModLoaded() { }

	/// <summary>
	/// Called when a mod is unloaded, or when it becomes disabled
	/// </summary>
	public virtual void OnModUnloaded() { }

	/// <summary>
	/// Called once every frame
	/// </summary>
	/// <param name="deltaTime">How much time passed since the previous update</param>
	public virtual void Update(float deltaTime) { }

	/// <summary>
	/// Called at the very beginning of when the game is loaded
	/// </summary>
	/// <param name="game"></param>
	public virtual void OnGameLoaded(Game game) { }

	/// <summary>
	/// Called after all assets have been loaded or reloaded.
	/// </summary>
	public virtual void OnAssetsLoaded() { }

	/// <summary>
	/// Called right before the Map load starts.
	/// This is probably the ideal place to register custom mod actors
	/// </summary>
	/// <param name="world">A reference to the world</param>
	/// <param name="map">A reference to the map that was loaded</param>
	public virtual void OnPreMapLoaded(World world, Map map) { }

	/// <summary>
	/// Called after a map is finished loading.
	/// </summary>
	/// <param name="map">A reference to the map that was loaded</param>
	public virtual void OnMapLoaded(Map map) { }

	/// <summary>
	/// Called after a scene transistion either when a scene is first loaded, or reloaded.
	/// </summary>
	/// <param name="scene">A reference to the Scene that was entered</param>
	public virtual void OnSceneEntered(Scene scene) { }

	/// <summary>
	/// Called after the world finishes loading.
	/// </summary>
	/// <param name="world">A reference to the World object that was created</param>
	public virtual void OnWorldLoaded(World world) { }

	/// <summary>
	/// Called whenever a new actor is first created.
	/// </summary>
	/// <param name="actor">A reference to the Actor that was created.</param>
	public virtual void OnActorCreated(Actor actor) { }

	/// <summary>
	/// Called after an actor is actually added to the world.
	/// </summary>
	/// <param name="actor">A reference to the Actor that was added</param>
	public virtual void OnActorAdded(Actor actor) { }

	/// <summary>
	/// Called when an actor is destroyed.
	/// </summary>
	/// <param name="actor">A reference to the actor that was destroyed</param>
	public virtual void OnActorDestroyed(Actor actor) { }

	/// <summary>
	/// Called when the player is killed
	/// </summary>
	/// <param name="player">A reference to the player</param>
	public virtual void OnPlayerKilled(Player player) { }

	/// <summary>
	/// Called whenever a player lands on the ground.
	/// </summary>
	/// <param name="player">A reference to the player</param>
	public virtual void OnPlayerLanded(Player player) { }

	/// <summary>
	/// Called whenever a player jumps.
	/// </summary>
	/// <param name="player">A reference to the player</param>
	public virtual void OnPlayerJumped(Player player, Player.JumpType jumpType) { }

	/// <summary>
	/// Called whenever the player's state changes
	/// </summary>
	/// <param name="player">A reference to the player</param>
	/// <param name="state">The new state</param>
	public virtual void OnPlayerStateChanged(Player player, Player.States? state) { }

	/// <summary>
	/// Called when the current skin is changed.
	/// </summary>
	/// <param name="player">A reference to the player</param>
	/// <param name="skin">The new skin that this changed to</param>
	public virtual void OnPlayerSkinChange(Player player, SkinInfo skin) { }

	/// <summary>
	/// Called whenever an item is pickuped up by the player
	/// </summary>
	/// <param name="player">The player that picked up the item</param>
	/// <param name="item">The IPickup item that was picked up</param>
	public virtual void OnItemPickup(Player player, IPickup item) { }

	#endregion
}
