namespace DemoApp {
	partial class frmMain {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.grpMain = new System.Windows.Forms.GroupBox();
			this.txtDemoDescription = new System.Windows.Forms.TextBox();
			this.btnRun = new System.Windows.Forms.Button();
			this.cboScripts = new System.Windows.Forms.ComboBox();
			this.lblScriptToRun = new System.Windows.Forms.Label();
			this.pgbExecution = new System.Windows.Forms.ProgressBar();
			this.tipMain = new System.Windows.Forms.ToolTip(this.components);
			this.grpMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// grpMain
			// 
			this.grpMain.Controls.Add(this.txtDemoDescription);
			this.grpMain.Controls.Add(this.btnRun);
			this.grpMain.Controls.Add(this.cboScripts);
			this.grpMain.Controls.Add(this.lblScriptToRun);
			this.grpMain.Location = new System.Drawing.Point(21, 12);
			this.grpMain.Name = "grpMain";
			this.grpMain.Size = new System.Drawing.Size(383, 114);
			this.grpMain.TabIndex = 0;
			this.grpMain.TabStop = false;
			this.grpMain.Text = "Demo Options";
			// 
			// txtDemoDescription
			// 
			this.txtDemoDescription.Location = new System.Drawing.Point(12, 58);
			this.txtDemoDescription.Multiline = true;
			this.txtDemoDescription.Name = "txtDemoDescription";
			this.txtDemoDescription.ReadOnly = true;
			this.txtDemoDescription.Size = new System.Drawing.Size(360, 47);
			this.txtDemoDescription.TabIndex = 3;
			// 
			// btnRun
			// 
			this.btnRun.Location = new System.Drawing.Point(295, 18);
			this.btnRun.Name = "btnRun";
			this.btnRun.Size = new System.Drawing.Size(75, 23);
			this.btnRun.TabIndex = 2;
			this.btnRun.Text = "Run";
			this.btnRun.UseVisualStyleBackColor = true;
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			// 
			// cboScripts
			// 
			this.cboScripts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboScripts.FormattingEnabled = true;
			this.cboScripts.Items.AddRange(new object[] {
            "(None)",
            "Notepad Automation"});
			this.cboScripts.Location = new System.Drawing.Point(93, 19);
			this.cboScripts.Name = "cboScripts";
			this.cboScripts.Size = new System.Drawing.Size(196, 21);
			this.cboScripts.TabIndex = 1;
			this.cboScripts.SelectedIndexChanged += new System.EventHandler(this.cboScripts_SelectedIndexChanged);
			// 
			// lblScriptToRun
			// 
			this.lblScriptToRun.AutoSize = true;
			this.lblScriptToRun.Location = new System.Drawing.Point(14, 23);
			this.lblScriptToRun.Name = "lblScriptToRun";
			this.lblScriptToRun.Size = new System.Drawing.Size(72, 13);
			this.lblScriptToRun.TabIndex = 0;
			this.lblScriptToRun.Text = "Script to Run:";
			// 
			// pgbExecution
			// 
			this.pgbExecution.Location = new System.Drawing.Point(21, 145);
			this.pgbExecution.Name = "pgbExecution";
			this.pgbExecution.Size = new System.Drawing.Size(383, 23);
			this.pgbExecution.TabIndex = 1;
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(423, 180);
			this.Controls.Add(this.pgbExecution);
			this.Controls.Add(this.grpMain);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "AutomationScript Demo App";
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.grpMain.ResumeLayout(false);
			this.grpMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox grpMain;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.ComboBox cboScripts;
		private System.Windows.Forms.Label lblScriptToRun;
		private System.Windows.Forms.ProgressBar pgbExecution;
		private System.Windows.Forms.TextBox txtDemoDescription;
		private System.Windows.Forms.ToolTip tipMain;
	}
}

