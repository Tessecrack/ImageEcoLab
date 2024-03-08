using ImageEcoLab.Infrastructure.Commands;
using ImageEcoLab.Models;
using ImageEcoLab.Services;
using ImageEcoLab.Services.Base;
using ImageEcoLab.Services.VideoServices;
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
		#region UserControls
		public HistogramViewModel HistogramViewModel { get; private set; }
		#endregion

		#region Services
		private readonly IDataService _dataService;
		private readonly IConverterService _converterService;
		private readonly ImageEngine _imageEngine;
		private readonly WebcamStreaming _webcamStreaming;
		#endregion

		#region Multithreading Helpers
		private BackgroundWorker _workerWebcamStreaming;
		#endregion

		private ImageModel _sourceImageModel;

		private ImageModel _grayscaleImageModel;

		private ImageModel _currentImageModel;

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

			var img = new BitmapImage(new Uri(uri));
			BitmapImageShowed = _converterService.Convert(img, PixelFormats.Bgr32);

			byte bitsPerPixel = 4;
			int width = BitmapImageShowed.PixelWidth;
			int height = BitmapImageShowed.PixelHeight;

			byte[] pixels = new byte[width * height * bitsPerPixel];

			BitmapImageShowed.CopyPixels(pixels, width * bitsPerPixel, 0);

			Task.Run(() =>
			{
				_sourceImageModel = _imageEngine.GetImageModel(pixels, width, height, uri, bitsPerPixel);
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
			HistogramViewModel.UpdateHistograms(_currentImageModel, AligmentCoefHist, IsLinearGraph);
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
			_currentImageModel = _imageEngine.EqualizeGrayscale(_grayscaleImageModel);
			ShowImage(_currentImageModel);
			DrawHistogramsCommand.Execute(this);
		}
		#endregion

		#region StartWebcamCommand
		public ICommand StartWebcamCommand { get; set; }
		private bool CanStartWebcamCommandExecute(object parameter) => true;
		private void OnStartWebcamCommandExecuted(object parameter)
		{
			PlayCamera();
		}

		public void PlayCamera()
		{
			_workerWebcamStreaming.RunWorkerAsync();
		}

		private void WebCamStreaming(object sender, DoWorkEventArgs eventArgs)
		{
			_webcamStreaming.SubscribeOnStream(Handler);
			_webcamStreaming.StartStream(100);
		}

		private void UpdateFrame(object sender, ProgressChangedEventArgs eventArgs)
		{
			var frame = _webcamStreaming.GetFrame(out int width, out int height);
			var imageModel = new ImageModel();
			imageModel.Width = width;
			imageModel.Height = height;
			imageModel.BitsPerPixel = 4;
			imageModel.Bytes = frame;
			_sourceImageModel = imageModel;
			_currentImageModel = imageModel;
			ShowImage(imageModel);
		}

		private void Handler()
		{
			_workerWebcamStreaming.ReportProgress(0);
		}
		#endregion

		#region StopWebcamCommand
		public ICommand StopWebcamCommand { get; set; }
		private bool CanStopWebcamCommandExecute(object parameter) => true;
		private void OnStopWebcamCommandExecuted(Object parameter)
		{

		}
		#endregion

		private void ShowImage(ImageModel _imageModel)
		{
			if (_imageModel == null)
			{
				return;
			}

			WriteableBitmap writeableBitmap = new WriteableBitmap(
				_imageModel.Width, _imageModel.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

			var height = _imageModel.Height;
			var width = _imageModel.Width;
			Int32Rect rect = new Int32Rect(0, 0, width, height);
			writeableBitmap.WritePixels(rect, _imageModel.Bytes, _imageModel.BitsPerPixel * width, 0);
			BitmapImageShowed = writeableBitmap;
			DrawHistogramsCommand?.Execute(this);
		}

		public MainWindowViewModel()
		{
			if (!App.IsDesignMode)
			{
				throw new InvalidOperationException("Cannot use this constructor in release");
			}
		}

		public MainWindowViewModel(ImageEngine imageEngine, HistogramViewModel histogramViewModel, 
			IDataService dataService, IConverterService converterService, WebcamStreaming webcamStreaming)
		{
			HistogramViewModel = histogramViewModel;
			_dataService = dataService;
			_converterService = converterService;
			_imageEngine = imageEngine;
			_webcamStreaming = webcamStreaming;

			CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseAppliationCommandExecute);
			DownloadImageCommand = new LambdaCommand(OnDownloadImageCommandExecuted, CanDownloadImageCommandExecute);
			SaveImageCommand = new LambdaCommand(OnSaveImageCommandExecuted, CanSaveImageCommandExecute);
			DrawHistogramsCommand = new LambdaCommand(OnDrawHistogramsCommandExecuted, CanDrawHistogramsCommandExecute);
			ConvertToGrayscaleCommand = new LambdaCommand(OnConvertToGrayscaleCommandExecuted, CanConvertToGrayscaleCommandExecute);
			EqualizationHistogramCommand = new LambdaCommand(OnEqualizationHistogramCommandExecuted, CanEqualizationHistogramCommandExecute);
			NormalizeImageCommand = new LambdaCommand(OnNormalizeImageCommandExecuted, CanNormalizeImageCommandExecute);
			StartWebcamCommand = new LambdaCommand(OnStartWebcamCommandExecuted, CanStartWebcamCommandExecute);
			StopWebcamCommand = new LambdaCommand(OnStopWebcamCommandExecuted, CanStopWebcamCommandExecute);

			_workerWebcamStreaming = new BackgroundWorker();
			_workerWebcamStreaming.WorkerReportsProgress = true;
			_workerWebcamStreaming.DoWork += WebCamStreaming;
			_workerWebcamStreaming.ProgressChanged += UpdateFrame;
		}
	}
}
