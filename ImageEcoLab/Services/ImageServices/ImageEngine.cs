using ImageEcoLab.Models;
using System;
using System.Linq;

namespace ImageEcoLab.Services
{
	internal class ImageEngine
	{
		public Histograms GetHistograms(ImageModel imageModel)
		{
			var width  = imageModel.Width;
			var height = imageModel.Height;
			var pixels = imageModel.Pixels;
			
			var redChannel   = new long[256];
			var greenChannel = new long[256];
			var blueChannel  = new long[256];
			var brightnessChannel = new long[256];

			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					var pixel = pixels[y, x];

					++redChannel[pixel.Red];
					++greenChannel[pixel.Green];
					++blueChannel[pixel.Blue];
					var average = (pixels[y, x].Blue + pixels[y, x].Red + pixels[y, x].Green) / 3;
					++brightnessChannel[average];
				}
			}

			return new Histograms()
			{
				RedChannel = redChannel,
				GreenChannel = greenChannel,
				BlueChannel = blueChannel,
				BrightnessHist = brightnessChannel
			};
		}

		public long[] AlignChannelHeight(long[] channel, short alignCoef)
		{
			if (alignCoef < 1)
			{
				return channel;
			}

			var max = channel.Max();
			var alignedChannel = new long[channel.Length];
			for(int i = 0; i < channel.Length; ++i)
			{
				alignedChannel[i] = alignCoef * channel[i] / max;
			}
			return alignedChannel;
		}

		private Pixel[,] GetPixels(byte[] pixels, int width, int height)
		{
			var result = new Pixel[height, width];
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

		public ImageModel GetImageModel(byte[] pixels, int width, int height, string uri, bool isGrayScale = false)
		{
			Pixel[,] resultPixels = GetPixels(pixels, width, height);

			return new ImageModel()
			{
				Uri = uri,
				Width = width,
				Height = height,
				Pixels = resultPixels,
				Bytes = pixels
			};
		}

		public ImageModel ConvertToGrayscale(ImageModel imageModel)
		{
			var result = new ImageModel();

			result.Uri = imageModel.Uri;
			result.Width = imageModel.Width;
			result.Height = imageModel.Height;

			var pixels = imageModel.Pixels;

			var width = imageModel.Width;
			var height = imageModel.Height;

			var grayscalePixels = new Pixel[height, width];

			byte[] bytes = new byte[width * height * 4];

			var counterBytes = 0;
			for (int y = 0; y < height; ++y)
			{
				for (int x = 0;  x < width; ++x)
				{
					var average = (pixels[y, x].Blue + pixels[y, x].Red + pixels[y, x].Green) / 3;

					grayscalePixels[y, x].Blue = (byte)average;
					grayscalePixels[y, x].Green = (byte)average;
					grayscalePixels[y, x].Red = (byte)average;
					grayscalePixels[y, x].Alpha = pixels[y, x].Alpha;

					bytes[counterBytes++] = (byte)average;
					bytes[counterBytes++] = (byte)average;
					bytes[counterBytes++] = (byte)average;
					bytes[counterBytes++] = pixels[y, x].Alpha;
				}
			}

			result.Pixels = grayscalePixels;
			result.Bytes = bytes;
			return result;
		}

		public ImageModel NormalizeGrayscale(ImageModel grayscaleImage, byte upperThreshold = 254, byte lowerThreshold = 1)
		{
			var result = new ImageModel();

			result.Uri = grayscaleImage.Uri;
			var width = result.Width = grayscaleImage.Width;
			var height = result.Height = grayscaleImage.Height;

			byte maxIntensity = 0;
			byte minIntensity = 255;

			if ((upperThreshold == 0 || upperThreshold == 255) && (lowerThreshold == 0 || lowerThreshold == 255))
			{
				for (int i = 0; i < grayscaleImage.Bytes.Length; i += 4)
				{
					var pixelIntensity = grayscaleImage.Bytes[i];

					if (pixelIntensity > maxIntensity)
					{
						maxIntensity = pixelIntensity;
					}

					if (pixelIntensity < minIntensity)
					{
						minIntensity = pixelIntensity;
					}
				}
			}
			else
			{
				minIntensity = lowerThreshold;
				maxIntensity = upperThreshold;
			}
			byte[] bytes = new byte[width * height * 4];
			result.Pixels = new Pixel[height, width];
			long counterPixels = 0;
			for (int y = 0; y < grayscaleImage.Height; ++y)
			{
				for (int x = 0; x < grayscaleImage.Width; ++x)
				{
					byte inputPixel = grayscaleImage.Pixels[y, x].Red;
					byte outputPixel = inputPixel;
					if (maxIntensity != minIntensity)
					{
						outputPixel = (byte)((((double)inputPixel - (double)minIntensity) / ((double)maxIntensity - (double)minIntensity)) * 255);
					}
					byte alpha = grayscaleImage.Pixels[y, x].Alpha;

					result.Pixels[y, x].Red = outputPixel;
					result.Pixels[y, x].Blue = outputPixel;
					result.Pixels[y, x].Green = outputPixel;
					result.Pixels[y, x].Alpha = alpha;

					bytes[counterPixels++] = outputPixel;
					bytes[counterPixels++] = outputPixel;
					bytes[counterPixels++] = outputPixel;
					bytes[counterPixels++] = alpha;
				}
			}
			result.Bytes = bytes;
			return result;
		}

		public ImageModel EqualizeGrayscale(ImageModel imageModel)
		{
			if (imageModel == null)
			{
				return imageModel;
			}

			var inHistograms = GetHistograms(imageModel);
			var hist = new double[256];
			var inHist = inHistograms.RedChannel;

			var newImageModel = new ImageModel();

			newImageModel.Width = imageModel.Width;
			newImageModel.Height = imageModel.Height;
			newImageModel.Uri = imageModel.Uri;

			for (int i = 0; i < hist.Length; ++i)
			{
				hist[i] = inHist[i] / 256;
			}

			for (int i = 1; i < hist.Length; ++i)
			{
				hist[i] = hist[i - 1] + hist[i];
			}

			var max = hist.Max();
			for (int i = 0; i < hist.Length; ++i)
			{
				hist[i] = 250 * hist[i] / max;
			}

			var height = newImageModel.Height;
			var width = newImageModel.Width;

			var pixels = new Pixel[height, width];
			byte[] bytes = new byte[width * height * 4];
			long counterBytes = 0;
			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					var color = (int)Math.Round(hist[imageModel.Pixels[y, x].Red]);
					if (color > 255) color = 255;
					var alpha = imageModel.Pixels[y, x].Alpha;
					pixels[y, x].Red = (byte)color;
					pixels[y, x].Green = (byte)color;
					pixels[y, x].Blue = (byte)color;
					pixels[y, x].Alpha = alpha;

					bytes[counterBytes++] = (byte)color;
					bytes[counterBytes++] = (byte)color;
					bytes[counterBytes++] = (byte)color;
					bytes[counterBytes++] = alpha;
				}
			}
			newImageModel.Pixels = pixels;
			newImageModel.Bytes = bytes;
			return newImageModel;
		}
	}
}
