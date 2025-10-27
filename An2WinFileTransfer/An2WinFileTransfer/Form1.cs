using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using An2WinFileTransfer.Enums;
using An2WinFileTransfer.Extensions;
using An2WinFileTransfer.Models;
using MediaDevices;

namespace An2WinFileTransfer
{
    public partial class Form1 : Form
    {
        private IEnumerable<string> _connectedDevices = new List<string>();
        private IEnumerable<FileType> _fileTypes = new List<FileType>();
        private Timer _elapsedTimer = new Timer();
        private DateTime backupStartTime;

        private string _selectedDeviceName = string.Empty;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;

            _elapsedTimer.Interval = 1000; // 1 second
            _elapsedTimer.Tick += (s, ev) =>
            {
                var elapsed = DateTime.Now - backupStartTime;
                labelElapsedTime.Text = $"Elapsed time: {elapsed:hh\\:mm\\:ss}";
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                PopulateDeviceList();
                UpdateFieldsFromConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateFieldsFromConfig()
        {
            textBoxBackupFolderPath.Text = EAppSettings.DefaultBackupFolderPath.GetValue();
            textBoxPhoneMtpPath.Text = EAppSettings.MtpPath.GetValue();

            bool.TryParse(EAppSettings.CopyAllFiles.GetValue(), out var copyAllFiles);

            if (copyAllFiles)
            {
                radioButtonCopyAll.Checked = true;
                radioButtonCopySelected.Checked = false;
            }
            else
            {
                radioButtonCopyAll.Checked = false;
                radioButtonCopySelected.Checked = true;
            }

            int controlCount = 0, checkBoxesPerRow = 6;
            int startX = 10;    // left margin
            int startY = 20;    // top margin
            int spacingX = 60;  // horizontal spacing between checkboxes
            int spacingY = 30;  // vertical spacing between rows

            _fileTypes = LoadFileTypes();
            groupBoxFileTypes.Controls.Clear();

            foreach (var fileType in _fileTypes)
            {
                // Compute grid position
                var col = controlCount % checkBoxesPerRow;
                var row = controlCount / checkBoxesPerRow;

                var checkBox = new CheckBox
                {
                    Text = fileType.Extension,
                    Checked = fileType.IsEnabled,
                    AutoSize = true,
                    Left = startX + (col * spacingX),
                    Top = startY + (row * spacingY),
                };

                checkBox.CheckedChanged += CheckBox_CheckedChanged;

                groupBoxFileTypes.Controls.Add(checkBox);
                controlCount++;
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var extension = checkBox.Text.Trim();
                var fileType = _fileTypes.FirstOrDefault(f => f.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase));

                if (fileType != null)
                {
                    fileType.IsEnabled = checkBox.Checked;
                    EAppSettings.FileTypesFilter.SaveFileTypeFilterSettings(_fileTypes);
                }
            }
        }

        private IEnumerable<FileType> LoadFileTypes()
        {
            var fileTypesConfigValues = EAppSettings.FileTypesFilter.GetValue();

            if (fileTypesConfigValues.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("File type configuration is missing or empty.");
            }

            var fileTypes = new List<FileType>();

            foreach (var fileTypesConfigValue in fileTypesConfigValues.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = fileTypesConfigValue.Split('-');

                if (parts.Length != 2)
                {
                    throw new FormatException($"Invalid file type entry format: '{fileTypesConfigValue}'. Expected 'ext-Y' or 'ext-N'.");
                }

                var extension = parts[0].Trim();
                var enabledFlag = parts[1].Trim().ToUpperInvariant();

                if (extension.IsNullOrWhiteSpace())
                {
                    throw new FormatException($"File type extension cannot be empty in entry: '{fileTypesConfigValue}'.");
                }

                if (enabledFlag != "Y" && enabledFlag != "N")
                {
                    throw new FormatException($"Invalid enable flag '{enabledFlag}' in entry: '{fileTypesConfigValue}'. Expected 'Y' or 'N'.");
                }

                fileTypes.Add(
                    new FileType
                    {
                        Extension = extension,
                        IsEnabled = enabledFlag == "Y"
                    }
                );
            }

            return fileTypes;
        }

        private void BackupFromDevice(MediaDevice device, string sourcePath, string targetRoot)
        {
            if (!device.DirectoryExists(sourcePath))
            {
                Append($"Source folder not found: {sourcePath}");
                return;
            }

            var copyAllFiles = radioButtonCopyAll.Checked;

            var enabledExtensions = new HashSet<string>(
                _fileTypes
                    .Where(ft => ft.IsEnabled && !string.IsNullOrWhiteSpace(ft.Extension))
                    .Select(ft => "." + ft.Extension.Trim().ToLowerInvariant()));

            var files = device.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    if (copyAllFiles)
                        return true; // include all files

                    var ext = Path.GetExtension(f)?.ToLowerInvariant() ?? string.Empty;
                    return enabledExtensions.Contains(ext);
                }).ToList();

            int processedFileCount = 0, copiedFileCount = 0, skippedFileCount = 0, copyFailedFileCount = 0, totalFileCount = files.Count;

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = device.GetFileInfo(file);
                    if (fileInfo == null)
                    {
                        Append($"Skipping (no info): {file}");
                        copyFailedFileCount++;
                        continue;
                    }

                    processedFileCount++;

                    this.Invoke((Action)(() =>
                    {
                        labelProgress.Text = $"Processing file {processedFileCount} of {totalFileCount}. Copied: {copiedFileCount} | Skipped: {skippedFileCount} | Failed: {copyFailedFileCount}";
                    }));

                    var relativePath = file.Substring(sourcePath.Length).TrimStart('\\', '/');
                    var localPath = Path.Combine(targetRoot, relativePath.Replace('/', '\\'));

                    var normalized = relativePath.Replace('/', '\\');

                    if (normalized.IndexOf("\\.thumbnails\\", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        normalized.EndsWith("\\.thumbnails", StringComparison.OrdinalIgnoreCase))
                    {
                        skippedFileCount++;
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                    if (ShouldCopyFile(fileInfo, localPath))
                    {
                        Append($"Copying: {relativePath}");
                        device.DownloadFile(file, localPath);

                        if (fileInfo.LastWriteTime.HasValue)
                        {
                            File.SetLastWriteTime(localPath, fileInfo.LastWriteTime.Value);
                        }

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
                    Append($"Error copying file: {file} - {ex.Message}");
                }
            }

            Append($"Copied: {copiedFileCount}, Skipped: {skippedFileCount}, Failed: {copyFailedFileCount}, Total Processed: {processedFileCount}.");
        }

        private bool ShouldCopyFile(MediaFileInfo fileInfo, string localPath)
        {
            if (!File.Exists(localPath))
                return true;

            var localFile = new FileInfo(localPath);

            return Math.Abs((long)fileInfo.Length - localFile.Length) > 1024 ||
                   Math.Abs((fileInfo.LastWriteTime - localFile.LastWriteTime).Value.TotalSeconds) > 2;
        }

        private void PopulateDeviceList()
        {
            _connectedDevices = MediaDevice.GetDevices().Select(d => d.FriendlyName);

#if !DEBUG
            if (_connectedDevices.IsNullOrEmpty())
            {
                MessageBox.Show("No connected MTP devices found. Please connect your device and enable file transfer mode.",
                    "No Devices Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
#endif

            comboBoxDeviceNames.Items.Clear();
            comboBoxDeviceNames.Items.AddRange(_connectedDevices.OrderBy(n => n).ToArray());

            if (comboBoxDeviceNames.Items.Count == 1)
            {
                comboBoxDeviceNames.SelectedIndex = 0;
            }

            _selectedDeviceName = comboBoxDeviceNames.SelectedItem as string;
        }

        private void Append(string text)
        {
            // ToDo: Add later.
        }

        private void buttonBrowseBackupFolderPath_Click(object sender, EventArgs e)
        {
            var folderDlg = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            var dialogResult = folderDlg.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                textBoxBackupFolderPath.Text = folderDlg.SelectedPath;

                EAppSettings.DefaultBackupFolderPath.SaveSettings(folderDlg.SelectedPath);
            }
        }

        private void radioButtonCopySelected_CheckedChanged(object sender, EventArgs e)
        {
            var isCopySelected = radioButtonCopySelected.Checked;
            groupBoxFileTypes.Enabled = isCopySelected;
            EAppSettings.CopyAllFiles.SaveSettings(isCopySelected ? "false" : "true");
        }

        private async void buttonStartBackup_Click(object sender, EventArgs e)
        {
            if (_selectedDeviceName.IsNullOrWhiteSpace())
            {
                Append("Please select a device before starting the backup.");
                MessageBox.Show("Please select a device before starting the backup.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            buttonStartBackup.Enabled = false;
            backupStartTime = DateTime.Now;
            _elapsedTimer.Start();

            labelProgress.Text = "Evaluating files to backup...";

            try
            {
                await Task.Run(() => PerformBackup());
            }
            catch (Exception ex)
            {
                Append($"Error: {ex.Message}");
            }
            finally
            {
                _elapsedTimer.Stop();
                buttonStartBackup.Enabled = true;
            }
        }

        private void PerformBackup()
        {
            var backupRoot = textBoxBackupFolderPath.Text;
            var phoneFolder = textBoxPhoneMtpPath.Text.TrimEnd('\\', '/');

            Directory.CreateDirectory(backupRoot);
            var device = MediaDevice.GetDevices().First(d => d.FriendlyName == _selectedDeviceName);

            try
            {
                device.Connect();
                Append($"Connected to: {device.FriendlyName}");
                BackupFromDevice(device, phoneFolder, backupRoot);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                device.Disconnect();
                Append($"Disconnected from: {device.FriendlyName}");
            }
        }
    }
}
