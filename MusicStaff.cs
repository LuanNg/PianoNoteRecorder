using System;
using System.ComponentModel;
using System.Drawing;
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
			e.Graphics.Clear(BackColor);
			base.OnPaint(e);
			Size clientSize = ClientSize;
			int y = 15;
			int i;
			do {
				for (i = 0; i < 5; ++i) {
					y += 15;
					e.Graphics.DrawLine(Pens.Black, 0, y, clientSize.Width, y);
				}
				y += 25;
			} while (y < clientSize.Height);
		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			if (se.ScrollOrientation == ScrollOrientation.VerticalScroll) {
			}
		}
	}
}