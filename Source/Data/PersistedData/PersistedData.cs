using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Celeste64;
public abstract class PersistedData
{
	public abstract int Version { get; }

	public virtual void Serialize(Stream stream, object instance)
	{
		JsonSerializer.Serialize(stream, instance, GetTypeInfo());
	}

	public virtual void Serialize(Utf8JsonWriter writer, object instance)
	{
		JsonSerializer.Serialize(writer, instance, GetTypeInfo());
	}

	public virtual T? Deserialize<T>(string data) where T : PersistedData
	{
		try
		{
			return JsonSerializer.Deserialize(data, GetTypeInfo()) as T;
		}
		catch (Exception e)
		{
			Log.Error(e.ToString());
			return null;
		}
	}

	public abstract JsonTypeInfo GetTypeInfo();
}
