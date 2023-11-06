using Microsoft.Extensions.DependencyInjection;

namespace ImageEcoLab.ViewModels
{
	internal class ViewModelLocator
    {
        public MainWindowViewModel MainWindowViewModel => App.Host.Services.GetRequiredService<MainWindowViewModel>();
		public HistogramViewModel HistogramViewModel => App.Host.Services.GetRequiredService<HistogramViewModel>();
	}
}
