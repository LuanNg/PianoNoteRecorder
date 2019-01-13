using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security;
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

		private enum KeyboardPianoKeys {
			None = 0,
			Q,
			W,
			E,
			R,
			T,
			Y,
			U,
			I,
			O,
			P,
			OemOpenBrackets,
			Oem6,
			A,
			S,
			D,
			F,
			G,
			H,
			J,
			K,
			L,
			Oem1,
			Oem7,
			Z,
			X,
			C,
			V,
			B,
			N,
			M,
			Oemcomma,
			OemPeriod,
			OemQuestion,
			ShiftKey
		}

		/// <summary>
		/// The thickness of the white key outline
		/// </summary>
		private const int lineThickness = 2;
		/// <summary>
		/// The number of white keys in the keyboard
		/// </summary>
		private const int WhiteKeyCount = 29; //7 per octave + 1
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
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque | ControlStyles.UserPaint, true);
			//DoubleBuffered = false;
			Text = "Top of piano can also be resized using mouse";
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
					Invalidate(false);
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
				if (!pressedNotes.Contains(currentNote)) {
					if (lastMouseNote != NoteEnum.None) {
						pressedNotes.Remove(lastMouseNote);
						MidiPlayer.PlayNote(lastMouseNote, NoteVolume.silent);
					}
					lastMouseNote = currentNote;
					if (currentNote != NoteEnum.None) {
						pressedNotes.Add(currentNote);
						MidiPlayer.PlayNote(currentNote);
					}
					Invalidate(false);
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
					lastMouseNote = NoteEnum.None;
				}
				Invalidate(false);
			}
		}

		public void MarkKeyPressed(KeyEventArgs key) {
			OnKeyDown(key);
		}

		private static NoteEnum GetNoteFromKey(Keys key) {
			KeyboardPianoKeys pianoKey;
			bool valid = Enum.TryParse(key.ToString(), out pianoKey);
			return valid ? ((NoteEnum) pianoKey + 12) : NoteEnum.None;
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			NoteEnum note = GetNoteFromKey(e.KeyCode);
			if (!(note == NoteEnum.None || pressedNotes.Contains(note))) {
				pressedNotes.Add(note);
				MidiPlayer.PlayNote(note);
				Invalidate(false);
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
				Invalidate(false);
			}
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
			return note.ToString().Contains("Sharp");
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
			//g.Clear(BackColor);
			//calculate metrics
			int whiteKeyWidth = WhiteKeyWidth;
			int blackKeyWidth = (whiteKeyWidth * 9) / 14;
			int halfBlackKeyWidth = blackKeyWidth / 2;
			int blackKeyOffset = whiteKeyWidth - halfBlackKeyWidth;
			int pianoWidth = PianoWidth;
			Size clientSize = ClientSize;
			int x = (clientSize.Width - pianoWidth) / 2;
			const int halfLineThickness = lineThickness / 2;
			int height = clientSize.Height - lineThickness;
			Rectangle clipRect = e.ClipRectangle;
			using (SolidBrush background = new SolidBrush(BackColor)) {
				FillRectangle(g, background, new Rectangle(0, 0, x, clientSize.Height), clipRect);
				FillRectangle(g, background, new Rectangle(x + pianoWidth, 0, clientSize.Width - (x + pianoWidth), clientSize.Height), clipRect);
			}
			//fill piano background
			FillRectangle(g, WhiteKeyBrush, new Rectangle(x, halfLineThickness, pianoWidth, height), clipRect);
			int i, keyStart;
			//draw white keys
			List<Rectangle> pressedWhite = new List<Rectangle>();
			List<Rectangle> pressedBlack = new List<Rectangle>();
			NoteEnum currentNote;
			for (i = 0; i < WhiteKeyCount; ++i) {
				keyStart = x + i * whiteKeyWidth;
				currentNote = WhiteNoteIndexToNote(i);
				if ((leftMouseDown && keyStart <= LastCursorLocation.X && keyStart + whiteKeyWidth > LastCursorLocation.X && !IsSharp(GetNoteAtPoint(LastCursorLocation))) ||
					pressedNotes.Contains(currentNote)) {
					pressedWhite.Add(new Rectangle(keyStart, halfLineThickness, whiteKeyWidth, height));
				}
				if (i == MiddleCPos)
					FillRectangle(g, MiddleCBrush, new Rectangle(keyStart, halfLineThickness, whiteKeyWidth, height), clipRect);
				DrawRectangle(g, KeyOutline, new Rectangle(keyStart, halfLineThickness, whiteKeyWidth, height), clipRect);
			}
			int temp;
			const int blackKeyIntervalCount = WhiteKeyCount - 1;
			int blackKeyHeight = (height * 4) / 7;
			//draw black keys
			for (i = 0; i < blackKeyIntervalCount; ++i) {
				temp = i % 7;
				if (!(temp == 2 || temp == 6)) { //skip every 2nd and 6th key
					keyStart = x + i * whiteKeyWidth + blackKeyOffset;
					if ((leftMouseDown && LastCursorLocation.Y < blackKeyHeight && keyStart <= LastCursorLocation.X && keyStart + blackKeyWidth > LastCursorLocation.X) ||
						pressedNotes.Contains(WhiteNoteIndexToNote(i) + 1))
						pressedBlack.Add(new Rectangle(keyStart, lineThickness, blackKeyWidth, blackKeyHeight));
					else
						FillRectangle(g, BlackKeyBrush, new Rectangle(keyStart, lineThickness, blackKeyWidth, blackKeyHeight), clipRect);
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