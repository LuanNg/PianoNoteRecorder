using System;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// The main window of the application
	/// </summary>
	public class MainWindow : Form {
		private MusicStaff musicStaff;
		private MusicKeyboard musicKeyboard;
		private SplitContainer splitContainer1, splitContainer2;
		private Button stopButton, playButton, saveButton, loadButton, clearAllButton;
		private NumericUpDown beatLengthSelector;
		private Panel controlPanel;
		private TrackBar zoomTrackBar;
		private Label beatLengthLabel;
		private bool isPlaying;

		/// <summary>
		/// The constructor of the window
		/// </summary>
		public MainWindow() {
			InitializeComponent();
			beatLengthSelector.TextChanged += beatLengthSelector_ValueChanged;
			splitContainer1.MouseUp += SplitContainer_MouseUp;
			splitContainer2.MouseUp += SplitContainer_MouseUp;
			zoomTrackBar.MouseDown += ZoomTrackBar_MouseDown;
			zoomTrackBar.MouseUp += ZoomTrackBar_MouseUp;
			musicStaff.StartedPlaying += MusicStaff_StartedPlaying;
			musicStaff.StoppedPlaying += MusicStaff_StoppedPlaying;
		}

		/// <summary>
		/// Called when the window is shown
		/// </summary>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			playButton.Focus();
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			musicKeyboard.MarkKeyPressed(e);
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			musicKeyboard.MarkKeyReleased(e);
		}

		/// <summary>
		/// Called when the window size has changed
		/// </summary>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			controlPanel.Left = (controlPanel.Parent.Width - controlPanel.Width) / 2; //places control panel in center
			musicStaff.PerformLayout(); //force music staff to refresh layout (because of maximize and restore bug)
		}

		/// <summary>
		/// Called when a split container is released
		/// </summary>
		private void SplitContainer_MouseUp(object sender, MouseEventArgs e) {
			playButton.Focus();
		}

		private void ZoomTrackBar_MouseDown(object sender, MouseEventArgs e) {
			musicKeyboard.ShowHint = true;
		}

		private void ZoomTrackBar_MouseUp(object sender, MouseEventArgs e) {
			musicKeyboard.ShowHint = false;
		}

		/// <summary>
		/// Called when the beat length has been changed
		/// </summary>
		private void beatLengthSelector_ValueChanged(object sender, EventArgs e) {
			musicStaff.MillisPerBeat = (float) beatLengthSelector.Value;
		}

		/// <summary>
		/// Called when the zoom trackbar has been moved
		/// </summary>
		private void zoomTrackBar_Scroll(object sender, EventArgs e) {
			musicKeyboard.WidthScalePercentage = zoomTrackBar.Value;
		}

		/// <summary>
		/// Called when the play button has been clicked
		/// </summary>
		private void playButton_Click(object sender, EventArgs e) {
			if (isPlaying) {
				isPlaying = false;
				musicStaff.PausePlayingNotes();
			} else {
				isPlaying = true;
				musicStaff.StartPlayingNotes();
			}
		}

		private void MusicStaff_StartedPlaying() {
			isPlaying = true;
			playButton.Text = "Pause ❚❚";
		}

		private void MusicStaff_StoppedPlaying() {
			isPlaying = false;
			playButton.Text = "Play ▶";
		}

		/// <summary>
		/// Called when the stop button has been clicked
		/// </summary>
		private void stopButton_Click(object sender, EventArgs e) {
			musicStaff.StopPlayingNotes();
		}

		/// <summary>
		/// Called when the save button has been clicked
		/// </summary>
		private void saveButton_Click(object sender, EventArgs e) {

		}

		/// <summary>
		/// Called when the load button has been clicked
		/// </summary>
		private void loadButton_Click(object sender, EventArgs e) {

		}

		/// <summary>
		/// Called when the Clear All button has been clicked
		/// </summary>
		private void clearAllButton_Click(object sender, EventArgs e) {
			if (MessageBox.Show("Are you sure you want to remove all notes from the staff?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				musicStaff.ClearAllNotes();
		}

		/// <summary>
		/// The main entry point for the application
		/// </summary>
		[STAThread]
		public static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow());
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.musicStaff = new PianoNoteRecorder.MusicStaff();
			this.controlPanel = new System.Windows.Forms.Panel();
			this.beatLengthLabel = new System.Windows.Forms.Label();
			this.clearAllButton = new System.Windows.Forms.Button();
			this.stopButton = new System.Windows.Forms.Button();
			this.playButton = new System.Windows.Forms.Button();
			this.beatLengthSelector = new System.Windows.Forms.NumericUpDown();
			this.saveButton = new System.Windows.Forms.Button();
			this.loadButton = new System.Windows.Forms.Button();
			this.musicKeyboard = new PianoNoteRecorder.MusicKeyboard(musicStaff);
			this.zoomTrackBar = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.controlPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.beatLengthSelector)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zoomTrackBar)).BeginInit();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.musicKeyboard);
			this.splitContainer1.Panel2.Controls.Add(this.zoomTrackBar);
			this.splitContainer1.Panel2MinSize = 35;
			this.splitContainer1.Size = new System.Drawing.Size(632, 454);
			this.splitContainer1.SplitterDistance = 321;
			this.splitContainer1.SplitterWidth = 7;
			this.splitContainer1.TabIndex = 2;
			// 
			// splitContainer2
			// 
			this.splitContainer2.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer2.IsSplitterFixed = true;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.musicStaff);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.Control;
			this.splitContainer2.Panel2.Controls.Add(this.controlPanel);
			this.splitContainer2.Panel2MinSize = 35;
			this.splitContainer2.Size = new System.Drawing.Size(632, 321);
			this.splitContainer2.SplitterDistance = 285;
			this.splitContainer2.SplitterWidth = 1;
			this.splitContainer2.TabIndex = 1;
			// 
			// musicStaff
			// 
			this.musicStaff.AutoScroll = true;
			this.musicStaff.AutoScrollMinSize = new System.Drawing.Size(594, 1000);
			this.musicStaff.BackColor = System.Drawing.Color.White;
			this.musicStaff.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.musicStaff.Dock = System.Windows.Forms.DockStyle.Fill;
			this.musicStaff.Location = new System.Drawing.Point(0, 0);
			this.musicStaff.Name = "musicStaff";
			this.musicStaff.Size = new System.Drawing.Size(632, 285);
			this.musicStaff.TabIndex = 0;
			// 
			// controlPanel
			// 
			this.controlPanel.BackColor = System.Drawing.SystemColors.Control;
			this.controlPanel.Controls.Add(this.beatLengthLabel);
			this.controlPanel.Controls.Add(this.clearAllButton);
			this.controlPanel.Controls.Add(this.stopButton);
			this.controlPanel.Controls.Add(this.playButton);
			this.controlPanel.Controls.Add(this.beatLengthSelector);
			this.controlPanel.Controls.Add(this.saveButton);
			this.controlPanel.Controls.Add(this.loadButton);
			this.controlPanel.Location = new System.Drawing.Point(12, 2);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(595, 30);
			this.controlPanel.TabIndex = 7;
			// 
			// beatLengthLabel
			// 
			this.beatLengthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.beatLengthLabel.Location = new System.Drawing.Point(0, 0);
			this.beatLengthLabel.Name = "beatLengthLabel";
			this.beatLengthLabel.Size = new System.Drawing.Size(96, 30);
			this.beatLengthLabel.TabIndex = 5;
			this.beatLengthLabel.Text = "Beat Length in Milliseconds:";
			this.beatLengthLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// clearAllButton
			// 
			this.clearAllButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.clearAllButton.Location = new System.Drawing.Point(495, 0);
			this.clearAllButton.Name = "clearAllButton";
			this.clearAllButton.Size = new System.Drawing.Size(95, 31);
			this.clearAllButton.TabIndex = 6;
			this.clearAllButton.Text = "Clear All ×";
			this.clearAllButton.UseVisualStyleBackColor = true;
			this.clearAllButton.Click += new System.EventHandler(this.clearAllButton_Click);
			// 
			// stopButton
			// 
			this.stopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.stopButton.Location = new System.Drawing.Point(246, 0);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = new System.Drawing.Size(77, 31);
			this.stopButton.TabIndex = 1;
			this.stopButton.Text = "Stop ■";
			this.stopButton.UseVisualStyleBackColor = true;
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
			// 
			// playButton
			// 
			this.playButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.playButton.Location = new System.Drawing.Point(163, 0);
			this.playButton.Name = "playButton";
			this.playButton.Size = new System.Drawing.Size(77, 31);
			this.playButton.TabIndex = 0;
			this.playButton.Text = "Play ▶";
			this.playButton.UseVisualStyleBackColor = true;
			this.playButton.Click += new System.EventHandler(this.playButton_Click);
			// 
			// beatLengthSelector
			// 
			this.beatLengthSelector.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.beatLengthSelector.Location = new System.Drawing.Point(97, 5);
			this.beatLengthSelector.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.beatLengthSelector.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.beatLengthSelector.Name = "beatLengthSelector";
			this.beatLengthSelector.Size = new System.Drawing.Size(60, 21);
			this.beatLengthSelector.TabIndex = 4;
			this.beatLengthSelector.Value = new decimal(new int[] {
            467,
            0,
            0,
            0});
			this.beatLengthSelector.ValueChanged += new System.EventHandler(this.beatLengthSelector_ValueChanged);
			// 
			// saveButton
			// 
			this.saveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.saveButton.Location = new System.Drawing.Point(329, 0);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(77, 31);
			this.saveButton.TabIndex = 2;
			this.saveButton.Text = "Save 💾";
			this.saveButton.UseVisualStyleBackColor = true;
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// loadButton
			// 
			this.loadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.loadButton.Location = new System.Drawing.Point(412, 0);
			this.loadButton.Name = "loadButton";
			this.loadButton.Size = new System.Drawing.Size(77, 31);
			this.loadButton.TabIndex = 3;
			this.loadButton.Text = "Load ←";
			this.loadButton.UseVisualStyleBackColor = true;
			this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
			// 
			// musicKeyboard
			// 
			this.musicKeyboard.BackColor = System.Drawing.SystemColors.Control;
			this.musicKeyboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.musicKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
			this.musicKeyboard.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.musicKeyboard.Location = new System.Drawing.Point(25, 0);
			this.musicKeyboard.Name = "musicKeyboard";
			this.musicKeyboard.ShowHint = false;
			this.musicKeyboard.Size = new System.Drawing.Size(607, 126);
			this.musicKeyboard.TabIndex = 1;
			this.musicKeyboard.Text = "Top of piano can also be resized using mouse";
			this.musicKeyboard.WidthScalePercentage = 100;
			// 
			// zoomTrackBar
			// 
			this.zoomTrackBar.AutoSize = false;
			this.zoomTrackBar.BackColor = System.Drawing.SystemColors.Control;
			this.zoomTrackBar.Dock = System.Windows.Forms.DockStyle.Left;
			this.zoomTrackBar.Location = new System.Drawing.Point(0, 0);
			this.zoomTrackBar.Maximum = 100;
			this.zoomTrackBar.Minimum = 33;
			this.zoomTrackBar.Name = "zoomTrackBar";
			this.zoomTrackBar.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.zoomTrackBar.Size = new System.Drawing.Size(25, 126);
			this.zoomTrackBar.TabIndex = 0;
			this.zoomTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.zoomTrackBar.Value = 100;
			this.zoomTrackBar.Scroll += new System.EventHandler(this.zoomTrackBar_Scroll);
			// 
			// MainWindow
			// 
			this.AcceptButton = this.playButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(632, 454);
			this.Controls.Add(this.splitContainer1);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(300, 100);
			this.Name = "MainWindow";
			this.Text = "Piano Note Recorder";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.controlPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.beatLengthSelector)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zoomTrackBar)).EndInit();
			this.ResumeLayout(false);

		}
	}
}