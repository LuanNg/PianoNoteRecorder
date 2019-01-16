using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// An interactive piano keyboard for note generation within a musical staff
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("An interactive piano keyboard for note generation within a musical staff")]
	[DisplayName(nameof(MusicKeyboard))]
	public class MusicKeyboard : Panel {
		/// <summary>
		/// Represents whether a note has adjacent black keys
		/// </summary>
		[Flags]
		private enum HasBlackKeys {
			/// <summary>
			/// The key has no adjacent black keys
			/// </summary>
			None = 0,
			/// <summary>
			/// Has flat
			/// </summary>
			Flat = 1,
			/// <summary>
			/// Has sharp
			/// </summary>
			Sharp = 2
		}

		/// <summary>
		/// Associates the notes with keyboard keys
		/// </summary>
		private static Dictionary<Keys, NoteEnum> NoteKeys = new Dictionary<Keys, NoteEnum> {
			{ Keys.D1, NoteEnum.C3 },
			{ Keys.D2, NoteEnum.CSharp3 },
			{ Keys.D3, NoteEnum.D3 },
			{ Keys.D4, NoteEnum.DSharp3 },
			{ Keys.D5, NoteEnum.E3 },
			{ Keys.D6, NoteEnum.F3 },
			{ Keys.D7, NoteEnum.FSharp3 },
			{ Keys.D8, NoteEnum.G3 },
			{ Keys.D9, NoteEnum.GSharp3 },
			{ Keys.D0, NoteEnum.A3 },
			{ Keys.OemMinus, NoteEnum.ASharp3 },
			{ Keys.Oemplus, NoteEnum.B3 },
			{ Keys.Q, NoteEnum.C4 },
			{ Keys.W, NoteEnum.CSharp4 },
			{ Keys.E, NoteEnum.D4 },
			{ Keys.R, NoteEnum.DSharp4 },
			{ Keys.T, NoteEnum.E4 },
			{ Keys.Y, NoteEnum.F4 },
			{ Keys.U, NoteEnum.FSharp4 },
			{ Keys.I, NoteEnum.G4 },
			{ Keys.O, NoteEnum.GSharp4 },
			{ Keys.P, NoteEnum.A4 },
			{ Keys.OemOpenBrackets, NoteEnum.ASharp4 },
			{ Keys.Oem6, NoteEnum.B4 },
			{ Keys.A, NoteEnum.C5 },
			{ Keys.S, NoteEnum.CSharp5 },
			{ Keys.D, NoteEnum.D5 },
			{ Keys.F, NoteEnum.DSharp5 },
			{ Keys.G, NoteEnum.E5 },
			{ Keys.H, NoteEnum.F5 },
			{ Keys.J, NoteEnum.FSharp5 },
			{ Keys.K, NoteEnum.G5 },
			{ Keys.L, NoteEnum.GSharp5 },
			{ Keys.Oem1, NoteEnum.A5 },
			{ Keys.Oem7, NoteEnum.ASharp5 },
			{ Keys.Z, NoteEnum.B5 },
			{ Keys.X, NoteEnum.C6 },
			{ Keys.C, NoteEnum.CSharp6 },
			{ Keys.V, NoteEnum.D6 },
			{ Keys.B, NoteEnum.DSharp6 },
			{ Keys.N, NoteEnum.E6 },
			{ Keys.M, NoteEnum.F6 },
			{ Keys.Oemcomma, NoteEnum.FSharp6 },
			{ Keys.OemPeriod, NoteEnum.G6 },
			{ Keys.OemQuestion, NoteEnum.GSharp6 },
			{ Keys.ShiftKey, NoteEnum.A6 }
		};
		/// <summary>
		/// The thickness of the white key outline
		/// </summary>
		private const int lineThickness = 2;
		/// <summary>
		/// The number of white keys in the keyboard
		/// </summary>
		private const int WhiteKeyCount = 29; //7 per octave + 2
		private const int MiddleCPos = 14; //7 per octave
		private static Pen KeyOutline = new Pen(Color.Black, lineThickness);
		private static Brush WhiteKeyBrush = Brushes.White;
		private static Brush BlackKeyBrush = Brushes.Black;
		private static Brush MiddleCBrush = Brushes.LightSkyBlue;
		private static Brush PressedKeyBrush = Brushes.Blue;
		private static Brush TextBrush = Brushes.Red;
		private static Pen TextOutline = Pens.White;
		private static StringFormat textFormat = new StringFormat(StringFormatFlags.NoClip) {
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		};
		/// <summary>
		/// The notes that are currently pressed on the piano keyboard
		/// </summary>
		private Dictionary<NoteEnum, MusicNote> pressedNotes = new Dictionary<NoteEnum, MusicNote>();
		/// <summary>
		/// Used for tracking rest lengths
		/// </summary>
		private Stopwatch SilenceStopwatch = new Stopwatch();
		/// <summary>
		/// The associated musical staff control
		/// </summary>
		internal MusicStaff Staff;
		/// <summary>
		/// The piano background
		/// </summary>
		private SolidBrush backgroundBrush;
		/// <summary>
		/// The note that is currently being played with the mouse
		/// </summary>
		private MusicNote currentMouseNote;
		/// <summary>
		/// The width percentage of the piano
		/// </summary>
		private int widthScale = 100;
		private Point lastCursorLocation;
		/// <summary>
		/// Whether the left mouse button is currently pressed
		/// </summary>
		private bool leftMouseDown;
		private bool showHint;
		/// <summary>
		/// The last note that was pressed with the mouse
		/// </summary>
		private NoteEnum lastMouseNote;

		/// <summary>
		/// Gets or sets whether to show the resize hint text
		/// </summary>
		[Browsable(true)]
		[Description("Gets or sets whether to show the resize hint text")]
		public bool ShowHint {
			get {
				return showHint;
			}
			set {
				if (value == showHint)
					return;
				showHint = value;
				Invalidate(false);
			}
		}

		/// <summary>
		/// Gets or sets the current piano width percentage
		/// </summary>
		[Browsable(true)]
		[Description("Gets or sets the piano width scale percentage")]
		public int WidthScalePercentage {
			get {
				return widthScale;
			}
			set {
				if (value < 1)
					value = 1;
				else if (value > 100)
					value = 100;
				if (value == widthScale)
					return;
				widthScale = value;
				Invalidate(false);
			}
		}

		/// <summary>
		/// Calculates the boundaries of the piano keyboard
		/// </summary>
		private Rectangle PianoBounds {
			get {
				int pianoWidth = PianoWidth;
				Size clientSize = ClientSize;
				return new Rectangle((clientSize.Width - pianoWidth) / 2, lineThickness / 2, pianoWidth, clientSize.Height - lineThickness);
			}
		}

		/// <summary>
		/// Calculates the width of the piano keyboard to be drawn
		/// </summary>
		private int PianoWidth {
			get {
				return WhiteKeyCount * WhiteKeyWidth;
			}
		}

		/// <summary>
		/// Calculates the width of a white piano key
		/// </summary>
		private int WhiteKeyWidth {
			get {
				return ((Width - lineThickness) * widthScale) / (100 * WhiteKeyCount);
			}
		}

		/// <summary>
		/// Calculates the width of a black piano key
		/// </summary>
		private int BlackKeyWidth {
			get {
				return (WhiteKeyWidth * 3) / 4;
			}
		}

		/// <summary>
		/// Initializes the musical keyboard
		/// </summary>
		public MusicKeyboard() {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.CacheText, true);
			Text = "Top of piano can also be resized using mouse";
		}

		/// <summary>
		/// Called when the backcolor of the keyboard has been changed
		/// </summary>
		protected override void OnBackColorChanged(EventArgs e) {
			base.OnBackColorChanged(e);
			if (backgroundBrush != null)
				backgroundBrush.Dispose();
			backgroundBrush = new SolidBrush(BackColor);
		}

		/// <summary>
		/// Called when a mouse button is pressed onto the piano keyboard
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				Stopwatch stopwatch = Stopwatch.StartNew();
				lastCursorLocation = e.Location;
				leftMouseDown = true;
				lastMouseNote = GetNoteAtPoint(e.Location);
				if (lastMouseNote != NoteEnum.None) {
					MidiPlayer.PlayNote(lastMouseNote);
					int silenceLength = (int) SilenceStopwatch.ElapsedMilliseconds;
					NoteLength restLength = Staff.ToNoteLength(silenceLength);
					if (restLength >= NoteLength.DemiSemiQuaver)
						Staff.AddNote(NoteEnum.None, restLength);
					else {
						MusicNote lastNote = Staff.LastNote;
						if (lastNote != null)
							lastNote.LengthInMilliseconds += silenceLength;
					}
					SilenceStopwatch.Restart();
					currentMouseNote = Staff.AddNote(lastMouseNote, NoteLength.HemiDemiSemiQuaver);
					if (!pressedNotes.ContainsKey(lastMouseNote))
						pressedNotes.Add(lastMouseNote, currentMouseNote);
					Staff.StartAdjustingNote(currentMouseNote, stopwatch);
					Invalidate(GetNoteArea(lastMouseNote), false);
				}
			}
		}

		/// <summary>
		/// Called when the mouse is moved on the piano keyboard
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (leftMouseDown) {
				Stopwatch stopwatch = Stopwatch.StartNew();
				lastCursorLocation = e.Location;
				NoteEnum currentNote = GetNoteAtPoint(e.Location);
				NoteEnum oldNote = lastMouseNote;
				if (!pressedNotes.ContainsKey(currentNote)) {
					lastMouseNote = currentNote;
					if (oldNote != NoteEnum.None)
						MidiPlayer.StopNote(oldNote);
					if (currentNote != NoteEnum.None)
						MidiPlayer.PlayNote(currentNote);
					if (oldNote != NoteEnum.None) {
						Staff.StopAdjustingNote(currentMouseNote);
						currentMouseNote = null;
						pressedNotes.Remove(oldNote);
						Invalidate(GetNoteArea(oldNote));
					}
					if (currentNote != NoteEnum.None) {
						currentMouseNote = Staff.AddNote(currentNote, NoteLength.HemiDemiSemiQuaver);
						pressedNotes.Add(currentNote, currentMouseNote);
						Staff.StartAdjustingNote(currentMouseNote, stopwatch);
						Invalidate(GetNoteArea(currentNote));
					}
				}
			}
		}

		/// <summary>
		/// Called when a mouse button is released on the piano keyboard
		/// </summary>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				leftMouseDown = false;
				if (lastMouseNote != NoteEnum.None) {
					MidiPlayer.StopNote(lastMouseNote);
					Staff.StopAdjustingNote(currentMouseNote);
					currentMouseNote = null;
					Invalidate(GetNoteArea(lastMouseNote), false);
					pressedNotes.Remove(lastMouseNote);
					lastMouseNote = NoteEnum.None;
				}
			}
			SilenceStopwatch.Restart();
		}

		/// <summary>
		/// Gets the note associated with the specified keyboard key
		/// </summary>
		/// <param name="key">The keyboard key that was pressed</param>
		private static NoteEnum GetNoteFromKey(Keys key) {
			NoteEnum note;
			if (NoteKeys.TryGetValue(key, out note))
				return note;
			else
				return NoteEnum.None;
		}

		/// <summary>
		/// Marks the specified keyboard key as pressed
		/// </summary>
		/// <param name="keyCode">The key that was pressed</param>
		public void MarkKeyPressed(Keys keyCode) {
			MarkKeyPressed(GetNoteFromKey(keyCode), true);
		}

		/// <summary>
		/// Marks the specified note as currently pressed
		/// </summary>
		/// <param name="note">The note to press on the piano keyboard</param>
		/// <param name="addToStaff">Whether to add the specified note to the staff</param>
		public void MarkKeyPressed(NoteEnum note, bool addToStaff) {
			if (!(note == NoteEnum.None || pressedNotes.ContainsKey(note))) {
				Stopwatch stopwatch = Stopwatch.StartNew();
				MidiPlayer.PlayNote(note);
				if (addToStaff) {
					int silenceLength = (int) SilenceStopwatch.ElapsedMilliseconds;
					NoteLength restLength = Staff.ToNoteLength(silenceLength);
					if (restLength >= NoteLength.DemiSemiQuaver)
						Staff.AddNote(NoteEnum.None, restLength);
					else {
						MusicNote lastNote = Staff.LastNote;
						if (lastNote != null)
							lastNote.LengthInMilliseconds += silenceLength;
					}
				}
				SilenceStopwatch.Restart();
				if (addToStaff) {
					MusicNote noteControl = Staff.AddNote(note, NoteLength.HemiDemiSemiQuaver);
					pressedNotes.Add(note, noteControl);
					Staff.StartAdjustingNote(noteControl, stopwatch);
				} else
					pressedNotes.Add(note, null);
				Invalidate(GetNoteArea(note), false);
			}
		}

		/// <summary>
		/// Marks the specified keyboard key as released
		/// </summary>
		/// <param name="keyCode">The keyboard key that was released</param>
		public void MarkKeyReleased(Keys keyCode) {
			MarkKeyReleased(GetNoteFromKey(keyCode));
		}

		/// <summary>
		/// Marks the specified note as released
		/// </summary>
		/// <param name="note">The note to release</param>
		public void MarkKeyReleased(NoteEnum note) {
			if (!(note == NoteEnum.None || note == lastMouseNote)) {
				MidiPlayer.StopNote(note);
				if (pressedNotes.ContainsKey(note)) {
					Staff.StopAdjustingNote(pressedNotes[note]);
					pressedNotes.Remove(note);
				}
				Invalidate(GetNoteArea(note), false);
			}
			SilenceStopwatch.Restart();
		}

		/// <summary>
		/// Gets the spceified note area
		/// </summary>
		/// <param name="note">The note whose area on the keybord to return</param>
		private Rectangle GetNoteArea(NoteEnum note) {
			Rectangle area = new Rectangle();
			if (note == NoteEnum.None)
				return area;
			Size clientSize = ClientSize;
			int height = clientSize.Height;
			int x = (clientSize.Width - PianoWidth) / 2;
			int whiteKeyWidth = WhiteKeyWidth;
			int blackKeyWidth = (whiteKeyWidth * 9) / 14;
			int halfBlackKeyWidth = blackKeyWidth / 2;
			int blackKeyOffset = whiteKeyWidth - halfBlackKeyWidth;
			int blackKeyHeight = (height * 4) / 7;
			int noteIndex = (int) note - 1;
			int tone = noteIndex % 12;
			int halfTone = tone / 2;
			int octave = noteIndex / 12;
			bool isSharp = (tone & 1) == 1;
			int index;
			if (tone >= 5)
				isSharp = !isSharp;
			index = octave * 7 + tone / 2;
			if (isSharp) {
				area.X = x + index * whiteKeyWidth + blackKeyOffset;
				area.Width = blackKeyWidth;
				area.Height = blackKeyHeight;
			} else {
				if (tone >= 5)
					index++;
				area.X = x + index * whiteKeyWidth;
				area.Width = whiteKeyWidth;
				area.Height = height;
			}
			return area;
		}

		/// <summary>
		/// Returns whether the specified note has adjacent black keys next to it
		/// </summary>
		/// <param name="note">The white note</param>
		private static HasBlackKeys CheckIfWhiteNoteHasBlackKeys(NoteEnum note) {
			HasBlackKeys flags = HasBlackKeys.None;
			if (note == NoteEnum.None || IsSharp(note))
				return flags;
			if (IsSharp(note - 1))
				flags |= HasBlackKeys.Flat;
			if (IsSharp(note + 1))
				flags |= HasBlackKeys.Sharp;
			return flags;
		}

		/// <summary>
		/// Returns whether the specified note is sharp (a black key)
		/// </summary>
		/// <param name="note">The note</param>
		public static bool IsSharp(NoteEnum note) {
			if (note == NoteEnum.None)
				return false;
			int tone = ((int) note - 1) % 12;
			bool isSharp = (tone & 1) == 1;
			if (tone >= 5)
				isSharp = !isSharp;
			return isSharp;
		}

		/// <summary>
		/// Calculates the specified nth white note
		/// </summary>
		/// <param name="noteIndex">The specified white note index</param>
		private static NoteEnum WhiteNoteIndexToNote(int noteIndex) {
			int index = noteIndex * 2 + 1;
			int toSubtract = noteIndex / 7;
			int portion = noteIndex % 7;
			index -= toSubtract * 2;
			if (portion >= 3)
				index--;
			return (NoteEnum) index;
		}

		/// <summary>
		/// Calculates the note at the specified position
		/// </summary>
		/// <param name="point">The point on the keyboard canvas</param>
		private NoteEnum GetNoteAtPoint(Point point) {
			int whiteKeyWidth = WhiteKeyWidth;
			Size clientSize = ClientSize;
			int x = (clientSize.Width - PianoWidth) / 2;
			if (point.X < x || point.X >= x + PianoWidth)
				return NoteEnum.None;
			int xLoc = point.X - x;
			NoteEnum whiteNote = WhiteNoteIndexToNote(xLoc / whiteKeyWidth);
			int height = clientSize.Height - lineThickness;
			int blackKeyHeight = (height * 4) / 7;
			if (point.Y < blackKeyHeight) {
				int blackKeyWidth = (whiteKeyWidth * 9) / 14;
				int halfBlackKeyWidth = blackKeyWidth / 2;
				int blackKeyOffset = whiteKeyWidth - halfBlackKeyWidth;
				HasBlackKeys hasBlackKeys = CheckIfWhiteNoteHasBlackKeys(whiteNote);
				int locInNote = xLoc % whiteKeyWidth;
				if (locInNote <= halfBlackKeyWidth && (hasBlackKeys & HasBlackKeys.Flat) == HasBlackKeys.Flat)
					return whiteNote - 1;
				else if (locInNote >= blackKeyOffset && (hasBlackKeys & HasBlackKeys.Sharp) == HasBlackKeys.Sharp)
					return whiteNote + 1;
				else
					return whiteNote;
			} else
				return whiteNote;
		}

		private static void FillRectangle(Graphics g, Brush brush, Rectangle area, Rectangle invalidatedRect) {
			area.Intersect(invalidatedRect);
			if (area.Width > 0 && area.Height > 0)
				g.FillRectangle(brush, area);
		}

		/// <summary>
		/// Called when the background is being drawn
		/// </summary>
		protected override void OnPaintBackground(PaintEventArgs e) {
		}

		/// <summary>
		/// Draws the interactive keyboard
		/// </summary>
		protected override void OnPaint(PaintEventArgs e) {
			Graphics g = e.Graphics;
			g.CompositingMode = CompositingMode.SourceCopy;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			g.SmoothingMode = SmoothingMode.HighSpeed;
			//calculate metrics
			int whiteKeyWidth = WhiteKeyWidth;
			int blackKeyWidth = (whiteKeyWidth * 9) / 14;
			int halfBlackKeyWidth = blackKeyWidth / 2;
			int blackKeyOffset = whiteKeyWidth - halfBlackKeyWidth;
			int pianoWidth = PianoWidth;
			Size clientSize = ClientSize;
			int x = (clientSize.Width - pianoWidth) / 2;
			int pianoEnd = x + pianoWidth;
			const int halfLineThickness = lineThickness / 2;
			int height = clientSize.Height - lineThickness;
			Rectangle clipRect = e.ClipRectangle;
			//fill piano background
			FillRectangle(g, WhiteKeyBrush, new Rectangle(x, halfLineThickness, pianoWidth, height), clipRect);
			int i, keyStart;
			//draw white keys
			List<Rectangle> pressedWhite = new List<Rectangle>();
			Rectangle currentRect;
			NoteEnum currentNote;
			for (i = 0; i < WhiteKeyCount; ++i) {
				keyStart = x + i * whiteKeyWidth;
				currentNote = WhiteNoteIndexToNote(i);
				currentRect = new Rectangle(keyStart, halfLineThickness, whiteKeyWidth, height);
				if ((leftMouseDown && keyStart <= lastCursorLocation.X && keyStart + whiteKeyWidth > lastCursorLocation.X && !IsSharp(GetNoteAtPoint(lastCursorLocation))) ||
					pressedNotes.ContainsKey(currentNote)) {
					pressedWhite.Add(currentRect);
				}
				if (i == MiddleCPos)
					FillRectangle(g, MiddleCBrush, currentRect, clipRect);
				g.DrawRectangle(KeyOutline, currentRect);
			}
			List<Rectangle> pressedBlack = new List<Rectangle>();
			int temp;
			int blackKeyHeight = (height * 4) / 7;
			//draw black keys
			for (i = 0; i < WhiteKeyCount; ++i) {
				temp = i % 7;
				if (!(temp == 2 || temp == 6)) { //skip every 2nd and 6th key
					keyStart = x + i * whiteKeyWidth + blackKeyOffset;
					currentRect = new Rectangle(keyStart, lineThickness, blackKeyWidth, blackKeyHeight);
					if ((leftMouseDown && lastCursorLocation.Y < blackKeyHeight && keyStart <= lastCursorLocation.X && Math.Min(keyStart + blackKeyWidth, pianoEnd) > lastCursorLocation.X) ||
						pressedNotes.ContainsKey(WhiteNoteIndexToNote(i) + 1))
						pressedBlack.Add(currentRect);
					else
						FillRectangle(g, BlackKeyBrush, currentRect, clipRect);
				}
			}
			//draw pressed white keys
			Rectangle rect;
			for (i = 0; i < pressedWhite.Count; i++) {
				rect = pressedWhite[i];
				FillRectangle(g, PressedKeyBrush, new Rectangle(rect.X + halfLineThickness, rect.Y + halfLineThickness, rect.Width - lineThickness, rect.Height - lineThickness), clipRect);
				NoteEnum note = WhiteNoteIndexToNote((rect.X - x) / whiteKeyWidth);
				HasBlackKeys hasBlackKeys = CheckIfWhiteNoteHasBlackKeys(note);
				if ((hasBlackKeys & HasBlackKeys.Flat) == HasBlackKeys.Flat && !pressedNotes.ContainsKey((NoteEnum) ((int) note - 1)))
					FillRectangle(g, BlackKeyBrush, new Rectangle(rect.X - halfBlackKeyWidth, lineThickness, blackKeyWidth, blackKeyHeight), clipRect);
				if ((hasBlackKeys & HasBlackKeys.Sharp) == HasBlackKeys.Sharp && !pressedNotes.ContainsKey((NoteEnum) ((int) note + 1)))
					FillRectangle(g, BlackKeyBrush, new Rectangle(rect.X + blackKeyOffset, lineThickness, blackKeyWidth, blackKeyHeight), clipRect);
			}
			//draw pressed black keys
			for (i = 0; i < pressedBlack.Count; i++) {
				rect = pressedBlack[i];
				FillRectangle(g, PressedKeyBrush, rect, clipRect);
				rect.X += halfLineThickness;
				rect.Y -= halfLineThickness;
				rect.Width -= lineThickness;
				g.DrawRectangle(KeyOutline, rect);
			}
			if (backgroundBrush == null)
				backgroundBrush = new SolidBrush(BackColor);
			//draw outside piano background
			FillRectangle(g, backgroundBrush, new Rectangle(0, 0, x - 1, clientSize.Height), clipRect);
			FillRectangle(g, backgroundBrush, new Rectangle(x + pianoWidth + halfLineThickness, 0, clientSize.Width - (x + pianoWidth + halfLineThickness), clientSize.Height), clipRect);
			if (showHint) {
				//draw render text
				e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				e.Graphics.CompositingMode = CompositingMode.SourceOver;
				e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
				e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
				using (GraphicsPath path = new GraphicsPath()) {
					Rectangle bounds = Bounds;
					bounds.X -= (Parent.Controls[0] == this ? Parent.Controls[1] : Parent.Controls[0]).Width;
					bounds.Y = blackKeyHeight;
					bounds.Height -= blackKeyHeight;
					path.AddString(Text, Font.FontFamily, (int) Font.Style, Font.Size, bounds, textFormat);
					e.Graphics.FillPath(TextBrush, path);
					e.Graphics.DrawPath(TextOutline, path);
				}
			}
			base.OnPaint(e);
		}

		/// <summary>
		/// Disposes of the resources used by the music keyboard control
		/// </summary>
		/// <param name="disposing">Whether to dispose managed resources</param>
		protected override void Dispose(bool disposing) {
			if (backgroundBrush != null) {
				backgroundBrush.Dispose();
				backgroundBrush = null;
			}
			base.Dispose(disposing);
		}
	}
}