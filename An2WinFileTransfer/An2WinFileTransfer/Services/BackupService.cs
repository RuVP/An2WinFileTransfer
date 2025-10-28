using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using An2WinFileTransfer.Enums;
using An2WinFileTransfer.Models;
using MediaDevices;
using Newtonsoft.Json;

namespace An2WinFileTransfer.Services
{
    public class BackupService
    {
        private readonly Action<string> _logAction;

        public BackupService(Action<string> logAction)
        {
            _logAction = logAction ?? (_ => { });
        }

        public void BackupFromDevice(MediaDevice device, string sourcePath, string targetRoot, IEnumerable<FileType> fileTypes, bool copyAllFiles)
        {
            if (!device.DirectoryExists(sourcePath))
            {
                _logAction($"Source folder not found: {sourcePath}");
                return;
            }

            var timestampedRootFolder = CreateNewTimeStampedFolder(targetRoot);

            var manifest = new BackupManifest
            {
                BackupTime = DateTime.UtcNow,
                SourceRoot = sourcePath,
                Files = new List<BackupFileEntry>()
            };

            _logAction("Evaluating files to backup...");

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

                _logAction($"Processing file {processedFileCount} of {totalFileCount}. Copied: {copiedFileCount} | Skipped: {skippedFileCount} | Failed: {copyFailedFileCount}");

                if (fileInfo == null)
                {
                    copyFailedFileCount++;
                    continue;
                }

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

                var localPath = Path.Combine(timestampedRootFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(localPath));

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
                    _logAction($"Error copying {file}: {ex.Message}");
                }
            }

            try
            {
                manifest.BackupDuration = DateTime.UtcNow - manifest.BackupTime;

                var manifestPath = Path.Combine(timestampedRootFolder, "BackupManifest.json");
                var json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                File.WriteAllText(manifestPath, json);

                _logAction($"Backup manifest saved: {manifestPath}. Backup duration: {manifest.BackupDuration}.");
            }
            catch (Exception ex)
            {
                _logAction($"Failed to save manifest: {ex.Message}");
            }

            _logAction($"Backup completed: Copied={copiedFileCount}, Skipped={skippedFileCount}, Failed={copyFailedFileCount}, Total={processedFileCount}");
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
    }
}
