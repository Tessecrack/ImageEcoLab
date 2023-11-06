using ImageEcoLab.Services.Base;

namespace ImageEcoLab.Services
{
	internal class WinFilePickerService : IDataService
    {
        private readonly string pathDebug = "D:\\Desktop\\IEco\\ImageEcoLab\\ImageEcoLab\\Resources\\TestImage";

		public string? Path { get; private set; }

        public WinFilePickerService()
        {
        }

        public bool OpenDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            //dialog.InitialDirectory = Directory.GetCurrentDirectory();
			dialog.InitialDirectory = pathDebug;
			dialog.FileName = "Image";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                Path = dialog.FileName;
				return true;
            }
            return false;
        }

		public string GetUri()
		{
			if (OpenDialog() == true)
            {
                return Path;
            }
            return null;
		}
	}
}
