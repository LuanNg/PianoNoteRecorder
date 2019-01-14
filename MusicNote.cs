using PianoNoteRecorder.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
		private static Bitmap Breve = Resources.Breve;
		private static Bitmap BreveUpsideDown = TransformImage(Resources.Breve, RotateFlipType.Rotate180FlipNone);
		private static Pen BlackPen = Pens.Black;
		private static Brush BlackBrush = Brushes.Black;
		private MusicStaff Staff;
		private MusicKeyboard Keyboard;
		private int bottomBarY;
		private Stopwatch noteLengthStopwatch = new Stopwatch();
		private int oldCursorY;
		private NoteEnum oldPitch, pitch;
		private NoteLength length;
		private bool leftButtonDown, rightButtonDown;

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

		private void UpdateYLoc() {
			if (pitch == NoteEnum.None)
				Top = bottomBarY - MusicStaff.LineSpace * 5;
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
				Keyboard.MarkKeyPressed(pitch);
			} else if (e.Button == MouseButtons.Right) {
				noteLengthStopwatch.Start();
				rightButtonDown = true;
			}
			Capture = true;
		}

		/// <summary>
		/// Called when the mouse is moved on the control
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			if (leftButtonDown) {
				NoteEnum newPitch = oldPitch + ((oldCursorY - Cursor.Position.Y) * 2) / MusicStaff.LineSpace;
				if (newPitch > NoteEnum.None && newPitch <= NoteEnum.CSharp7 && newPitch != pitch) {
					Keyboard.MarkKeyReleased(pitch, false);
					Pitch = newPitch;
					Keyboard.MarkKeyPressed(pitch);
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
				length = Staff.ToNoteLength(noteLengthStopwatch.ElapsedMilliseconds);
				noteLengthStopwatch.Reset();
				Invalidate(false);
			}
			if (!(leftButtonDown || rightButtonDown))
				Capture = false;
		}

		public static bool IsDotted(NoteLength length) {
			return (length > 0) ? ((length & (length - 1)) != 0) : true;
		}

		//protected override void OnPaintBackground(PaintEventArgs pevent) {
		//}

		private int DrawNoteImage(Graphics g, Bitmap image, int offset = 0) {
			Size size = ClientSize;
			size.Height -= MusicStaff.LineSpace * 2;
			int width = (size.Height * image.Width) / image.Height;
			int centerLeft = (size.Width - width) / 2;
			g.DrawImage(image, centerLeft + offset, MusicStaff.LineSpace, width, size.Height);
			return centerLeft;
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
			int centerLeft = 0;
			bool upsideDown = pitch >= NoteEnum.C6 || (pitch <= NoteEnum.B4 && pitch >= NoteEnum.D4);
			if (pitch == NoteEnum.None) {

			} else {
				switch (length) {
					case NoteLength.HemiDemiSemiQuaver:
					case NoteLength.DottedHemiDemiSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, HemiDemiSemiQuaverUpsideDown) : DrawNoteImage(g, HemiDemiSemiQuaver, 3);
						break;
					case NoteLength.DemiSemiQuaver:
					case NoteLength.DottedDemiSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, DemiSemiQuaverUpsideDown) : DrawNoteImage(g, DemiSemiQuaver, 3);
						break;
					case NoteLength.SemiQuaver:
					case NoteLength.DottedSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, SemiQuaverUpsideDown) : DrawNoteImage(g, SemiQuaver, 3);
						break;
					case NoteLength.Quaver:
					case NoteLength.DottedQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, QuaverUpsideDown) : DrawNoteImage(g, Quaver, 3);
						break;
					case NoteLength.Crotchet:
					case NoteLength.DottedCrotchet:
						centerLeft = DrawNoteImage(g, upsideDown ? CrotchetUpsideDown : Crotchet);
						break;
					case NoteLength.Minim:
					case NoteLength.DottedMinim:
						centerLeft = DrawNoteImage(g, upsideDown ? MinimUpsideDown : Minim);
						break;
					case NoteLength.SemiBreve:
					case NoteLength.DottedSemiBreve:
						centerLeft = DrawNoteImage(g, upsideDown ? SemiBreveUpsideDown : SemiBreve);
						break;
					case NoteLength.Breve:
					case NoteLength.DottedBreve:
						centerLeft = DrawNoteImage(g, upsideDown ? BreveUpsideDown : Breve);
						break;
				}
				if (MusicKeyboard.IsSharp(pitch))
					e.Graphics.DrawImage(Sharp, 0, upsideDown ? MusicStaff.LineSpace / 3: (MusicStaff.LineSpace * 13) / 4, (MusicStaff.LineSpace * 5 * Sharp.Width) / (Sharp.Height * 2), (MusicStaff.LineSpace * 5) / 2);
				if (IsDotted(length))
					e.Graphics.FillEllipse(BlackBrush, size.Width - 3, upsideDown ? (MusicStaff.LineSpace * 9) / 8 : (MusicStaff.LineSpace * 33) / 8, 3, 3);
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
			base.OnPaint(e);
		}
	}
}