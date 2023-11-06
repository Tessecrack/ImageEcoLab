using ImageEcoLab.Infrastructure.Commands;
using ImageEcoLab.Models;
using ImageEcoLab.Services;
using ImageEcoLab.Services.Base;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEcoLab.ViewModels
{
	[MarkupExtensionReturnType(typeof(MainWindowViewModel))]
	internal class MainWindowViewModel : ViewModel
	{
		private readonly IDataService _dataService;

		private readonly ImageEngine _imageEngine = new ImageEngine();

		private ImageModel _sourceImageModel;

		private ImageModel _grayscaleImageModel;

		private ImageModel _currentImageModel;

		private ConverterService _converterService = new ConverterService();

		#region ProgressBarValue
		private int _progressBarValue = 0;
		public int ProgressBarValue { get => _progressBarValue; set => Set(ref _progressBarValue, value); }
		#endregion

		#region UpperThresholdNormalization
		private byte _upperThresholdNormalization = 255;
		public byte UpperThresholdNormalization 
		{ 
			get => _upperThresholdNormalization;
			set
			{
				if (!Set(ref _upperThresholdNormalization, value))
				{
					return;
				}
				if (_grayscaleImageModel == null)
				{
					return;
				}
				var backgroundWorker = new BackgroundWorker();
				ImageModel result = null;
				backgroundWorker.DoWork += (s, e) =>
				{
					result = _imageEngine.NormalizeGrayscale(_grayscaleImageModel, _upperThresholdNormalization, _lowerThresholdNormalization);
					_currentImageModel = result;
					DrawHistogramsCommand.Execute(backgroundWorker);
				};
				backgroundWorker.RunWorkerCompleted += (s, e) =>
				{
					if (result != null)
					{
						ShowImage(result);
					}
				};
				backgroundWorker.RunWorkerAsync();
			} 
		}
		#endregion

		#region LowerThresholdNormalization
		private byte _lowerThresholdNormalization = 1;
		public byte LowerThresholdNormalization 
		{ 
			get => _lowerThresholdNormalization;
			set
			{
				if (!Set(ref _lowerThresholdNormalization, value))
				{
					return;
				}
				if (_grayscaleImageModel == null)
				{
					return;
				}
				var backgroundWorker = new BackgroundWorker();
				ImageModel result = null;
				backgroundWorker.DoWork += (s, e) =>
				{
					result = _imageEngine.NormalizeGrayscale(_grayscaleImageModel, _upperThresholdNormalization, _lowerThresholdNormalization);
					_currentImageModel = result;
					DrawHistogramsCommand.Execute(backgroundWorker);
				};
				backgroundWorker.RunWorkerCompleted += (s, e) =>
				{
					if (result != null)
					{
						ShowImage(result);
					}
				};
				backgroundWorker.RunWorkerAsync();
			}
		}
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
				Task.Run(() => DrawHistogramsCommand.Execute(this));
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

		#region Property BitmapImageShowed
		private BitmapSource _bitmapImageShowed = new WriteableBitmap(1, 1, 1, 1, PixelFormats.Bgra32, null);
		public BitmapSource BitmapImageShowed
		{
			get => _bitmapImageShowed;
			set => Set(ref _bitmapImageShowed, value);
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

		#region Histograms
		private Histograms _histograms;
		#endregion

		#region BrightnessHist
		private PlotModel _brightnessHist = new PlotModel();
		public PlotModel BrightnessHist { get => _brightnessHist; set => Set(ref _brightnessHist, value); }
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

		#region ConvertToGrayscaleCommand
		public ICommand ConvertToGrayscaleCommand { get; set; }

		private bool CanConvertToGrayscaleCommandExecute(object parameter) => _sourceImageModel != null;

		private void OnConvertToGrayscaleCommandExecuted(object paramter)
		{
			var newImageModel = _imageEngine.ConvertToGrayscale(_sourceImageModel);
			ShowImage(newImageModel);
			_grayscaleImageModel = newImageModel;
			_currentImageModel = _grayscaleImageModel;
			DrawHistogramsCommand.Execute(this);
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

			BitmapImageShowed = _converterService.ConvertToBgra32(new BitmapImage(new Uri(uri)));

			int width = BitmapImageShowed.PixelWidth;
			int height = BitmapImageShowed.PixelHeight;

			byte[] pixels = new byte[width * height * 4];

			BitmapImageShowed.CopyPixels(pixels, width * 4, 0);

			Task.Run(() =>
			{
				_sourceImageModel = _imageEngine.GetImageModel(pixels, width, height, uri);
				_currentImageModel = _sourceImageModel;
				DrawHistogramsCommand.Execute(this);
			});

			PathCurrentImage = uri;
			CurrentStatusStr = PathCurrentImage;
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
		private bool CanDrawHistogramsCommandExecute(object arg) => _currentImageModel != null;
		private void OnDrawHistogramsCommandExecuted(object obj)
		{
			if (_currentImageModel == null)
			{
				return;
			}
			if (BitmapImageShowed == null)
			{
				return;
			}
			this._histograms = _imageEngine.GetHistograms(_currentImageModel);

			var redChannel = this._histograms.RedChannel;
			var greenChannel = this._histograms.GreenChannel;
			var blueChannel = this._histograms.BlueChannel;
			var brightness = this._histograms.BrightnessHist;

			RedChannelHist = CreatePlotModel(redChannel, OxyColor.FromRgb(255, 0, 0));
			GreenChannelHist = CreatePlotModel(greenChannel, OxyColor.FromRgb(0, 255, 0));
			BlueChannelHist = CreatePlotModel(blueChannel, OxyColor.FromRgb(0, 0, 255));
			BrightnessHist = CreatePlotModel(brightness, OxyColor.FromRgb(0, 0, 0));
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
		#endregion

		#region NormalizeImageCommand

		public ICommand NormalizeImageCommand { get; set; }
		private bool CanNormalizeImageCommandExecute(object parameter) => _grayscaleImageModel != null;
		private void OnNormalizeImageCommandExecuted(object parameter)
		{
			var newImageModel = _imageEngine.NormalizeGrayscale(_grayscaleImageModel, UpperThresholdNormalization, LowerThresholdNormalization);
			ShowImage(newImageModel);
			_currentImageModel = newImageModel;
			DrawHistogramsCommand.Execute(this);
		}


		#endregion

		#region EqualizationHistogramCommand
		public ICommand EqualizationHistogramCommand { get; set; }
		private bool CanEqualizationHistogramCommandExecute(object parameter) => _grayscaleImageModel != null;
		private void OnEqualizationHistogramCommandExecuted(object parameter)
		{
			_currentImageModel = _imageEngine.EqualizeGrayscale(_grayscaleImageModel, _histograms);
			ShowImage(_currentImageModel);
			DrawHistogramsCommand.Execute(this);
		}
		#endregion

		private void ShowImage(ImageModel _imageModel)
		{
			if (_imageModel == null)
			{
				return;
			}

			WriteableBitmap writeableBitmap = new WriteableBitmap(
				_imageModel.Width, _imageModel.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

			var height = _imageModel.Height;
			var width = _imageModel.Width;
			Int32Rect rect = new Int32Rect(0, 0, width, height);
			writeableBitmap.WritePixels(rect, _imageModel.Bytes, width * writeableBitmap.Format.BitsPerPixel / 8, 0);
			BitmapImageShowed = writeableBitmap;
		}

		public MainWindowViewModel()
		{
			_dataService = new WinFilePickerService(new ConverterService()); // TODO: DEPENDENCY INJECTION

			CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseAppliationCommandExecute);
			DownloadImageCommand = new LambdaCommand(OnDownloadImageCommandExecuted, CanDownloadImageCommandExecute);
			SaveImageCommand = new LambdaCommand(OnSaveImageCommandExecuted, CanSaveImageCommandExecute);
			DrawHistogramsCommand = new LambdaCommand(OnDrawHistogramsCommandExecuted, CanDrawHistogramsCommandExecute);
			ConvertToGrayscaleCommand = new LambdaCommand(OnConvertToGrayscaleCommandExecuted, CanConvertToGrayscaleCommandExecute);
			EqualizationHistogramCommand = new LambdaCommand(OnEqualizationHistogramCommandExecuted, CanEqualizationHistogramCommandExecute);
			NormalizeImageCommand = new LambdaCommand(OnNormalizeImageCommandExecuted, CanNormalizeImageCommandExecute);
		}
	}
}
