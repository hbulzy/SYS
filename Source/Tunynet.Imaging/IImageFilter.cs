using System;
using System.Drawing;
namespace Tunynet.Imaging
{
	public interface IImageFilter
	{
		Image Process(Image inputImage, out bool isProcessed);
	}
}
