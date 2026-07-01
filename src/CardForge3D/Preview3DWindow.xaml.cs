using CardForge3D.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CardForge3D;

public partial class Preview3DWindow : Window
{
    private readonly ObservableCollection<CardLayer> _layers;
    private double _thickness = 1.0;

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
        DepthValueText.Text = $"{e.NewValue:0}";
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

            double offset = 0;

            for (int t = _layers.Count - 1; t > i; t--)
            {
                offset += _layers[t].LayerThickness * depth;
            }

            double xDirection = XDirectionSlider?.Value ?? 1;
            double yDirection = YDirectionSlider?.Value ?? -1;

            Canvas.SetLeft(image, 170 + offset * xDirection);
            Canvas.SetTop(image, 160 + offset * yDirection);

            PreviewCanvas.Children.Add(image);
        }
    }
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        RenderPreview();
    }
    private void DirectionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (PreviewCanvas is null)
            return;

        RenderPreview();
    }
    private void PreviewTransformSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (PreviewRotateTransform is null || PreviewSkewTransform is null || RotateValueText is null)
            return;

        PreviewRotateTransform.Angle = RotateSlider.Value;
        RotateValueText.Text = $"{RotateSlider.Value:0}°";

        PreviewSkewTransform.AngleX = TiltSlider.Value * 30;
    }

}