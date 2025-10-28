namespace An2WinFileTransfer.UI.Forms
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxDeviceNames = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxBackupFolderPath = new System.Windows.Forms.TextBox();
            this.buttonBrowseBackupFolderPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButtonCopyAll = new System.Windows.Forms.RadioButton();
            this.radioButtonCopySelected = new System.Windows.Forms.RadioButton();
            this.groupBoxFileTypes = new System.Windows.Forms.GroupBox();
            this.buttonStartBackup = new System.Windows.Forms.Button();
            this.textBoxPhoneMtpPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelProgress = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelElapsedTime = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxDeviceNames
            // 
            this.comboBoxDeviceNames.FormattingEnabled = true;
            this.comboBoxDeviceNames.Location = new System.Drawing.Point(128, 44);
            this.comboBoxDeviceNames.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBoxDeviceNames.Name = "comboBoxDeviceNames";
            this.comboBoxDeviceNames.Size = new System.Drawing.Size(178, 21);
            this.comboBoxDeviceNames.TabIndex = 0;
            this.comboBoxDeviceNames.SelectedIndexChanged += new System.EventHandler(this.comboBoxDeviceNames_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 47);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connected Devices: ";
            // 
            // textBoxBackupFolderPath
            // 
            this.textBoxBackupFolderPath.Location = new System.Drawing.Point(128, 19);
            this.textBoxBackupFolderPath.Name = "textBoxBackupFolderPath";
            this.textBoxBackupFolderPath.Size = new System.Drawing.Size(264, 20);
            this.textBoxBackupFolderPath.TabIndex = 2;
            // 
            // buttonBrowseBackupFolderPath
            // 
            this.buttonBrowseBackupFolderPath.Location = new System.Drawing.Point(398, 16);
            this.buttonBrowseBackupFolderPath.Name = "buttonBrowseBackupFolderPath";
            this.buttonBrowseBackupFolderPath.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseBackupFolderPath.TabIndex = 3;
            this.buttonBrowseBackupFolderPath.Text = "Browse";
            this.buttonBrowseBackupFolderPath.UseVisualStyleBackColor = true;
            this.buttonBrowseBackupFolderPath.Click += new System.EventHandler(this.buttonBrowseBackupFolderPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 22);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Backup Folder: ";
            // 
            // radioButtonCopyAll
            // 
            this.radioButtonCopyAll.AutoSize = true;
            this.radioButtonCopyAll.Location = new System.Drawing.Point(20, 126);
            this.radioButtonCopyAll.Name = "radioButtonCopyAll";
            this.radioButtonCopyAll.Size = new System.Drawing.Size(169, 17);
            this.radioButtonCopyAll.TabIndex = 5;
            this.radioButtonCopyAll.TabStop = true;
            this.radioButtonCopyAll.Text = "Copy All Files (time consuming)";
            this.radioButtonCopyAll.UseVisualStyleBackColor = true;
            // 
            // radioButtonCopySelected
            // 
            this.radioButtonCopySelected.AutoSize = true;
            this.radioButtonCopySelected.Location = new System.Drawing.Point(20, 103);
            this.radioButtonCopySelected.Name = "radioButtonCopySelected";
            this.radioButtonCopySelected.Size = new System.Drawing.Size(142, 17);
            this.radioButtonCopySelected.TabIndex = 6;
            this.radioButtonCopySelected.TabStop = true;
            this.radioButtonCopySelected.Text = "Copy selected file types: ";
            this.radioButtonCopySelected.UseVisualStyleBackColor = true;
            this.radioButtonCopySelected.CheckedChanged += new System.EventHandler(this.radioButtonCopySelected_CheckedChanged);
            // 
            // groupBoxFileTypes
            // 
            this.groupBoxFileTypes.Location = new System.Drawing.Point(195, 103);
            this.groupBoxFileTypes.Name = "groupBoxFileTypes";
            this.groupBoxFileTypes.Size = new System.Drawing.Size(403, 108);
            this.groupBoxFileTypes.TabIndex = 7;
            this.groupBoxFileTypes.TabStop = false;
            this.groupBoxFileTypes.Text = "File Types";
            // 
            // buttonStartBackup
            // 
            this.buttonStartBackup.Location = new System.Drawing.Point(266, 241);
            this.buttonStartBackup.Name = "buttonStartBackup";
            this.buttonStartBackup.Size = new System.Drawing.Size(96, 42);
            this.buttonStartBackup.TabIndex = 8;
            this.buttonStartBackup.Text = "Start Backup";
            this.buttonStartBackup.UseVisualStyleBackColor = true;
            this.buttonStartBackup.Click += new System.EventHandler(this.buttonStartBackup_Click);
            // 
            // textBoxPhoneMtpPath
            // 
            this.textBoxPhoneMtpPath.Location = new System.Drawing.Point(128, 70);
            this.textBoxPhoneMtpPath.Name = "textBoxPhoneMtpPath";
            this.textBoxPhoneMtpPath.Size = new System.Drawing.Size(264, 20);
            this.textBoxPhoneMtpPath.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Phone MTP Name: ";
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(12, 288);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(235, 13);
            this.labelProgress.TabIndex = 11;
            this.labelProgress.Text = "Configure settings and click Start Backup button";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.comboBoxDeviceNames);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxPhoneMtpPath);
            this.groupBox1.Controls.Add(this.textBoxBackupFolderPath);
            this.groupBox1.Controls.Add(this.buttonBrowseBackupFolderPath);
            this.groupBox1.Controls.Add(this.groupBoxFileTypes);
            this.groupBox1.Controls.Add(this.radioButtonCopyAll);
            this.groupBox1.Controls.Add(this.radioButtonCopySelected);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(604, 223);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // labelElapsedTime
            // 
            this.labelElapsedTime.AutoSize = true;
            this.labelElapsedTime.Location = new System.Drawing.Point(488, 288);
            this.labelElapsedTime.Name = "labelElapsedTime";
            this.labelElapsedTime.Size = new System.Drawing.Size(119, 13);
            this.labelElapsedTime.TabIndex = 13;
            this.labelElapsedTime.Text = "Elapsed Time: 00:00:00";
            this.labelElapsedTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 311);
            this.Controls.Add(this.labelElapsedTime);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.buttonStartBackup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Android 2 Windows File Transfer";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxDeviceNames;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxBackupFolderPath;
        private System.Windows.Forms.Button buttonBrowseBackupFolderPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radioButtonCopyAll;
        private System.Windows.Forms.RadioButton radioButtonCopySelected;
        private System.Windows.Forms.GroupBox groupBoxFileTypes;
        private System.Windows.Forms.Button buttonStartBackup;
        private System.Windows.Forms.TextBox textBoxPhoneMtpPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelElapsedTime;
    }
}

