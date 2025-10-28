using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using An2WinFileTransfer.Models;
using MediaDevices;

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

            _logAction("Evaluating files to backup...");

            var enabledExtensions = new HashSet<string>(
                fileTypes.Where(ft => ft.IsEnabled && !string.IsNullOrWhiteSpace(ft.Extension))
                         .Select(ft => "." + ft.Extension.Trim().ToLowerInvariant()));

            var files = device.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    if (copyAllFiles)
                        return true;
                    var ext = Path.GetExtension(f)?.ToLowerInvariant() ?? string.Empty;
                    return enabledExtensions.Contains(ext);
                }).ToList();

            int processedFileCount = 0, copiedFileCount = 0, skippedFileCount = 0, copyFailedFileCount = 0;
            int totalFileCount = files.Count;

            foreach (var file in files)
            {
                try
                {
                    processedFileCount++;
                    var fileInfo = device.GetFileInfo(file);

                    _logAction($"Processing file {processedFileCount} of {totalFileCount}. Copied: {copiedFileCount} | Skipped: {skippedFileCount} | Failed: {copyFailedFileCount}");

                    if (fileInfo == null)
                    {
                        copyFailedFileCount++;
                        continue;
                    }

                    var relative = file.Substring(sourcePath.Length).TrimStart('\\', '/');
                    var normalized = relative.Replace('/', '\\');

                    if (normalized.IndexOf("\\.thumbnails\\", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        normalized.EndsWith("\\.thumbnails", StringComparison.OrdinalIgnoreCase))
                    {
                        skippedFileCount++;
                        continue;
                    }

                    var localPath = Path.Combine(targetRoot, normalized);
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                    if (ShouldCopyFile(fileInfo, localPath))
                    {
                        device.DownloadFile(file, localPath);
                        if (fileInfo.LastWriteTime.HasValue)
                            File.SetLastWriteTime(localPath, fileInfo.LastWriteTime.Value);
                        copiedFileCount++;
                    }
                    else
                    {
                        skippedFileCount++;
                    }
                }
                catch (Exception ex)
                {
                    copyFailedFileCount++;
                    _logAction($"Error copying {file}: {ex.Message}");
                }
            }

            _logAction($"Backup completed: Copied={copiedFileCount}, Skipped={skippedFileCount}, Failed={copyFailedFileCount}, Total={processedFileCount}");
        }

        private bool ShouldCopyFile(MediaFileInfo fileInfo, string localPath)
        {
            if (!File.Exists(localPath))
                return true;

            var localFile = new FileInfo(localPath);

            return Math.Abs((long)fileInfo.Length - localFile.Length) > 1024 ||
                   Math.Abs((fileInfo.LastWriteTime - localFile.LastWriteTime).Value.TotalSeconds) > 2;
        }
    }
}
