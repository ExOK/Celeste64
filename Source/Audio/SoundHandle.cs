using FMOD;

namespace Celeste64;

/// <summary>
/// Wrapper around an FMOD Sound Object
/// Fuji Custom Struct
/// </summary>
public readonly struct SoundHandle
{
	private readonly Channel channel;
	private readonly FMOD.Sound sound;

	public SoundHandle(in Channel channel, in FMOD.Sound sound)
	{
		this.channel = channel;
		this.sound = sound;
	}

	public bool IsLooping
	{
		get
		{
			return channel.getLoopCount(out int loopcount) == FMOD.RESULT.OK && loopcount != 0;
		}
	}

	public bool IsOneshot => !IsLooping;

	public bool IsPlaying
	{
		get
		{
			return channel.isPlaying(out bool playing) == RESULT.OK && playing;
		}
	}

	public Vec3 Position
	{
		get
		{
			if (channel.get3DAttributes(out VECTOR pos, out VECTOR vel) == RESULT.OK)
				return new Vec3(pos.x, pos.y, pos.z);
			return Vec3.Zero;
		}

		set
		{
			FMOD.ATTRIBUTES_3D attr = new();
			attr.position.x = value.X;
			attr.position.y = value.Y;
			attr.position.z = value.Z;
			attr.up.x = 0;
			attr.up.y = 0;
			attr.up.z = 1;
			attr.forward.x = 1;
			attr.forward.y = 0;
			attr.forward.z = 0;
			attr.velocity.x = 0;
			attr.velocity.y = 0;
			attr.velocity.z = 0;
			Audio.Check(channel.set3DAttributes(ref attr.position, ref attr.velocity));
		}
	}

	public float Volume
	{
		get
		{
			if (channel.getVolume(out float value) == FMOD.RESULT.OK)
				return value;
			return 0.0f;
		}
		set
		{
			Audio.Check(channel.setVolume(value));
		}
	}

	public bool Paused
	{
		get
		{
			if (channel.getPaused(out bool value) == FMOD.RESULT.OK)
				return value;
			return false;
		}
		set
		{
			Audio.Check(channel.setPaused(value));
		}
	}

	public void SetCallback(CHANNELCONTROL_CALLBACK callback)
	{
		channel.setCallback(callback);
	}

	public void SetLoopData(int loopCount = 0, int loopStart = 0, int loopEnd = 1000)
	{
		Audio.Check(channel.setMode(loopCount != 0 ? MODE.LOOP_NORMAL : MODE.LOOP_OFF));
		Audio.Check(channel.setLoopCount(loopCount));
		Audio.Check(sound.getLength(out uint length, TIMEUNIT.MS));
		Audio.Check(channel.setLoopPoints((uint)Math.Clamp(loopStart, 0, length), TIMEUNIT.MS, (uint)Math.Clamp(loopEnd, 0, length - 1), FMOD.TIMEUNIT.MS));
	}

	public void Stop()
	{
		if (IsPlaying)
		{
			channel.stop();
		}
	}
}