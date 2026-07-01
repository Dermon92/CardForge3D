using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CardForge3D.Models;
using System.Collections.ObjectModel;

namespace CardForge3D;



public partial class MainWindow : Window
{
    private bool _isPanning;
    private Point _lastPanPoint;
    private double _zoom = 1.0;
    private ObservableCollection<CardLayer> _layers = new();
    public MainWindow()
    {
        InitializeComponent();
        InitializeLayers();
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


    private void CanvasCardFrame_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        const double zoomStep = 0.1;
        const double minZoom = 0.25;
        const double maxZoom = 5.0;

        _zoom += e.Delta > 0 ? zoomStep : -zoomStep;
        _zoom = Math.Clamp(_zoom, minZoom, maxZoom);

        CanvasScaleTransform.ScaleX = _zoom;
        CanvasScaleTransform.ScaleY = _zoom;
    }

    private void CanvasCardFrame_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isPanning = true;
        _lastPanPoint = e.GetPosition(this);
        CanvasCardFrame.CaptureMouse();
        Cursor = System.Windows.Input.Cursors.Hand;
    }

    private void CanvasCardFrame_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _isPanning = false;
        CanvasCardFrame.ReleaseMouseCapture();
        Cursor = System.Windows.Input.Cursors.Arrow;
    }

    private void CanvasCardFrame_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_isPanning)
            return;

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _lastPanPoint;

        CanvasTranslateTransform.X += delta.X;
        CanvasTranslateTransform.Y += delta.Y;

        _lastPanPoint = currentPoint;
    }

    private void InitializeLayers()
    {
        for (int i = 8; i >= 1; i--)
        {
            _layers.Add(new CardLayer($"Layer {i}"));
        }

        LayersListBox.ItemsSource = _layers;
    }
    private void AddLayer_Click(object sender, RoutedEventArgs e)
    {
        int layerNumber = _layers.Count + 1;

        var newLayer = new CardLayer($"Layer {layerNumber}");

        _layers.Insert(0, newLayer);
        LayersListBox.SelectedItem = newLayer;
    }

    private void RemoveLayer_Click(object sender, RoutedEventArgs e)
    {
        if (LayersListBox.SelectedItem is not CardLayer selectedLayer)
            return;

        _layers.Remove(selectedLayer);

        if (_layers.Count > 0)
            LayersListBox.SelectedIndex = 0;
    }

    private void LayersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (LayersListBox.SelectedItem is CardLayer selectedLayer)
        {
            Title = $"CardForge 3D - {selectedLayer.Name}";
        }
    }
    private void ToggleLayerVisibility_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        if (element.DataContext is not CardLayer layer)
            return;

        layer.IsVisible = !layer.IsVisible;

        LayersListBox.Items.Refresh();
    }
}