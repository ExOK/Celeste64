using System.Text.Json;

namespace Celeste64;
public abstract class VersionedPersistedData<V> : PersistedData where V : PersistedData, new()
{
	public override T? Deserialize<T>(string data) where T : class
	{
		try
		{
			var doc = JsonDocument.Parse(data);
			int version = doc.RootElement.TryGetProperty("Version", out JsonElement prop) ? (prop.TryGetInt32(out int result) ? result : 1) : 1;

			if (version != Version)
			{
				// TODO: possibly revisit this and see if we can find a way to not need this extra new here.
				// I tried to do it like that originally, but System.Text.Json doesn't seem to have a way to deserialize in place,
				// and it can't be static because it needs to use the overridden version and type info.
				return UpgradeFrom(new V().Deserialize<V>(data)) as T;
			}
			else
			{
				return base.Deserialize<T>(data);
			}
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
			return null;
		}
	}

	public abstract PersistedData? UpgradeFrom(V? data);
}
