using PianoNoteRecorder.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	/// <summary>
	/// A musical staff that will contain musical notes
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("A musical staff that will contain musical notes")]
	[DisplayName(nameof(MusicStaff))]
	public class MusicStaff : Panel {
		private static Bitmap Treble = Resources.Treble;

		public MusicStaff() {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
			BackColor = Color.White;
			VScroll = true;
			HScroll = false;
		}

		/// <summary>
		/// Called when the panel size changed
		/// </summary>
		protected override void OnClientSizeChanged(EventArgs e) {
			base.OnClientSizeChanged(e);
			AutoScrollMinSize = new Size(ClientSize.Width - SystemInformation.VerticalScrollBarWidth, AutoScrollMinSize.Height);
		}

		/// <summary>
		/// Called when the background is being drawn
		/// </summary>
		protected override void OnPaintBackground(PaintEventArgs e) {
		}

		/// <summary>
		/// Draws the musical staff
		/// </summary>
		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.CompositingMode = CompositingMode.SourceCopy;
			e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
			e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
			e.Graphics.Clear(BackColor);
			base.OnPaint(e);
			Size clientSize = ClientSize;
			const int barVerticalDistance = 20;
			int y = barVerticalDistance / 2;
			const int lineSpace = 10;
			const int barHeight = lineSpace * 5;
			int i;
			do {
				for (i = 0; i < 5; ++i) {
					y += lineSpace;
					e.Graphics.DrawLine(Pens.Black, 0, y, clientSize.Width, y);
				}
				y += barVerticalDistance;
			} while (y < clientSize.Height);
			e.Graphics.CompositingMode = CompositingMode.SourceOver;
			e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
			e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			y = barVerticalDistance / 2 + lineSpace;
			int x = 10;
			do {
				e.Graphics.DrawImage(Treble, x, y, (barHeight * Treble.Width) / Treble.Height, barHeight);
				y += barHeight + barVerticalDistance;
			} while (y < clientSize.Height);
		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			if (se.ScrollOrientation == ScrollOrientation.VerticalScroll) {
			}
		}
	}
}