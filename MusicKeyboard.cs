using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		[Flags]
		private enum HasBlackKeys {
			None,
			/// <summary>
			/// Has flat
			/// </summary>
			Flat,
			/// <summary>
			/// Has sharp
			/// </summary>
			Sharp
		}

		private static Dictionary<Keys, NoteEnum> NoteKeys = new Dictionary<Keys, NoteEnum> {
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
										   /// <summary>
										   /// The key outline
										   /// </summary>
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
		private int widthScale = 100;
		private Point LastCursorLocation;
		private bool leftMouseDown, showHint;
		private NoteEnum lastMouseNote;
		private HashSet<NoteEnum> pressedNotes = new HashSet<NoteEnum>();

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
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.UserPaint, true);
			SetDoubleBuffered(false);
			Text = "Top of piano can also be resized using mouse";
		}

		public void SetDoubleBuffered(bool doubleBuffered) {
			SetStyle(ControlStyles.OptimizedDoubleBuffer, doubleBuffered);
		}

		/// <summary>
		/// Called when a mouse button is pressed onto the piano keyboard
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				LastCursorLocation = e.Location;
				leftMouseDown = true;
				lastMouseNote = GetNoteAtPoint(e.Location);
				if (lastMouseNote != NoteEnum.None) {
					pressedNotes.Add(lastMouseNote);
					MidiPlayer.PlayNote(lastMouseNote);
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
				LastCursorLocation = e.Location;
				NoteEnum currentNote = GetNoteAtPoint(e.Location);
				NoteEnum oldNote = lastMouseNote;
				if (!pressedNotes.Contains(currentNote)) {
					if (oldNote != NoteEnum.None) {
						pressedNotes.Remove(oldNote);
						MidiPlayer.PlayNote(oldNote, NoteVolume.silent);
					}
					lastMouseNote = currentNote;
					if (currentNote != NoteEnum.None) {
						pressedNotes.Add(currentNote);
						MidiPlayer.PlayNote(currentNote);
					}
					if (currentNote != NoteEnum.None)
						Invalidate(GetNoteArea(currentNote), false);
					if (oldNote != NoteEnum.None)
						Invalidate(GetNoteArea(oldNote), false);
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
					pressedNotes.Remove(lastMouseNote);
					MidiPlayer.PlayNote(lastMouseNote, NoteVolume.silent);
					Invalidate(GetNoteArea(lastMouseNote), false);
					lastMouseNote = NoteEnum.None;
				}
			}
		}

		public void MarkKeyPressed(KeyEventArgs key) {
			OnKeyDown(key);
		}

		private static NoteEnum GetNoteFromKey(Keys key) {
			NoteEnum note;
			if (NoteKeys.TryGetValue(key, out note))
				return note;
			else
				return NoteEnum.None;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			NoteEnum note = GetNoteFromKey(e.KeyCode);
			if (!(note == NoteEnum.None || pressedNotes.Contains(note))) {
				pressedNotes.Add(note);
				MidiPlayer.PlayNote(note);
				Invalidate(GetNoteArea(note), false);
			}
		}

		public void MarkKeyReleased(KeyEventArgs key) {
			OnKeyUp(key);
		}

		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			NoteEnum note = GetNoteFromKey(e.KeyCode);
			if (!(note == NoteEnum.None || note == lastMouseNote) && pressedNotes.Contains(note)) {
				pressedNotes.Remove(note);
				MidiPlayer.PlayNote(note, NoteVolume.silent);
				Invalidate(GetNoteArea(note), false);
			}
		}

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
			if (isSharp) {
				index = octave * 7 + tone / 2;
				area.X = x + index * whiteKeyWidth + blackKeyOffset;
				area.Width = blackKeyWidth;
				area.Height = blackKeyHeight;
			} else {
				index = octave * 7 + tone / 2;
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

		private static bool IsSharp(NoteEnum note) {
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

		private static void DrawRectangle(Graphics g, Pen pen, Rectangle area, Rectangle invalidatedRect) {
			area.Intersect(invalidatedRect);
			if (area.Width > 0 && area.Height > 0)
				g.DrawRectangle(pen, area);
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
			List<Rectangle> pressedBlack = new List<Rectangle>();
			Rectangle currentRect;
			NoteEnum currentNote;
			for (i = 0; i < WhiteKeyCount; ++i) {
				keyStart = x + i * whiteKeyWidth;
				currentNote = WhiteNoteIndexToNote(i);
				currentRect = new Rectangle(keyStart, halfLineThickness, whiteKeyWidth, height);
				if ((leftMouseDown && keyStart <= LastCursorLocation.X && keyStart + whiteKeyWidth > LastCursorLocation.X && !IsSharp(GetNoteAtPoint(LastCursorLocation))) ||
					pressedNotes.Contains(currentNote)) {
					pressedWhite.Add(currentRect);
				}
				if (i == MiddleCPos)
					FillRectangle(g, MiddleCBrush, currentRect, clipRect);
				DrawRectangle(g, KeyOutline, currentRect, clipRect);
			}
			int temp;
			int blackKeyHeight = (height * 4) / 7;
			//draw black keys
			for (i = 0; i < WhiteKeyCount; ++i) {
				temp = i % 7;
				if (!(temp == 2 || temp == 6)) { //skip every 2nd and 6th key
					keyStart = x + i * whiteKeyWidth + blackKeyOffset;
					currentRect = new Rectangle(keyStart, lineThickness, blackKeyWidth, blackKeyHeight);
					if ((leftMouseDown && LastCursorLocation.Y < blackKeyHeight && keyStart <= LastCursorLocation.X && Math.Min(keyStart + blackKeyWidth, pianoEnd) > LastCursorLocation.X) ||
						pressedNotes.Contains(WhiteNoteIndexToNote(i) + 1))
						pressedBlack.Add(currentRect);
					else
						FillRectangle(g, BlackKeyBrush, currentRect, clipRect);
				}
			}
			Rectangle rect;
			for (i = 0; i < pressedWhite.Count; i++) {
				rect = pressedWhite[i];
				FillRectangle(g, PressedKeyBrush, new Rectangle(rect.X + halfLineThickness, rect.Y + halfLineThickness, rect.Width - lineThickness, rect.Height - lineThickness), clipRect);
				NoteEnum note = WhiteNoteIndexToNote((rect.X - x) / whiteKeyWidth);
				HasBlackKeys hasBlackKeys = CheckIfWhiteNoteHasBlackKeys(note);
				if ((hasBlackKeys & HasBlackKeys.Flat) == HasBlackKeys.Flat)
					FillRectangle(g, BlackKeyBrush, new Rectangle(rect.X - halfBlackKeyWidth, lineThickness, blackKeyWidth, blackKeyHeight), clipRect);
				if ((hasBlackKeys & HasBlackKeys.Sharp) == HasBlackKeys.Sharp)
					FillRectangle(g, BlackKeyBrush, new Rectangle(rect.X + blackKeyOffset, lineThickness, blackKeyWidth, blackKeyHeight), clipRect);
			}
			for (i = 0; i < pressedBlack.Count; i++) {
				rect = pressedBlack[i];
				FillRectangle(g, PressedKeyBrush, rect, clipRect);
				rect.X += halfLineThickness;
				rect.Y -= halfLineThickness;
				rect.Width -= lineThickness;
				DrawRectangle(g, KeyOutline, rect, clipRect);
			}
			using (SolidBrush background = new SolidBrush(BackColor)) {
				FillRectangle(g, background, new Rectangle(0, 0, x - 1, clientSize.Height), clipRect);
				FillRectangle(g, background, new Rectangle(x + pianoWidth + halfLineThickness, 0, clientSize.Width - (x + pianoWidth + halfLineThickness), clientSize.Height), clipRect);
			}
			if (showHint) {
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
	}
}