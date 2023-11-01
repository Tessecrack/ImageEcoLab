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

		private Pixel[,] GetPixels(BitmapSource source)
		{
			var convertedSource = ConvertToBgra32(source);

			int width = convertedSource.PixelWidth;
			int height = convertedSource.PixelHeight;

			byte[] pixels = new byte[width * height * 4];

			var result = new Pixel[height, width];

			convertedSource.CopyPixels(pixels, width * 4, 0);
			for (int i = 0, j = 0; i < pixels.Length; i += 4, ++j)
			{
				var blueChannel = pixels[i];
				var greenChannel = pixels[i + 1];
				var redChannel = pixels[i + 2];
				var alphaChannel = pixels[i + 3];

				var indexRow = j / width;
				var indexColumn = j % width;
				result[indexRow, indexColumn] = new Pixel()
				{
					Blue = blueChannel,
					Green = greenChannel,
					Red = redChannel,
					Alpha = alphaChannel
				};
			}

			return result;
		}

		public ImageModel GetImageModel(BitmapSource source)
		{
			var pixels = GetPixels(source);
			var height = pixels.GetLength(0);
			var width = pixels.GetLength(1);

			WriteableBitmap bitmap = new WriteableBitmap(width, height, width, height, PixelFormats.Bgra32, null);

			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					var pixel = pixels[y, x];

					byte[] colorData = { pixel.Blue, pixel.Green, pixel.Red, pixel.Alpha };

					Int32Rect rect = new Int32Rect(x, y, 1, 1);

					bitmap.WritePixels(rect, colorData, 4, 0);
				}
			}

			return new ImageModel()
			{
				Width = width,
				Height = height,
				Pixels = pixels,
				SourceImage = bitmap
			};
		}
	}
}
