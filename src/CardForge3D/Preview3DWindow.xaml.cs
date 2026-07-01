using CardForge3D.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CardForge3D;

public partial class Preview3DWindow : Window
{
    private readonly ObservableCollection<CardLayer> _layers;

    public Preview3DWindow(ObservableCollection<CardLayer> layers)
    {
        InitializeComponent();
        _layers = layers;
        RenderPreview();
    }

    private void DepthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (PreviewCanvas is null)
            return;

        RenderPreview();
    }

    private void RenderPreview()
    {
        PreviewCanvas.Children.Clear();

        double depth = DepthSlider.Value;

        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var layer = _layers[i];

            if (!layer.IsVisible || layer.ImageSource is null)
                continue;

            var image = new Image
            {
                Source = layer.ImageSource,
                Width = 420,
                Height = 585,
                Stretch = Stretch.Uniform,
                Opacity = layer.Opacity
            };

            if (layer.MaskImageSource is not null)
            {
                image.OpacityMask = new ImageBrush(layer.MaskImageSource)
                {
                    Stretch = Stretch.Uniform
                };
            }

            double offset = (_layers.Count - 1 - i) * depth;

            Canvas.SetLeft(image, 170 + offset);
            Canvas.SetTop(image, 160 - offset);

            PreviewCanvas.Children.Add(image);
        }
    }
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        RenderPreview();
    }
}