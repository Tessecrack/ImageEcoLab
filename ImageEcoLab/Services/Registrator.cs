using ImageEcoLab.Services.Base;
using ImageEcoLab.Services.VideoServices;
using Microsoft.Extensions.DependencyInjection;

namespace ImageEcoLab.Services
{
	internal static class Registrator
    {
		public static IServiceCollection RegisterServices(this IServiceCollection services)
		{
			services.AddSingleton<IDataService, WinFilePickerService>();
			services.AddSingleton<IConverterService, ConverterService>();
			services.AddSingleton<ImageEngine>();
			services.AddSingleton<WebcamStreaming>();

			return services;
		}
	}
}
