namespace ImageEcoLab.Models
{
	internal class ImageModel
	{
		public string Uri { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public byte[] Bytes { get; set; }
		public byte BitsPerPixel { get; set; }
	}
}
