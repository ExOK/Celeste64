namespace Celeste64;

public class Sound(Actor following, string sound) : Actor
{
	public AudioHandle Handle;
	public Actor? Following = following;

	public readonly string SoundEvent = sound;

	public void Resume()
	{
		if (!Handle)
		{
			Handle.Stop();
			Handle = Audio.Play(SoundEvent, Following?.Position);
			UpdateOffScreen = true;
		}
	}

	public void Stop()
	{
		Handle.Stop();
		UpdateOffScreen = false;
	}

	public override void LateUpdate()
	{
		if (Following != null)
		{
			Handle.Position = Following.Position;

			if (Following is Solid solid)
				Handle.Set("Velocity", solid.Velocity.Length());
		}
		else if (!Destroying)
		{
			World.Destroy(this);
		}
	}

	public override void Destroyed()
	{
		Stop();
		Following = null;
	}
}
