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
		private static NoteLength[] NoteLengths = (NoteLength[]) Enum.GetValues(typeof(NoteLength));
		private static Bitmap Sharp = Resources.Sharp;
		private static Bitmap Crotchet = Resources.Crotchet;
		private static Bitmap CrotchetUpsideDown = TransformImage(Resources.Crotchet, RotateFlipType.Rotate180FlipNone);
		private static Pen BlackPen = Pens.Black;
		private static Brush BlackBrush = Brushes.Black;
		/// <summary>
		/// An integer enumeration that represents note lengths with their relative value
		/// </summary>
		public NoteLength Length;
		private MusicStaff Staff;
		private MusicKeyboard Keyboard;
		private int bottomBarY;
		private Stopwatch noteLengthStopwatch = new Stopwatch();
		private int oldCursorY;
		private NoteEnum oldPitch, pitch;
		private bool leftButtonDown, rightButtonDown;

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
				return (int) Length * Staff.millisPerHalfHemiDemiSemiQuaver;
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

		public static NoteLength ToNoteLength(float ms) {
			/*int length = (int) (ms / millisPerHalfHemiDemiSemiQuaver);
			for (int i = NoteLengths.Length - 1; i >= 0; i--) {
				if (length >= (int) NoteLengths[i])
					return (NoteLength) length;
			}
			return NoteLength.None;*/
			return NoteLength.DottedCrotchet;
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
				Length = ToNoteLength(noteLengthStopwatch.ElapsedMilliseconds);
				noteLengthStopwatch.Reset();
				Invalidate(false);
			}
			if (!(leftButtonDown || rightButtonDown))
				Capture = false;
		}

		public static bool IsDotted(NoteLength length) {
			return (length > 0) ? ((length & (length - 1)) != 0) : true;
		}

		/*protected override void OnPaintBackground(PaintEventArgs pevent) {
		}*/

		/// <summary>
		/// Draws the current note
		/// </summary>
		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.CompositingMode = CompositingMode.SourceOver;
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			Size size = ClientSize;
			size.Height -= MusicStaff.LineSpace * 2;
			int width = 9;
			int centerLeft = (size.Width - width) / 2;
			bool upsideDown = pitch >= NoteEnum.C6 || (pitch <= NoteEnum.B4 && pitch >= NoteEnum.D4);
			if (pitch == NoteEnum.None) {

			} else {
				switch (Length) {
					case NoteLength.Crotchet:
					case NoteLength.DottedCrotchet:
						width = (size.Height * Crotchet.Width) / Crotchet.Height;
						centerLeft = (size.Width - width) / 2;
						e.Graphics.DrawImage(upsideDown ? CrotchetUpsideDown : Crotchet, centerLeft, MusicStaff.LineSpace, width, size.Height);
						break;
				}
				if (MusicKeyboard.IsSharp(pitch))
					e.Graphics.DrawImage(Sharp, 0, upsideDown ? 0 : MusicStaff.LineSpace * 3, (MusicStaff.LineSpace * 3 * Sharp.Width) / Sharp.Height, MusicStaff.LineSpace * 3);
				if (IsDotted(Length))
					e.Graphics.FillEllipse(BlackBrush, size.Width - 5, upsideDown ? (MusicStaff.LineSpace * 9) / 8 : (MusicStaff.LineSpace * 33) / 8, 3, 3);
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