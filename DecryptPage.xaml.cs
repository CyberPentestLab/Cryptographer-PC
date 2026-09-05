using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Cryptograph
{
    public sealed partial class EncryptPage : Page
    {
        public EncryptPage()
        {
            this.InitializeComponent();
            EncryptButton.Click += OnEncryptClick;
            CopyButton.Click += OnCopyClick;
        }

        private void OnEncryptClick(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            string keyStr = KeyTextBox.Text;

            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(keyStr))
            {
                ResultTextBox.Text = "Please enter both message and key.";
                return;
            }

            if (!int.TryParse(keyStr, out int key))
            {
                ResultTextBox.Text = "Key must be a number.";
                return;
            }

            int normalized = ((key % 26) + 26) % 26;
            string result = CaesarEncrypt(message, normalized);
            ResultTextBox.Text = result;
        }

        private async void OnCopyClick(object sender, RoutedEventArgs e)
        {
            string text = ResultTextBox.Text;
            if (string.IsNullOrEmpty(text))
            {
                var dialog = new ContentDialog
                {
                    Title = "Nothing to copy",
                    Content = "No result to copy.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

            var successDialog = new ContentDialog
            {
                Title = "Copied",
                Content = "Result copied to clipboard.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();
        }

        private string CaesarEncrypt(string text, int key)
        {
            char[] result = text.ToCharArray();
            for (int i = 0; i < result.Length; i++)
            {
                char c = result[i];
                if (c >= 'A' && c <= 'Z')
                {
                    result[i] = (char)('A' + (c - 'A' + key) % 26);
                }
                else if (c >= 'a' && c <= 'z')
                {
                    result[i] = (char)('a' + (c - 'a' + key) % 26);
                }
            }
            return new string(result);
        }
    }
}