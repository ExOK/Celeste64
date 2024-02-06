using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace Celeste64;

public sealed class ZipModFilesystem : IModFilesystem {
    public event Action<ModFileChangedCtx>? OnFileChanged;
    
    public string Root { get; init; }
    internal GameMod? Mod { get; set; }

    private ZipArchive? currentZip;
    
    private readonly ConcurrentBag<Stream> openedFiles = [ ];

    private readonly object _lock = new();

    private readonly FileSystemWatcher watcher;
    
    // keeps track of whether a file is known to exist or known not to exist in the zip.
    private readonly ConcurrentDictionary<string, bool> _knownExistingFiles = new();

    private readonly string modRoot = "";

    public ZipModFilesystem(string zipFilePath) {
        Root = zipFilePath;

        watcher = new FileSystemWatcher(zipFilePath.Directory()!.CorrectSlashes());
        watcher.Changed += (s, e) => {
            if (e.FullPath != Root.CorrectSlashes())
                return;

            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;
            
            _knownExistingFiles.Clear();

            if (Mod is {} mod)
                OnFileChanged?.Invoke(new(mod, null));
        };

			var zip = OpenZipIfNeeded();
      var modFolder = $"{Path.GetFileNameWithoutExtension(zipFilePath)}/";

      foreach (ZipArchiveEntry entry in zip.Entries) {
        if (entry.FullName.StartsWith(modFolder, StringComparison.OrdinalIgnoreCase)) {
          modRoot = modFolder;
          break;
        }
      }

      watcher.EnableRaisingEvents = true;
    }
    
    public void AssociateWithMod(GameMod mod)
    {
        Mod = mod;
    }
    
    private ZipArchive OpenZipIfNeeded() {
        return currentZip ??= ZipFile.OpenRead(Root);
    }
    
    private Stream? OpenFile(string path, ZipArchive zip) {
        var entry = zip.GetEntry(path);
        var stream = entry?.Open();
        if (stream is { }) {
            openedFiles.Add(stream);
        }

        return stream;
    }

    public bool TryOpenFile<T>(string path, Func<Stream, T> callback, [NotNullWhen(true)] out T? value)
    {
        lock (_lock)
        {
            var zip = OpenZipIfNeeded();

            var stream = OpenFile(path, zip);
            if (stream is null) {
              stream = OpenFile(modRoot + path, zip);

              if (stream is null) {
                value = default;
                return false;
              }
            }

            value = callback(stream)!;
            return true;
        }
    }

    public IEnumerable<string> FindFilesInDirectoryRecursive(string directory, string extension)
    {
        List<string> files;
        
        lock (_lock)
        {
            var zip = OpenZipIfNeeded();

            files = zip.Entries.Select(e => {
                var fullName = e.FullName;
                var valid = !fullName.EndsWith('/') 
                            && fullName.StartsWith(modRoot + directory, StringComparison.Ordinal)
                            && fullName.EndsWith(extension, StringComparison.Ordinal);
                return valid ? fullName : null;
            }).Where(x => x is { }).ToList()!;
        }

        return files;
    }

    public void BackgroundCleanup()
    {
        lock (_lock)
        {
            if (currentZip is not { })
                return;
            
            if (!openedFiles.IsEmpty && openedFiles.All(x => !x.CanRead)) {
                openedFiles.Clear();
            }
            
            if (!openedFiles.IsEmpty)
                return;
            
            // No more files are open, we're free to close the zip
            currentZip.Dispose();
            currentZip = null;
        }
    }

    public bool FileExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (_knownExistingFiles.TryGetValue(path, out var knownResult))
            return knownResult;

        bool exists;
        lock (_lock)
        {
            var zip = OpenZipIfNeeded();
            exists = zip.GetEntry(path) is { };
        }

        _knownExistingFiles[path] = exists;
        
        return exists;
    }
    
    public void Dispose()
    {
        watcher.Dispose();
        currentZip?.Dispose();
    }
}