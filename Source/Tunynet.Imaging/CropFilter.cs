using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace Tunynet.Imaging
{
	public class CropFilter : IImageFilter
	{
		public Size TargetSize
		{
			get;
			private set;
		}
		public Rectangle CropArea
		{
			get;
			private set;
		}
		public InterpolationMode InterpoliationMode
		{
			get;
			set;
		}
		public SmoothingMode SmoothingMode
		{
			get;
			set;
		}
		public CropFilter(Rectangle cropArea, int descWidth, int descHeight)
		{
			this.CropArea = cropArea;
			this.TargetSize = new Size(descWidth, descHeight);
			this.InterpoliationMode = InterpolationMode.HighQualityBicubic;
			this.SmoothingMode = SmoothingMode.HighQuality;
		}
		public Image Process(Image inputImage, out bool isProcessed)
		{
			if (this.TargetSize.Height > inputImage.Height && this.TargetSize.Width > inputImage.Width)
			{
				isProcessed = false;
				return inputImage;
			}
			Size arg_39_0 = inputImage.Size;
			Rectangle cropArea = this.CropArea;
			int num = cropArea.X + cropArea.Width;
			if (num > inputImage.Width)
			{
				cropArea.Width -= num - inputImage.Width;
			}
			int num2 = cropArea.Y + cropArea.Height;
			if (num2 > inputImage.Height)
			{
				cropArea.Height -= num2 - inputImage.Height;
			}
			Bitmap bitmap = new Bitmap(this.TargetSize.Width, this.TargetSize.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.InterpolationMode = this.InterpoliationMode;
				graphics.SmoothingMode = this.SmoothingMode;
				Rectangle destRect = new Rectangle(0, 0, this.TargetSize.Width, this.TargetSize.Height);
				graphics.DrawImage(inputImage, destRect, cropArea, GraphicsUnit.Pixel);
			}
			inputImage.Dispose();
			isProcessed = true;
			return bitmap;
		}
	}
}
