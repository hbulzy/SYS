using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
namespace Tunynet.Imaging
{
	public abstract class WatermarkFilterBase : IImageFilter
	{
		private float _opacity = 0.8f;
		public AnchorLocation AnchorLocation
		{
			get;
			set;
		}
		public float Opacity
		{
			get
			{
				return this._opacity;
			}
			set
			{
				if (value < 0f)
				{
					this._opacity = 0f;
					return;
				}
				if (value > 1f)
				{
					this._opacity = 1f;
					return;
				}
				this._opacity = value;
			}
		}
		public abstract Image Process(Image inputImage, out bool isProcessed);
		protected static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
		{
			PixelFormat[] source = new PixelFormat[]
			{
				PixelFormat.Undefined,
				PixelFormat.Undefined,
				PixelFormat.Format16bppArgb1555,
				PixelFormat.Format1bppIndexed,
				PixelFormat.Format4bppIndexed,
				PixelFormat.Format8bppIndexed
			};
			return source.Contains(imgPixelFormat);
		}
	}
}
