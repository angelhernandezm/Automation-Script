using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DemoApp {
	public partial class frmViewer : Form {
		/// <summary>
		/// 
		/// </summary>
		private string fileToDisplay = string.Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="frmViewer"/> class.
		/// </summary>
		public frmViewer() {
			InitializeComponent();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="frmViewer"/> class.
		/// </summary>
		/// <param name="selectedFile">The selected file.</param>
		public frmViewer(string selectedFile)
			: this() {
			fileToDisplay = selectedFile;
		}

		/// <summary>
		/// Handles the Load event of the frmViewer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void frmViewer_Load(object sender, EventArgs e) {
			rtfViewer.Text = File.ReadAllText(fileToDisplay);
		}
	}
}
