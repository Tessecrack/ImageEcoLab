using Microsoft.Extensions.DependencyInjection;

namespace ImageEcoLab.ViewModels
{
	internal static class Registrator
    {
        public static IServiceCollection RegisterViewModels(this IServiceCollection services)
        {
			services.AddSingleton<HistogramViewModel>();
			services.AddSingleton<MainWindowViewModel>();

            return services;
        }
    }
}
