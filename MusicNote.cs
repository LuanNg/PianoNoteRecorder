using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// Represents a musical note object to be placed on a musical staff
	/// </summary>
	public class MusicNote {
		private static Pen BlackPen = Pens.Black;
		private static Brush DotBrush = Brushes.Black;
		private static Brush HighlightedBrush = Brushes.Blue;
		/// <summary>
		/// The parent staff control
		/// </summary>
		private MusicStaff Staff;
		/// <summary>
		/// The associated piano keyboard control
		/// </summary>
		private MusicKeyboard Keyboard;
		private int bottomBarY, oldCursorY;
		private NoteEnum oldPitch, pitch;
		private float lengthInMilliseconds;
		private NoteLength length;
		private bool leftButtonDown, highlighted;
		/// <summary>
		/// The music note bounds within its parent staff control
		/// </summary>
		public Rectangle Bounds;

		/// <summary>
		/// Gets or sets whether the note is highlighted
		/// </summary>
		public bool Highlighted {
			get {
				return highlighted;
			}
			set {
				if (value == highlighted)
					return;
				highlighted = value;
				Invalidate(new Rectangle(Point.Empty, Bounds.Size));
			}
		}

		/// <summary>
		/// Gets or sets the note length
		/// </summary>
		public NoteLength Length {
			get {
				return length;
			}
			set {
				if (value < NoteLength.HemiDemiSemiQuaver)
					value = NoteLength.HemiDemiSemiQuaver;
				if (value == length)
					return;
				length = value;
				lengthInMilliseconds = (int) length * Staff.MillisPerHalfHemiDemiSemiQuaver;
				Staff.Invalidate(false);
			}
		}

		/// <summary>
		/// Gets the current note length in milliseconds
		/// </summary>
		public float LengthInMilliseconds {
			get {
				return lengthInMilliseconds;
			}
			set {
				Length = Staff.ToNoteLength(value);
				lengthInMilliseconds = value;
			}
		}

		/// <summary>
		/// Get or sets the point of reference for the bottom F4 note of the current bar
		/// </summary>
		public int BottomBarY {
			get {
				return bottomBarY;
			}
			set {
				if (value == bottomBarY)
					return;
				bottomBarY = value;
				UpdateYLoc();
			}
		}

		/// <summary>
		/// Gets or sets the note pitch
		/// </summary>
		public NoteEnum Pitch {
			get {
				return pitch;
			}
			set {
				if (value == pitch)
					return;
				pitch = value;
				UpdateYLoc();
			}
		}

		/// <summary>
		/// Initializes a new music note control
		/// </summary>
		/// <param name="parent">The parent staff control</param>
		/// <param name="keyboard">The associated piano keyboard control</param>
		/// <param name="pitch">The note pitch</param>
		/// <param name="length">The note length</param>
		/// <param name="location">The location on the score</param>
		public MusicNote(MusicStaff parent, MusicKeyboard keyboard, NoteEnum pitch, NoteLength length, Point location) :
			this(parent, keyboard, pitch, (int) length * parent.MillisPerHalfHemiDemiSemiQuaver, location){

		}

		/// <summary>
		/// Initializes a new music note control
		/// </summary>
		/// <param name="parent">The parent staff control</param>
		/// <param name="keyboard">The associated piano keyboard control</param>
		/// <param name="pitch">The note pitch</param>
		/// <param name="lengthInMs">The note length in milliseconds</param>
		/// <param name="location">The location on the score</param>
		public MusicNote(MusicStaff parent, MusicKeyboard keyboard, NoteEnum pitch, float lengthInMs, Point location) {
			Staff = parent;
			Keyboard = keyboard;
			bottomBarY = location.Y;
			Bounds = new Rectangle(location.X, location.Y, MusicStaff.NoteWidth, MusicStaff.LineSpace * 6);
			Pitch = pitch;
			LengthInMilliseconds = lengthInMs;
		}

		/// <summary>
		/// Redraws the note on the staff
		/// </summary>
		public void Invalidate() {
			Invalidate(new Rectangle(Point.Empty, Bounds.Size));
		}

		/// <summary>
		/// Redraws the specified region of the note in client coordinates
		/// </summary>
		/// <param name="bounds">The are of the note to redraw</param>
		public void Invalidate(Rectangle bounds) {
			bounds.X += Bounds.X;
			bounds.Y += Bounds.Y - Staff.VerticalScroll.Value;
			Staff.Invalidate(bounds, false);
		}

		/// <summary>
		/// Updates the Y location of the note 
		/// </summary>
		private void UpdateYLoc() {
			Rectangle oldBounds = Bounds;
			if (pitch == NoteEnum.None) //if is rest
				Bounds.Y = bottomBarY - MusicStaff.LineSpace;
			else { //if is note
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
				Bounds.Y = newTop - MusicStaff.LineSpace;
			}
			Rectangle toInvalidate = Rectangle.Union(oldBounds, Bounds);
			toInvalidate.X -= Bounds.X;
			toInvalidate.Y -= Bounds.Y;
			Invalidate(toInvalidate);
		}

		/// <summary>
		/// Called when a mouse button is pressed on the music note
		/// </summary>
		public void MarkMouseDown(MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				oldCursorY = Cursor.Position.Y;
				leftButtonDown = true;
				oldPitch = pitch;
				Keyboard.MarkKeyPressed(pitch, false);
			} else if (e.Button == MouseButtons.Right)
				Staff.StartAdjustingNote(this, Stopwatch.StartNew());
			Highlighted = true;
		}

		/// <summary>
		/// Called when the mouse is moved on the control
		/// </summary>
		public void MarkMouseMove() {
			if (leftButtonDown) {
				NoteEnum newPitch = oldPitch + ((oldCursorY - Cursor.Position.Y) * 2) / MusicStaff.LineSpace;
				if (newPitch >= NoteEnum.None && newPitch <= NoteEnum.CSharp7 && newPitch != pitch) {
					Keyboard.MarkKeyReleased(pitch);
					Pitch = newPitch;
					Keyboard.MarkKeyPressed(pitch, false);
				}
			}
		}

		/// <summary>
		/// Called when a mouse button is release on the music note
		/// </summary>
		public void MarkMouseUp(MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				leftButtonDown = false;
				Keyboard.MarkKeyReleased(pitch);
			} else if (e.Button == MouseButtons.Right)
				Staff.StopAdjustingNote(this);
			Highlighted = false;
		}

		/// <summary>
		/// Returns whether the specified note length is dotted
		/// </summary>
		/// <param name="length">The length of the note</param>
		public static bool IsDotted(NoteLength length) {
			return (length > 0) ? ((length & (length - 1)) != 0) : true; //return false if power not of 2, else true
		}

		private int DrawNoteImage(Graphics g, Bitmap image, int offsetX, int offsetY) {
			int height = Bounds.Height - MusicStaff.LineSpace * 2;
			int width = (height * image.Width) / image.Height;
			int centerLeft = (Bounds.Width - width) / 2;
			g.DrawImage(image, centerLeft + offsetX, MusicStaff.LineSpace + offsetY, width, height);
			return centerLeft;
		}

		private void DrawCenteredImage(Graphics g, Bitmap image, int offsetX, int offsetY) {
			g.DrawImage(image, (Bounds.Width - image.Width) / 2 + offsetX, (Bounds.Height - image.Height) / 2 + offsetY, image.Width, image.Height);
		}

		/// <summary>
		/// Draws the current note
		/// </summary>
		public void DrawNote(Graphics g, Point location) {
			g.CompositingMode = CompositingMode.SourceOver;
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;
			Size size = Bounds.Size;
			size.Height -= MusicStaff.LineSpace * 2;
			bool upsideDown = pitch >= NoteEnum.C6 || (pitch <= NoteEnum.B4 && pitch >= NoteEnum.D4);
			if (pitch == NoteEnum.None) { //rest
				switch (length) {
					case NoteLength.HemiDemiSemiQuaver:
					case NoteLength.DottedHemiDemiSemiQuaver:
						DrawCenteredImage(g, highlighted ? Images.HemiDemiSemiQuaverRHighlight : Images.HemiDemiSemiQuaverR, location.X, location.Y);
						break;
					case NoteLength.DemiSemiQuaver:
					case NoteLength.DottedDemiSemiQuaver:
						DrawCenteredImage(g, highlighted ? Images.DemiSemiQuaverRHighlight : Images.DemiSemiQuaverR, location.X, location.Y);
						break;
					case NoteLength.SemiQuaver:
					case NoteLength.DottedSemiQuaver:
						DrawCenteredImage(g, highlighted ? Images.SemiQuaverRHighlight : Images.SemiQuaverR, location.X, location.Y);
						break;
					case NoteLength.Quaver:
					case NoteLength.DottedQuaver:
						DrawCenteredImage(g, highlighted ? Images.QuaverRHighlight : Images.QuaverR, location.X, location.Y);
						break;
					case NoteLength.Crotchet:
					case NoteLength.DottedCrotchet:
						DrawCenteredImage(g, highlighted ? Images.CrotchetRHighlight : Images.CrotchetR, location.X, location.Y);
						break;
					case NoteLength.Minim:
					case NoteLength.DottedMinim:
						DrawCenteredImage(g, highlighted ? Images.MinimRHighlight : Images.MinimR, location.X, location.Y);
						break;
					case NoteLength.SemiBreve:
						DrawCenteredImage(g, highlighted ? Images.SemiBreveRHighlight : Images.SemiBreveR, location.X, location.Y);
						break;
				}
			} else { //note
				int centerLeft = 0;
				switch (length) {
					case NoteLength.HemiDemiSemiQuaver:
					case NoteLength.DottedHemiDemiSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? Images.HemiDemiSemiQuaverUpsideDownHighlight : Images.HemiDemiSemiQuaverUpsideDown, location.X, location.Y) : DrawNoteImage(g, highlighted ? Images.HemiDemiSemiQuaverHighlight : Images.HemiDemiSemiQuaver, location.X + 3, location.Y);
						break;
					case NoteLength.DemiSemiQuaver:
					case NoteLength.DottedDemiSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? Images.DemiSemiQuaverUpsideDownHighlight : Images.DemiSemiQuaverUpsideDown, location.X, location.Y) : DrawNoteImage(g, highlighted ? Images.DemiSemiQuaverHighlight : Images.DemiSemiQuaver, location.X + 3, location.Y);
						break;
					case NoteLength.SemiQuaver:
					case NoteLength.DottedSemiQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? Images.SemiQuaverUpsideDownHighlight : Images.SemiQuaverUpsideDown, location.X, location.Y) : DrawNoteImage(g, highlighted ? Images.SemiQuaverHighlight : Images.SemiQuaver, location.X + 3, location.Y);
						break;
					case NoteLength.Quaver:
					case NoteLength.DottedQuaver:
						centerLeft = upsideDown ? DrawNoteImage(g, highlighted ? Images.QuaverUpsideDownHighlight : Images.QuaverUpsideDown, location.X, location.Y) : DrawNoteImage(g, highlighted ? Images.QuaverHighlight : Images.Quaver, location.X + 3, location.Y);
						break;
					case NoteLength.Crotchet:
					case NoteLength.DottedCrotchet:
						centerLeft = DrawNoteImage(g, upsideDown ? (highlighted ? Images.CrotchetUpsideDownHighlight : Images.CrotchetUpsideDown) : (highlighted ? Images.CrotchetHighlight : Images.Crotchet), location.X, location.Y);
						break;
					case NoteLength.Minim:
					case NoteLength.DottedMinim:
						centerLeft = DrawNoteImage(g, upsideDown ? (highlighted ? Images.MinimUpsideDownHighlight : Images.MinimUpsideDown) : (highlighted ? Images.MinimHighlight : Images.Minim), location.X, location.Y);
						break;
					case NoteLength.SemiBreve:
						centerLeft = DrawNoteImage(g, upsideDown ? (highlighted ? Images.SemiBreveUpsideDownHighlight : Images.SemiBreveUpsideDown) : (highlighted ? Images.SemiBreveHighlight : Images.SemiBreve), location.X, location.Y);
						break;
				}
				if (MusicKeyboard.IsSharp(pitch)) //draw sharp
					g.DrawImage(highlighted ? Images.SharpHighlight : Images.Sharp, location.X, location.Y + (upsideDown ? MusicStaff.LineSpace / 3: (MusicStaff.LineSpace * 13) / 4), ((MusicStaff.LineSpace * 5 * Images.Sharp.Width) / (Images.Sharp.Height * 2)), (MusicStaff.LineSpace * 5) / 2);
				g.CompositingMode = CompositingMode.SourceCopy;
				g.CompositingQuality = CompositingQuality.HighSpeed;
				g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
				g.SmoothingMode = SmoothingMode.HighSpeed;
				const int lineProtrusion = 2;
				//draw auxiliary lines if note is outside of the bar
				if (pitch <= NoteEnum.E3) {
					int bottom = MusicStaff.LineSpace * 7 + bottomBarY + MusicStaff.BarTopDistance * 2 - Bounds.Y;
					int y = size.Height + MusicStaff.LineSpace / 2;
					if ((y - bottom) % MusicStaff.LineSpace != 0)
						y -= MusicStaff.LineSpace / 2;
					for (; y > bottom; y -= MusicStaff.LineSpace)
						g.DrawLine(BlackPen, location.X + centerLeft - lineProtrusion, location.Y + y, location.X + size.Width + lineProtrusion - centerLeft, location.Y + y);
				} else if (pitch == NoteEnum.C5 || pitch == NoteEnum.CSharp5) {
					const int y = (MusicStaff.LineSpace * 9) / 2;
					g.DrawLine(BlackPen, location.X + centerLeft - lineProtrusion, location.Y + y, location.X + size.Width + lineProtrusion - centerLeft, location.Y + y);
				} else if (pitch >= NoteEnum.A6) {
					int bottom = bottomBarY - Bounds.Y;
					int y = (MusicStaff.LineSpace * 3) / 2;
					if ((bottom - y) % MusicStaff.LineSpace != 0)
						y += MusicStaff.LineSpace / 2;
					for (; y < bottom; y += MusicStaff.LineSpace)
						g.DrawLine(BlackPen, location.X + centerLeft - lineProtrusion, location.Y + y, location.X + size.Width + lineProtrusion - centerLeft, location.Y + y);
				}
			}
			if (IsDotted(length)) { //draw dot noxt to note
				g.CompositingMode = CompositingMode.SourceOver;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.FillEllipse(highlighted ? HighlightedBrush : DotBrush, location.X + size.Width - 3, location.Y + (pitch == NoteEnum.None ? ((size.Height + MusicStaff.LineSpace - 3) / 2) : (upsideDown ? (MusicStaff.LineSpace * 9) / 8 : (MusicStaff.LineSpace * 33) / 8)), 3, 3);
			}
		}
	}
}