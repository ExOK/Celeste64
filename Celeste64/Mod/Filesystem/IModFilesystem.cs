using System.Diagnostics.CodeAnalysis;

namespace Celeste64.Mod;

public record struct ModFileChangedCtx(GameMod Mod, string? Path);

/// <summary>
/// Provides methods for accessing files for a given mod, regardless of whether it is in a zip or folder.
/// </summary>
public interface IModFilesystem : IDisposable
{
    public event Action<ModFileChangedCtx> OnFileChanged;
    
    /// <summary>
    /// Gets the root folder/zip name for this filesystem
    /// </summary>
    public string Root { get; }
    
    /// <summary>
    /// Checks whether a file at the given path exists.
    /// </summary>
    public bool FileExists(string path);

    /// <summary>
    /// Opens a stream for the file at the given path.
    /// </summary>
    public Stream OpenFile(string path);
    
    /// <summary>
    /// Tries to open a file at a given virtual path, which includes the file extension.
    /// If the file was found, calls <paramref name="callback"/> with the stream for that file, then returns true and <paramref name="value"/> gets set to the return value of the callback.
    /// If not, the function returns false.
    /// DO NOT capture the <see cref="Stream"/> received in the callback, as it might get disposed as soon as this method finishes.
    /// </summary>
    public bool TryOpenFile<T>(string path, Func<Stream, T> callback, [NotNullWhen(true)] out T? value);

    /// <summary>
    /// Finds all files that are contained in the <paramref name="directory"/> with the file extension <paramref name="extension"/>.
    /// Returned paths use forward slashes, and contain the file extension.
    /// If extension is an empty string, extensions are ignored.
    /// Calling OpenFile(path) using paths returned by this function allows you to access the file.
    /// </summary>
    public IEnumerable<string> FindFilesInDirectoryRecursive(string directory, string extension);

    /// <summary>
    /// Cleans up resources if they're no longer needed. Called occasionally.
    /// </summary>
    internal void BackgroundCleanup();

    /// <summary>
    /// Associates the filesystem with the given mod, so that it can emit OnFileChanged events.
    /// </summary>
    internal void AssociateWithMod(GameMod mod);
}

public static class ModFilesystemExt
{
    /// <summary>
    /// Loads a png file from the given path.
    /// Returns whether the file exists.
    /// </summary>
    public static bool TryLoadImage(this IModFilesystem fs, string path, [NotNullWhen(true)] out Image? img)
    {
        return fs.TryOpenFile(path, stream =>
        {
            Image img;
            
            // The Image(Stream) ctor assumes its possible to get the .Length of the input Stream.
            // Not all streams support this, but there isn't a nice way to check whether it is supported.
            // We'll just try to get it and catch the exception if it isn't (for example - for zips)
            var isLengthGettable = true;
            try
            {
                _ = stream.Length;
            }
            catch (Exception)
            {
                isLengthGettable = false;
            }

            if (isLengthGettable)
            {
                img = new Image(stream);
            }
            else
            {
                // Oh well
                using var memStream = new MemoryStream();
                stream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                img = new Image(memStream);
            }

            img.Premultiply();
            return img;
        }, out img);
    }
    
    /// <summary>
    /// Reads all text from a given file.
    /// Returns whether the file exists.
    /// </summary>
    public static bool TryLoadText(this IModFilesystem fs, string path, [NotNullWhen(true)] out string? text)
    {
        return fs.TryOpenFile(path, stream =>
        {
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }, out text);
    }

    /// <summary>
    /// Tries to open a file, calling <paramref name="cb"/> if the file exists.
    /// Returns whether the file exists.
    /// </summary>
    public static bool TryOpenFile(this IModFilesystem fs, string path, Action<Stream> cb)
    {
        return fs.TryOpenFile(path, stream =>
        {
            cb(stream);
            return true;
        }, out _);
    }
}