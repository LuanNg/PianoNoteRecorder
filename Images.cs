using PianoNoteRecorder.Properties;
using System.Drawing;
using System.Drawing.Imaging;

namespace PianoNoteRecorder {
	/// <summary>
	/// Contains the image resources used by the project
	/// </summary>
	public static class Images {
		/// <summary>
		/// The treble clef image
		/// </summary>
		public static readonly Bitmap Treble = Resources.Treble;
		/// <summary>
		/// The bass clef image
		/// </summary>
		public static readonly Bitmap Bass = Resources.Bass;
		/// <summary>
		/// The time signature image
		/// </summary>
		public static readonly Bitmap TimeSignature = Resources._44;

		public static readonly Bitmap Sharp = Resources.Sharp;
		public static readonly Bitmap HemiDemiSemiQuaverR = Resources.HemiDemiSemiQuaverR;
		public static readonly Bitmap DemiSemiQuaverR = Resources.DemiSemiQuaverR;
		public static readonly Bitmap SemiQuaverR = Resources.SemiQuaverR;
		public static readonly Bitmap QuaverR = Resources.QuaverR;
		public static readonly Bitmap CrotchetR = Resources.CrotchetR;
		public static readonly Bitmap MinimR = Resources.MinimR;
		public static readonly Bitmap SemiBreveR = Resources.SemiBreveR;
		public static readonly Bitmap HemiDemiSemiQuaver = Resources.HemiDemiSemiQuaver;
		public static readonly Bitmap HemiDemiSemiQuaverUpsideDown = Resources.HemiDemiSemiQuaverUpsideDown;
		public static readonly Bitmap DemiSemiQuaver = Resources.DemiSemiQuaver;
		public static readonly Bitmap DemiSemiQuaverUpsideDown = Resources.DemiSemiQuaverUpsideDown;
		public static readonly Bitmap SemiQuaver = Resources.SemiQuaver;
		public static readonly Bitmap SemiQuaverUpsideDown = Resources.SemiQuaverUpsideDown;
		public static readonly Bitmap Quaver = Resources.Quaver;
		public static readonly Bitmap QuaverUpsideDown = Resources.QuaverUpsideDown;
		public static readonly Bitmap Crotchet = Resources.Crotchet;
		public static readonly Bitmap CrotchetUpsideDown = TransformImage(Resources.Crotchet, RotateFlipType.Rotate180FlipNone);
		public static readonly Bitmap Minim = Resources.Minim;
		public static readonly Bitmap MinimUpsideDown = TransformImage(Resources.Minim, RotateFlipType.Rotate180FlipNone);
		public static readonly Bitmap SemiBreve = Resources.SemiBreve;
		public static readonly Bitmap SemiBreveUpsideDown = TransformImage(Resources.SemiBreve, RotateFlipType.Rotate180FlipNone);

		public static readonly Bitmap SharpHighlight = Highlight(Sharp);
		public static readonly Bitmap HemiDemiSemiQuaverRHighlight = Highlight(HemiDemiSemiQuaverR);
		public static readonly Bitmap DemiSemiQuaverRHighlight = Highlight(DemiSemiQuaverR);
		public static readonly Bitmap SemiQuaverRHighlight = Highlight(SemiQuaverR);
		public static readonly Bitmap QuaverRHighlight = Highlight(QuaverR);
		public static readonly Bitmap CrotchetRHighlight = Highlight(CrotchetR);
		public static readonly Bitmap MinimRHighlight = Highlight(MinimR);
		public static readonly Bitmap SemiBreveRHighlight = Highlight(SemiBreveR);
		public static readonly Bitmap HemiDemiSemiQuaverHighlight = Highlight(HemiDemiSemiQuaver);
		public static readonly Bitmap HemiDemiSemiQuaverUpsideDownHighlight = Highlight(HemiDemiSemiQuaverUpsideDown);
		public static readonly Bitmap DemiSemiQuaverHighlight = Highlight(DemiSemiQuaver);
		public static readonly Bitmap DemiSemiQuaverUpsideDownHighlight = Highlight(DemiSemiQuaverUpsideDown);
		public static readonly Bitmap SemiQuaverHighlight = Highlight(SemiQuaver);
		public static readonly Bitmap SemiQuaverUpsideDownHighlight = Highlight(SemiQuaverUpsideDown);
		public static readonly Bitmap QuaverHighlight = Highlight(Quaver);
		public static readonly Bitmap QuaverUpsideDownHighlight = Highlight(QuaverUpsideDown);
		public static readonly Bitmap CrotchetHighlight = Highlight(Crotchet);
		public static readonly Bitmap CrotchetUpsideDownHighlight = Highlight(CrotchetUpsideDown);
		public static readonly Bitmap MinimHighlight = Highlight(Minim);
		public static readonly Bitmap MinimUpsideDownHighlight = Highlight(MinimUpsideDown);
		public static readonly Bitmap SemiBreveHighlight = Highlight(SemiBreve);
		public static readonly Bitmap SemiBreveUpsideDownHighlight = Highlight(SemiBreveUpsideDown);

		/// <summary>
		/// Applies the specified transformation to the image
		/// </summary>
		/// <param name="image">The image to transform</param>
		/// <param name="transformation">The transformation to apply</param>
		public static Bitmap TransformImage(Bitmap image, RotateFlipType transformation) {
			image.RotateFlip(transformation);
			return image;
		}

		/// <summary>
		/// Marks the specified image as highlighted in blue
		/// </summary>
		/// <param name="image">The image to highlight</param>
		public static Bitmap Highlight(Bitmap image) {
			Bitmap newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
			BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			BitmapData newData = newImage.LockBits(new Rectangle(Point.Empty, newImage.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			unsafe
			{
				byte* ptr = ((byte*) data.Scan0) + 3;
				byte* newPtr = (byte*) newData.Scan0;
				int count = newImage.Width * newImage.Height;
				for (int i = 0; i < count; ++i) {
					*newPtr = 255;
					newPtr += 3;
					*newPtr = *ptr;
					newPtr++;
					ptr += 4;
				}
			}
			image.UnlockBits(data);
			newImage.UnlockBits(newData);
			return newImage;
		}
	}
}