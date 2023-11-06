using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEcoLab.Services.Base
{
	interface IConverterService
    {
        BitmapSource Convert(BitmapSource source, PixelFormat pixelFormat);
    }
}
