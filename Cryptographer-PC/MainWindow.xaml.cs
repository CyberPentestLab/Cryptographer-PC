using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Cryptograph
{
    public sealed partial class MainWindow : Window
    {
        private bool _sizeConfigured = false;

        public MainWindow()
        {
            this.InitializeComponent();

            RootGrid.Loaded += RootGrid_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;

            // Main screen button handlers
            EncryptButton.Click += (s, e) => SlideToSecond("Encrypt");
            DecryptButton.Click += (s, e) => SlideToSecond("Decrypt");
            AboutButton.Click += (s, e) => SlideToSecond("About");

            // Back button on the second screen
            BackButton.Click += (s, e) => SlideBack();

            // Buttons on the second screen: navigation in the right area
            SecondEncryptButton.Click += (s, e) => ContentFrame.Navigate(typeof(EncryptPage));
            SecondDecryptButton.Click += (s, e) => ContentFrame.Navigate(typeof(DecryptPage));
            // Temporarily commented out because AboutPage and LanguagePage have not been created
            // SecondAboutButton.Click += (s, e) => ContentFrame.Navigate(typeof(AboutPage));
            // SecondLanguageButton.Click += (s, e) => ContentFrame.Navigate(typeof(LanguagePage));

            // Language button on the main screen (dialog)
            LanguageButton.Click += (s, e) => new ContentDialog().ShowAsync("Language", "Language selection (coming later)", this);
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_sizeConfigured)
            {
                ConfigureWindowSize();
                _sizeConfigured = true;
            }

            var size = this.Content.XamlRoot.Size;
            if (size.Width > 0 && size.Height > 0)
                UpdateLayoutForSize(size);
        }

        private void ConfigureWindowSize()
        {
            try
            {
                var appWindow = this.AppWindow;
                if (appWindow != null)
                {
                    var presenter = appWindow.Presenter as OverlappedPresenter;
                    if (presenter != null)
                    {
                        presenter.IsResizable = false;
                        presenter.IsMaximizable = false;
                        presenter.IsMinimizable = true;
                    }

                    const int width = 900;
                    const int height = 700;
                    appWindow.ResizeClient(new Windows.Graphics.SizeInt32 { Width = width, Height = height });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Window setup error: {ex.Message}");
            }
        }

        private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            UpdateLayoutForSize(e.Size);
        }

        private void UpdateLayoutForSize(Windows.Foundation.Size size)
        {
            MainContainer.Width = size.Width * 2;
            FirstScreen.Width = size.Width;
            SecondScreen.Width = size.Width;
            MainClip.Rect = new Windows.Foundation.Rect(0, 0, size.Width, size.Height);
        }

        private void SlideToSecond(string pageType)
        {
            // Load the desired page into ContentFrame
            switch (pageType)
            {
                case "Encrypt":
                    ContentFrame.Navigate(typeof(EncryptPage));
                    break;
                case "Decrypt":
                    ContentFrame.Navigate(typeof(DecryptPage));
                    break;
                    // About not implemented yet
                    // case "About":
                    // ContentFrame.Navigate(typeof(AboutPage));
                    // break;
            }

            // Slide animation
            double windowWidth = this.Content.XamlRoot.Size.Width;
            var animation = new DoubleAnimation
            {
                To = -windowWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(animation, MainTransform);
            Storyboard.SetTargetProperty(animation, "X");

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void SlideBack()
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(animation, MainTransform);
            Storyboard.SetTargetProperty(animation, "X");

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }

    public static class DialogExtensions
    {
        public static async void ShowAsync(this ContentDialog dialog, string title, string content, Window window)
        {
            dialog.Title = title;
            dialog.Content = content;
            dialog.CloseButtonText = "OK";
            dialog.XamlRoot = window.Content.XamlRoot;
            await dialog.ShowAsync();
        }
    }
}