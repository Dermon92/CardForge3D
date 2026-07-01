using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CardForge3D;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open card image",
            Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|PNG files (*.png)|*.png|JPEG files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(dialog.FileName);
            bitmap.EndInit();
            bitmap.Freeze();

            CanvasImage.Source = bitmap;
            CanvasImage.Visibility = Visibility.Visible;
            CanvasPlaceholder.Visibility = Visibility.Collapsed;

            ImageStatus.Content = $"Loaded: {Path.GetFileName(dialog.FileName)}";
            Title = $"CardForge 3D - {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load image.\n\n{ex.Message}",
                "Image loading error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}