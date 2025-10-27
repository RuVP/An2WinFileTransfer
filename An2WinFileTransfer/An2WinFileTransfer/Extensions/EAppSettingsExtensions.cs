using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using An2WinFileTransfer.Enums;
using An2WinFileTransfer.Models;

namespace An2WinFileTransfer.Extensions
{
    public static class EAppSettingsExtensions
    {
        public static void SaveSettings(this EAppSettings eAppSettings, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[eAppSettings.ToString()].Value = value;
            config.Save();
        }

        public static string GetValue(this EAppSettings eAppSettings)
        {
            return ConfigurationManager.AppSettings[eAppSettings.ToString()];
        }

        public static bool SaveFileTypeFilterSettings(this EAppSettings eAppSettings, IEnumerable<FileType> fileTypes)
        {
            if (eAppSettings != EAppSettings.FileTypesFilter)
            {
                return false;
            }

            var configValue = string.Join(";", fileTypes.Select(ft => $"{ft.Extension}-{(ft.IsEnabled ? "Y" : "N")}"));
            EAppSettings.FileTypesFilter.SaveSettings(configValue);

            return true;
        }
    }
}
