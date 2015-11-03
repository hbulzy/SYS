using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
namespace Tunynet.Imaging
{
	public class ImageWatermarkFilter : WatermarkFilterBase
	{
		public Image WatermarkImage
		{
			get;
			private set;
		}
		public string WatermarkImagePhysicalPath
		{
			get;
			private set;
		}
		public ImageWatermarkFilter(string watermarkImagePhysicalPath, AnchorLocation anchorLocation) : this(watermarkImagePhysicalPath, anchorLocation, 0.6f)
		{
		}
		public ImageWatermarkFilter(Image watermarkImage, AnchorLocation anchorLocation) : this(watermarkImage, anchorLocation, 0.6f)
		{
		}
		public ImageWatermarkFilter(string watermarkImagePhysicalPath, AnchorLocation anchorLocation, float opacity)
		{
			this.WatermarkImagePhysicalPath = watermarkImagePhysicalPath;
			base.AnchorLocation = anchorLocation;
			base.Opacity = opacity;
		}
		public ImageWatermarkFilter(Image watermarkImage, AnchorLocation anchorLocation, float opacity)
		{
			this.WatermarkImage = watermarkImage;
			base.AnchorLocation = anchorLocation;
			base.Opacity = opacity;
		}
		public override Image Process(Image inputImage, out bool isProcessed)
		{
			if (this.WatermarkImage == null && string.IsNullOrEmpty(this.WatermarkImagePhysicalPath))
			{
				isProcessed = false;
				return inputImage;
			}
			Image image;
			if (this.WatermarkImage != null)
			{
				image = this.WatermarkImage;
			}
			else
			{
				image = Image.FromFile(this.WatermarkImagePhysicalPath);
			}
			Graphics graphics;
			Image result;
			if (WatermarkFilterBase.IsPixelFormatIndexed(inputImage.PixelFormat))
			{
				Bitmap bitmap = new Bitmap(inputImage.Width, inputImage.Height, PixelFormat.Format24bppRgb);
				graphics = Graphics.FromImage(bitmap);
				graphics.DrawImage(inputImage, 0, 0);
				result = bitmap;
			}
			else
			{
				graphics = Graphics.FromImage(inputImage);
				result = inputImage;
			}
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Rectangle watermarkArea = this.GetWatermarkArea(inputImage, image);
			ImageAttributes imageAttr = this.BuildImageAttributes();
			graphics.DrawImage(image, watermarkArea, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
			isProcessed = true;
			return result;
		}
		private Rectangle GetWatermarkArea(Image inputImage, Image watermarkImage)
		{
			Rectangle sourceRect = new Rectangle(Point.Empty, inputImage.Size);
			Rectangle result = new Rectangle(Point.Empty, watermarkImage.Size);
			RectangleUtil.PositionRectangle(base.AnchorLocation, sourceRect, ref result);
			int num = (int)((float)inputImage.Width * 0.01f);
			int num2 = (int)((float)inputImage.Height * 0.01f);
			switch (base.AnchorLocation)
			{
			case AnchorLocation.LeftTop:
				result.Offset(num, num2);
				break;
			case AnchorLocation.MiddleTop:
				result.Offset(0, num2);
				break;
			case AnchorLocation.RightTop:
				result.Offset(-num, num2);
				break;
			case AnchorLocation.LeftMiddle:
				result.Offset(num, 0);
				break;
			case AnchorLocation.RightMiddle:
				result.Offset(-num, 0);
				break;
			case AnchorLocation.LeftBottom:
				result.Offset(num, -num2);
				break;
			case AnchorLocation.MiddleBottom:
				result.Offset(0, -num2);
				break;
			case AnchorLocation.RightBottom:
				result.Offset(-num, -num2);
				break;
			}
			return result;
		}
		private ImageAttributes BuildImageAttributes()
		{
			ColorMatrix colorMatrix = new ColorMatrix();
			colorMatrix.Matrix33 = base.Opacity;
			ImageAttributes imageAttributes = new ImageAttributes();
			imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			return imageAttributes;
		}
	}
}
