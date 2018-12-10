﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace NezvalPiano {
	/// <summary>
	/// Represents a musical note object to be placed on a musical staff
	/// </summary>
	[ToolboxItem(true)]
	[DesignTimeVisible(true)]
	[DesignerCategory("CommonControls")]
	[Description("Represents a musical note object to be placed on a musical staff")]
	[DisplayName(nameof(MusicNote))]
	public class MusicNote : Control {
		/// <summary>
		/// The length in milliseconds for a half-hemidemisemiquaver
		/// </summary>
		public const float MillisPerHalfHemiDemiSemiQuaver = 18;
		/// <summary>
		/// An integer enumeration that represents notes on a scale
		/// </summary>
		public NoteEnum Pitch;
		/// <summary>
		/// An integer enumeration that represents note lengths with their relative value
		/// </summary>
		public NoteLength Length;
		private Stopwatch noteLengthStopwatch = new Stopwatch();
		private Point dragCursorPos;
		private bool leftButtonDown, rightButtonDown, mouseMovedDuringLeftButton;

		/// <summary>
		/// Gets the curent note length in milliseconds
		/// </summary>
		public float LengthInMilliseconds {
			get {
				return (int) Length * MillisPerHalfHemiDemiSemiQuaver;
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
		public MusicNote() {
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.CacheText, true);
			BackColor = Color.Transparent;
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
	}
}