
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Celeste64.Mod;

/// <summary>
/// Stores Meta-Info about a specific Mod
/// </summary>
public class ModInfo
{
	/// <summary>
	/// The mod's unique identifier. Must only be alpha-numeric + underscore.
	/// </summary>
	public string Id { get; set; } = "";

	/// <summary>
	/// The mod's display name.
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// The mod's version.
	/// </summary>
	[JsonIgnore]
	public Version Version { get; internal set; } = null!;

	/// <summary>
	/// The mod's version string. Can contain an optional suffix after a hyphen (-).
	/// </summary>
	[JsonPropertyName("Version")]
	public string VersionString
	{
		get => _versionString;
		set
		{
			_versionString = value;
			int splitIdx = value.IndexOf('-');
			if (splitIdx == -1)
				Version = new Version(value);
			else
				Version = new Version(value[..splitIdx]);

		}
	}
	private string _versionString { get; set; } = "";

	/// <summary>
	/// (Optional) The mod's author.
	/// </summary>
	public string? ModAuthor { get; set; }

	/// <summary>
	/// (Optional) The mod's description.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// (Optional) The mod's icon path.
	/// </summary>
	public string? Icon { get; set; }

	/// <summary>
	/// (Optional) The mod's minimum Fuji version.
	/// </summary>
	public string? FujiRequiredVersion { get; set; }

	/// <summary>
	/// (Optional) The mod's dependencies with ModID -> MinimumVersion.
	/// </summary>
	public Dictionary<string, string> Dependencies { get; set; } = new();

	/// <summary>
	/// (Optional) The mod's asset replacements with Original -> Overwrite.
	/// </summary>
	public Dictionary<string, string> AssetReplaceItems { get; set; } = new();

	[JsonIgnore]
	internal ModAssemblyLoadContext? AssemblyContext = null;

	private static readonly Regex IdRegex = new("[a-zA-z0-9_]", RegexOptions.Compiled);

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(VersionString) &&
			   IdRegex.IsMatch(Id) && Id != "_";
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ModInfo))]
internal partial class ModInfoContext : JsonSerializerContext { }
