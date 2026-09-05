using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Windows.ApplicationModel.DataTransfer;

namespace Cryptograph
{
    public sealed partial class DecryptPage : Page
    {
        public DecryptPage()
        {
            this.InitializeComponent();

            DecryptButton.Click += OnDecryptClick;
            CopyButton.Click += OnCopyClick;
        }

        private void OnDecryptClick(object sender, RoutedEventArgs e)
        {
            string encryptedBase64 = MessageTextBox.Text;
            string key = KeyTextBox.Text;

            if (string.IsNullOrWhiteSpace(encryptedBase64) || string.IsNullOrWhiteSpace(key))
            {
                ResultTextBox.Text = "Enter text and key";
                return;
            }

            try
            {
                string result = DecryptHybrid(encryptedBase64, key);
                ResultTextBox.Text = result;
            }
            catch (FormatException)
            {
                ResultTextBox.Text = "Base64 error.";
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = "Error: " + ex.Message;
            }
        }

        private string DecryptHybrid(string base64, string key)
        {
            byte[] fullData = Convert.FromBase64String(base64);

            // 1. AES (extract IV)
            byte[] aesDecrypted = AesDecrypt(fullData, key);

            // 2. XOR
            byte[] original = Xor(aesDecrypted, Encoding.UTF8.GetBytes(key));

            return Encoding.UTF8.GetString(original);
        }

        private byte[] Xor(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ key[i % key.Length]);

            return result;
        }

        private byte[] AesDecrypt(byte[] data, string key)
        {
            using var aes = Aes.Create();

            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            // 📦 extract IV
            byte[] iv = new byte[16];
            byte[] encrypted = new byte[data.Length - 16];

            Buffer.BlockCopy(data, 0, iv, 0, 16);
            Buffer.BlockCopy(data, 16, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            return PerformCryptography(encrypted, decryptor);
        }

        private byte[] PerformCryptography(byte[] data, ICryptoTransform transform)
        {
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write);

            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }

        private async void OnCopyClick(object sender, RoutedEventArgs e)
        {
            string text = ResultTextBox.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                await ShowDialog("Attention", "Enter text to copy");
                return;
            }

            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);

            await ShowDialog("Done", "Copied.");
        }

        private async System.Threading.Tasks.Task ShowDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}