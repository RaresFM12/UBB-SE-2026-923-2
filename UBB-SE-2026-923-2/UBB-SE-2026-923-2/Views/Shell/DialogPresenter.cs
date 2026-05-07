namespace UBB_SE_2026_923_2.Views.Shell
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    public sealed class DialogPresenter
    {
        private const string CloseButtonLabel = "OK";

        private XamlRoot? xamlRoot;

        public void SetXamlRoot(XamlRoot xamlRoot) => this.xamlRoot = xamlRoot;

        public async Task ShowMessageAsync(string title, string message)
        {
            if (this.xamlRoot == null)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = CloseButtonLabel,
                XamlRoot = this.xamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
