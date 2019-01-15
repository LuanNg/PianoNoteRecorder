using PianoNoteRecorder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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
		private static Pen ScorePen = Pens.Black;
		public readonly List<MusicNote> Notes = new List<MusicNote>();
		public event Action StartedPlaying;
		public event Action StoppedPlaying;
		public MusicKeyboard Keyboard;
		private Timer Timer;
		/// <summary>
		/// The length in milliseconds for a half-hemidemisemiquaver
		/// </summary>
		internal float millisPerHalfHemiDemiSemiQuaver = DefaultBeatLengthInMs / 32f;
		public const int DefaultBeatLengthInMs = 400;
		public const int LineSpace = 10;
		public const int BarTopDistance = 20;
		public const int BarVerticalSpacing = LineSpace * 2;
		public const int NoteWidth = 36;
		public const int NoteSpacing = 10;
		public const int NoteStartX = 65;
		private MusicNote inputNote;
		private Point lastNoteLoc = new Point(NoteStartX, BarTopDistance);
		private Point nextNoteLoc = new Point(NoteStartX, BarTopDistance);
		private int lastWidth, playerNoteIndex;

		[Browsable(false)]
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
			Timer = new Timer();
			Timer.Tick += Timer_Tick;
			Timer.Interval = 1;
			HScroll = false;
			VScroll = true;
			lastWidth = Width;
			AutoScrollMinSize = new Size(1, Height);
			VerticalScroll.Value = VerticalScroll.Minimum;
			PerformLayout();
		}

		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
			VerticalScroll.Value = VerticalScroll.Minimum;
			PerformLayout();
		}

		/// <summary>
		/// Called when the panel size changed
		/// </summary>
		protected override void OnClientSizeChanged(EventArgs e) {
			base.OnClientSizeChanged(e);
			if (ClientSize.Width != lastWidth) {
				lastWidth = ClientSize.Width;
				nextNoteLoc = new Point(NoteStartX, BarTopDistance);
				lastNoteLoc = nextNoteLoc;
				foreach (MusicNote note in Notes) {
					note.Bounds.Location = nextNoteLoc;
					note.BottomBarY = nextNoteLoc.Y;
					UpdateNextLoc(note.Bounds.Width, false, false);
				}
				UpdateScroll(false);
				Invalidate(false);
			}
			VScroll = true;
		}

		private void UpdateScroll(bool scrollToBottom) {
			int oldMaximum = Math.Max(Height, lastNoteLoc.Y + (BarTopDistance * 2 + LineSpace * 10 + BarVerticalSpacing) * 2);
			if (AutoScrollMinSize.Height != oldMaximum) {
				AutoScrollMinSize = new Size(1, oldMaximum);
				if (scrollToBottom)
					VerticalScroll.Value = VerticalScroll.Maximum;
				PerformLayout();
				VScroll = true;
			}
		}

		private void UpdateNextLoc(int lastNoteWidth, bool updateScroll, bool scrollToBottom) {
			lastNoteLoc = nextNoteLoc;
			nextNoteLoc.X += lastNoteWidth + NoteSpacing;
			if (nextNoteLoc.X > ClientSize.Width - NoteWidth) {
				nextNoteLoc = new Point(NoteStartX, nextNoteLoc.Y + BarVerticalSpacing + (BarTopDistance + LineSpace * 5) * 2);
				if (updateScroll)
					UpdateScroll(scrollToBottom);
			}
		}

		private void Timer_Tick(object sender, EventArgs e) {
			if (playerNoteIndex < Notes.Count) {
				if (playerNoteIndex != 0) {
					MusicNote oldNote = Notes[playerNoteIndex - 1];
					Keyboard.MarkKeyReleased(oldNote.Pitch, false);
					oldNote.Highlighted = false;
				}
				MusicNote currentNote = Notes[playerNoteIndex];
				Timer.Interval = Math.Max((int) currentNote.LengthInMilliseconds, 1);
				Keyboard.MarkKeyPressed(currentNote.Pitch, false);
				currentNote.Highlighted = true;
				playerNoteIndex++;
			} else
				StopPlayingNotes();
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
			if (index >= 0 && index < Notes.Count) {
				MusicNote current = Notes[index];
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
			Notes.Clear();
			nextNoteLoc = new Point(NoteStartX, BarTopDistance);
			VerticalScroll.Value = 0;
			PerformLayout();
			Invalidate(false);
		}

		public void LoadAllNotes(string path) {
			ClearAllNotes();
			using (StreamReader reader = new StreamReader(path)) {
				string line;
				string[] sections;
				while (!reader.EndOfStream) {
					line = reader.ReadLine();
					sections = line.Split(',');
					AddNote((NoteEnum) int.Parse(sections[0]), (NoteLength) int.Parse(sections[1]));
				}
			}
		}

		public void SaveAllNotes(string path) {
			using (StreamWriter writer = new StreamWriter(path)) {
				foreach (MusicNote note in Notes)
					writer.WriteLine((int) note.Pitch + "," + (int) note.Length);
			}
		}

		public void AddNote(NoteEnum pitch, float ms) {
			AddNote(pitch, ToNoteLength(ms));
		}

		public void AddNote(NoteEnum pitch, NoteLength length) {
			if (IsPlaying || (pitch == NoteEnum.None && length < NoteLength.HemiDemiSemiQuaver))
				return;
			MusicNote noteControl = new MusicNote(this, Keyboard, pitch, length, nextNoteLoc);
			Notes.Add(noteControl);
			UpdateNextLoc(noteControl.Bounds.Width, true, true);
			Invalidate(noteControl.Bounds, false);
		}

		public NoteLength ToNoteLength(float ms) {
			int length = (int) (ms / millisPerHalfHemiDemiSemiQuaver);
			for (int i = NoteLengths.Length - 1; i >= 0; i--) {
				if (length >= (int) NoteLengths[i])
					return NoteLengths[i];
			}
			return NoteLength.None;
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			MusicNote note;
			for (int i = 0; i < Notes.Count; ++i) {
				note = Notes[i];
				if (note.Bounds.Contains(e.Location)) {
					if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
						Notes.RemoveAt(i);
						Invalidate(false);
					} else {
						inputNote = note;
						note.MarkMouseDown(new MouseEventArgs(e.Button, e.Clicks, e.X - note.Bounds.X, e.Y - note.Bounds.Y, e.Delta));
					}
					break;
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (inputNote != null)
				inputNote.MarkMouseMove(new MouseEventArgs(e.Button, e.Clicks, e.X - inputNote.Bounds.X, e.Y - inputNote.Bounds.Y, e.Delta));
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (inputNote != null)
				inputNote.MarkMouseUp(new MouseEventArgs(e.Button, e.Clicks, e.X - inputNote.Bounds.X, e.Y - inputNote.Bounds.Y, e.Delta));
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
			Graphics g = e.Graphics;
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			g.SmoothingMode = SmoothingMode.HighSpeed;
			g.Clear(BackColor);
			base.OnPaint(e);
			Size clientSize = ClientSize;
			int verticalScroll = VerticalScroll.Value;
			int noteLocY = nextNoteLoc.Y - verticalScroll;
			noteLocY += BarTopDistance + LineSpace * 4;
			int y = BarTopDistance / 2 - verticalScroll;
			const int barHeight = LineSpace * 5;
			int i;
			bool addSpacing = false;
			while (y <= noteLocY) {
				for (i = 0; i < 5; ++i) {
					y += LineSpace;
					g.DrawLine(ScorePen, 0, y, clientSize.Width, y);
				}
				y += BarTopDistance;
				if (addSpacing)
					y += BarVerticalSpacing;
				addSpacing = !addSpacing;
			}
			g.CompositingMode = CompositingMode.SourceOver;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			y = BarTopDistance / 2 + LineSpace - verticalScroll;
			int max = noteLocY + LineSpace;
			const int bassHeight = LineSpace * 3;
			while (y <= max) {
				g.DrawImage(Treble, 10, y, (barHeight * Treble.Width) / Treble.Height, barHeight);
				g.DrawImage(TimeSignature, 40, y, ((barHeight - LineSpace) * TimeSignature.Width) / TimeSignature.Height, barHeight - LineSpace);
				y += barHeight + BarTopDistance;
				g.DrawImage(Bass, 10, y + LineSpace / 2, (bassHeight * Bass.Width) / Bass.Height, bassHeight);
				g.DrawImage(TimeSignature, 40, y, ((barHeight - LineSpace) * TimeSignature.Width) / TimeSignature.Height, barHeight - LineSpace);
				y += barHeight + BarTopDistance + BarVerticalSpacing;
			}
			int x, bar = 0;
			foreach (MusicNote note in Notes) {
				note.DrawNote(g, new Point(note.Bounds.X, note.Bounds.Y - verticalScroll));
				bar += (int) note.Length;
				if (bar / (int) NoteLength.SemiBreve > 0) {
					bar = 0;
					x = note.Bounds.X + NoteWidth + NoteSpacing / 2;
					y = note.BottomBarY - verticalScroll;
					g.CompositingMode = CompositingMode.SourceCopy;
					g.CompositingQuality = CompositingQuality.HighSpeed;
					g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
					g.SmoothingMode = SmoothingMode.HighSpeed;
					g.DrawLine(ScorePen, x, y, x, y + LineSpace * 8 + BarTopDistance + LineSpace);
				}
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