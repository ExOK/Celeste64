
using System.Text.Json.Serialization;

namespace Celeste64;

/// <summary>
/// Stores Meta-Info about a specific skin
/// </summary>
public class SkinInfo
{
	public virtual string Name { get; set; } = "";
	public virtual string Model { get; set; } = "";
	public virtual bool HideHair { get; set; } = false;
	public virtual string CollectableId { get; set; } = "";
	public virtual int HairNormal { get; set; } = 0;
	public virtual int HairNoDash { get; set; } = 0;
	public virtual int HairTwoDash { get; set; } = 0;
	public virtual int HairRefillFlash { get; set; } = 0;
	public virtual int HairFeather { get; set; } = 0;

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Model);
	}

	public virtual bool IsUnlocked()
	{
		return true;
	}

	public virtual void OnEquipped(Player player, Model m) { }
	public virtual void OnRemoved(Player player, Model m) { }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SkinInfo))]
internal partial class SkinInfoContext : JsonSerializerContext { }