using System.Windows.Media.Imaging;

namespace ImageEcoLab.Models
{
	internal class ImageModel
	{
		public BitmapSource? SourceImage { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public Pixel[,]? Pixels 
		{ 
			get; 
			set; 
		}
	}
}
