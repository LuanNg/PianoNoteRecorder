using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// A musical staff that contains musical notes
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("A musical staff that contains musical notes")]
	[DisplayName(nameof(MusicStaff))]
	public class MusicStaff : Panel {
		/// <summary>
		/// An array containing all standard note lengths
		/// </summary>
		private static NoteLength[] NoteLengths = (NoteLength[]) Enum.GetValues(typeof(NoteLength));
		private static Pen ScorePen = Pens.Black;
		/// <summary>
		/// Fired when the notes are being played sequentially
		/// </summary>
		public event Action StartedPlaying;
		/// <summary>
		/// Fired when sequential playback has stopped
		/// </summary>
		public event Action StoppedPlaying;
		/// <summary>
		/// The associated piano keyboard
		/// </summary>
		public MusicKeyboard Keyboard;
		/// <summary>
		/// The length in milliseconds for a half-hemidemisemiquaver
		/// </summary>
		internal float MillisPerHalfHemiDemiSemiQuaver = DefaultBeatLengthInMs / 32f;
		/// <summary>
		/// The default beat length
		/// </summary>
		public const int DefaultBeatLengthInMs = 450;
		/// <summary>
		/// The vertical distance between the lines in a bar
		/// </summary>
		public const int LineSpace = 10;
		/// <summary>
		/// The vertical distance between every bar
		/// </summary>
		public const int BarTopDistance = 20;
		/// <summary>
		/// The vertical distance between every pair of bars (treble and bass clef)
		/// </summary>
		public const int BarVerticalSpacing = LineSpace * 2;
		/// <summary>
		/// The width of note controls
		/// </summary>
		public const int NoteWidth = 36;
		/// <summary>
		/// The left margin of the note
		/// </summary>
		public const int NoteStartLeftMargin = 65;
		/// <summary>
		/// The minimum interval for the timer
		/// </summary>
		private const int MinimumTimerInterval = 1;
		/// <summary>
		/// The noten to render on the staff
		/// </summary>
		private List<MusicNote> Notes = new List<MusicNote>();
		/// <summary>
		/// The playback and note adjustment timer
		/// </summary>
		private Timer Timer;
		/// <summary>
		/// The current note that is taking mouse input
		/// </summary>
		private MusicNote inputNote;
		/// <summary>
		/// The notes whose lengths are being updated
		/// </summary>
		private Dictionary<MusicNote, Stopwatch> noteLengthCandidates = new Dictionary<MusicNote, Stopwatch>();
		private Point currentNoteLoc = new Point(NoteStartLeftMargin, BarTopDistance);
		private Point nextNoteLoc = new Point(NoteStartLeftMargin, BarTopDistance);
		private Stopwatch currentNoteLength = new Stopwatch();
		private int lastWidth, playerNoteIndex;
		private bool isPlaying;

		/// <summary>
		/// Gets the last note
		/// </summary>
		public MusicNote LastNote {
			get {
				return Notes.Count == 0 ? null : Notes[Notes.Count - 1];
			}
		}

		/// <summary>
		/// Gets the current note count
		/// </summary>
		[Browsable(false)]
		public int NoteCount {
			get {
				return Notes.Count;
			}
		}


		/// <summary>
		/// Gets whether sequential note playback is currently running
		/// </summary>
		[Browsable(false)]
		public bool IsPlaying {
			get {
				return isPlaying;
			}
		}

		/// <summary>
		/// Gets or sets the global length in milliseconds for a crotchet (single beat)
		/// </summary>
		public float MillisPerBeat {
			get {
				return MillisPerHalfHemiDemiSemiQuaver * 32;
			}
			set {
				MillisPerHalfHemiDemiSemiQuaver = value * 0.03125f;
			}
		}

		/// <summary>
		/// Initializes a new music staff
		/// </summary>
		public MusicStaff() {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, false);
			BackColor = Color.White;
			Timer = new Timer();
			Timer.Tick += Timer_Tick;
			Timer.Interval = MinimumTimerInterval;
			HScroll = false;
			VScroll = true;
			lastWidth = Width;
			AutoScrollMinSize = new Size(1, Height);
			VerticalScroll.Value = VerticalScroll.Minimum;
			PerformLayout();
		}

		/// <summary>
		/// Called when the staff is shown or hidden
		/// </summary>
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
			if (Visible) {
				VerticalScroll.Value = VerticalScroll.Minimum;
				PerformLayout();
			}
		}

		/// <summary>
		/// Called when the panel size changed
		/// </summary>
		protected override void OnClientSizeChanged(EventArgs e) {
			base.OnClientSizeChanged(e);
			if (ClientSize.Width != lastWidth) { //reposition notes
				lastWidth = ClientSize.Width;
				nextNoteLoc = new Point(NoteStartLeftMargin, BarTopDistance);
				currentNoteLoc = nextNoteLoc;
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

		/// <summary>
		/// Updates the scrollbar values
		/// </summary>
		/// <param name="scrollToBottom">Whether to scroll to the bottom</param>
		private void UpdateScroll(bool scrollToBottom) {
			int oldMaximum = Math.Max(Height, currentNoteLoc.Y + (BarTopDistance * 2 + LineSpace * 10 + BarVerticalSpacing) * 2);
			if (AutoScrollMinSize.Height != oldMaximum) {
				AutoScrollMinSize = new Size(1, oldMaximum);
				if (scrollToBottom)
					VerticalScroll.Value = VerticalScroll.Maximum;
				PerformLayout();
				VScroll = true;
			}
		}

		/// <summary>
		/// Updates the next note position
		/// </summary>
		/// <param name="lastNoteWidth">The note width</param>
		/// <param name="updateScroll">Whether to update the scroll bar</param>
		/// <param name="scrollToBottom">Whether to scroll to the bottom if the scroll bar is updated</param>
		private void UpdateNextLoc(int lastNoteWidth, bool updateScroll, bool scrollToBottom) {
			currentNoteLoc = nextNoteLoc;
			nextNoteLoc.X += lastNoteWidth + LineSpace;
			if (nextNoteLoc.X > ClientSize.Width - NoteWidth) {
				nextNoteLoc = new Point(NoteStartLeftMargin, nextNoteLoc.Y + BarVerticalSpacing + (BarTopDistance + LineSpace * 5) * 2);
				if (updateScroll)
					UpdateScroll(scrollToBottom);
			}
		}

		/// <summary>
		/// Called when the timer has elapsed
		/// </summary>
		private void Timer_Tick(object sender, EventArgs e) {
			if (noteLengthCandidates.Count != 0) { //if has notes whose lengths are being adjusted live
				foreach (KeyValuePair<MusicNote, Stopwatch> pair in noteLengthCandidates)
					pair.Key.Length = ToNoteLength(pair.Value.ElapsedMilliseconds);
			}
			if (isPlaying) { //if sequential note playback is running
				if (playerNoteIndex < Notes.Count) {
					MusicNote currentNote = Notes[playerNoteIndex];
					if (currentNoteLength.ElapsedMilliseconds >= currentNote.LengthInMilliseconds) { //if current note is finished
						if (playerNoteIndex < Notes.Count) { //stop old note
							Keyboard.MarkKeyReleased(currentNote.Pitch);
							currentNote.Highlighted = false;
						}
						playerNoteIndex++;
						if (playerNoteIndex < Notes.Count) {
							//play new note
							currentNote = Notes[playerNoteIndex];
							Timer.Interval = Math.Max((int) currentNote.LengthInMilliseconds, MinimumTimerInterval);
							Keyboard.MarkKeyPressed(currentNote.Pitch, false);
							currentNoteLength.Restart();
							currentNote.Highlighted = true;
						} else
							StopPlayingNotes(); //arrived to the end of the notes
					}
				} else
					StopPlayingNotes();
			}
			if (!isPlaying && noteLengthCandidates.Count == 0)
				Timer.Enabled = false; //nothing left to do
		}

		/// <summary>
		/// Starts the timer for note length adjustment
		/// </summary>
		/// <param name="note">The note whose length to start adjusting</param>
		/// <param name="stopwatch">The stopwatch to measure with</param>
		public void StartAdjustingNote(MusicNote note, Stopwatch stopwatch) {
			if (note == null)
				return;
			else if (!noteLengthCandidates.ContainsKey(note))
				noteLengthCandidates.Add(note, stopwatch);
			Timer.Interval = MinimumTimerInterval;
			Timer.Enabled = true;
		}

		/// <summary>
		/// Stops the length adjustment of the specified note
		/// </summary>
		/// <param name="note">The note whose length to stop adjusting</param>
		public void StopAdjustingNote(MusicNote note) {
			if (note != null)
				noteLengthCandidates.Remove(note);
		}

		/// <summary>
		/// Starts sequential playback of the notes
		/// </summary>
		public void StartPlayingNotes() {
			if (isPlaying)
				return;
			isPlaying = true;
			currentNoteLength.Restart();
			Timer.Interval = MinimumTimerInterval;
			Timer.Enabled = true;
			if (StartedPlaying != null)
				StartedPlaying();
		}

		/// <summary>
		/// Pauses sequential note playback
		/// </summary>
		/// <param name="leaveHighlighted">Whether to leave the current note highlighted</param>
		public void PausePlayingNotes(bool leaveHighlighted = false) {
			if (!isPlaying)
				return;
			isPlaying = false;
			if (playerNoteIndex >= 0 && playerNoteIndex < Notes.Count) {
				MusicNote current = Notes[playerNoteIndex];
				Keyboard.MarkKeyReleased(current.Pitch);
				current.Highlighted = leaveHighlighted;
			}
			currentNoteLength.Reset();
			if (StoppedPlaying != null)
				StoppedPlaying();
		}

		/// <summary>
		/// Stops sequential playback of the notes
		/// </summary>
		public void StopPlayingNotes() {
			PausePlayingNotes();
			playerNoteIndex = 0;
			if (StoppedPlaying != null)
				StoppedPlaying();
		}

		/// <summary>
		/// Removes all notes from the staff
		/// </summary>
		public void ClearAllNotes() {
			StopPlayingNotes();
			Notes.Clear();
			nextNoteLoc = new Point(NoteStartLeftMargin, BarTopDistance);
			VerticalScroll.Value = 0;
			PerformLayout();
			Invalidate(false);
		}

		/// <summary>
		/// Loads the notes from the specified file
		/// </summary>
		/// <param name="path">The path whose notes to load</param>
		public void LoadAllNotes(string path) {
			ClearAllNotes();
			using (StreamReader reader = new StreamReader(path)) {
				string line;
				string[] sections;
				NoteLength currentLength;
				while (!reader.EndOfStream) {
					line = reader.ReadLine();
					sections = line.Split(',');
					currentLength = (NoteLength) int.Parse(sections[1]);
					if (currentLength != NoteLength.None)
						AddNote((NoteEnum) int.Parse(sections[0]), currentLength);
				}
			}
		}

		/// <summary>
		/// Saves all the notes in the staff into the specified file
		/// </summary>
		/// <param name="path">The path of the file to save notes into</param>
		public void SaveAllNotes(string path) {
			using (StreamWriter writer = new StreamWriter(path)) {
				foreach (MusicNote note in Notes)
					writer.WriteLine((int) note.Pitch + "," + (int) note.Length);
			}
		}

		/// <summary>
		/// Adds the specified note at the end of the staff
		/// </summary>
		/// <param name="pitch">The pitch of the note</param>
		/// <param name="length">The length of the note</param>
		public MusicNote AddNote(NoteEnum pitch, NoteLength length) {
			return AddNote(pitch, (int) length * MillisPerHalfHemiDemiSemiQuaver);
		}

		/// <summary>
		/// Adds the specified note at the end of the staff
		/// </summary>
		/// <param name="pitch">The pitch of the note</param>
		/// <param name="lengthInMillisecs">The length of the note in milliseconds</param>
		public MusicNote AddNote(NoteEnum pitch, float lengthInMillisecs) {
			MusicNote note = new MusicNote(this, Keyboard, pitch, lengthInMillisecs, nextNoteLoc);
			Notes.Add(note);
			UpdateNextLoc(note.Bounds.Width, true, true);
			Invalidate(note.Bounds, false);
			return note;
		}

		/// <summary>
		/// Gets the nearest note length from the specified length in milliseconds
		/// </summary>
		/// <param name="ms">The length of the note in milliseconds</param>
		public NoteLength ToNoteLength(float ms) {
			int length = (int) (ms / MillisPerHalfHemiDemiSemiQuaver);
			for (int i = NoteLengths.Length - 1; i >= 0; i--) {
				if (length >= (int) NoteLengths[i])
					return NoteLengths[i];
			}
			return NoteLength.None;
		}

		/// <summary>
		/// Called when a mouse button is pressed
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			MusicNote note;
			int x = e.X, y = e.Y + VerticalScroll.Value;
			for (int i = 0; i < Notes.Count; ++i) {
				note = Notes[i];
				if (note.Bounds.Contains(x, y)) {
					if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
						Notes.RemoveAt(i); //if ctrl is pressed remove the note
						Invalidate(false);
					} else { //else pass the event to the note
						inputNote = note;
						note.MarkMouseDown(new MouseEventArgs(e.Button, e.Clicks, x - note.Bounds.X, y - note.Bounds.Y, e.Delta));
					}
					break;
				}
			}
		}

		/// <summary>
		/// Called when the mouse is moved
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (inputNote != null)
				inputNote.MarkMouseMove();
		}

		/// <summary>
		/// Called when a mouse button is released
		/// </summary>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (inputNote != null)
				inputNote.MarkMouseUp(new MouseEventArgs(e.Button, e.Clicks, e.X - inputNote.Bounds.X, e.Y + VerticalScroll.Value - inputNote.Bounds.Y, e.Delta));
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
			//draw the bar lines
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
			//draws the clefs and time signatures
			Size trebleSize = new Size((barHeight * Images.Treble.Width) / Images.Treble.Height, barHeight);
			Size bassSize = new Size((bassHeight * Images.Bass.Width) / Images.Bass.Height, bassHeight);
			Size timeSignatureSize = new Size(((barHeight - LineSpace) * Images.TimeSignature.Width) / Images.TimeSignature.Height, barHeight - LineSpace);
			while (y <= max) {
				g.DrawImage(Images.Treble, 10, y, trebleSize.Width, trebleSize.Height);
				g.DrawImage(Images.TimeSignature, 40, y, timeSignatureSize.Width, timeSignatureSize.Height);
				y += barHeight + BarTopDistance;
				g.DrawImage(Images.Bass, 10, y + LineSpace / 2, bassSize.Width, bassSize.Height);
				g.DrawImage(Images.TimeSignature, 40, y, timeSignatureSize.Width, timeSignatureSize.Height);
				y += barHeight + BarTopDistance + BarVerticalSpacing;
			}
			int x, bar = 0;
			//draw all notes
			foreach (MusicNote note in Notes) {
				note.DrawNote(g, new Point(note.Bounds.X, note.Bounds.Y - verticalScroll));
				bar += (int) note.Length;
				if (bar / (int) NoteLength.SemiBreve > 0) {
					bar = 0;
					x = note.Bounds.X + NoteWidth + LineSpace / 2;
					y = note.BottomBarY - verticalScroll;
					g.CompositingMode = CompositingMode.SourceCopy;
					g.CompositingQuality = CompositingQuality.HighSpeed;
					g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
					g.SmoothingMode = SmoothingMode.HighSpeed;
					g.DrawLine(ScorePen, x, y, x, y + LineSpace * 9 + BarTopDistance);
				}
			}
		}

		/// <summary>
		/// Called when the scrollbar value has changed
		/// </summary>
		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			Invalidate(false);
		}

		/// <summary>
		/// Disposes of the resources used by the music staff control
		/// </summary>
		/// <param name="disposing">Whether to dispose managed resources</param>
		protected override void Dispose(bool disposing) {
			Timer.Dispose();
			base.Dispose(disposing);
		}
	}
}