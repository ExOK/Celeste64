using FMOD.Studio;

namespace Celeste64;

/// <summary>
/// Wrapper around an FMOD Event Instance
/// </summary>
public readonly struct AudioHandle
{
	private readonly EventInstance instance;

	public AudioHandle() { }
	public AudioHandle(in EventInstance instance) { this.instance = instance; }

	public FMOD.GUID ID
	{
		get
		{
			if (instance.isValid() && 
				instance.getDescription(out var desc) == FMOD.RESULT.OK &&
				desc.getID(out var id) == FMOD.RESULT.OK)
				return id;
			return new();
		}
	}

	public string Path
	{
		get
		{
			if (instance.isValid() && 
				instance.getDescription(out var desc) == FMOD.RESULT.OK &&
				desc.getPath(out var path) == FMOD.RESULT.OK)
				return path;
			return string.Empty;
		}
	}

	public bool IsLooping
	{
		get
		{
			if (instance.isValid() && 
				instance.getDescription(out var desc) == FMOD.RESULT.OK &&
				desc.isOneshot(out var oneshot) == FMOD.RESULT.OK)
				return !oneshot;
			return false;
		}
	}

	public bool IsOneshot => !IsLooping;

	public bool IsPlaying
	{
		get
		{
			if (instance.isValid() && 
				instance.getPlaybackState(out var state) == FMOD.RESULT.OK &&
				(state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING))
				return true;
			return false;
		}
	}

	public bool IsStopping
	{
		get
		{
			if (instance.isValid() && 
				instance.getPlaybackState(out var state) == FMOD.RESULT.OK &&
				(state == PLAYBACK_STATE.STOPPING || state == PLAYBACK_STATE.STOPPED))
				return true;
			return false;
		}
	}

	public Vec3 Position
	{
		get
		{
			if (instance.isValid() && instance.get3DAttributes(out var attr) == FMOD.RESULT.OK)
				return new Vec3(attr.position.x, attr.position.y, attr.position.z);
			return Vec3.Zero;
		}

		set
		{
			if (instance.isValid())
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
				Audio.Check(instance.set3DAttributes(attr));
			}
		}
	}
	
	public float Volume
	{
		get
		{
			if (instance.isValid() && instance.getVolume(out float value) == FMOD.RESULT.OK)
				return value;
			return 0.0f;
		}
		set
		{
			if (instance.isValid())
				Audio.Check(instance.setVolume(value));
		}
	}

	public void SetCallback(EVENT_CALLBACK callback)
	{
		if (instance.isValid())
			instance.setCallback(callback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
	}

	public void Set(PARAMETER_ID id, float value)
	{
		if (instance.isValid())
			instance.setParameterByID(id, value);
	}

	public void Set(ulong id, float value)
	{
		if (instance.isValid())
			instance.setParameterByID(AudioUtil.U64ToParameterID(id), value);
	}

	public void Set((ulong ID, float Value) p)
		=> Set(p.ID, p.Value);

	public void Set(string id, float value)
	{
		if (instance.isValid())
			instance.setParameterByName(id, value);

	}

	public bool Has(string id) =>
		instance.isValid() && instance.getParameterByName(id, out _) == FMOD.RESULT.OK;

	public void Play()
	{
		PlayNoRelease();
		Release();
	}

	public void PlayNoRelease()
	{
		if (instance.isValid())
			Audio.Check(instance.start());
	}
	
	public void Stop()
	{
		StopNoRelease();
		Release();
	}
	
	public void StopNoRelease()
	{
		if (instance.isValid())
		{
			Audio.Check(instance.getPlaybackState(out var state));

			if (state == PLAYBACK_STATE.STARTING ||
				state == PLAYBACK_STATE.PLAYING || 
				state == PLAYBACK_STATE.SUSTAINING)
			{
				Audio.Check(instance.stop(STOP_MODE.ALLOWFADEOUT));
			}
		}
	}

	public void Release()
	{
		if (instance.isValid())
			instance.release();
	}

	internal void SetCallback()
	{
		throw new NotImplementedException();
	}

	public static implicit operator bool(AudioHandle handle) 
		=> handle.instance.isValid();
}