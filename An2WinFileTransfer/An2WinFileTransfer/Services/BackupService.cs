using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using An2WinFileTransfer.Enums;
using An2WinFileTransfer.Interfaces;
using An2WinFileTransfer.Models;
using MediaDevices;
using Newtonsoft.Json;

namespace An2WinFileTransfer.Services
{
    public class BackupService
    {
        private readonly ILoggingService _logService;

        public BackupService(ILoggingService log)
        {
            _logService = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void BackupFromDevice(MediaDevice device, string sourcePath, string targetRoot, IEnumerable<FileType> fileTypes, bool copyAllFiles)
        {
            if (!device.DirectoryExists(sourcePath))
            {
                _logService.Warn($"Source folder not found: {sourcePath}");
                return;
            }

            var timestampedRootFolder = CreateNewTimeStampedFolder(targetRoot);

            _logService.Info("Scanning previous backups...");

            var previousManifests = LoadPreviousManifests(targetRoot);
            var existingFiles = BuildExistingFileMap(previousManifests);

            _logService.Info($"Loaded {existingFiles.Count} entries from previous backups.");

            var manifest = new BackupManifest
            {
                BackupTime = DateTime.UtcNow,
                SourceRoot = sourcePath,
                Files = new List<BackupFileEntry>()
            };

            _logService.Info("Evaluating files to backup...");

            var enabledExtensions = new HashSet<string>(
                fileTypes.Where(ft => ft.IsEnabled && !string.IsNullOrWhiteSpace(ft.Extension))
                         .Select(ft => "." + ft.Extension.Trim().ToLowerInvariant()));

            var files = device.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    if (copyAllFiles)
                    {
                        return true;
                    }

                    var ext = Path.GetExtension(f)?.ToLowerInvariant() ?? string.Empty;
                    return enabledExtensions.Contains(ext);
                }).ToList();

            int processedFileCount = 0, copiedFileCount = 0, skippedFileCount = 0, copyFailedFileCount = 0;
            var totalFileCount = files.Count;

            foreach (var file in files)
            {
                processedFileCount++;
                var fileInfo = device.GetFileInfo(file);

                _logService.Info($"Processing file {processedFileCount} of {totalFileCount}. Copied: {copiedFileCount} | Skipped: {skippedFileCount} | Failed: {copyFailedFileCount}");

                if (fileInfo == null)
                {
                    copyFailedFileCount++;
                    continue;
                }

                _logService.Info($"Evaluating file: {file}");

                var relativePath = GetRelativePath(sourcePath, file);

                var entry = new BackupFileEntry
                {
                    RelativePath = relativePath,
                    Size = fileInfo.Length,
                    LastWriteTime = fileInfo.LastWriteTime ?? DateTime.MinValue,
                    CopyStatus = ECopyStatus.Skipped // default
                };

                manifest.Files.Add(entry);

                if (relativePath.IndexOf("\\.thumbnails\\", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    relativePath.EndsWith("\\.thumbnails", StringComparison.OrdinalIgnoreCase))
                {
                    skippedFileCount++;
                    continue;
                }

                var localPath = Path.Combine(timestampedRootFolder, SanitizePath(relativePath));
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                // Skip if file already backed up in a previous manifest
                if (existingFiles.TryGetValue(relativePath, out var oldEntry))
                {
                    if (oldEntry.Size == fileInfo.Length &&
                        Math.Abs((fileInfo.LastWriteTime - oldEntry.LastWriteTime).Value.TotalSeconds) < 2)
                    {
                        entry.CopyStatus = ECopyStatus.Skipped;
                        skippedFileCount++;
                        continue;
                    }
                }

                try
                {
                    if (ShouldCopyFile(fileInfo, localPath))
                    {
                        device.DownloadFile(file, localPath);

                        if (fileInfo.LastWriteTime.HasValue)
                        {
                            File.SetLastWriteTime(localPath, fileInfo.LastWriteTime.Value);
                        }

                        entry.CopyStatus = ECopyStatus.Success;
                        copiedFileCount++;
                    }
                    else
                    {
                        skippedFileCount++;
                    }
                }
                catch (Exception ex)
                {
                    entry.CopyStatus = ECopyStatus.Failed;
                    copyFailedFileCount++;
                    _logService.Error($"Error copying {file}: {ex.Message}");
                }
            }

            try
            {
                manifest.BackupDuration = DateTime.UtcNow - manifest.BackupTime;

                var manifestPath = Path.Combine(timestampedRootFolder, "BackupManifest.json");
                var json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                File.WriteAllText(manifestPath, json);

                _logService.Info($"Backup manifest saved: {manifestPath}. Backup duration: {manifest.BackupDuration}.");
            }
            catch (Exception ex)
            {
                _logService.Error($"Failed to save manifest: {ex.Message}");
            }

            _logService.Info($"Backup completed: Copied={copiedFileCount}, Skipped={skippedFileCount}, Failed={copyFailedFileCount}, Total={processedFileCount}");
        }

        private string GetRelativePath(string basePath, string fullPath)
        {
            if (fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(basePath.Length).TrimStart('\\', '/');
            }

            var baseParts = basePath.Trim('\\', '/').Split('\\', '/');
            var fullParts = fullPath.Trim('\\', '/').Split('\\', '/');

            var index = fullParts.Length > baseParts.Length
                ? baseParts.Length
                : 0;

            return string.Join("\\", fullParts.Skip(index)).Replace('/', '\\');
        }

        private bool ShouldCopyFile(MediaFileInfo fileInfo, string localPath)
        {
            if (!File.Exists(localPath))
            {
                return true;
            }

            var localFile = new FileInfo(localPath);

            return Math.Abs((long)fileInfo.Length - localFile.Length) > 1024 ||
                   Math.Abs((fileInfo.LastWriteTime - localFile.LastWriteTime).Value.TotalSeconds) > 2;
        }

        private string CreateNewTimeStampedFolder(string basePath)
        {
            var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var newFolderPath = Path.Combine(basePath, $"Backup_{timeStamp}");

            return Directory.CreateDirectory(newFolderPath).FullName;
        }

        private List<BackupManifest> LoadPreviousManifests(string baseBackupPath)
        {
            var manifests = new List<BackupManifest>();

            try
            {
                var manifestFiles = Directory.GetFiles(baseBackupPath, "BackupManifest.json", SearchOption.AllDirectories);

                foreach (var file in manifestFiles)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var manifest = JsonConvert.DeserializeObject<BackupManifest>(json);

                        if (manifest != null)
                        {
                            manifests.Add(manifest);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logService.Info($"Failed to read manifest {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"Error while scanning for manifests: {ex.Message}");
            }

            return manifests;
        }

        private Dictionary<string, BackupFileEntry> BuildExistingFileMap(IEnumerable<BackupManifest> manifests)
        {
            var map = new Dictionary<string, BackupFileEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var manifest in manifests)
            {
                foreach (var entry in manifest.Files)
                {
                    if (!map.ContainsKey(entry.RelativePath))
                    {
                        map[entry.RelativePath] = entry;
                    }
                }
            }

            return map;
        }

        private string SanitizePath(string relativePath)
        {
            foreach (var pathPart in relativePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var safePart = SanitizeFileOrFolderName(pathPart);
                relativePath = relativePath.Replace(pathPart, safePart);
            }

            return relativePath;
        }

        private string SanitizeFileOrFolderName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars()
                                    .Concat(Path.GetInvalidPathChars())
                                    .Distinct()
                                    .ToArray();

            var safePath = string.Join("_",
                name.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Select(part => string.Concat(part.Select(ch => invalidChars.Contains(ch) ? '_' : ch)))
            );

            return safePath;
        }
    }
}
