using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace NezvalPiano {
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

		/// <summary>
		/// The thickness of the white key outline
		/// </summary>
		private const int lineThickness = 2;
		/// <summary>
		/// The number of white keys in the keyboard
		/// </summary>
		private const int WhiteKeyCount = 22;
		/// <summary>
		/// The key outline
		/// </summary>
		private static Pen KeyOutline = new Pen(Color.Black, lineThickness);
		private static StringFormat textFormat = new StringFormat(StringFormatFlags.NoClip) {
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Center
		};
		private int widthScale = 100;
		private Point LastCursorLocation;
		private bool leftMouseDown, showHint;
		private NoteEnum lastNote;

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

		private Rectangle PianoBounds {
			get {
				int pianoWidth = PianoWidth;
				Size clientSize = ClientSize;
				return new Rectangle((clientSize.Width - pianoWidth) / 2, lineThickness / 2, pianoWidth, clientSize.Height - lineThickness);
			}
		}

		/// <summary>
		/// Calculates the width of the piano to be drawn
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
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			Text = "Top of piano can also be resized using mouse";
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				LastCursorLocation = e.Location;
				leftMouseDown = true;
				lastNote = CalculateNoteFromPoint(e.Location);
				if (lastNote != NoteEnum.None)
					MidiPlayer.PlayNote(lastNote);
				Rectangle bounds = PianoBounds;
				if (bounds.Contains(e.Location))
					Invalidate(false);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (leftMouseDown) {
				LastCursorLocation = e.Location;
				NoteEnum currentNote = CalculateNoteFromPoint(e.Location);
				if (currentNote != lastNote) {
					if (lastNote != NoteEnum.None)
						MidiPlayer.PlayNote(lastNote, NoteVolume.silent);
					lastNote = currentNote;
					if (currentNote != NoteEnum.None)
						MidiPlayer.PlayNote(currentNote);
				}
				Invalidate(false);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				leftMouseDown = false;
				if (lastNote != NoteEnum.None)
					MidiPlayer.PlayNote(lastNote, NoteVolume.silent);
				Invalidate(false);
			}
		}

		private static HasBlackKeys CheckIfWhiteNoteHasBlackKeys(NoteEnum note) {
			HasBlackKeys flags = HasBlackKeys.None;
			if (note == NoteEnum.None || note.ToString().Contains("Sharp"))
				return flags;
			if ((note - 1).ToString().Contains("Sharp"))
				flags |= HasBlackKeys.Flat;
			if ((note + 1).ToString().Contains("Sharp"))
				flags |= HasBlackKeys.Sharp;
			return flags;
		}

		private static NoteEnum WhiteNoteIndexToNote(int noteIndex) {
			int index = noteIndex * 2 + 1;
			int toSubtract = noteIndex / 7;
			int portion = noteIndex % 7;
			index -= toSubtract * 2;
			if (portion >= 3)
				index--;
			return (NoteEnum) index;
		}

		private NoteEnum CalculateNoteFromPoint(Point point) {
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

		/// <summary>
		/// Called when the background is being drawn
		/// </summary>
		protected override void OnPaintBackground(PaintEventArgs e) {
		}

		/// <summary>
		/// Draws the interactive keyboard
		/// </summary>
		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.CompositingMode = CompositingMode.SourceCopy;
			e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
			e.Graphics.Clear(BackColor);
			base.OnPaint(e);
			//calculate metrics
			int whiteKeyWidth = WhiteKeyWidth;
			int blackKeyWidth = (whiteKeyWidth * 9) / 14;
			int halfBlackKeyWidth = blackKeyWidth / 2;
			int blackKeyOffset = whiteKeyWidth - halfBlackKeyWidth;
			int pianoWidth = PianoWidth;
			int x = (Width - pianoWidth) / 2;
			const int halfLineThickness = lineThickness / 2;
			int height = ClientSize.Height - lineThickness;
			//fill piano background
			e.Graphics.FillRectangle(Brushes.White, x, halfLineThickness, pianoWidth, height);
			int i, keyStart;
			//draw white keys
			Rectangle rect = new Rectangle();
			bool isWhite = true;
			for (i = 0; i < WhiteKeyCount; ++i) {
				keyStart = x + i * whiteKeyWidth;
				if (leftMouseDown && rect.Width == 0 && keyStart <= LastCursorLocation.X && keyStart + whiteKeyWidth > LastCursorLocation.X)
					rect = new Rectangle(keyStart, halfLineThickness, whiteKeyWidth, height);
				e.Graphics.DrawRectangle(KeyOutline, keyStart, halfLineThickness, whiteKeyWidth, height);
			}
			int temp;
			const int blackKeyIntervalCount = WhiteKeyCount - 1;
			int blackKeyHeight = (height * 4) / 7;
			//draw black keys
			for (i = 0; i < blackKeyIntervalCount; ++i) {
				temp = i % 7;
				if (!(temp == 2 || temp == 6)) { //skip every 2nd and 6th key
					keyStart = x + i * whiteKeyWidth + blackKeyOffset;
					if (leftMouseDown && LastCursorLocation.Y < blackKeyHeight && keyStart <= LastCursorLocation.X && keyStart + blackKeyWidth > LastCursorLocation.X) {
						rect = new Rectangle(keyStart, lineThickness, blackKeyWidth, blackKeyHeight);
						isWhite = false;
					} else
						e.Graphics.FillRectangle(Brushes.Black, keyStart, lineThickness, blackKeyWidth, blackKeyHeight);
				}
			}
			if (rect.Width != 0) {
				if (isWhite) {
					e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(rect.X + halfLineThickness, rect.Y + halfLineThickness, rect.Width - lineThickness, rect.Height - lineThickness));
					NoteEnum note = WhiteNoteIndexToNote((rect.X - x) / whiteKeyWidth);
					HasBlackKeys hasBlackKeys = CheckIfWhiteNoteHasBlackKeys(note);
					if ((hasBlackKeys & HasBlackKeys.Flat) == HasBlackKeys.Flat)
						e.Graphics.FillRectangle(Brushes.Black, rect.X - halfBlackKeyWidth, lineThickness, blackKeyWidth, blackKeyHeight);
					if ((hasBlackKeys & HasBlackKeys.Sharp) == HasBlackKeys.Sharp)
						e.Graphics.FillRectangle(Brushes.Black, rect.X + blackKeyOffset, lineThickness, blackKeyWidth, blackKeyHeight);
				} else {
					e.Graphics.FillRectangle(Brushes.Blue, rect);
					rect.X += halfLineThickness;
					rect.Y -= halfLineThickness;
					rect.Width -= lineThickness;
					e.Graphics.DrawRectangle(KeyOutline, rect);
				}
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
					e.Graphics.FillPath(Brushes.Red, path);
					e.Graphics.DrawPath(Pens.White, path);
				}
			}
		}
	}
}