using ImageEcoLab.Infrastructure.Commands;
using ImageEcoLab.Services.VideoServices;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageEcoLab.ViewModels
{
	internal class ViewportVideoViewModel : ViewModel
	{
		private WebcamStreaming _webcamStreaming;
		private BackgroundWorker _backgroundWorker;

		#region ViewportVideo property
		private BitmapSource _viewportVideo;
		public BitmapSource ViewportVideo { get => _viewportVideo; set => Set(ref _viewportVideo, value); }
		#endregion

		#region StartVideoCommand
		public ICommand StartVideoCommand { get; set; }
		private bool CanStartVideoCommandExecute(object parameter) => true;
		private void OnStartVideoCommandExecuted(object parameter)
		{
			PlayCamera();
		}
		#endregion

		#region StopVideoCommand
		public ICommand StopVideoCommand { get; set; }
		private bool CanStopVideoCommandExecute(object parameter) => true;
		private void OnStopVideoCommandExecuted(object parameter)
		{
			_webcamStreaming.UnSubscribeOnStream(Handler);
			_webcamStreaming.StopStream();
			//_backgroundWorker.CancelAsync();
		}
		#endregion

		#region WebCamera
		public void PlayCamera()
		{
			_backgroundWorker.RunWorkerAsync();
		}

		private void WebCamStreaming(object sender, DoWorkEventArgs eventArgs)
		{
			_webcamStreaming.SubscribeOnStream(Handler);
			_webcamStreaming.StartStream(100);
		}

		private void UpdateFrame(object sender, ProgressChangedEventArgs eventArgs)
		{
			var frame = _webcamStreaming.GetFrame(out int width, out int height);

			WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgra32, null);
			Int32Rect rect = new Int32Rect(0, 0, width, height);
			writeableBitmap.WritePixels(rect, frame, 4 * width, 0);
			ViewportVideo = writeableBitmap;
		}

		private void Handler()
		{
			_backgroundWorker.ReportProgress(0);
		}

		#endregion
		public ViewportVideoViewModel(WebcamStreaming webcamStreaming)
		{
			_webcamStreaming = webcamStreaming;

			_backgroundWorker = new BackgroundWorker();

			_backgroundWorker.WorkerReportsProgress = true;

			_backgroundWorker.DoWork += WebCamStreaming;
			_backgroundWorker.ProgressChanged += UpdateFrame;

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
