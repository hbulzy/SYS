using System;
using System.Drawing;
namespace Tunynet.Imaging
{
	internal sealed class RectangleUtil
	{
		public static void PositionRectangle(AnchorLocation anchorLocation, Rectangle sourceRect, ref Rectangle destRect)
		{
			switch (anchorLocation)
			{
			case AnchorLocation.LeftTop:
				destRect.X = (destRect.Y = 0);
				return;
			case AnchorLocation.MiddleTop:
				destRect.X = (sourceRect.Width - destRect.Width) / 2;
				destRect.Y = 0;
				return;
			case AnchorLocation.RightTop:
				destRect.X = sourceRect.Width - destRect.Width;
				destRect.Y = 0;
				return;
			case AnchorLocation.LeftMiddle:
				destRect.X = 0;
				destRect.Y = (sourceRect.Height - destRect.Height) / 2;
				return;
			case AnchorLocation.Middle:
				destRect.X = (sourceRect.Width - destRect.Width) / 2;
				destRect.Y = (sourceRect.Height - destRect.Height) / 2;
				return;
			case AnchorLocation.RightMiddle:
				destRect.X = sourceRect.Width - destRect.Width;
				destRect.Y = (sourceRect.Height - destRect.Height) / 2;
				return;
			case AnchorLocation.LeftBottom:
				destRect.X = 0;
				destRect.Y = sourceRect.Height - destRect.Height;
				return;
			case AnchorLocation.MiddleBottom:
				destRect.X = (sourceRect.Width - destRect.Width) / 2;
				destRect.Y = sourceRect.Height - destRect.Height;
				return;
			case AnchorLocation.RightBottom:
				destRect.X = sourceRect.Width - destRect.Width;
				destRect.Y = sourceRect.Height - destRect.Height;
				return;
			default:
				return;
			}
		}
	}
}
