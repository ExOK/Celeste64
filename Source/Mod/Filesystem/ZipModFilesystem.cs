using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace Celeste64.Mod;

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
    
    public Stream OpenFile(string path)
    {
        var zip = OpenZipIfNeeded();
        var entry = zip.GetEntry(path) ?? throw new FileNotFoundException($"Couldn't find zip entry for mod '{Mod?.ModInfo.Id}'", path);
        var stream = entry.Open();
        openedFiles.Add(stream);
        return stream;
    }
    
    private Stream? OpenFile(string path, ZipArchive zip) {
        var entry = GetZipEntryCaseInsensitive(path, zip);
        var stream = entry?.Open();
        if (stream is { }) {
            openedFiles.Add(stream);
        }

        return stream;
    }

    // Get first entry that matches this path regardless of casing.
    private static ZipArchiveEntry? GetZipEntryCaseInsensitive(string pathAndFileName, ZipArchive zip)
    {
        return zip.Entries.FirstOrDefault(e => e.FullName.ToLower() == pathAndFileName.ToLower());
    }

    public bool TryOpenFile<T>(string path, Func<Stream, T> callback, [NotNullWhen(true)] out T? value)
    {
        lock (_lock)
        {
            var zip = OpenZipIfNeeded();

            var stream = OpenFile(modRoot + path, zip);
            if (stream is null)
            {
                value = default;
                return false;
            }
            if (stream.CanSeek)
            {
                value = callback(stream)!;
                stream.Dispose();
            }
            else
            {
                // If the stream cannot seek, we need to copy it to a memory stream so consumers can use position and lenght.
                // This is mostly needed to support how Foster uses stream.Position and stream.Length in the Image constructor
                // and stream.Position and stream.Length are not supported by the DeflateStreams that OpenFile can return.
                MemoryStream resultStream = new MemoryStream();
                stream.CopyTo(resultStream);
                stream.Dispose();
                resultStream.Position = 0;
                value = callback(resultStream)!;
                resultStream.Dispose();
            }

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
                return valid ? fullName.Substring(modRoot.Length) : null;
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

         var realPath = modRoot + path;

        if (_knownExistingFiles.TryGetValue(realPath, out var knownResult))
            return knownResult;

        bool exists;
        lock (_lock)
        {
            var zip = OpenZipIfNeeded();
            exists = zip.GetEntry(realPath) is { };
        }

        _knownExistingFiles[realPath] = exists;
        
        return exists;
    }
    
    public void Dispose()
    {
        watcher.Dispose();
        currentZip?.Dispose();
    }
}