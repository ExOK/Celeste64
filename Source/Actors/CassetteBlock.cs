
namespace Celeste64;

public class CassetteBlock : Solid, IListenToAudioCallback
{
	public CassetteBlock(bool startOn)
	{
		Model.MakeMaterialsUnique();
		Transparent = true;
		SetOn(startOn);
	}

	public void SetOn(bool enabled)
	{
		Collidable = enabled;
		Model.Flags = enabled ? ModelFlags.Terrain : ModelFlags.Transparent;

		foreach (var mat in Model.Materials)
			mat.Color = enabled ? Color.White : Color.White * 0.30f;
	}

	public void AudioCallbackEvent(int index)
	{
		if (index % 2 == 0)
			SetOn(!Collidable);
	}
}
