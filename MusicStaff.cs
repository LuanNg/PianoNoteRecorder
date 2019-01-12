using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace NezvalPiano {
	/// <summary>
	/// A musical staff that will contain musical notes
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("A musical staff that will contain musical notes")]
	[DisplayName(nameof(MusicStaff))]
	public class MusicStaff : Panel {
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
			int i;
			do {
				for (i = 0; i < 5; ++i) {
					y += 10;
					e.Graphics.DrawLine(Pens.Black, 0, y, clientSize.Width, y);
				}
				y += barVerticalDistance;
			} while (y < clientSize.Height);

		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			if (se.ScrollOrientation == ScrollOrientation.VerticalScroll) {
			}
		}
	}
}