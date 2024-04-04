
using System.Text.Json;
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

	[JsonConverter(typeof(DecimalOrHexConverter))]
	public virtual int HairNormal { get; set; } = 0xdb2c00;
	[JsonConverter(typeof(DecimalOrHexConverter))]
	public virtual int HairNoDash { get; set; } = 0x6ec0ff;
	[JsonConverter(typeof(DecimalOrHexConverter))]
	public virtual int HairTwoDash { get; set; } = 0xfa91ff;
	[JsonConverter(typeof(DecimalOrHexConverter))]
	public virtual int HairRefillFlash { get; set; } = 0xffffff;
	[JsonConverter(typeof(DecimalOrHexConverter))]
	public virtual int HairFeather { get; set; } = 0xf2d450;


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

// This allows us to support either decimal integers or hex strings.
public class DecimalOrHexConverter : JsonConverter<int>
{
	public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
		{
			return reader.TryGetInt32(out var i) ? i : 0;
		}
		else if (reader.TokenType == JsonTokenType.String)
		{
			try
			{
				return Convert.ToInt32(reader.GetString(), 16);
			}
			catch (Exception ex)
			{
				Log.Error("Error: Could not parse value in skin file: " + ex.ToString());
				return 0;
			}
		}
		else
		{
			return 0;
		}
	}

	public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value, value.GetType());
	}
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SkinInfo))]
internal partial class SkinInfoContext : JsonSerializerContext { }
