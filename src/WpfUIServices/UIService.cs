using Microsoft.Win32;

namespace WpfUIServices
{
    public interface IUIService
    {
        string GetFileNameFromOpenFileDialog(string initialDirectory = null, string filter = null, string title = null);
    }

    public class UIService : IUIService
    {
        public string GetFileNameFromOpenFileDialog(string initialDirectory = null, string filter = null, string title = null)
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                Filter = filter,
                Title = title,
            };

            if (ofd.ShowDialog() == false)
                return null;

            return ofd.FileName;
        }

        #region Singleton

        // Jon Skeet: https://csharpindepth.com/articles/singleton

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static UIService() { }

        private UIService() { }

        public static UIService Instance { get; } = new UIService();

        #endregion
    }
}
