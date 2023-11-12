using System;

namespace ImageEcoLab.Services
{
	internal abstract class StreamService
	{
		protected byte[] _buffer;

		private event Action OnTranslateStream;

		public void SubscribeOnStream(Action handler)
		{
			OnTranslateStream += handler;
		}

		public void UnSubscribeOnStream(Action handler)
		{
			OnTranslateStream -= handler;
		}

		protected void Translate()
		{
			OnTranslateStream?.Invoke();
		}

		public bool IsInitialized { get; protected set; }

		public bool IsActiveStream { get; protected set; }

		public abstract void Initialize();

		public abstract void StartStream(int delay);

		public abstract void StopStream();
	}
}
