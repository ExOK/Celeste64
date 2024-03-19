namespace Celeste64;

public static class StreamExtensions
{
	/// <summary>
	/// Reads all bytes of the given stream into a byte array.
	/// </summary>
	public static byte[] ReadAllToByteArray(this Stream stream)
	{
		// Just in case a memory stream gets passed in, no need to make another copy.
		if (stream is MemoryStream streamAsMemStream)
		{
			return streamAsMemStream.ToArray();
		}

		using var memStream = new MemoryStream();
		stream.CopyTo(memStream);
		memStream.Seek(0, SeekOrigin.Begin);

		return memStream.ToArray();
	}
}
