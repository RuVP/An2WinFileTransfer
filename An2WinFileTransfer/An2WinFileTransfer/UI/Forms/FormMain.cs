using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using An2WinFileTransfer.Enums;
using An2WinFileTransfer.Extensions;
using An2WinFileTransfer.Models;
using An2WinFileTransfer.Services;
using MediaDevices;

namespace An2WinFileTransfer.UI.Forms
{
    public partial class FormMain : Form
    {
        private IEnumerable<string> _connectedDevices = new List<string>();
        private IEnumerable<FileType> _fileTypes = new List<FileType>();
        private Timer _elapsedTimer = new Timer();
        private DateTime backupStartTime;

        private readonly DeviceService _deviceService = new DeviceService();
        private BackupService _backupService;

        private string _selectedDeviceName = string.Empty;

        public FormMain()
        {
            InitializeComponent();
            this.Load += FormMain_Load;

            _elapsedTimer.Interval = 1000; // 1 second
            _elapsedTimer.Tick += (s, ev) =>
            {
                var elapsed = DateTime.Now - backupStartTime;
                labelElapsedTime.Text = $"Elapsed time: {elapsed:hh\\:mm\\:ss}";
            };
        }

        private void FormMain_Load(object sender, EventArgs e)
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

            if (radioButtonCopyAll.Checked)
            {
                groupBoxFileTypes.Enabled = false;
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
                _selectedDeviceName = comboBoxDeviceNames.SelectedItem as string;
            }
        }

        private void Append(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => Append(text)));
                return;
            }

            // update label safely on UI thread
            labelProgress.Text = text;
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
            EAppSettings.CopyAllFiles.SaveSettings(isCopySelected ? false.ToString() : true.ToString());
        }

        private async void buttonStartBackup_Click(object sender, EventArgs e)
        {
            buttonStartBackup.Enabled = false;
            backupStartTime = DateTime.Now;
            _elapsedTimer.Start();

            _backupService = new BackupService(Append);

            await Task.Run(() =>
            {
                var backupRoot = textBoxBackupFolderPath.Text;
                var sourcePath = textBoxPhoneMtpPath.Text;
                var copyAll = radioButtonCopyAll.Checked;

                using (var device = _deviceService.ConnectToDevice(_selectedDeviceName))
                {
                    _backupService.BackupFromDevice(device, sourcePath, backupRoot, _fileTypes, copyAll);
                    _deviceService.DisconnectDevice(device);
                }

                BeginInvoke((Action)(() =>
                {
                    _elapsedTimer.Stop();
                    buttonStartBackup.Enabled = true;
                }));
            });
        }

        private void comboBoxDeviceNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedDeviceName = comboBoxDeviceNames.SelectedItem as string;
        }
    }
}
