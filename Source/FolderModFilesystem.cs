using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Celeste64;

public sealed class FolderModFilesystem : IModFilesystem {
    public event Action<ModFileChangedCtx>? OnFileChanged;
    
    public string Root { get; }
    
    internal GameMod? Mod { get; set; }

    private readonly FileSystemWatcher watcher;
    // keeps track of whether a file is known to exist or known not to exist in the directory.
    private readonly ConcurrentDictionary<string, bool> _knownExistingFiles = new();

    public string VirtToRealPath(string virtPath) => $"{Root}/{virtPath}";

    public FolderModFilesystem(string dirName) {
        Root = dirName;

        watcher = new FileSystemWatcher(dirName.CorrectSlashes());
        watcher.Changed += (s, e) => {
            if (e.Name is null)
                return;
            
            _knownExistingFiles.Clear();

            var path = e.Name.Unbackslash();
            
            if (Mod is {} mod)
                OnFileChanged?.Invoke(new(mod, path));
        };
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
    }

    public void AssociateWithMod(GameMod mod)
    {
        Mod = mod;
    }

    public bool FileExists(string path) {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        
        if (_knownExistingFiles.TryGetValue(path, out var knownResult))
            return knownResult;

        var realPath = VirtToRealPath(path);

        var exists = File.Exists(realPath);
        _knownExistingFiles[path] = exists;

        return exists;
    }

    public Stream? OpenFile(string path) {
        var realPath = VirtToRealPath(path);
        
        try
        {
            return File.OpenRead(realPath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool TryOpenFile<T>(string path, Func<Stream, T> callback, [NotNullWhen(true)] out T? value) {
        ArgumentNullException.ThrowIfNull(callback);

        using var stream = OpenFile(path);
        if (stream is { }) {
            value = callback(stream)!;
            return true;
        }

        value = default;
        return false;
    }

    public IEnumerable<string> FindFilesInDirectoryRecursive(string directory, string extension) {
        var realPath = VirtToRealPath(directory);
        if (!Directory.Exists(realPath)) {
            return Array.Empty<string>();
        }

        var searchFilter = string.IsNullOrWhiteSpace(extension) ? "*" : $"*.{extension}";

        return Directory.EnumerateFiles(realPath, searchFilter, SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(Root, f).Unbackslash());
    }

    public void BackgroundCleanup()
    {
        
    }

    public void Dispose()
    {
        watcher.Dispose();
    }
}