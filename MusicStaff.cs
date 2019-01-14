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
		private static NoteLength[] NoteLengths = (NoteLength[]) Enum.GetValues(typeof(NoteLength));
		private static Bitmap Treble = Resources.Treble;
		private static Bitmap Bass = Resources.Bass;
		private static Bitmap TimeSignature = Resources._44;
		public event Action StartedPlaying;
		public event Action StoppedPlaying;
		public MusicKeyboard Keyboard;
		private Timer Timer;
		/// <summary>
		/// The length in milliseconds for a half-hemidemisemiquaver
		/// </summary>
		internal float millisPerHalfHemiDemiSemiQuaver = 467 / 32f;
		public const int LineSpace = 10;
		public const int BarTopDistance = 20;
		public const int BarVerticalSpacing = LineSpace * 2;
		public const int NoteWidth = 36;
		public const int NoteSpacing = 5;
		public const int NoteStartX = 65;
		private Point nextNoteLoc = new Point(NoteStartX, BarTopDistance);
		private int lastWidth;
		private int playerNoteIndex;

		public bool IsPlaying {
			get {
				return Timer.Enabled;
			}
		}

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
				if (playerNoteIndex != 0) {
					MusicNote oldNote = (MusicNote) Controls[playerNoteIndex - 1];
					Keyboard.MarkKeyReleased(oldNote.Pitch, false);
					oldNote.Highlighted = false;
				}
				MusicNote currentNote = (MusicNote) Controls[playerNoteIndex];
				Timer.Interval = (int) currentNote.LengthInMilliseconds;
				Keyboard.MarkKeyPressed(currentNote.Pitch, false);
				currentNote.Highlighted = true;
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
				nextNoteLoc = new Point(NoteStartX, BarTopDistance);
				int verticalScroll = VerticalScroll.Value;
				foreach (MusicNote note in Controls) {
					note.Location = new Point(nextNoteLoc.X, nextNoteLoc.Y - verticalScroll);
					note.BottomBarY = nextNoteLoc.Y - verticalScroll;
					UpdateNextLoc(note.Width);
				}
				Invalidate(false);
			}
		}

		private void UpdateNextLoc(int lastNoteWidth) {
			nextNoteLoc.X += lastNoteWidth + NoteSpacing;
			if (nextNoteLoc.X > ClientSize.Width - NoteWidth) {
				nextNoteLoc = new Point(NoteStartX, nextNoteLoc.Y + BarVerticalSpacing + (BarTopDistance + LineSpace * 5) * 2);
				Invalidate(false);
			}
		}

		public void AddNote(NoteEnum pitch, float ms) {
			AddNote(pitch, ToNoteLength(ms));
		}

		public void AddNote(NoteEnum pitch, NoteLength length) {
			if (pitch == NoteEnum.None && length < NoteLength.HemiDemiSemiQuaver)
				return;
			MusicNote noteControl = new MusicNote(this, Keyboard, pitch, length, new Point(nextNoteLoc.X, nextNoteLoc.Y - VerticalScroll.Value));
			UpdateNextLoc(noteControl.Width);
		}

		public NoteLength ToNoteLength(float ms) {
			int length = (int) (ms / millisPerHalfHemiDemiSemiQuaver);
			for (int i = NoteLengths.Length - 1; i >= 0; i--) {
				if (length >= (int) NoteLengths[i])
					return NoteLengths[i];
			}
			return NoteLength.None;
		}

		public void StartPlayingNotes() {
			Timer.Interval = 1;
			Timer.Start();
			if (StartedPlaying != null)
				StartedPlaying();
		}

		public void PausePlayingNotes(bool leaveHighlighted = false) {
			Timer.Stop();
			int index = playerNoteIndex - 1;
			if (index >= 0 && index < Controls.Count) {
				MusicNote current = (MusicNote) Controls[index];
				Keyboard.MarkKeyReleased(current.Pitch, false);
				current.Highlighted = leaveHighlighted;
			}
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
			StopPlayingNotes();
			Controls.Clear();
			nextNoteLoc = new Point(NoteStartX, BarTopDistance);
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
			noteLocY += BarTopDistance + LineSpace * 4;
			int y = BarTopDistance / 2 - VerticalScroll.Value;
			const int barHeight = LineSpace * 5;
			int i;
			bool addSpacing = false;
			while (y <= noteLocY) {
				for (i = 0; i < 5; ++i) {
					y += LineSpace;
					e.Graphics.DrawLine(Pens.Black, 0, y, clientSize.Width, y);
				}
				y += BarTopDistance;
				if (addSpacing)
					y += BarVerticalSpacing;
				addSpacing = !addSpacing;
			} 
			e.Graphics.CompositingMode = CompositingMode.SourceOver;
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			y = BarTopDistance / 2 + LineSpace - VerticalScroll.Value;
			int max = noteLocY + LineSpace;
			const int bassHeight = LineSpace * 3;
			while (y <= max) {
				e.Graphics.DrawImage(Treble, 10, y, (barHeight * Treble.Width) / Treble.Height, barHeight);
				e.Graphics.DrawImage(TimeSignature, 40, y, ((barHeight - LineSpace) * TimeSignature.Width) / TimeSignature.Height, barHeight - LineSpace);
				y += barHeight + BarTopDistance;
				e.Graphics.DrawImage(Bass, 10, y + LineSpace / 2, (bassHeight * Bass.Width) / Bass.Height, bassHeight);
				e.Graphics.DrawImage(TimeSignature, 40, y, ((barHeight - LineSpace) * TimeSignature.Width) / TimeSignature.Height, barHeight - LineSpace);
				y += barHeight + BarTopDistance + BarVerticalSpacing;
			}
		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			Invalidate(false);
		}

		protected override void Dispose(bool disposing) {
			Timer.Dispose();
			base.Dispose(disposing);
		}
	}
}