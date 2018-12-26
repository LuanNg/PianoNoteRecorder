﻿using System.ComponentModel;
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
		private bool showHint, leftMouseDown;

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
				return new Rectangle((Width - pianoWidth) / 2, lineThickness / 2, pianoWidth, Height - lineThickness);
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
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				leftMouseDown = true;
				Rectangle bounds = PianoBounds;
				if (bounds.Contains(e.Location))
					Invalidate(false);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (leftMouseDown) {
			}
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				leftMouseDown = false;
				Invalidate(false);
			}
		}

		private NoteEnum WhiteNoteIndexToNote(int noteIndex) {

		}

		private NoteEnum CalculateNoteFromPoint(Point point) {
			int whiteKeyWidth = WhiteKeyWidth;
			int blackKeyWidth = (whiteKeyWidth * 9) / 14;
			int blackKeyOffset = whiteKeyWidth - blackKeyWidth / 2;
			int pianoWidth = PianoWidth;
			int x = (Width - pianoWidth) / 2;
			if (point.X < x)
				return NoteEnum.None;
			int noteIndex = (point.X - x) / whiteKeyWidth;
			int height = Height - lineThickness;
			int blackKeyHeight = (height * 4) / 7;
			if (point.Y < blackKeyHeight) {
				int temp;
				const int blackKeyIntervalCount = WhiteKeyCount - 1;

				//draw black keys
				for (i = 0; i < blackKeyIntervalCount; ++i) {
					temp = i % 7;
					if (!(temp == 2 || temp == 6)) //skip every 2nd and 6th key
						e.Graphics.FillRectangle(Brushes.Black, x + i * whiteKeyWidth + blackKeyOffset, y, blackKeyWidth, blackKeyHeight);
				}
				return (NoteEnum) noteIndex;
			} else
				return WhiteNoteIndexToNote(noteIndex);
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
			int blackKeyOffset = whiteKeyWidth - blackKeyWidth / 2;
			int pianoWidth = PianoWidth;
			int x = (Width - pianoWidth) / 2;
			int y = lineThickness / 2;
			int height = Height - lineThickness;
			//fill piano background
			e.Graphics.FillRectangle(Brushes.White, x, y, pianoWidth, height);
			int i;
			//draw white keys
			for (i = 0; i < WhiteKeyCount; ++i)
				e.Graphics.DrawRectangle(KeyOutline, x + i * whiteKeyWidth, y, whiteKeyWidth, height);
			int temp;
			const int blackKeyIntervalCount = WhiteKeyCount - 1;
			int blackKeyHeight = (height * 4) / 7;
			//draw black keys
			for (i = 0; i < blackKeyIntervalCount; ++i) {
				temp = i % 7;
				if (!(temp == 2 || temp == 6)) //skip every 2nd and 6th key
					e.Graphics.FillRectangle(Brushes.Black, x + i * whiteKeyWidth + blackKeyOffset, y, blackKeyWidth, blackKeyHeight);
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
					path.AddString("Top of piano can also be resized using mouse", Font.FontFamily, (int) Font.Style, Font.Size, bounds, textFormat);
					e.Graphics.FillPath(Brushes.Red, path);
					e.Graphics.DrawPath(Pens.White, path);
				}
			}
		}
	}
}