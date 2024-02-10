
using System.Text.Json.Serialization;

namespace Celeste64;

/// <summary>
/// Stores Meta-Info about a specific skin
/// </summary>
public class SkinInfo
{
	public string Name { get; set; } = "";
	public string Model { get; set; } = "";
	public bool HideHair { get; set; } = false;
	public string CollectableId { get; set; } = "";
	public int HairNormal { get; set; } = 0;
	public int HairNoDash { get; set; } = 0;
	public int HairTwoDash { get; set; } = 0;
	public int HairRefillFlash { get; set; } = 0;
	public int HairFeather { get; set; } = 0;

	[JsonIgnore]
	public string ModId = "";

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Model);
	}

	public bool IsEnabled()
	{
		return Save.Instance.GetOrMakeMod(ModId).Enabled;
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SkinInfo))]
internal partial class SkinInfoContext : JsonSerializerContext { }