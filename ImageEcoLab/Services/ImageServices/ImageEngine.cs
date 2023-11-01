using ImageEcoLab.Models;
using System.Linq;

namespace ImageEcoLab.Services
{
	internal class ImageEngine
	{
		public HistogramsRGB GetHistograms(ImageModel imageModel)
		{
			var width  = imageModel.Width;
			var height = imageModel.Height;
			var pixels = imageModel.Pixels;

			var redChannel   = new long[256];
			var greenChannel = new long[256];
			var blueChannel  = new long[256];

			for (int y = 0; y < height; ++y)
			{
				for (int x = 0; x < width; ++x)
				{
					var pixel = pixels[y, x];

					++redChannel[pixel.Red];
					++greenChannel[pixel.Green];
					++blueChannel[pixel.Blue];
				}
			}

			return new HistogramsRGB()
			{
				RedChannel = redChannel,
				GreenChannel = greenChannel,
				BlueChannel = blueChannel
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
	}
}
