using ImageEcoLab.Models;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Effects;
using ImageEcoLab.Services.Base;

namespace ImageEcoLab.Services
{
	internal class ConverterService : IConverterService
	{
		public BitmapSource Convert(BitmapSource source, PixelFormat pixelFormat)
		{
			if (source.Format != pixelFormat)
			{
				source = new FormatConvertedBitmap(source, pixelFormat, null, 0);
			}
			return source;
		}
	}
}
