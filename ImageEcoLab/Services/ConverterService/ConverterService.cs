using ImageEcoLab.Models;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Effects;

namespace ImageEcoLab.Services
{
	internal class ConverterService
	{
		public BitmapSource ConvertToBgra32(BitmapSource source)
		{
			if (source.Format != PixelFormats.Bgra32)
			{
				source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
			}
			return source;
		}
	}
}
