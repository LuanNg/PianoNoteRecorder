using System;
using System.ComponentModel;
using System.Diagnostics;
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

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

		}
	}
}