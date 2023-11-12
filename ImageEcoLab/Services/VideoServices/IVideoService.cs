namespace ImageEcoLab.Services.VideoServices
{
	internal interface IVideoService
    {
        byte[] GetFrame(out int width, out int height);
    }
}
