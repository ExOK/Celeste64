namespace Celeste64;

public static class StringExtensions
{
    /// <summary>
    /// Replaces backslashes with slashes in the given string
    /// </summary>
    public static string Unbackslash(this string from)
        => from.Replace('\\', '/');

    /// <summary>
    /// Corrects the slashes in the given path to be correct for the given OS.
    /// </summary>
    public static string CorrectSlashes(this string path)
        => path switch {
            null => "",
            _ => path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar),
        };
    
    /// <summary>
    /// Calls <see cref="Path.GetDirectoryName(string?)"/> on this string
    /// </summary>
    public static string? Directory(this string? path) {
        return Path.GetDirectoryName(path);
    }
}