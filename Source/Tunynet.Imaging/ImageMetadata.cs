using System;
using System.IO;
namespace Tunynet.Imaging
{
	public class ImageMetadata
	{
		private System.IO.Stream stream;
		private int width;
		private int height;
		private int bitsPerPixel;
		private int colorType = ImageMetadata.COLOR_TYPE_UNKNOWN;
		private bool progressive;
		private int format;
		private bool determineNumberOfImages;
		private int numberOfImageFrames;
		private int physicalHeightDpi;
		private int physicalWidthDpi;
		private static readonly int FORMAT_JPEG = 0;
		private static readonly int FORMAT_GIF = 1;
		private static readonly int FORMAT_PNG = 2;
		private static readonly int FORMAT_BMP = 3;
		private static readonly int COLOR_TYPE_UNKNOWN = -1;
		private static readonly string[] FORMAT_NAMES = new string[]
		{
			"JPEG",
			"GIF",
			"PNG",
			"BMP"
		};
		private static readonly string[] MIME_TYPE_STRINGS = new string[]
		{
			"image/jpeg",
			"image/gif",
			"image/png",
			"image/bmp"
		};
		public System.IO.Stream Stream
		{
			get
			{
				return this.stream;
			}
			set
			{
				this.stream = value;
			}
		}
		public int Width
		{
			get
			{
				return this.width;
			}
		}
		public int Height
		{
			get
			{
				return this.height;
			}
		}
		public int BitsPerPixel
		{
			get
			{
				return this.bitsPerPixel;
			}
		}
		public int Format
		{
			get
			{
				return this.format;
			}
		}
		public int ColorType
		{
			get
			{
				return this.colorType;
			}
		}
		public int NumberOfImageFrames
		{
			get
			{
				return this.numberOfImageFrames;
			}
		}
		public string FormatName
		{
			get
			{
				if (this.format >= 0 && this.format < ImageMetadata.FORMAT_NAMES.Length)
				{
					return ImageMetadata.FORMAT_NAMES[this.format];
				}
				return "?";
			}
		}
		public string MimeType
		{
			get
			{
				if (this.format < 0 || this.format >= ImageMetadata.MIME_TYPE_STRINGS.Length)
				{
					return null;
				}
				if (this.format == ImageMetadata.FORMAT_JPEG && this.progressive)
				{
					return "image/jpeg";
				}
				return ImageMetadata.MIME_TYPE_STRINGS[this.format];
			}
		}
		public ImageMetadata(System.IO.Stream stream)
		{
			this.stream = stream;
			this.stream.Seek(0L, System.IO.SeekOrigin.Begin);
		}
		public static bool Check(System.IO.Stream inputStream, out string contentType, out int width, out int height)
		{
			ImageMetadata imageMetadata = new ImageMetadata(inputStream);
			if (imageMetadata.Check())
			{
				contentType = imageMetadata.MimeType;
				width = imageMetadata.Width;
				height = imageMetadata.Height;
				return true;
			}
			contentType = string.Empty;
			width = 0;
			height = 0;
			return false;
		}
		public bool Check()
		{
			if (this.stream == null)
			{
				return false;
			}
			this.format = -1;
			this.width = -1;
			this.height = -1;
			this.bitsPerPixel = -1;
			this.numberOfImageFrames = 1;
			this.physicalHeightDpi = -1;
			this.physicalWidthDpi = -1;
			this.stream.Position = 0L;
			bool result;
			try
			{
				int num = this.stream.ReadByte() & 255;
				int num2 = this.stream.ReadByte() & 255;
				if (num == 71 && num2 == 73)
				{
					result = this.CheckGif();
				}
				else
				{
					if (num == 137 && num2 == 80)
					{
						result = this.CheckPng();
					}
					else
					{
						if (num == 255 && num2 == 216)
						{
							result = this.CheckJpeg();
						}
						else
						{
							if (num == 66 && num2 == 77)
							{
								result = this.CheckBmp();
							}
							else
							{
								result = false;
							}
						}
					}
				}
			}
			catch (System.IO.IOException)
			{
				result = false;
			}
			return result;
		}
		private bool CheckBmp()
		{
			byte[] array = new byte[44];
			if (this.stream.Read(array, 0, 44) != 44)
			{
				return false;
			}
			this.width = this.getIntLittleEndian(array, 16);
			this.height = this.getIntLittleEndian(array, 20);
			if (this.width < 1 || this.height < 1)
			{
				return false;
			}
			this.bitsPerPixel = this.getShortLittleEndian(array, 26);
			if (this.bitsPerPixel != 1 && this.bitsPerPixel != 4 && this.bitsPerPixel != 8 && this.bitsPerPixel != 16 && this.bitsPerPixel != 24 && this.bitsPerPixel != 32)
			{
				return false;
			}
			int num = (int)((double)this.getIntLittleEndian(array, 36) * 0.0254);
			if (num > 0)
			{
				this.physicalWidthDpi = num;
			}
			int num2 = (int)((double)this.getIntLittleEndian(array, 40) * 0.0254);
			if (num2 > 0)
			{
				this.physicalHeightDpi = num2;
			}
			this.format = ImageMetadata.FORMAT_BMP;
			return true;
		}
		private bool CheckGif()
		{
			byte[] a = new byte[]
			{
				70,
				56,
				55,
				97
			};
			byte[] a2 = new byte[]
			{
				70,
				56,
				57,
				97
			};
			byte[] array = new byte[11];
			if (this.stream.Read(array, 0, 11) != 11)
			{
				return false;
			}
			if (!this.equals(array, 0, a2, 0, 4) && !this.equals(array, 0, a, 0, 4))
			{
				return false;
			}
			this.format = ImageMetadata.FORMAT_GIF;
			this.width = this.getShortLittleEndian(array, 4);
			this.height = this.getShortLittleEndian(array, 6);
			int num = (int)(array[8] & 255);
			this.bitsPerPixel = (num >> 4 & 7) + 1;
			this.progressive = ((num & 2) != 0);
			if (!this.determineNumberOfImages)
			{
				return true;
			}
			if ((num & 128) != 0)
			{
				int num2 = (1 << (num & 7) + 1) * 3;
				this.stream.Position += (long)num2;
			}
			this.numberOfImageFrames = 0;
			while (true)
			{
				int num3 = this.stream.ReadByte();
				int num4 = num3;
				if (num4 != 44)
				{
					if (num4 != 59)
					{
						break;
					}
				}
				else
				{
					if (this.stream.Read(array, 0, 9) != 9)
					{
						return false;
					}
					num = (int)(array[8] & 255);
					int num5 = (num & 7) + 1;
					if (num5 > this.bitsPerPixel)
					{
						this.bitsPerPixel = num5;
					}
					if ((num & 128) != 0)
					{
						this.stream.Position += (long)((1 << num5) * 3);
					}
					this.stream.Position += 1L;
					int num6;
					do
					{
						num6 = this.stream.ReadByte();
						if (num6 > 0)
						{
							this.stream.Position += (long)num6;
						}
						else
						{
							if (num6 == -1)
							{
								return false;
							}
						}
					}
					while (num6 > 0);
					this.numberOfImageFrames++;
				}
				if (num3 == 59)
				{
					return true;
				}
			}
			return false;
		}
		private bool CheckJpeg()
		{
			byte[] array = new byte[12];
			while (this.stream.Read(array, 0, 4) == 4)
			{
				int shortBigEndian = this.getShortBigEndian(array, 0);
				int shortBigEndian2 = this.getShortBigEndian(array, 2);
				if ((shortBigEndian & 65280) != 65280)
				{
					return false;
				}
				if (shortBigEndian == 65504)
				{
					if (shortBigEndian2 < 14 && shortBigEndian2 != 8)
					{
						return false;
					}
					if (this.stream.Read(array, 0, 12) != 12)
					{
						return false;
					}
					byte[] a = new byte[]
					{
						74,
						70,
						73,
						70,
						0
					};
					if (this.equals(a, 0, array, 0, 5))
					{
						if (array[7] == 1)
						{
							this.physicalWidthDpi = this.getShortBigEndian(array, 8);
							this.physicalHeightDpi = this.getShortBigEndian(array, 10);
						}
						else
						{
							if (array[7] == 2)
							{
								int shortBigEndian3 = this.getShortBigEndian(array, 8);
								int shortBigEndian4 = this.getShortBigEndian(array, 10);
								this.physicalWidthDpi = (int)((float)shortBigEndian3 * 2.54f);
								this.physicalHeightDpi = (int)((float)shortBigEndian4 * 2.54f);
							}
						}
					}
					this.stream.Position += (long)(shortBigEndian2 - 14);
				}
				else
				{
					if (shortBigEndian >= 65472 && shortBigEndian <= 65487 && shortBigEndian != 65476 && shortBigEndian != 65480)
					{
						if (this.stream.Read(array, 0, 6) != 6)
						{
							return false;
						}
						this.format = ImageMetadata.FORMAT_JPEG;
						this.bitsPerPixel = (int)((array[0] & 255) * (array[5] & 255));
						this.progressive = (shortBigEndian == 65474 || shortBigEndian == 65478 || shortBigEndian == 65482 || shortBigEndian == 65486);
						this.width = this.getShortBigEndian(array, 3);
						this.height = this.getShortBigEndian(array, 1);
						return true;
					}
					else
					{
						this.stream.Position += (long)(shortBigEndian2 - 2);
					}
				}
			}
			return false;
		}
		private bool CheckPng()
		{
			byte[] a = new byte[]
			{
				78,
				71,
				13,
				10,
				26,
				10
			};
			byte[] array = new byte[27];
			if (this.stream.Read(array, 0, 27) != 27)
			{
				return false;
			}
			if (!this.equals(array, 0, a, 0, 6))
			{
				return false;
			}
			this.format = ImageMetadata.FORMAT_PNG;
			this.width = this.getIntBigEndian(array, 14);
			this.height = this.getIntBigEndian(array, 18);
			this.bitsPerPixel = (int)(array[22] & 255);
			int num = (int)(array[23] & 255);
			if (num == 2 || num == 6)
			{
				this.bitsPerPixel *= 3;
			}
			this.progressive = ((array[26] & 255) != 0);
			return true;
		}
		private bool equals(byte[] a1, int offs1, byte[] a2, int offs2, int num)
		{
			while (num-- > 0)
			{
				if (a1[offs1++] != a2[offs2++])
				{
					return false;
				}
			}
			return true;
		}
		private int getIntBigEndian(byte[] a, int offs)
		{
			return (int)(a[offs] & 255) << 24 | (int)(a[offs + 1] & 255) << 16 | (int)(a[offs + 2] & 255) << 8 | (int)(a[offs + 3] & 255);
		}
		private int getIntLittleEndian(byte[] a, int offs)
		{
			return (int)(a[offs + 3] & 255) << 24 | (int)(a[offs + 2] & 255) << 16 | (int)(a[offs + 1] & 255) << 8 | (int)(a[offs] & 255);
		}
		private int getShortBigEndian(byte[] a, int offs)
		{
			return (int)(a[offs] & 255) << 8 | (int)(a[offs + 1] & 255);
		}
		private int getShortLittleEndian(byte[] a, int offs)
		{
			return (int)(a[offs] & 255) | (int)(a[offs + 1] & 255) << 8;
		}
	}
}
