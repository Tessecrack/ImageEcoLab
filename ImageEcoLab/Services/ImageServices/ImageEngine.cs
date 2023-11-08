using ImageEcoLab.Models;
using System;
using System.Linq;

namespace ImageEcoLab.Services
{
	internal class ImageEngine
	{
		public Histograms GetHistograms(byte[] pixels, byte bitsPerPixel)
		{
			var redChannel   = new long[256];
			var greenChannel = new long[256];
			var blueChannel  = new long[256];
			var brightnessChannel = new long[256];

			for (int i = 0; i < pixels.Length; i += bitsPerPixel)
			{
				var blue = pixels[i];
				var green = pixels[i + 1];
				var red = pixels[i + 2];
				var average = (blue + green + red) / 3;

				++redChannel[red];
				++greenChannel[green];
				++blueChannel[blue];
				++brightnessChannel[average];
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

		public ImageModel GetImageModel(byte[] pixels, int width, int height, string uri, byte bitsPerPixel)
		{
			return new ImageModel()
			{
				Uri = uri,
				Width = width,
				Height = height,
				Bytes = pixels,
				BitsPerPixel = bitsPerPixel
			};
		}

		public ImageModel ConvertToGrayscale(ImageModel imageModel)
		{
			var result = new ImageModel();

			result.Uri = imageModel.Uri;
			result.Width = imageModel.Width;
			result.Height = imageModel.Height;

			var bitsPerPixel = imageModel.BitsPerPixel;
			result.BitsPerPixel = bitsPerPixel;

			byte[] bytes = imageModel.Bytes;
			byte[] grayscaleBytes = new byte[bytes.Length];

			for (int i = 0; i < bytes.Length; i += bitsPerPixel)
			{
				var average = (bytes[i] + bytes[i + 1] + bytes[i + 2]) / 3;

				grayscaleBytes[i] = (byte)average;
				grayscaleBytes[i + 1] = (byte)average;
				grayscaleBytes[i + 2] = (byte)average;

				if (bitsPerPixel == 4)
				{
					grayscaleBytes[i + 3] = bytes[i + 3];
				}
			}
			
			result.Bytes = grayscaleBytes;
			return result;
		}

		public ImageModel NormalizeGrayscale(ImageModel grayscaleImage, byte upperThreshold = 254, byte lowerThreshold = 1)
		{
			var result = new ImageModel();

			result.Uri = grayscaleImage.Uri;
			result.Width = grayscaleImage.Width;
			result.Height = grayscaleImage.Height;

			byte maxIntensity = 0;
			byte minIntensity = 255;

			var bitsPerPixel = grayscaleImage.BitsPerPixel;

			if ((upperThreshold == 0 || upperThreshold == 255) && (lowerThreshold == 0 || lowerThreshold == 255))
			{
				for (int i = 0; i < grayscaleImage.Bytes.Length; i += bitsPerPixel)
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
			byte[] grayscaleBytes = grayscaleImage.Bytes;
			byte[] normalizeBytes = new byte[grayscaleBytes.Length];
			for (int i = 0; i < grayscaleBytes.Length; i += bitsPerPixel)
			{
				byte inputPixel = grayscaleBytes[i];
				byte outputPixel = inputPixel;
				if (maxIntensity != minIntensity)
				{
					outputPixel = (byte)((((double)inputPixel - (double)minIntensity) / ((double)maxIntensity - (double)minIntensity)) * 255);
				}

				normalizeBytes[i] = outputPixel;
				normalizeBytes[i + 1] = outputPixel;
				normalizeBytes[i + 2] = outputPixel;
				if (bitsPerPixel == 4)
				{
					normalizeBytes[i + 3] = grayscaleBytes[i + 3];
				}
			}
			result.Bytes = normalizeBytes;
			result.BitsPerPixel = bitsPerPixel;
			return result;
		}

		public ImageModel EqualizeGrayscale(ImageModel imageModel)
		{
			if (imageModel == null)
			{
				return imageModel;
			}

			var inHistograms = GetHistograms(imageModel.Bytes, imageModel.BitsPerPixel);
			var hist = new double[256];
			var inHist = inHistograms.RedChannel;

			var newImageModel = new ImageModel();

			newImageModel.Width = imageModel.Width;
			newImageModel.Height = imageModel.Height;
			newImageModel.Uri = imageModel.Uri;

			var bitsPerPixel = imageModel.BitsPerPixel;
			newImageModel.BitsPerPixel = bitsPerPixel;

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
			byte[] sourcePixels = imageModel.Bytes;
			byte[] equalizeBytes = new byte[sourcePixels.Length];
			for (int i = 0; i < equalizeBytes.Length; i += bitsPerPixel)
			{
				var color = (int)Math.Round(hist[sourcePixels[i]]);
				if (color > 255) color = 255;
				equalizeBytes[i] = (byte)color;
				equalizeBytes[i + 1] = (byte)color;
				equalizeBytes[i + 2] = (byte)color;

				if (bitsPerPixel == 4)
				{
					equalizeBytes[i + 3] = sourcePixels[i + 3];
				}
			}
			newImageModel.Bytes = equalizeBytes;
			return newImageModel;
		}
	}
}
