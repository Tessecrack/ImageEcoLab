using ImageEcoLab.Infrastructure.Commands;
using ImageEcoLab.Models;
using ImageEcoLab.Services;
using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEcoLab.ViewModels
{
	internal class ViewportVideoViewModel : ViewModel
	{
		private Mat _matImage;

		private byte[] _buffer;

		private ImageEngine _imageEngine;

		#region ViewportVideo property
		private BitmapSource _viewportVideo;
		public BitmapSource ViewportVideo { get => _viewportVideo; set => Set(ref _viewportVideo, value); }
		#endregion

		#region StartVideoCommand
		public ICommand StartVideoCommand { get; set; }
		private bool CanStartVideoCommandExecute(object parameter) => true;
		private void OnStartVideoCommandExecuted(object parameter)
		{
			InitializeCamera();
			PlayCamera();
		}
		#endregion

		#region StopVideoCommand
		public ICommand StopVideoCommand { get; set; }
		private bool CanStopVideoCommandExecute(object parameter) => true;
		private void OnStopVideoCommandExecuted(object parameter)
		{

		}
		#endregion

		#region WebCamera
		private VideoCapture _capCamera;
		public void InitializeCamera()
		{
			_matImage = new Mat();
			_capCamera = new VideoCapture(0);
		}

		public void PlayCamera()
		{
			var backgroundWorker = new BackgroundWorker();
			backgroundWorker.WorkerReportsProgress = true;

			backgroundWorker.DoWork += WebCamStreaming;
			backgroundWorker.ProgressChanged += UpdateFrame;

			backgroundWorker.RunWorkerAsync();
		}

		private void WebCamStreaming(object sender, DoWorkEventArgs eventArgs)
		{
			while (!_capCamera.IsDisposed)
			{
				_capCamera.Read(_matImage);
				if (_matImage.Empty())
				{
					break;
				}
				_buffer = new byte[4 * _matImage.Total()];
				var counter = 0;
				for (int y = 0; y < _matImage.Rows; ++y)
				{
					for (int x = 0; x < _matImage.Cols; ++x)
					{
						var pixel = _matImage.At<Vec3b>(y, x);
						byte alpha = 255;
						_buffer[counter++] = pixel.Item0;
						_buffer[counter++] = pixel.Item1;
						_buffer[counter++] = pixel.Item2;
						_buffer[counter++] = alpha;
					}
				}
				Thread.Sleep(100);
				((BackgroundWorker)sender).ReportProgress(0);
			}
		}

		private void UpdateFrame(object sender, ProgressChangedEventArgs eventArgs)
		{
			var width = _matImage.Width;
			var height = _matImage.Height;

			WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgra32, null);
			Int32Rect rect = new Int32Rect(0, 0, width, height);
			writeableBitmap.WritePixels(rect, _buffer, 4 * width, 0);
			ViewportVideo = writeableBitmap;
		}

		#endregion
		public ViewportVideoViewModel(ImageEngine imageEngine)
		{
			_imageEngine = imageEngine;

			StartVideoCommand = new LambdaCommand(OnStartVideoCommandExecuted, CanStartVideoCommandExecute);
			StopVideoCommand = new LambdaCommand(OnStopVideoCommandExecuted, CanStopVideoCommandExecute);
		}

        public ViewportVideoViewModel()
        {
			if (!App.IsDesignMode)
			{
				throw new InvalidOperationException("Cannot use this constructor in release");
			}
		}
    }
}
