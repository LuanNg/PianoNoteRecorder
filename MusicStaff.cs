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
			
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

		}
	}
}