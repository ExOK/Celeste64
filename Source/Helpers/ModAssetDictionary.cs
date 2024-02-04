using Sledge.Formats.Map.Objects;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Path = System.IO.Path;

namespace Celeste64;
public class ModAssetDictionary<V> : IEnumerable<KeyValuePair<string, V>>
{
	public delegate Dictionary<string, V> GetDictionary(GameMod mod);

	private GetDictionary getDictionary;

	public ModAssetDictionary(GetDictionary getDictionary)
	{
		this.getDictionary = getDictionary;
	}

	/// <summary>
	/// Clear out all the assets of this type for every mod.
	/// </summary>
	public void Clear()
	{
		foreach (var mod in ModManager.Instance.EnabledMods)
		{
			getDictionary(mod).Clear();
		}
	}

	/// <summary>
	/// Get the correct asset based on a given string key. See get function for more details
	/// </summary>
	/// <param name="key">The key we're trying to match to.</param>
	/// <returns></returns>
	public V this[string key]
	{
		get
		{
			return Get(key);
		}
	}

	/// <summary>
	/// This function gets a given asset base on a given string key following some complicated matching logic.
	/// The priority for how it determines what to return is as follows:
	/// - If the key includes a namespace and the namespace is an empty string, prioritize pulling the asset from the current map's mod's asset files
	/// - If the key includes a namespace and the namespace is an underscore, prioritize pulling the asset from the vanilla game's asset files
	/// - If the key includes a namespace and the namespace is a string, prioritize pulling the asset from the mod with a mod folder name that matches that string.
	/// - If the key has any asset replacements for the key, get the asset from the asset replacement. Prioritize the current map's mod's assets first, then look at other mods.
	/// - If the asset exists in the current map's mod's asset files, use that asset 
	/// - If the asset exists in the vanilla game's asset files, use that asset 
	/// - If the asset exists in the another mod's asset files, use that asset (It will pull in load order, which may be unreliable)
	/// - Otherwise, throw an exception because their is no asset for the given key.
	/// </summary>
	/// <param name="stringKey">The key we're trying to match against. Can also include optional namespace info.</param>
	/// <returns>the asset that corresponds to the given key based on the matching logic</returns>
	/// <exception cref="KeyNotFoundException"></exception>
	public V Get(string stringKey)
	{
		string[] splitkey = stringKey.Split(":");

		string? modName = null;
		string key = stringKey;

		if(splitkey.Length == 2)
		{
			modName = splitkey[0];
			key = splitkey[1];
		}

		if(modName != null)
		{
			if (
				modName == "" &&
				ModManager.Instance.CurrentLevelMod != null && 
				getDictionary(ModManager.Instance.CurrentLevelMod).TryGetValue(key, out V? currentModValue) &&
				currentModValue != null
			)
			{
				return currentModValue;
			}
			else if (
				modName == "_" &&
				ModManager.Instance.VanillaGameMod != null
				&& getDictionary(ModManager.Instance.VanillaGameMod).TryGetValue(key, out V? vanillaValue) &&
				vanillaValue != null
			)
			{
				return vanillaValue;
			}
			else
			{
				GameMod? targetMod = ModManager.Instance.EnabledMods.FirstOrDefault(mod => mod.ModInfo?.Id == modName);
				if (targetMod != null 
					&& getDictionary(targetMod).TryGetValue(key, out V? targetModValue)
					&& targetModValue != null)
				{
					return targetModValue;
				}
			}
		}

		if (TryGetAssetReplaceForKey(key, out V? assetReplaceValue) && assetReplaceValue != null)
		{
			return assetReplaceValue;
		}

		if (
			ModManager.Instance.CurrentLevelMod != null &&
			getDictionary(ModManager.Instance.CurrentLevelMod).TryGetValue(key, out V? currentModAsset) &&
			currentModAsset != null
		)
		{
			return currentModAsset;
		}
		else
		{
			// Note: This assumes the vanilla game will always be loaded as the first mod, so vanilla game assets take priority
			foreach (GameMod mod in ModManager.Instance.EnabledMods)
			{
				if (getDictionary(mod).TryGetValue(key, out V? modValue) && modValue != null)
				{
					return modValue;
				}
			}
		}

		throw new KeyNotFoundException(key);
	}

	/// <summary>
	/// Tries to get an asset if there are any asset replacement assets that coorespond with the given key.
	/// Prioritizes the currently loaded maps's assets first, then the rest of the mods asset replacements.
	/// The vanilla game will never have asset replacements, so it is skipped.
	/// If no asset replacement is found, return false.
	/// </summary>
	/// <param name="key">key we are matching against</param>
	/// <param name="asset"></param>
	/// <returns></returns>
	private bool TryGetAssetReplaceForKey(string key, [MaybeNullWhen(false)] out V asset)
	{
		if (TryGetAssetReplaceForKeyInMod(key, ModManager.Instance.CurrentLevelMod, out V? currentModAsset))
		{
			asset = currentModAsset;
			return true;
		}

		foreach (var mod in ModManager.Instance.EnabledMods)
		{
			if (
				mod != ModManager.Instance.CurrentLevelMod
				&& mod != ModManager.Instance.VanillaGameMod
				&& TryGetAssetReplaceForKeyInMod(key, mod, out V? modAsset)
			)
			{
				asset = modAsset;
				return true;
			}
		}

		asset = default;
		return false;
	}

	/// <summary>
	/// Looks through a mods list of asset replacements.
	/// If the given key matches any asset replacements, and the new asset exists in the given mod's asset files, 
	///     return true and the mod replacement asset
	/// If the given key matches any asset replacements, and the new asset exists in the vanilla game's asset files,
	///     return true the vanilla replacement asset
	/// Otherwise, return false because there is no valid asset replacement for this mod.
	/// </summary>
	private bool TryGetAssetReplaceForKeyInMod(string key, GameMod? mod, [MaybeNullWhen(false)] out V asset)
	{
		if (mod != null && mod.ModInfo != null && mod.ModInfo.AssetReplaceItems != null
			&& mod.ModInfo.AssetReplaceItems.TryGetValue(key, out string? modKey)
			&& modKey != null
		)
		{
			if (getDictionary(mod).TryGetValue(modKey, out V? modAsset) && modAsset != null)
			{
				asset = modAsset;
				return true;
			}
			else if (ModManager.Instance.VanillaGameMod != null
				&& getDictionary(ModManager.Instance.VanillaGameMod).TryGetValue(modKey, out V? vanillaValue)
				&& vanillaValue != null)
			{
				asset = vanillaValue;
				return true;
			}
		}

		asset = default;
		return false;
	}

	/// <summary>
	/// Returns true if any mod has loaded an asset cooresponding to this key, or if there are any asset replacements for that match this key.
	/// Otherwise, return false.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public bool ContainsKey(string key)
	{
		string[] split = key.Split(":");
		foreach (GameMod mod in ModManager.Instance.EnabledMods)
		{
			if(getDictionary(mod).ContainsKey(key) || (split.Length == 2 && getDictionary(mod).ContainsKey(split[1])))
			{
				return true;
			}

			if (TryGetAssetReplaceForKeyInMod(key, mod, out _))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Add a new asset to the correct dictionary in a given mod.
	/// </summary>
	/// <param name="key">The key associated with the asset</param>
	/// <param name="value">The asset to add</param>
	/// <param name="mod">The mod this sasset comes from</param>
	public void Add(string key, V value, GameMod mod)
	{
		getDictionary(mod).Add(key, value);
	}

	/// <summary>
	/// The first asset in any dictionary in any mods.
	/// Will probably come from the vanilla game if it's available.
	/// </summary>
	/// <returns></returns>
	public KeyValuePair<string, V> First()
	{
		return getDictionary(ModManager.Instance.EnabledMods.First(mod => getDictionary(mod).Any())).First();
	}

	/// <summary>
	/// If there is an asset cooresponding to this key, return true and the asset. Uses the same Get and Contains logic from about.
	/// Otherwise, return false.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public bool TryGetValue(string key, [MaybeNullWhen(false)] out V value)
	{
		if(ContainsKey(key))
		{
			value = this[key];
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// Similar to TryGetValue, but accepts a full path, instead of just the main key.
	/// Used to pass through namespace data.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public bool TryGetValueFromFullPath(string path, [MaybeNullWhen(false)] out V value)
	{
		string[] parts = path.Split(":");
		string? prop;
		if (parts.Length == 2)
		{
			prop = $"{parts[0]}:{Path.GetFileNameWithoutExtension(parts[1])}";
		}
		else
		{
			prop = Path.GetFileNameWithoutExtension(path);
		}
		if (TryGetValue(prop, out V? asset))
		{
			value = asset;
			return true;
		}

		value = default;
		return false;
	}

	/// <summary>
	/// If there is an asset cooresponding to this key, return the asset. Uses the same Get and Contains logic from about.
	/// Otherwise, return the default value.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public V? GetValueOrDefault(string key)
	{
		if (ContainsKey(key))
		{
			return this[key];
		}

		return default;
	}

	public int Count { get { return ModManager.Instance.EnabledMods.SelectMany(mod => getDictionary(mod)).Count(); } }

	/// <summary>
	/// Used to iterate through all assets for this type in all mods.
	/// </summary>
	/// <returns></returns>
	public IEnumerator<KeyValuePair<string, V>> GetEnumerator()
	{
		return ModManager.Instance.EnabledMods.SelectMany(mod => getDictionary(mod)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}