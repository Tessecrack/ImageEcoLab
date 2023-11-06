using ImageEcoLab.Services;
using ImageEcoLab.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImageEcoLab
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static bool IsDesignMode { get; private set; } = true;

		private static IHost _host;
		public static IHost Host => _host ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

		protected override async void OnStartup(StartupEventArgs e)
		{
			IsDesignMode = false;
			var host = Host;
			base.OnStartup(e);

			await host.StartAsync().ConfigureAwait(false);
		}

		protected override async void OnExit(ExitEventArgs e)
		{
			var host = Host;
			base.OnExit(e);

			await host.StopAsync().ConfigureAwait(false);
			host.Dispose();
			_host = null;
		}

		internal static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
		{
			services.RegisterServices();
			services.RegisterViewModels();
		}

		public static string CurrentDirectory => IsDesignMode 
			? Path.GetDirectoryName(GetSourcePath()) 
			: Environment.CurrentDirectory;

		private static string GetSourcePath([CallerFilePath] string path = null) => path;
	}
}
