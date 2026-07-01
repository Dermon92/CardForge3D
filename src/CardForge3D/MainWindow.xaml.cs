using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CardForge3D.Models;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CardForge3D;



public partial class MainWindow : Window
{
    private bool _isPanning;
    private Point _lastPanPoint;
    private double _zoom = 1.0;
    private ObservableCollection<CardLayer> _layers = new();
    private CardLayer? _selectedLayer;
    private EditorTool _activeTool = EditorTool.Pan;
    private bool _isPainting;
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

            foreach (var layer in _layers)
            {
                layer.ImageSource = bitmap;
                layer.Mask = new LayerMask(bitmap.PixelWidth, bitmap.PixelHeight);
                layer.MaskImageSource = CreateMaskImageSource(layer.Mask);
            }

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
        if (_activeTool == EditorTool.Brush)
        {
            _isPainting = true;
            CanvasCardFrame.CaptureMouse();
            PaintMaskAt(e.GetPosition(CanvasCardFrame));
            return;
        }

        _isPanning = true;
        _lastPanPoint = e.GetPosition(this);
        CanvasCardFrame.CaptureMouse();
        Cursor = System.Windows.Input.Cursors.Hand;
    }

    private void CanvasCardFrame_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_activeTool == EditorTool.Brush)
        {
            _isPainting = false;
            CanvasCardFrame.ReleaseMouseCapture();
            return;
        }

        _isPanning = false;
        CanvasCardFrame.ReleaseMouseCapture();
        Cursor = System.Windows.Input.Cursors.Arrow;
    }

    private void CanvasCardFrame_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_activeTool == EditorTool.Brush)
        {
            if (_isPainting)
                PaintMaskAt(e.GetPosition(CanvasCardFrame));

            return;
        }
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
        if (LayersListBox.SelectedItem is not CardLayer selectedLayer)
            return;

        _selectedLayer = selectedLayer;

        SelectedLayerNameText.Text = $"Selected: {selectedLayer.Name}";
        LayerOpacitySlider.Value = selectedLayer.Opacity * 100;
        LayerOpacityValueText.Text = $"{LayerOpacitySlider.Value:0}%";

        Title = $"CardForge 3D - {selectedLayer.Name}";
    }
    private void ToggleLayerVisibility_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        if (element.DataContext is not CardLayer layer)
            return;

        layer.IsVisible = !layer.IsVisible;


    }
    private void EditLayerName_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        if (element.DataContext is not CardLayer layer)
            return;

        layer.IsEditing = true;
    }

    private void LayerNameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != System.Windows.Input.Key.Enter)
            return;

        FinishLayerNameEditing(sender);
    }

    private void LayerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        FinishLayerNameEditing(sender);
    }

    private static void FinishLayerNameEditing(object sender)
    {
        if (sender is not FrameworkElement element)
            return;

        if (element.DataContext is not CardLayer layer)
            return;

        layer.IsEditing = false;
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.RefreshSelectedLayerProperties();
        }
    }
    private void LayerOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_selectedLayer is null)
            return;

        _selectedLayer.Opacity = e.NewValue / 100.0;
        LayerOpacityValueText.Text = $"{e.NewValue:0}%";
    }

    private void MoveLayerUp_Click(object sender, RoutedEventArgs e)
    {
        if (LayersListBox.SelectedItem is not CardLayer selectedLayer)
            return;

        int index = _layers.IndexOf(selectedLayer);

        if (index <= 0)
            return;

        _layers.Move(index, index - 1);
        LayersListBox.SelectedItem = selectedLayer;
    }

    private void MoveLayerDown_Click(object sender, RoutedEventArgs e)
    {
        if (LayersListBox.SelectedItem is not CardLayer selectedLayer)
            return;

        int index = _layers.IndexOf(selectedLayer);

        if (index < 0 || index >= _layers.Count - 1)
            return;

        _layers.Move(index, index + 1);
        LayersListBox.SelectedItem = selectedLayer;
    }
    private void RefreshSelectedLayerProperties()
    {
        if (_selectedLayer is null)
            return;

        SelectedLayerNameText.Text = $"Selected: {_selectedLayer.Name}";
        LayerOpacityValueText.Text = $"{LayerOpacitySlider.Value:0}%";
    }
    private void HideLayerMask_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLayer?.Mask is null)
            return;

        Array.Fill(_selectedLayer.Mask.Alpha, (byte)0);
        _selectedLayer.MaskImageSource = CreateMaskImageSource(_selectedLayer.Mask);
    }

    private void ShowLayerMask_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLayer?.Mask is null)
            return;

        Array.Fill(_selectedLayer.Mask.Alpha, (byte)255);
        _selectedLayer.MaskImageSource = CreateMaskImageSource(_selectedLayer.Mask);
    }
    private static ImageSource CreateMaskImageSource(LayerMask mask)
    {
        int width = mask.Width;
        int height = mask.Height;
        int stride = width * 4;
        byte[] pixels = new byte[height * stride];

        for (int i = 0; i < mask.Alpha.Length; i++)
        {
            int p = i * 4;
            byte alpha = mask.Alpha[i];

            pixels[p + 0] = 255;
            pixels[p + 1] = 255;
            pixels[p + 2] = 255;
            pixels[p + 3] = alpha;
        }

        var bitmap = BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            pixels,
            stride);

        bitmap.Freeze();
        return bitmap;
    }
    private enum EditorTool
    {
        Pan,
        Brush
    }
    private void PanTool_Click(object sender, RoutedEventArgs e)
    {
        _activeTool = EditorTool.Pan;
        ImageStatus.Content = "Tool: Pan";
    }

    private void BrushTool_Click(object sender, RoutedEventArgs e)
    {
        _activeTool = EditorTool.Brush;
        ImageStatus.Content = "Tool: Brush";
    }
    private void PaintMaskAt(Point point)
    {
        if (_selectedLayer?.Mask is null)
            return;

        var mask = _selectedLayer.Mask;

        double frameWidth = CanvasCardFrame.ActualWidth;
        double frameHeight = CanvasCardFrame.ActualHeight;

        double imageAspect = (double)mask.Width / mask.Height;
        double frameAspect = frameWidth / frameHeight;

        double drawWidth;
        double drawHeight;
        double offsetX;
        double offsetY;

        if (imageAspect > frameAspect)
        {
            drawWidth = frameWidth;
            drawHeight = frameWidth / imageAspect;
            offsetX = 0;
            offsetY = (frameHeight - drawHeight) / 2;
        }
        else
        {
            drawHeight = frameHeight;
            drawWidth = frameHeight * imageAspect;
            offsetX = (frameWidth - drawWidth) / 2;
            offsetY = 0;
        }

        double localX = point.X - offsetX;
        double localY = point.Y - offsetY;

        if (localX < 0 || localY < 0 || localX > drawWidth || localY > drawHeight)
            return;

        int pixelX = (int)(localX / drawWidth * mask.Width);
        int pixelY = (int)(localY / drawHeight * mask.Height);

        int radius = (int)(BrushSizeSlider.Value / 2);

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y > radius * radius)
                    continue;

                int targetX = pixelX + x;
                int targetY = pixelY + y;

                if (targetX < 0 || targetY < 0 || targetX >= mask.Width || targetY >= mask.Height)
                    continue;

                mask.SetAlpha(targetX, targetY, 0);
            }
        }

        _selectedLayer.MaskImageSource = CreateMaskImageSource(mask);
    }
}