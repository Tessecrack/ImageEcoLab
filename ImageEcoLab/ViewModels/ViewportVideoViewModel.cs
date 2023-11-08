using ImageEcoLab.Infrastructure.Commands;
using ImageEcoLab.Models;
using ImageEcoLab.Services;
using OpenCvSharp;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageEcoLab.ViewModels
{
	internal class ViewportVideoViewModel : ViewModel
	{
		private ImageEngine _imageEngine;

		private Mat _matImage;

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

			new Thread(PlayCamera).Start();
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
			//_capCamera = VideoCapture.FromCamera(0);
		}

		public void PlayCamera()
		{
			while(!_capCamera.IsDisposed)
			{
				_capCamera.Read(_matImage);
				if (_matImage.Empty())
				{
					break;
				}
					
				App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
				{
					//Vec3b[] data = new Vec3b[_matImage.Width * _matImage.Height];

					_matImage.GetArray(out Vec3b[] data);

					var width = _matImage.Width;
					var height = _matImage.Height;

					WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgra32, null);
					Int32Rect rect = new Int32Rect(0, 0, width, height);
					//writeableBitmap.WritePixels(rect, buffer, width * writeableBitmap.Format.BitsPerPixel / 8, 0);
					//ViewportVideo = writeableBitmap;
				}));
			}
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
