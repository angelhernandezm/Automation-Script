namespace DemoApp {
	partial class frmViewer {
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
			this.rtfViewer = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// rtfViewer
			// 
			this.rtfViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtfViewer.Location = new System.Drawing.Point(0, 0);
			this.rtfViewer.Name = "rtfViewer";
			this.rtfViewer.Size = new System.Drawing.Size(570, 434);
			this.rtfViewer.TabIndex = 0;
			this.rtfViewer.Text = "";
			// 
			// frmViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(570, 434);
			this.Controls.Add(this.rtfViewer);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmViewer";
			this.ShowInTaskbar = false;
			this.Text = "Text File Viewer";
			this.Load += new System.EventHandler(this.frmViewer_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox rtfViewer;
	}
}