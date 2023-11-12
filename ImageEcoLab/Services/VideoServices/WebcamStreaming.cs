using OpenCvSharp;
using System;
using System.Threading;

namespace ImageEcoLab.Services.VideoServices
{
	internal class WebcamStreaming : StreamService, IVideoService
	{
		private VideoCapture _capCamera;
		private Mat _frame;
		private bool _needStopStream;
		public override void Initialize()
		{
			if (!IsInitialized)
			{
				_capCamera = new VideoCapture(0);
				_frame = new Mat();
				IsInitialized = true;
			}
		}

		public override void StartStream(int delay)
		{
			Initialize();

			IsActiveStream = true;
			_needStopStream = false;

			while (!_capCamera.IsDisposed)
			{
				_capCamera.Read(_frame);
				if (_frame.Empty())
				{
					break;
				}
				_buffer = new byte[4 * _frame.Total()];
				var counter = 0;
				_frame = _frame.Flip(FlipMode.Y);
				for (int y = 0; y < _frame.Rows; ++y)
				{
					for (int x = 0; x < _frame.Cols; ++x)
					{
						var pixel = _frame.At<Vec3b>(y, x);
						byte alpha = 255;
						_buffer[counter++] = pixel.Item0;
						_buffer[counter++] = pixel.Item1;
						_buffer[counter++] = pixel.Item2;
						_buffer[counter++] = alpha;
					}
				}
				Translate();
				if (_needStopStream)
				{
					break;
				}
				Thread.Sleep(delay);
			}
		}

		public override void StopStream()
		{
			if (!IsActiveStream)
			{
				_capCamera?.Dispose();
				IsActiveStream = false;
				_needStopStream = true;
			}
		}

		public byte[] GetFrame(out int width, out int height)
		{
			width = _frame.Width;
			height = _frame.Height;

			return _buffer;
		}
	}
}
