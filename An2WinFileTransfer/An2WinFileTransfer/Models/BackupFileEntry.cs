using System;
using An2WinFileTransfer.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace An2WinFileTransfer.Models
{
    public class BackupFileEntry
    {
        public string RelativePath { get; set; } = string.Empty;
        public ulong Size { get; set; }
        public DateTime LastWriteTime { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ECopyStatus CopyStatus { get; set; }
    }
}
