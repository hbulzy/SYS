using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace Tunynet.Imaging
{
	public class ImageProcessor
	{
		private System.Collections.Generic.List<IImageFilter> _filters;
		private int jpegQuality = 92;
		public System.Collections.Generic.List<IImageFilter> Filters
		{
			get
			{
				if (this._filters == null)
				{
					this._filters = new System.Collections.Generic.List<IImageFilter>();
				}
				return this._filters;
			}
		}
		public int JpegQuality
		{
			get
			{
				return this.jpegQuality;
			}
			set
			{
				if (value > 0 && value <= 100)
				{
					this.jpegQuality = value;
				}
			}
		}
		public System.IO.Stream Process(System.IO.Stream inputStream)
		{
			if (inputStream == null || !inputStream.CanRead)
			{
				throw new System.ArgumentException("inputStream isn't validate", "inputStream");
			}
			inputStream.Seek(0L, System.IO.SeekOrigin.Begin);
			bool flag = false;
			Image image = Image.FromStream(inputStream);
			ImageFormat rawFormat = image.RawFormat;
			if (ImageProcessor.IsGIFAnimation(image))
			{
				inputStream.Seek(0L, System.IO.SeekOrigin.Begin);
				return inputStream;
			}
			foreach (IImageFilter current in this.Filters)
			{
				bool flag2;
				image = current.Process(image, out flag2);
				if (flag2)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				inputStream.Seek(0L, System.IO.SeekOrigin.Begin);
				return inputStream;
			}
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
			if (rawFormat.Guid == ImageFormat.Gif.Guid)
			{
				image.Save(memoryStream, ImageFormat.Jpeg);
			}
			else
			{
				EncoderParameters encoderParameters = new EncoderParameters(1);
				encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)this.JpegQuality);
				ImageCodecInfo imageCodecInfo = ImageProcessor.GetImageCodecInfo(rawFormat);
				image.Save(memoryStream, imageCodecInfo, encoderParameters);
				encoderParameters.Dispose();
			}
			memoryStream.Seek(0L, System.IO.SeekOrigin.Begin);
			return memoryStream;
		}
		public static System.IO.Stream Resize(System.IO.Stream inputStream, int width, int height, ResizeMethod resizeMethod)
		{
			ImageProcessor imageProcessor = new ImageProcessor();
			ResizeFilter item = new ResizeFilter(width, height, resizeMethod);
			imageProcessor.Filters.Add(item);
			return imageProcessor.Process(inputStream);
		}
		public static System.IO.Stream Crop(System.IO.Stream inputStream, Rectangle cropArea, int descWidth, int descHeight)
		{
			ImageProcessor imageProcessor = new ImageProcessor();
			CropFilter item = new CropFilter(cropArea, descWidth, descHeight);
			imageProcessor.Filters.Add(item);
			return imageProcessor.Process(inputStream);
		}
		private static ImageCodecInfo GetImageCodecInfo(ImageFormat imageFormat)
		{
			ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
			ImageCodecInfo[] array = imageEncoders;
			for (int i = 0; i < array.Length; i++)
			{
				ImageCodecInfo imageCodecInfo = array[i];
				if (imageCodecInfo.FormatID == imageFormat.Guid)
				{
					return imageCodecInfo;
				}
			}
			return null;
		}
		public static bool IsGIFAnimation(Image image)
		{
			System.Guid[] frameDimensionsList = image.FrameDimensionsList;
			for (int i = 0; i < frameDimensionsList.Length; i++)
			{
				System.Guid guid = frameDimensionsList[i];
				FrameDimension dimension = new FrameDimension(guid);
				if (image.GetFrameCount(dimension) > 1)
				{
					return true;
				}
			}
			return false;
		}
	}
}
