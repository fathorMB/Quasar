namespace BEAM.Emulator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDiscover = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lstServers = new System.Windows.Forms.ListBox();
            this.txtDeviceName = new System.Windows.Forms.TextBox();
            this.cmbDeviceType = new System.Windows.Forms.ComboBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblServers = new System.Windows.Forms.Label();
            this.lblDeviceName = new System.Windows.Forms.Label();
            this.lblDeviceType = new System.Windows.Forms.Label();
            this.lblDeviceId = new System.Windows.Forms.Label();
            this.txtDeviceId = new System.Windows.Forms.TextBox();
            this.lblMacAddress = new System.Windows.Forms.Label();
            this.txtMacAddress = new System.Windows.Forms.TextBox();
            this.btnStartHeartbeat = new System.Windows.Forms.Button();
            this.btnStopHeartbeat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDiscover
            // 
            this.btnDiscover.Location = new System.Drawing.Point(12, 12);
            this.btnDiscover.Name = "btnDiscover";
            this.btnDiscover.Size = new System.Drawing.Size(150, 30);
            this.btnDiscover.TabIndex = 0;
            this.btnDiscover.Text = "Discover Servers";
            this.btnDiscover.UseVisualStyleBackColor = true;
            this.btnDiscover.Click += new System.EventHandler(this.btnDiscover_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(462, 295);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(150, 30);
            this.btnGenerate.TabIndex = 13;
            this.btnGenerate.Text = "Generate Random";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lstServers
            // 
            this.lstServers.FormattingEnabled = true;
            this.lstServers.ItemHeight = 15;
            this.lstServers.Location = new System.Drawing.Point(12, 72);
            this.lstServers.Name = "lstServers";
            this.lstServers.Size = new System.Drawing.Size(600, 94);
            this.lstServers.TabIndex = 1;
            // 
            // txtDeviceName
            // 
            this.txtDeviceName.Location = new System.Drawing.Point(120, 200);
            this.txtDeviceName.Name = "txtDeviceName";
            this.txtDeviceName.Size = new System.Drawing.Size(300, 23);
            this.txtDeviceName.TabIndex = 2;
            // 
            // cmbDeviceType
            // 
            this.cmbDeviceType.FormattingEnabled = true;
            this.cmbDeviceType.Items.AddRange(new object[] {
            "ESP32-Generic",
            "ES32-TempSensor",
            "ESP32-MotionDetector",
            "ESP32-Relay"});
            this.cmbDeviceType.Location = new System.Drawing.Point(120, 235);
            this.cmbDeviceType.Name = "cmbDeviceType";
            this.cmbDeviceType.Size = new System.Drawing.Size(300, 23);
            this.cmbDeviceType.TabIndex = 3;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(12, 325);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(150, 30);
            this.btnRegister.TabIndex = 4;
            this.btnRegister.Text = "Register Device";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 370);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(600, 150);
            this.txtLog.TabIndex = 5;
            // 
            // lblServers
            // 
            this.lblServers.AutoSize = true;
            this.lblServers.Location = new System.Drawing.Point(12, 54);
            this.lblServers.Name = "lblServers";
            this.lblServers.Size = new System.Drawing.Size(116, 15);
            this.lblServers.TabIndex = 6;
            this.lblServers.Text = "Discovered Servers:";
            // 
            // lblDeviceName
            // 
            this.lblDeviceName.AutoSize = true;
            this.lblDeviceName.Location = new System.Drawing.Point(12, 203);
            this.lblDeviceName.Name = "lblDeviceName";
            this.lblDeviceName.Size = new System.Drawing.Size(82, 15);
            this.lblDeviceName.TabIndex = 7;
            this.lblDeviceName.Text = "Device Name:";
            // 
            // lblDeviceType
            // 
            this.lblDeviceType.AutoSize = true;
            this.lblDeviceType.Location = new System.Drawing.Point(12, 238);
            this.lblDeviceType.Name = "lblDeviceType";
            this.lblDeviceType.Size = new System.Drawing.Size(74, 15);
            this.lblDeviceType.TabIndex = 8;
            this.lblDeviceType.Text = "Device Type:";
            // 
            // lblDeviceId
            // 
            this.lblDeviceId.AutoSize = true;
            this.lblDeviceId.Location = new System.Drawing.Point(12, 273);
            this.lblDeviceId.Name = "lblDeviceId";
            this.lblDeviceId.Size = new System.Drawing.Size(61, 15);
            this.lblDeviceId.TabIndex = 9;
            this.lblDeviceId.Text = "Device ID:";
            // 
            // txtDeviceId
            // 
            this.txtDeviceId.Location = new System.Drawing.Point(120, 270);
            this.txtDeviceId.Name = "txtDeviceId";
            this.txtDeviceId.ReadOnly = true;
            this.txtDeviceId.Size = new System.Drawing.Size(300, 23);
            this.txtDeviceId.TabIndex = 10;
            // 
            // lblMacAddress
            // 
            this.lblMacAddress.AutoSize = true;
            this.lblMacAddress.Location = new System.Drawing.Point(12, 302);
            this.lblMacAddress.Name = "lblMacAddress";
            this.lblMacAddress.Size = new System.Drawing.Size(82, 15);
            this.lblMacAddress.TabIndex = 11;
            this.lblMacAddress.Text = "MAC Address:";
            // 
            // txtMacAddress
            // 
            this.txtMacAddress.Location = new System.Drawing.Point(120, 299);
            this.txtMacAddress.Name = "txtMacAddress";
            this.txtMacAddress.ReadOnly = true;
            this.txtMacAddress.Size = new System.Drawing.Size(300, 23);
            this.txtMacAddress.TabIndex = 12;
            // 
            // btnStartHeartbeat
            // 
            this.btnStartHeartbeat.Location = new System.Drawing.Point(180, 325);
            this.btnStartHeartbeat.Name = "btnStartHeartbeat";
            this.btnStartHeartbeat.Size = new System.Drawing.Size(150, 30);
            this.btnStartHeartbeat.TabIndex = 14;
            this.btnStartHeartbeat.Text = "Start Heartbeat";
            this.btnStartHeartbeat.UseVisualStyleBackColor = true;
            this.btnStartHeartbeat.Click += new System.EventHandler(this.btnStartHeartbeat_Click);
            // 
            // btnStopHeartbeat
            // 
            this.btnStopHeartbeat.Location = new System.Drawing.Point(342, 325);
            this.btnStopHeartbeat.Name = "btnStopHeartbeat";
            this.btnStopHeartbeat.Size = new System.Drawing.Size(150, 30);
            this.btnStopHeartbeat.TabIndex = 15;
            this.btnStopHeartbeat.Text = "Stop Heartbeat";
            this.btnStopHeartbeat.UseVisualStyleBackColor = true;
            this.btnStopHeartbeat.Click += new System.EventHandler(this.btnStopHeartbeat_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 541);
            this.Controls.Add(this.btnStopHeartbeat);
            this.Controls.Add(this.btnStartHeartbeat);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.txtMacAddress);
            this.Controls.Add(this.lblMacAddress);
            this.Controls.Add(this.txtDeviceId);
            this.Controls.Add(this.lblDeviceId);
            this.Controls.Add(this.lblDeviceType);
            this.Controls.Add(this.lblDeviceName);
            this.Controls.Add(this.lblServers);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.cmbDeviceType);
            this.Controls.Add(this.txtDeviceName);
            this.Controls.Add(this.lstServers);
            this.Controls.Add(this.btnDiscover);
            this.Name = "MainForm";
            this.Text = "BEAM Device Emulator";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private Button btnDiscover;
        private Button btnGenerate;
        private ListBox lstServers;
        private TextBox txtDeviceName;
        private ComboBox cmbDeviceType;
        private Button btnRegister;
        private TextBox txtLog;
        private Label lblServers;
        private Label lblDeviceName;
        private Label lblDeviceType;
        private Label lblDeviceId;
        private TextBox txtDeviceId;
        private Label lblMacAddress;
        private TextBox txtMacAddress;
        private Button btnStartHeartbeat;
        private Button btnStopHeartbeat;
    }
}
