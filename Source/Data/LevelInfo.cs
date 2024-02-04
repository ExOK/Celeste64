
using System.Text.Json.Serialization;

namespace Celeste64;

/// <summary>
/// Stores Meta-Info about a specific Level
/// </summary>
public class LevelInfo
{
	public string ID { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Label { get; set; } = string.Empty;
	public int Strawberries { get; set; } = 0;
	public string Preview { get; set; } = string.Empty;
	public string Map { get; set; } = string.Empty;

	public void Enter(ScreenWipe? toBlack = null, float holdTime = 0)
	{
		Save.Instance.LevelID = ID;
		Game.Instance.Goto(new Transition()
		{
			Mode = Transition.Modes.Replace,
			Scene = () => new World(new(Map, Save.CurrentRecord.Checkpoint, false, World.EntryReasons.Entered)),
			ToBlack = toBlack ?? new SpotlightWipe(),
			FromBlack = new SpotlightWipe(),
			StopMusic = true,
			HoldOnBlackFor = holdTime
        });
	}
}

[JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true)]
[JsonSerializable(typeof(List<LevelInfo>))]
internal partial class LevelInfoListContext : JsonSerializerContext {}