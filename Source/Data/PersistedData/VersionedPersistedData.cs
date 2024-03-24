using System.Text.Json;

namespace Celeste64;
public abstract class VersionedPersistedData<T> : PersistedData where T : PersistedData
{
	public override object? Deserialize(string data)
	{
		try
		{
			var doc = JsonDocument.Parse(data);
			int version = doc.RootElement.GetProperty("Version").TryGetInt32(out int result) ? result : 1;

			if (version != Version)
			{
				return UpgradeFrom(data);
			}
			else
			{
				return base.Deserialize(data);
			}
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
			return null;
		}
	}

	public abstract object? UpgradeFrom(string data);
}
