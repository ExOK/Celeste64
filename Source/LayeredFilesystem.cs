using System.Diagnostics.CodeAnalysis;

namespace Celeste64;

/// <summary>
/// Represents a list of IModFilesystem.
/// </summary>
public sealed class LayeredFilesystem : IModFilesystem {
    public event Action<ModFileChangedCtx>? OnFileChanged;
    
    private readonly List<GameMod> mods = [];
    
    public void AssociateWithMod(GameMod mod)
    {
    }
    
    public IEnumerable<IModFilesystem> InnerFilesystems 
        => mods.Select(m => m.Filesystem);

    private void OnInnerFileChanged(ModFileChangedCtx ctx)
    {
        OnFileChanged?.Invoke(ctx);
    }
    
    public void Add(GameMod mod)
    {
        mod.Filesystem.OnFileChanged += OnInnerFileChanged;
        mods.Add(mod);
    }

    public void Remove(GameMod mod)
    {
        mod.Filesystem.OnFileChanged -= OnInnerFileChanged;
        mods.Remove(mod);
    }

    public string Root => "";
    
    public void BackgroundCleanup()
    {
        foreach (var mod in mods)
        {
            mod.Filesystem.BackgroundCleanup();
        }
    }

    public GameMod? FindFirstContaining(string filepath) {
        return mods.FirstOrDefault(m => m.Filesystem.FileExists(filepath));
    }

    public bool TryOpenFile<T>(string path, Func<Stream, T> callback, [NotNullWhen(true)] out T? value) {
        foreach (var mod in mods) {
            if (mod.Filesystem.TryOpenFile(path, callback, out value))
                return true;
        }

        value = default;
        return false;
    }
    
    public IEnumerable<string> FindFilesInDirectoryRecursive(string directory, string extension) {
        foreach (var mod in mods) {
            foreach (var item in mod.Filesystem.FindFilesInDirectoryRecursive(directory, extension)) {
                yield return item;
            }
        }
    }
    
    /// <summary>
    /// Same as <see cref="FindFilesInDirectoryRecursive"/>, but also returns which mod contains the given file.
    /// </summary>
    public IEnumerable<(string, GameMod)> FindFilesInDirectoryRecursiveWithMod(string directory, string extension) {
        foreach (var mod in mods) {
            foreach (var item in mod.Filesystem.FindFilesInDirectoryRecursive(directory, extension)) {
                yield return (item, mod);
            }
        }
    }

    public bool FileExists(string path) {
        return mods.Any(mod => mod.Filesystem.FileExists(path));
    }
    
    public void Dispose()
    {
        foreach (var m in mods)
        {
            m.Filesystem.Dispose();
        }

        mods.Clear();
    }
}