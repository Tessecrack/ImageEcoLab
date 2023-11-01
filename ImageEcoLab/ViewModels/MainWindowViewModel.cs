using ImageEcoLab.Infrastructure.Commands;
using ImageEcoLab.Models;
using ImageEcoLab.Services;
using ImageEcoLab.Services.Base;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageEcoLab.ViewModels
{
	[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
	internal class MainWindowViewModel : ViewModel
	{
		private readonly IDataService _dataService;

		private readonly ImageEngine _imageEngine = new ImageEngine();

		private ImageModel _currentImageModel;

		private ConverterService _converterService = new ConverterService();

		#region ProgressBarValue
		private int _progressBarValue = 0;
		public int ProgressBarValue { get => _progressBarValue; set => Set(ref _progressBarValue, value); }
		#endregion

		#region Property AligmentCoefficientHist
		private short _aligmentCoefHist = 0;
		public short AligmentCoefHist
		{
			get => _aligmentCoefHist;
			set
			{
				if (!Set(ref _aligmentCoefHist, value))
				{
					return;
				}
				try
				{
					Task.Run(() => DrawHistogramsCommand.Execute(this));
				}
				catch
				{
					return;
				}
			}
		}
		#endregion

		#region Property IsHistogramGraph
		private bool _isHistogramGraph = false;
		public bool IsHistogramGraph { get => _isHistogramGraph; set => Set(ref _isHistogramGraph, value); }
		#endregion

		#region Property IsLinearGraph
		private bool _isLinearGraph = true;
		public bool IsLinearGraph { get => _isLinearGraph; set => Set(ref _isLinearGraph, value); }
		#endregion

		#region Property BitmapImageSource
		private BitmapSource _bitmapImageSource = new WriteableBitmap(1, 1, 1, 1, PixelFormats.Bgra32, null);
		public BitmapSource BitmapImageSource
		{
			get => _bitmapImageSource;
			set => Set(ref _bitmapImageSource, value);
		}
		#endregion

		#region Property Path Image
		private string _pathCurrentImage;
		public string PathCurrentImage
		{
			get => _pathCurrentImage;
			set => Set(ref _pathCurrentImage, value);
		}
		#endregion

		#region Property CurrentStatusStr - status bar
		private string _currentStatusStr = "Welcome to ImageEcoLab";
		public string CurrentStatusStr
		{
			get => _currentStatusStr;
			set => Set(ref _currentStatusStr, value);
		}
		#endregion

		#region Property RedChannelHist
		private PlotModel _redChannelHist = new PlotModel();
		public PlotModel RedChannelHist { get => _redChannelHist; set => Set(ref _redChannelHist, value); }
		#endregion

		#region Property GreenChannelHist
		private PlotModel _greenChannelHist = new PlotModel();
		public PlotModel GreenChannelHist { get => _greenChannelHist; set => Set(ref _greenChannelHist, value); }
		#endregion

		#region Property BlueChannelHist
		private PlotModel _blueChannelHist = new PlotModel();
		public PlotModel BlueChannelHist { get => _blueChannelHist; set => Set(ref _blueChannelHist, value); }
		#endregion

		#region CloseApplicationCommand
		public ICommand CloseApplicationCommand { get; set; }
		private bool CanCloseAppliationCommandExecute(object parameter) => true;
		private void OnCloseApplicationCommandExecuted(object parameter)
		{
			Application.Current.Shutdown();
		}
		#endregion

		#region DownloadImageCommand
		public ICommand DownloadImageCommand { get; set; }
		private bool CanDownloadImageCommandExecute(object parameter) => true;
		private void OnDownloadImageCommandExecuted(object parameter)
		{
			var uri = _dataService.GetUri();

			if (uri == null)
			{
				return;
			}

			_isUploadedImage = false;
			Task.Run(() => StartProgressBar());
			Task.Run(() => UploadImage(uri));

			PathCurrentImage = uri;
			CurrentStatusStr = PathCurrentImage;
		}
		private bool _isUploadedImage; // TODO: NEED TO IMPROVE!!!!
		private void UploadImage(string uri)
		{
			var bitmapSource = new BitmapImage(new Uri(uri));
			_currentImageModel = _converterService.GetImageModel(bitmapSource);
			BitmapImageSource = _currentImageModel.SourceImage;
			BitmapImageSource.Freeze();
			StopProgressBar();
		}

		private void StartProgressBar()
		{
			while(!_isUploadedImage)
			{
				ProgressBarValue += 1;
				Thread.Sleep(50);
			}
		}

		private void StopProgressBar()
		{
			_isUploadedImage = true;
			ProgressBarValue = 0;
		}

		#endregion

		#region SaveImageCommand
		public ICommand SaveImageCommand { get; set; }

		private bool CanSaveImageCommandExecute(object parameter) => true;

		private void OnSaveImageCommandExecuted(object parameter)
		{

		}
		#endregion

		#region DrawHistogramsCommand
		public ICommand DrawHistogramsCommand { get; set; }
		private void OnDrawHistogramsCommandExecuted(object obj)
		{
			if (_currentImageModel == null)
			{
				return;
			}
			var bitmap = _currentImageModel.SourceImage;
			if (bitmap == null)
			{
				return;
			}
			var histograms = _imageEngine.GetHistograms(_currentImageModel);

			var redChannel = histograms.RedChannel;
			var greenChannel = histograms.GreenChannel;
			var blueChannel = histograms.BlueChannel;


			RedChannelHist = CreatePlotModel(redChannel, OxyColor.FromRgb(255, 0, 0));
			GreenChannelHist = CreatePlotModel(greenChannel, OxyColor.FromRgb(0, 255, 0));
			BlueChannelHist = CreatePlotModel(blueChannel, OxyColor.FromRgb(0, 0, 255));
		}

		private LineSeries CreateLine(long[] data, OxyColor color)
		{
			var line = new LineSeries();

			line.Color = color;

			for (int i = 0; i < data.Length; ++i)
			{
				line.Points.Add(new DataPoint(i, data[i]));
			}

			return line;
		}

		private HistogramSeries CreateHistogram(long[] data, OxyColor color)
		{
			var series = new HistogramSeries()
			{
				FillColor = color
			};

			for (int i = 0; i < data.Length; ++i)
			{
				var item = new HistogramItem(i, i + 1, data[i], 10, color);
				series.Items.Add(item);
			}

			return series;
		}

		private PlotModel CreatePlotModel(long[] data, OxyColor color)
		{
			var plotModel = new PlotModel();

			XYAxisSeries series;
			var alignedData = _imageEngine.AlignChannelHeight(data, AligmentCoefHist);
			if (IsLinearGraph)
			{
				series = CreateLine(alignedData, color);
			}
			else
			{
				series = CreateHistogram(alignedData, color);
			}

			plotModel.Series.Add(series);

			return plotModel;
		}

		private bool CanDrawHistogramsCommandExecute(object arg) => _currentImageModel != null;
		#endregion

		public MainWindowViewModel()
		{
			_dataService = new WinFilePickerService(new ConverterService()); // TODO: DEPENDENCY INJECTION

			CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseAppliationCommandExecute);
			DownloadImageCommand = new LambdaCommand(OnDownloadImageCommandExecuted, CanDownloadImageCommandExecute);
			SaveImageCommand = new LambdaCommand(OnSaveImageCommandExecuted, CanSaveImageCommandExecute);
			DrawHistogramsCommand = new LambdaCommand(OnDrawHistogramsCommandExecuted, CanDrawHistogramsCommandExecute);
		}
	}
}
