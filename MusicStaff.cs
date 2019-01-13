using PianoNoteRecorder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// A musical staff that will contain musical notes
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("A musical staff that will contain musical notes")]
	[DisplayName(nameof(MusicStaff))]
	public class MusicStaff : Panel {
		public event Action StartedPlaying;
		public event Action StoppedPlaying;
		private static Bitmap Treble = Resources.Treble;
		private static Bitmap TimeSignature = Resources._44;
		private Timer Timer;
		/// <summary>
		/// The length in milliseconds for a half-hemidemisemiquaver
		/// </summary>
		internal float millisPerHalfHemiDemiSemiQuaver = 467 / 32f;
		public const int LineSpace = 10;
		public const int BarTopDistance = 20;
		public const int NoteWidth = 9;
		private Point nextNoteLoc = new Point(60, BarTopDistance);
		private int lastWidth;
		private int playerNoteIndex;

		/// <summary>
		/// Gets or sets the global length in milliseconds for a crotchet (single beat)
		/// </summary>
		public float MillisPerBeat {
			get {
				return millisPerHalfHemiDemiSemiQuaver * 32;
			}
			set {
				millisPerHalfHemiDemiSemiQuaver = value * 0.03125f;
			}
		}

		public MusicStaff() {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, false);
			BackColor = Color.White;
			VScroll = true;
			HScroll = false;
			Timer = new Timer();
			Timer.Tick += Timer_Tick;
			Timer.Interval = 1;
			lastWidth = Width;
		}

		private void Timer_Tick(object sender, EventArgs e) {
			if (playerNoteIndex < Controls.Count) {
				if (playerNoteIndex != 0)
					MidiPlayer.PlayNote(((MusicNote) Controls[playerNoteIndex - 1]).Pitch, NoteVolume.silent);
				MusicNote currentNote = (MusicNote) Controls[playerNoteIndex];
				Timer.Interval = (int) currentNote.LengthInMilliseconds;
				MidiPlayer.PlayNote(currentNote.Pitch);
				playerNoteIndex++;
			} else
				StopPlayingNotes();
		}

		/// <summary>
		/// Called when the panel size changed
		/// </summary>
		protected override void OnClientSizeChanged(EventArgs e) {
			base.OnClientSizeChanged(e);
			if (ClientSize.Width != lastWidth) {
				lastWidth = ClientSize.Width;
				nextNoteLoc = new Point(60, BarTopDistance);
				foreach (MusicNote note in Controls) {
					note.Location = nextNoteLoc;
					note.BottomBarY = nextNoteLoc.Y;
					nextNoteLoc.X += note.Width + 10;
					if (nextNoteLoc.X > lastWidth - 10)
						nextNoteLoc = new Point(60, nextNoteLoc.Y + BarTopDistance + LineSpace * 5);
				}
			
			}
			Invalidate(false);
		}

		public void AddNote(NoteEnum pitch, float ms) {
			AddNote(pitch, MusicNote.ToNoteLength(ms));
		}

		public void AddNote(NoteEnum pitch, NoteLength note) {
			MusicNote noteControl = new MusicNote(this, pitch, note, nextNoteLoc);
			nextNoteLoc.X += noteControl.Width + 10;
			if (nextNoteLoc.X > ClientSize.Width - 10) {
				nextNoteLoc = new Point(60, nextNoteLoc.Y + BarTopDistance + LineSpace * 5);
				Invalidate(false);
			}
		}

		public void StartPlayingNotes() {
			Timer.Interval = 1;
			Timer.Start();
			if (StartedPlaying != null)
				StartedPlaying();
		}

		public void PausePlayingNotes() {
			Timer.Stop();
			int index = playerNoteIndex - 1;
			if (index >= 0 && index < Controls.Count)
				MidiPlayer.PlayNote(((MusicNote) Controls[index]).Pitch, NoteVolume.silent);
			if (StoppedPlaying != null)
				StoppedPlaying();
		}

		public void StopPlayingNotes() {
			PausePlayingNotes();
			playerNoteIndex = 0;
			if (StoppedPlaying != null)
				StoppedPlaying();
		}

		public void ClearAllNotes() {
			Controls.Clear();
			nextNoteLoc = new Point(60, BarTopDistance);
			Invalidate(false);
		}

		/// <summary>
		/// Called when the background is being drawn
		/// </summary>
		protected override void OnPaintBackground(PaintEventArgs e) {
		}

		/// <summary>
		/// Draws the musical staff
		/// </summary>
		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.CompositingMode = CompositingMode.SourceCopy;
			e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
			e.Graphics.Clear(BackColor);
			base.OnPaint(e);
			Size clientSize = ClientSize;
			int noteLocY = nextNoteLoc.Y - VerticalScroll.Value;
			int y = BarTopDistance / 2 - VerticalScroll.Value;
			const int barHeight = LineSpace * 5;
			int i;
			while (y <= noteLocY) {
				for (i = 0; i < 5; ++i) {
					y += LineSpace;
					e.Graphics.DrawLine(Pens.Black, 0, y, clientSize.Width, y);
				}
				y += BarTopDistance;
			} 
			e.Graphics.CompositingMode = CompositingMode.SourceOver;
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			y = BarTopDistance / 2 + LineSpace - VerticalScroll.Value;
			int max = noteLocY + LineSpace;
			while (y <= max) {
				e.Graphics.DrawImage(Treble, 10, y, (barHeight * Treble.Width) / Treble.Height, barHeight);
				e.Graphics.DrawImage(TimeSignature, 30, y, ((barHeight - LineSpace) * TimeSignature.Width) / TimeSignature.Height, barHeight - LineSpace);
				y += barHeight + BarTopDistance;
			}
		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			if (se.ScrollOrientation == ScrollOrientation.VerticalScroll) {
				Invalidate(false);
			}
		}

		protected override void Dispose(bool disposing) {
			Timer.Dispose();
			base.Dispose(disposing);
		}
	}
}