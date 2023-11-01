using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImageEcoLab.ViewModels
{
	internal abstract class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		protected bool Set<T>(ref T value, T newValue, [CallerMemberName] string? propertyName = null)
		{
			if (Equals(value, newValue))
			{
				return false;
			}
			value = newValue;
			OnPropertyChanged(propertyName);
			return true;
		}
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = "") 
		{ 
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
