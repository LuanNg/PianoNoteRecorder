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
		public MusicStaff Staff;
		private static NoteLength[] NoteLengths = (NoteLength[]) Enum.GetValues(typeof(NoteLength));
		private static Bitmap Crotchet = Resources.Crotchet;
		/// <summary>
		/// An integer enumeration that represents note lengths with their relative value
		/// </summary>
		public NoteLength Length;
		private int bottomBarY;
		private Stopwatch noteLengthStopwatch = new Stopwatch();
		private Point dragCursorPos;
		private NoteEnum pitch;
		private bool leftButtonDown, rightButtonDown, mouseMovedDuringLeftButton;

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
		public MusicNote(MusicStaff parent, NoteEnum pitch, NoteLength length, Point location) {
			Staff = parent;
			Parent = parent;
			bottomBarY = location.Y;
			Bounds = new Rectangle(location.X, location.Y, (int) (MusicStaff.NoteWidth * 1.75f), MusicStaff.LineSpace * 4);
			Pitch = pitch;
			Length = length;
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.CacheText | ControlStyles.OptimizedDoubleBuffer, true);
			BackColor = Color.Transparent;
		}

		private void UpdateYLoc() {
			if (pitch == NoteEnum.None)
				Top = bottomBarY - MusicStaff.LineSpace * 4;
			else {
				int noteIndex = (int) pitch - 1;
				int note = noteIndex % 12;
				if (note >= 5)
					noteIndex++;
				noteIndex += (noteIndex / 12) * 2;
				if (note == 11)
					noteIndex--;
				noteIndex /= 2;
				Top = bottomBarY - ((noteIndex - 17) * MusicStaff.LineSpace / 2);
			}
		}

		public static NoteLength ToNoteLength(float ms) {
			/*int length = (int) (ms / millisPerHalfHemiDemiSemiQuaver);
			for (int i = NoteLengths.Length - 1; i >= 0; i--) {
				if (length >= (int) NoteLengths[i])
					return (NoteLength) length;
			}
			return NoteLength.None;*/
			return NoteLength.Crotchet;
		}

		/// <summary>
		/// Called when a mouse button is pressed on the music note
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left) {
				dragCursorPos = Cursor.Position;
				leftButtonDown = true;
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
				mouseMovedDuringLeftButton = true;
			}
		}

		/// <summary>
		/// Called when a mouse button is release on the music note
		/// </summary>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				leftButtonDown = false;
				if (mouseMovedDuringLeftButton)
					mouseMovedDuringLeftButton = false;
				else {
					//Play note
				}
			} else if (e.Button == MouseButtons.Right) {
				long millisecs = noteLengthStopwatch.ElapsedMilliseconds;

				noteLengthStopwatch.Reset();
			}
			if (!(leftButtonDown || rightButtonDown))
				Capture = false;
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
			if (pitch == NoteEnum.None) {
			} else {
				switch (Length) {
					case NoteLength.Crotchet:
						e.Graphics.DrawImage(Crotchet, 0, 0, (size.Width * Crotchet.Height) / size.Height, size.Height);
						break;
				}
			}
			base.OnPaint(e);
		}
	}
}