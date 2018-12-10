using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NezvalPiano {
	public class MainWindow : Form {
		private MusicStaff musicStaff1;

		public MainWindow() {
			InitializeComponent();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow());
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
			this.musicStaff1 = new NezvalPiano.MusicStaff();
			this.SuspendLayout();
			// 
			// musicStaff1
			// 
			this.musicStaff1.Location = new System.Drawing.Point(13, 13);
			this.musicStaff1.Name = "musicStaff1";
			this.musicStaff1.Size = new System.Drawing.Size(238, 109);
			this.musicStaff1.TabIndex = 0;
			// 
			// MainWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(423, 317);
			this.Controls.Add(this.musicStaff1);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainWindow";
			this.Text = "Note Recorder";
			this.ResumeLayout(false);

		}
	}
}