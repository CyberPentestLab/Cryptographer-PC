using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Windows.ApplicationModel.DataTransfer;

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
            string plainText = MessageTextBox.Text;
            string key = KeyTextBox.Text;

            if (string.IsNullOrWhiteSpace(plainText) || string.IsNullOrWhiteSpace(key))
            {
                ResultTextBox.Text = "Enter text and key";
                return;
            }

            try
            {
                string result = EncryptHybrid(plainText, key);
                ResultTextBox.Text = result;
            }
            catch (Exception ex)
            {
                ResultTextBox.Text = "Error: " + ex.Message;
            }
        }

        private string EncryptHybrid(string text, string key)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // 1. XOR
            byte[] xorBytes = Xor(textBytes, keyBytes);

            // 2. AES + random IV
            byte[] aesBytes = AesEncrypt(xorBytes, key);

            // 3. Base64
            return Convert.ToBase64String(aesBytes);
        }

        private byte[] Xor(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ key[i % key.Length]);

            return result;
        }

        private byte[] AesEncrypt(byte[] data, string key)
        {
            using var aes = Aes.Create();

            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            // 🔥 random IV
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using var encryptor = aes.CreateEncryptor();
            byte[] encrypted = PerformCryptography(data, encryptor);

            // 📦 IV + data
            byte[] result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return result;
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
                await ShowDialog("Error", "Enter text to copy");
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