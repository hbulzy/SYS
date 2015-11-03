using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
namespace Tunynet.Imaging
{
	public class TextWatermarkFilter : WatermarkFilterBase
	{
		public string Text
		{
			get;
			set;
		}
		public TextWatermarkFilter(string text, AnchorLocation anchorLocation) : this(text, anchorLocation, 0.6f)
		{
		}
		public TextWatermarkFilter(string text, AnchorLocation anchorLocation, float opacity)
		{
			this.Text = text;
			base.AnchorLocation = anchorLocation;
			base.Opacity = opacity;
		}
		public override Image Process(Image inputImage, out bool isProcessed)
		{
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
			graphics.InterpolationMode = InterpolationMode.High;
			Font font;
			Rectangle watermarkArea = this.GetWatermarkArea(graphics, inputImage, out font);
			StringFormat format = new StringFormat();
			int alpha = System.Convert.ToInt32(256f * base.Opacity);
			SolidBrush solidBrush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
			graphics.DrawString(this.Text, font, solidBrush, (float)watermarkArea.X + 1f, (float)watermarkArea.Y + 1f, format);
			SolidBrush solidBrush2 = new SolidBrush(Color.FromArgb(153, 255, 255, 255));
			graphics.DrawString(this.Text, font, solidBrush2, (float)watermarkArea.X, (float)watermarkArea.Y, format);
			solidBrush.Dispose();
			solidBrush2.Dispose();
			isProcessed = true;
			return result;
		}
		protected virtual Rectangle GetWatermarkArea(Graphics graphics, Image inputImage, out Font watermarkFont)
		{
			int[] array = new int[]
			{
				16,
				14,
				12,
				10,
				8,
				6,
				4,
				3,
				2,
				1
			};
			watermarkFont = null;
			Size size = Size.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				watermarkFont = new Font("arial", (float)array[i], FontStyle.Bold);
				size = graphics.MeasureString(this.Text, watermarkFont).ToSize();
				if ((double)size.Width < (double)inputImage.Width * 0.8)
				{
					break;
				}
			}
			Rectangle sourceRect = new Rectangle(Point.Empty, inputImage.Size);
			Rectangle result = new Rectangle(Point.Empty, size);
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
	}
}
