using PianoNoteRecorder.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// Represents a musical note object to be placed on a musical staff
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("Represents a musical note object to be placed on a musical staff")]
	[DisplayName(nameof(MusicNote))]
	public class MusicNote : Control {
		private static Bitmap Sharp = Resources.Sharp;
		private static Bitmap HemiDemiSemiQuaverR = Resources.HemiDemiSemiQuaverR;
		private static Bitmap DemiSemiQuaverR = Resources.DemiSemiQuaverR;
		private static Bitmap SemiQuaverR = Resources.SemiQuaverR;
		private static Bitmap QuaverR = Resources.QuaverR;
		private static Bitmap CrotchetR = Resources.CrotchetR;
		private static Bitmap MinimR = Resources.MinimR;
		private static Bitmap SemiBreveR = Resources.SemiBreveR;
		private static Bitmap HemiDemiSemiQuaver = Resources.HemiDemiSemiQuaver;
		private static Bitmap HemiDemiSemiQuaverUpsideDown = Resources.HemiDemiSemiQuaverUpsideDown;
		private static Bitmap DemiSemiQuaver = Resources.DemiSemiQuaver;
		private static Bitmap DemiSemiQuaverUpsideDown = Resources.DemiSemiQuaverUpsideDown;
		private static Bitmap SemiQuaver = Resources.SemiQuaver;
		private static Bitmap SemiQuaverUpsideDown = Resources.SemiQuaverUpsideDown;
		private static Bitmap Quaver = Resources.Quaver;
		private static Bitmap QuaverUpsideDown = Resources.QuaverUpsideDown;
		private static Bitmap Crotchet = Resources.Crotchet;
		private static Bitmap CrotchetUpsideDown = TransformImage(Resources.Crotchet, RotateFlipType.Rotate180FlipNone);
		private static Bitmap Minim = Resources.Minim;
		private static Bitmap MinimUpsideDown = TransformImage(Resources.Minim, RotateFlipType.Rotate180FlipNone);
		private static Bitmap SemiBreve = Resources.SemiBreve;
		private static Bitmap SemiBreveUpsideDown = TransformImage(Resources.SemiBreve, RotateFlipType.Rotate180FlipNone);

		private static Bitmap SharpHighlight = Highlight(Sharp);
		private static Bitmap HemiDemiSemiQuaverRHighlight = Highlight(HemiDemiSemiQuaverR);
		private static Bitmap DemiSemiQuaverRHighlight = Highlight(DemiSemiQuaverR);
		private static Bitmap SemiQuaverRHighlight = Highlight(SemiQuaverR);
		private static Bitmap QuaverRHighlight = Highlight(QuaverR);
		private static Bitmap CrotchetRHighlight = Highlight(CrotchetR);
		private static Bitmap MinimRHighlight = Highlight(MinimR);
		private static Bitmap SemiBreveRHighlight = Highlight(SemiBreveR);
		private static Bitmap HemiDemiSemiQuaverHighlight = Highlight(HemiDemiSemiQuaver);
		private static Bitmap HemiDemiSemiQuaverUpsideDownHighlight = Highlight(HemiDemiSemiQuaverUpsideDown);
		private static Bitmap DemiSemiQuaverHighlight = Highlight(DemiSemiQuaver);
		private static Bitmap DemiSemiQuaverUpsideDownHighlight = Highlight(DemiSemiQuaverUpsideDown);
		private static Bitmap SemiQuaverHighlight = Highlight(SemiQuaver);
		private static Bitmap SemiQuaverUpsideDownHighlight = Highlight(SemiQuaverUpsideDown);
		private static Bitmap QuaverHighlight = Highlight(Quaver);
		private static Bitmap QuaverUpsideDownHighlight = Highlight(QuaverUpsideDown);
		private static Bitmap CrotchetHighlight = Highlight(Crotchet);
		private static Bitmap CrotchetUpsideDownHighlight = Highlight(CrotchetUpsideDown);
		private static Bitmap MinimHighlight = Highlight(Minim);
		private static Bitmap MinimUpsideDownHighlight = Highlight(MinimUpsideDown);
		private static Bitmap SemiBreveHighlight = Highlight(SemiBreve);
		private static Bitmap SemiBreveUpsideDownHighlight = Highlight(SemiBreveUpsideDown);
		private static Pen BlackPen = Pens.Black;
		private static Brush BlackBrush = Brushes.Black;
		private static Brush HighlightedBrush = Brushes.Blue;
		internal int verticalScrollAtInit;
		private MusicStaff Staff;
		private MusicKeyboard Keyboard;
		private int bottomBarY;
		private Stopwatch noteLengthStopwatch = new Stopwatch();
		private int oldCursorY;
		private NoteEnum oldPitch, pitch;
		private NoteLength length;
		private bool leftButtonDown, rightButtonDown, highlighted;

		public bool Highlighted {
			get {
				return highlighted;
			}
			set {
				if (value == highlighted)
					return;
				highlighted = value;
				Invalidate(false);
			}
		}

		/// <summary>
		/// An integer enumeration that represents note lengths with their relative value
		/// </summary>
		public NoteLength Length {
			get {
				return length;
			}
			set {
				if (value < NoteLength.HemiDemiSemiQuaver)
					value = NoteLength.HemiDemiSemiQuaver;
				length = value;
				Invalidate(false);
				Staff.Invalidate(false);
			}
		}

		/// <summary>
		/// The point of reference for the bottom F note of the current bar
		/// </summary>
		public int BottomBarY {
			get {
				return bottomBarY;
			}
			set {
				bottomBarY = value;
				UpdateYLoc();
			}
		}

		/// <summary>
		/// An integer enumeration that represents notes on a scale
		/// </summary>
		public NoteEnum Pitch {
			get {
				return pitch;
			}
			set {
				pitch = value;
				UpdateYLoc();
				Invalidate(false);
			}
		}

		/// <summary>
		/// Gets the current note length in milliseconds
		/// </summary>
		public float LengthInMilliseconds {
			get {
				return (int) length * Staff.millisPerHalfHemiDemiSemiQuaver;
			}
		}

		/// <summary>
		/// Initializes the control as transparent
		/// </summary>
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x00000020; //transparent
				return cp;
			}
		}

		/// <summary>
		/// Initializes a new music note control
		/// </summary>
		public MusicNote(MusicStaff parent, MusicKeyboard keyboard, NoteEnum pitch, NoteLength length, Point location) {
			Staff = parent;
			Keyboard = keyboard;
			Parent = parent;
			bottomBarY = location.Y;
			Bounds = new Rectangle(location.X, location.Y, MusicStaff.NoteWidth, MusicStaff.LineSpace * 6);
			Pitch = pitch;
			Length = length;
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
			BackColor = Color.Transparent;
		}

		private static Bitmap TransformImage(Bitmap image, RotateFlipType transformation) {
			image.RotateFlip(transformation);
			return image;
		}

		private static Bitmap Highlight(Bitmap image) {
			Bitmap newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
			BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			BitmapData newData = newImage.LockBits(new Rectangle(Point.Empty, newImage.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			unsafe
			{
				byte* ptr = ((byte*) data.Scan0) + 3;
				byte* newPtr = (byte*) newData.Scan0;
				int count = newImage.Width * newImage.Height;
				for (int i = 0; i < count; ++i) {
					*newPtr = 255;
					newPtr += 3;
					*newPtr = *ptr;
					newPtr++;
					ptr += 4;
				}
			}
			image.UnlockBits(data);
			newImage.UnlockBits(newData);
			return newImage;
		}

		private void UpdateYLoc() {
			if (pitch == NoteEnum.None)
				Top = bottomBarY - MusicStaff.LineSpace;
			else {
				int noteIndex = (int) pitch - 1;
				int note = noteIndex % 12;
				if (note >= 5)
					noteIndex++;
				noteIndex += (noteIndex / 12) * 2;
				if (note == 11)
					noteIndex--;
				noteIndex /= 2;
				int newTop = bottomBarY - ((noteIndex - 17) * MusicStaff.LineSpace / 2);
				if (pitch >= NoteEnum.C6)
					newTop += MusicStaff.LineSpace * 3;
				else if (pitch <= NoteEnum.CSharp4)
					newTop += MusicStaff.BarTopDistance - MusicStaff.LineSpace;
				else if (pitch <= NoteEnum.B4)
					newTop += MusicStaff.LineSpace * 2 + MusicStaff.BarTopDistance;
				Top = newTop - MusicStaff.LineSpace;
			}
		}

		/// <summary>
		/// Called when a mouse button is pressed on the music note
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				if ((Control.ModifierKeys & Keys.Control) == Keys.Control) {
					Parent = null;
					Dispose();
					return;
				}
				oldCursorY = Cursor.Position.Y;
				leftButtonDown = true;
				oldPitch = pitch;
				Keyboard.MarkKeyPressed(pitch, false);
			} else if (e.Button == MouseButtons.Right) {
				noteLengthStopwatch.Start();
				rightButtonDown = true;
			}
			Capture = true;
			Highlighted = true;
		}

		/// <summary>
		/// Called when the mouse is moved on the control
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (leftButtonDown) {
				NoteEnum newPitch = oldPitch + ((oldCursorY - Cursor.Position.Y) * 2) / MusicStaff.LineSpace;
				if (newPitch >= NoteEnum.None && newPitch <= NoteEnum.CSharp7 && newPitch != pitch) {
					Keyboard.MarkKeyReleased(pitch, false);
					Pitch = newPitch;
					Keyboard.MarkKeyPressed(pitch, false);
				}
			}
		}

		/// <summary>
		/// Called when a mouse button is release on the music note
		/// </summary>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				leftButtonDown = false;
				Keyboard.MarkKeyReleased(pitch, false);
			} else if (e.Button == MouseButtons.Right) {
				Length = Staff.ToNoteLength(noteLengthStopwatch.ElapsedMilliseconds);
				noteLengthStopwatch.Reset();
			}
			if (!(leftButtonDown || rightButtonDown))
				Capture = false;
			Highlighted = false;
		}

		public static bool IsDotted(NoteLength length) {
			return (length > 0) ? ((length & (length - 1)) != 0) : true;
		}

		//protected override void OnPaintBackground(PaintEventArgs pevent) {
		//}

		private int DrawNoteImage(Graphics g, Bitmap image, int offsetX = 0, int offsetY = 0) {
			Size size = ClientSize;
			size.Height -= MusicStaff.LineSpace * 2;
			int width = (size.Height * image.Width) / image.Height;
			int centerLeft = (size.Width - width) / 2;
			g.DrawImage(image, centerLeft + offsetX, MusicStaff.LineSpace + offsetY, width, size.Height);
			return centerLeft;
		}

		private void DrawCenteredImage(Graphics g, Bitmap image, int offsetX = 0, int offsetY = 0) {
			Size size = ClientSize;
			g.DrawImage(image, (size.Width - image.Width) / 2 + offsetX, (size.Height - image.Height) / 2 + offsetY, image.Width, image.Height);
		}

		/// <summary>
		/// Draws the current note
		/// </summary>
		protected override void OnPaint(PaintEventArgs e) {
			Graphics g = e.Graphics;
			g.CompositingMode = CompositingMode.SourceOver;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			Size size = ClientSize;
			size.Height -= MusicStaff.LineSpace * 2;
			bool upsideDown = pitch >= NoteEnum.C6 || (pitch <= NoteEnum.B4 && pitch >= NoteEnum.D4);
			if (pitch == NoteEnum.None) { //rest
				switch (length) {
					case NoteLength.HemiDemiSemiQuaver:
					case NoteLength.DottedHemiDemiSemiQuaver:
						DrawCenteredImage(g, highlighted ? HemiDemiSemiQuaverRHighlight : HemiDemiSemiQuaverR);
						break;
					case NoteLength.DemiSemiQuaver:
					case NoteLength.DottedDemiSemiQuaver:
						DrawCenteredImage(g, highlighted ? DemiSemiQuaverRHighlight : DemiSemiQuaverR);
						break;
					case NoteLength.SemiQuaver:
					case NoteLength.DottedSemiQuaver:
						DrawCenteredImage(g, highlighted ? SemiQuaverRHighlight : SemiQuaverR);
						break;
					case NoteLength.Quaver:
					case NoteLength.DottedQuaver:
						DrawCenteredImage(g, highlighted ? QuaverRHighlight : QuaverR);
						break;
					case NoteLength.Crotchet:
					case NoteLength.DottedCrotchet:
						DrawCenteredImage(g, highlighted ? CrotchetRHighlight : CrotchetR);
						break;
					case NoteLength.Minim:
					case NoteLength.DottedMinim:
						DrawCenteredImage(g, highlighted ? MinimRHighlight : MinimR);
						break;
					case NoteLength.SemiBreve:
					case NoteLength.DottedSemiBreve:
						DrawCenteredImage(g, highlighted ? SemiBreveRHighlight : SemiBreveR);
						break;
				}
			} else {
				int centerLeft = 0;
				switch (length) {
					case NoteLength.HemiDemiSemiQuaver:
					case NoteLength.DottedHemiDemiSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? HemiDemiSemiQuaverUpsideDownHighlight : HemiDemiSemiQuaverUpsideDown) : DrawNoteImage(g, highlighted ? HemiDemiSemiQuaverHighlight : HemiDemiSemiQuaver, 3);
						break;
					case NoteLength.DemiSemiQuaver:
					case NoteLength.DottedDemiSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? DemiSemiQuaverUpsideDownHighlight : DemiSemiQuaverUpsideDown) : DrawNoteImage(g, highlighted ? DemiSemiQuaverHighlight : DemiSemiQuaver, 3);
						break;
					case NoteLength.SemiQuaver:
					case NoteLength.DottedSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? SemiQuaverUpsideDownHighlight : SemiQuaverUpsideDown) : DrawNoteImage(g, highlighted ? SemiQuaverHighlight : SemiQuaver, 3);
						break;
					case NoteLength.Quaver:
					case NoteLength.DottedQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? QuaverUpsideDownHighlight : QuaverUpsideDown) : DrawNoteImage(g, highlighted ? QuaverHighlight : Quaver, 3);
						break;
					case NoteLength.Crotchet:
					case NoteLength.DottedCrotchet:
						centerLeft = DrawNoteImage(g, upsideDown ? (highlighted ? CrotchetUpsideDownHighlight : CrotchetUpsideDown) : (highlighted ? CrotchetHighlight : Crotchet));
						break;
					case NoteLength.Minim:
					case NoteLength.DottedMinim:
						centerLeft = DrawNoteImage(g, upsideDown ? (highlighted ? MinimUpsideDownHighlight : MinimUpsideDown) : (highlighted ? MinimHighlight : Minim));
						break;
					case NoteLength.SemiBreve:
					case NoteLength.DottedSemiBreve:
						centerLeft = DrawNoteImage(g, upsideDown ? (highlighted ? SemiBreveUpsideDownHighlight : SemiBreveUpsideDown) : (highlighted ? SemiBreveHighlight : SemiBreve));
						break;
				}
				if (MusicKeyboard.IsSharp(pitch))
					e.Graphics.DrawImage(highlighted ? SharpHighlight : Sharp, 0, upsideDown ? MusicStaff.LineSpace / 3: (MusicStaff.LineSpace * 13) / 4, (MusicStaff.LineSpace * 5 * Sharp.Width) / (Sharp.Height * 2), (MusicStaff.LineSpace * 5) / 2);
				e.Graphics.CompositingMode = CompositingMode.SourceCopy;
				e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
				e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
				const int lineProtrusion = 3;
				int top = Top;
				if (pitch <= NoteEnum.E3) {
					int bottom = MusicStaff.LineSpace * 3 + bottomBarY + MusicStaff.BarTopDistance - top;
					int y = size.Height + MusicStaff.LineSpace / 2;
					if ((y - bottom) % MusicStaff.LineSpace != 0)
						y -= MusicStaff.LineSpace / 2;
					for (; y > bottom; y -= MusicStaff.LineSpace)
						e.Graphics.DrawLine(BlackPen, centerLeft - lineProtrusion, y, size.Width + lineProtrusion - centerLeft, y);
				} else if (pitch == NoteEnum.C5 || pitch == NoteEnum.CSharp5) {
					const int y = (MusicStaff.LineSpace * 9) / 2;
					e.Graphics.DrawLine(BlackPen, centerLeft - lineProtrusion, y, size.Width + lineProtrusion - centerLeft, y);
				} else if (pitch >= NoteEnum.A6) {
					int bottom = bottomBarY - (top - MusicStaff.LineSpace);
					int y = (MusicStaff.LineSpace * 3) / 2;
					if ((bottom - y) % MusicStaff.LineSpace != 0)
						y += MusicStaff.LineSpace / 2;
					for (; y < bottom; y += MusicStaff.LineSpace)
						e.Graphics.DrawLine(BlackPen, centerLeft - lineProtrusion, y, size.Width + lineProtrusion - centerLeft, y);
				}
			}
			if (IsDotted(length))
				e.Graphics.FillEllipse(highlighted ? HighlightedBrush : BlackBrush, size.Width - 3, pitch == NoteEnum.None ? ((size.Height + MusicStaff.LineSpace - 3) / 2) : (upsideDown ? (MusicStaff.LineSpace * 9) / 8 : (MusicStaff.LineSpace * 33) / 8), 3, 3);
			base.OnPaint(e);
		}
	}
}