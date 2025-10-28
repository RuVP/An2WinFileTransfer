using System;
using System.Collections.Generic;

namespace An2WinFileTransfer.Models
{
    public class BackupManifest
    {
        public DateTime BackupTime { get; set; }
        public string SourceRoot { get; set; } = string.Empty;
        public List<BackupFileEntry> Files { get; set; } = new List<BackupFileEntry>();
        public TimeSpan BackupDuration { get; set; }
    }
}
