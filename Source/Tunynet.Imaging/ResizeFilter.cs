using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace Tunynet.Imaging
{
	public class ResizeFilter : IImageFilter
	{
		public Size TargetSize
		{
			get;
			set;
		}
		public ResizeMethod ResizeMethod
		{
			get;
			set;
		}
		public AnchorLocation AnchorLocation
		{
			get;
			set;
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
		public ResizeFilter(int width, int height) : this(width, height, ResizeMethod.KeepAspectRatio)
		{
		}
		public ResizeFilter(int width, int height, ResizeMethod resizeMethod) : this(width, height, resizeMethod, AnchorLocation.Middle)
		{
		}
		public ResizeFilter(int width, int height, ResizeMethod resizeMethod, AnchorLocation anchorLocation)
		{
			this.TargetSize = new Size(width, height);
			this.ResizeMethod = resizeMethod;
			this.AnchorLocation = anchorLocation;
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
			Size empty = Size.Empty;
			Size newSize = this.GetNewSize(inputImage, this.TargetSize, this.ResizeMethod, out empty);
			Bitmap bitmap = new Bitmap(empty.Width, empty.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.InterpolationMode = this.InterpoliationMode;
				graphics.SmoothingMode = this.SmoothingMode;
				Rectangle destRect = new Rectangle(new Point(0, 0), newSize);
				Rectangle largestInset = new Rectangle(0, 0, inputImage.Width, inputImage.Height);
				float desiredAspect = (float)newSize.Width / (float)newSize.Height;
				if (this.ResizeMethod == ResizeMethod.Crop)
				{
					largestInset = this.GetLargestInset(largestInset, desiredAspect, this.AnchorLocation);
				}
				graphics.DrawImage(inputImage, destRect, largestInset, GraphicsUnit.Pixel);
			}
			inputImage.Dispose();
			isProcessed = true;
			return bitmap;
		}
		protected virtual Size GetNewSize(Image img, Size requestedSize, ResizeMethod resizeMethod, out Size bitmapSize)
		{
			Size size = default(Size);
			if (img.Width <= requestedSize.Width && img.Height <= requestedSize.Height)
			{
				size.Width = img.Width;
				size.Height = img.Height;
			}
			else
			{
				switch (resizeMethod)
				{
				case ResizeMethod.Absolute:
				case ResizeMethod.Crop:
					size = requestedSize;
					if (size.Width > img.Width)
					{
						size.Width = img.Width;
					}
					if (size.Height > img.Height)
					{
						size.Height = img.Height;
					}
					break;
				case ResizeMethod.KeepAspectRatio:
				{
					float num = (float)img.Width / (float)img.Height;
					float num2 = (float)requestedSize.Width / (float)requestedSize.Height;
					if (num <= num2)
					{
						size.Width = (int)((float)requestedSize.Height * num);
						size.Height = requestedSize.Height;
					}
					else
					{
						size.Width = requestedSize.Width;
						size.Height = (int)((float)requestedSize.Width / num);
					}
					break;
				}
				}
			}
			bitmapSize = size;
			return size;
		}
		protected virtual Rectangle GetLargestInset(Rectangle sourceRect, float desiredAspect, AnchorLocation anchorLocation)
		{
			Rectangle result = default(Rectangle);
			float num = (float)sourceRect.Width / (float)sourceRect.Height;
			float num2 = desiredAspect / num;
			if (num > desiredAspect)
			{
				result.Width = (int)((float)sourceRect.Width * num2);
				result.Height = sourceRect.Height;
			}
			else
			{
				result.Width = sourceRect.Width;
				result.Height = (int)((float)sourceRect.Height / num2);
			}
			RectangleUtil.PositionRectangle(anchorLocation, sourceRect, ref result);
			return result;
		}
	}
}
