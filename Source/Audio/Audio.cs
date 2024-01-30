using FMOD.Studio;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Celeste64;

public static class Audio
{
	private class Module : Foster.Framework.Module
	{
		public override void Update() 
			=> Audio.Update();

		public override void Shutdown()
			=> Audio.Shutdown();
	}

	private static FMOD.Studio.System system;
	private static readonly List<Bank> banks = [];
	private static readonly Dictionary<string, FMOD.GUID> events = [];
	private static readonly Dictionary<string, FMOD.GUID> buses = [];

	public static void Init()
	{
		// live upate allows FMOD UI to interact with sounds in-game in real time
		var flags = FMOD.INITFLAGS.NORMAL;
		var studioFlags = INITFLAGS.NORMAL;
		#if DEBUG
			studioFlags |= INITFLAGS.LIVEUPDATE;
		#endif

		Log.Info($"FMOD Bindings: v{FMOD.VERSION.number:x}");

		// Dynamically load FMOD binaries. For some reason this is the only way
		// I could make M1 Macs properly load the FMOD .dylib files ???
		LoadDynamicLibraries();

		// make a core API call before initializing the Studio API
		FMOD.Memory.GetStats(out _, out _);

		// create the system and get the core api
		Check(FMOD.Studio.System.create(out system));

		// get the core system & version number
		Check(system.getCoreSystem(out var core));
		Check(core.getVersion(out var version));
		Log.Info($"FMOD: v{version:x}");

		// Initialize FMOD
		Check(system.initialize(1024, studioFlags, flags, IntPtr.Zero));

		App.Register<Module>();
	}

	private static bool isResolverSet = false;

	private static void LoadDynamicLibraries()
	{
		if (isResolverSet)
			return;
		isResolverSet = true;

		var path 
			=  Path.GetDirectoryName(AppContext.BaseDirectory) 
			?? Directory.GetCurrentDirectory();

		NativeLibrary.SetDllImportResolver(typeof(FMOD.Studio.System).Assembly, 
			(name, assembly, dllImportSearchPath) =>
			{
				name = Path.GetFileNameWithoutExtension(name);

				if (OperatingSystem.IsWindows())
					name = $"{name}.dll";
				else if (OperatingSystem.IsLinux())
					name = $"lib{name}.so";
				else if (OperatingSystem.IsMacOS())
					name = $"lib{name}.dylib";
				else
					throw new PlatformNotSupportedException();

				return NativeLibrary.Load(Path.Join(path, name));
			}
		);
	}

	public static void SetListener(in Camera camera)
	{
		FMOD.ATTRIBUTES_3D attr = new();
		attr.forward.x = camera.Forward.X;
		attr.forward.y = camera.Forward.Y;
		attr.forward.z = camera.Forward.Z;
		attr.up.x = camera.Up.X;
		attr.up.y = camera.Up.Y;
		attr.up.z = camera.Up.Z;
		attr.position.x = camera.Position.X;
		attr.position.y = camera.Position.Y;
		attr.position.z = camera.Position.Z;
		attr.velocity.x = 0;
		attr.velocity.y = 0;
		attr.velocity.z = 0;

		var result = system.setListenerAttributes(0, attr);
		Check(result);
	}

	private static void Update()
	{
		system.update();
	}

	private static void Shutdown()
	{
		Unload();
		Check(system.release());
	}

	public static void Load(string directory)
	{
		if (!Directory.Exists(directory))
			return;

		// load strings first
		foreach (var file in Directory.EnumerateFiles(directory))
		{
			if (file.EndsWith(".strings.bank"))
				LoadBank(file);
		}

		// load banks second
		foreach (var file in Directory.EnumerateFiles(directory))
		{
			if (file.EndsWith(".bank") && !file.EndsWith(".strings.bank"))
				LoadBank(file);
		}
	}

	public static void LoadBank(string path)
	{
		Check(system.loadBankFile(path, LOAD_BANK_FLAGS.NORMAL, out var bank));

		banks.Add(bank);
		bank.getEventList(out var evs);
		bank.getBusList(out var bs);

		foreach (var ev in evs)
			if (ev.isValid())
			{
				ev.getPath(out var eventPath);
				ev.getID(out var eventID);
				events[eventPath] = eventID;
			}

		foreach (var bus in bs)
			if (bus.isValid())
			{
				bus.getPath(out var busPath);
				bus.getID(out var busID);
				buses[busPath] = busID;
			}
	}

	public static void Unload()
	{
		foreach (var bank in banks)
			bank.unload();
		banks.Clear();
		events.Clear();
		buses.Clear();
	}

	public static AudioHandle Create(in FMOD.GUID id, in Vec3 position)
	{
		var it = Create(id);
		it.Position = position;
		return it;
	}

	public static AudioHandle Create(in FMOD.GUID id)
	{
		if (id.Data1 != 0 || id.Data2 != 0 || id.Data3 != 0 || id.Data4 != 0)
		{
			var result = system.getEventByID(id, out var desc);
			if (result != FMOD.RESULT.OK)
			{
				Log.Warning($"Failed to create Audio Event Instance: {result}");
				return new AudioHandle();
			}

			return Create(desc);
		}

		return new();
	}

	public static AudioHandle Create(string path)
	{
		if (!string.IsNullOrEmpty(path) && events.TryGetValue(path, out var id))
			return Create(id);
		else
			Log.Warning($"Audio Event {path} doesn't exist");
		return new();
	}

	private static AudioHandle Create(in EventDescription desc)
	{
		var result = desc.createInstance(out var instance);
		if (result != FMOD.RESULT.OK)
		{
			Log.Warning($"Failed to create Audio Event Instance: {result}");
			return new AudioHandle();
		}
		
		return new AudioHandle(instance);
	}

	public static AudioHandle Play(in FMOD.GUID id, Vec3? position = null, float volume = 1.0f)
	{
		var it = Create(id);
		if (position.HasValue)
			it.Position = position.Value;
		it.Volume = volume;
		it.Play();
		return it;
	}

	public static AudioHandle Play(string ev, Vec3? position = null, float volume = 1.0f)
	{
		var it = Create(ev);
		if (position.HasValue)
			it.Position = position.Value;
		it.Volume = volume;
		it.Play();
		return it;
	}

	public static void StopAll(bool immediate)
	{
		if (system.isValid())
		{
			var mode = immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT;
			if (system.getBus("bus:/", out var masterBus) == FMOD.RESULT.OK)
				masterBus.stopAllEvents(mode);
		}
	}

	public static void StopBus(string name, bool immediate)
	{
		if (system.isValid())
		{
			var mode = immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT;
			if (system.getBus(name, out var bus) == FMOD.RESULT.OK)
				bus.stopAllEvents(mode);
		}
	}
	public static void SetBusPaused(string name, bool paused)
	{
		if (system.isValid())
		{
			if (system.getBus(name, out var bus) == FMOD.RESULT.OK)
				bus.setPaused(paused);
		}
	}

	public static void SetBusVolume(FMOD.GUID busGuid, float value)
	{
		if (system.getBusByID(busGuid, out var bus) == FMOD.RESULT.OK)
			bus.setVolume(value);
	}

	public static void SetBusVolume(string busName, float value)
	{
		if (buses.TryGetValue(busName, out var id) &&
			system.getBusByID(id, out var bus) == FMOD.RESULT.OK)
			bus.setVolume(value);
	}

	public static void SetVCAVolume(string vcaName, float value)
	{
		if (system.getVCA(vcaName, out var vca) == FMOD.RESULT.OK)
			vca.setVolume(value);
	}

	public static EventDescription GetEventByID(in FMOD.GUID id)
	{
		if (system.getEventByID(id, out var desc) == FMOD.RESULT.OK)
			return desc;
		return new();
	}

	public static void SetParameter(string id, float value)
	{
		system.setParameterByName(id, value);
	}

	internal static void Check(FMOD.RESULT result)
		=> Debug.Assert(result == FMOD.RESULT.OK, $"FMOD Failed: {result}");
}

public static class AudioUtil
{
	public static PARAMETER_ID U64ToParameterID(ulong id)
	{
		return new PARAMETER_ID { data1 = (uint)(id >> 32), data2 = (uint)id };
	}

	public static ulong ParameterIDToU64(PARAMETER_ID id)
	{
		return ((ulong)id.data1 << 32) | (id.data2);
	}

}