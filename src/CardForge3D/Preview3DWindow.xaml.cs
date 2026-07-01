using CardForge3D.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace CardForge3D;

public partial class Preview3DWindow : Window
{
    private readonly ObservableCollection<CardLayer> _layers;

    public Preview3DWindow(ObservableCollection<CardLayer> layers)
    {
        InitializeComponent();
        _layers = layers;

        RenderPreview();
        SetDefaultCamera();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        RenderPreview();
    }

    private void DepthScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DepthScaleValueText is null)
            return;

        DepthScaleValueText.Text = $"{e.NewValue:0.00}";
        RenderPreview();
    }

    private void RenderPreview()
    {
        if (Viewport is null)
            return;

        Viewport.Children.Clear();
        var lights = new Model3DGroup();
        lights.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 80)));
        lights.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-0.5, -1, -1)));

        Viewport.Children.Add(new ModelVisual3D { Content = lights });

        var group = new Model3DGroup();

        double width = 6.3;
        double height = 8.8;
        double depthScale = DepthScaleSlider.Value;

        double currentZ = 0;

        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            var layer = _layers[i];

            if (!layer.IsVisible || layer.ImageSource is not BitmapSource imageSource)
                continue;

            var maskedImage = CreateMaskedBitmap(imageSource, layer.Mask);
            var brush = new ImageBrush(maskedImage)
            {
                Stretch = Stretch.Fill,
                Opacity = layer.Opacity
            };

            var material = new DiffuseMaterial(brush);

            var mesh = CreateCardPlane(width, height, currentZ);

            var model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = material,
                BackMaterial = material
            };

            group.Children.Add(model);

            currentZ += layer.LayerThickness * depthScale;
        }

        var visual = new ModelVisual3D
        {
            Content = group
        };

        Viewport.Children.Add(visual);
        Viewport.ZoomExtents();
    }

    private static MeshGeometry3D CreateCardPlane(double width, double height, double z)
    {
        double halfW = width / 2;
        double halfH = height / 2;

        var mesh = new MeshGeometry3D();

        mesh.Positions.Add(new Point3D(-halfW, halfH, z));
        mesh.Positions.Add(new Point3D(halfW, halfH, z));
        mesh.Positions.Add(new Point3D(halfW, -halfH, z));
        mesh.Positions.Add(new Point3D(-halfW, -halfH, z));

        mesh.TextureCoordinates.Add(new Point(0, 0));
        mesh.TextureCoordinates.Add(new Point(1, 0));
        mesh.TextureCoordinates.Add(new Point(1, 1));
        mesh.TextureCoordinates.Add(new Point(0, 1));

        mesh.TriangleIndices.Add(0);
        mesh.TriangleIndices.Add(1);
        mesh.TriangleIndices.Add(2);

        mesh.TriangleIndices.Add(0);
        mesh.TriangleIndices.Add(2);
        mesh.TriangleIndices.Add(3);

        return mesh;
    }

    private static BitmapSource CreateMaskedBitmap(BitmapSource source, LayerMask? mask)
    {
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;

        var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        byte[] pixels = new byte[height * stride];
        converted.CopyPixels(pixels, stride, 0);

        if (mask is not null && mask.Width == width && mask.Height == height)
        {
            for (int i = 0; i < mask.Alpha.Length; i++)
            {
                int alphaIndex = i * 4 + 3;
                pixels[alphaIndex] = mask.Alpha[i];
            }
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
    private void ResetView_Click(object sender, RoutedEventArgs e)
    {
        SetDefaultCamera();
        Viewport.ZoomExtents();
    }
    private void SetDefaultCamera()
    {
        Viewport.Camera = new PerspectiveCamera
        {
            Position = new Point3D(7, -10, 8),
            LookDirection = new Vector3D(-7, 10, -8),
            UpDirection = new Vector3D(0, 0, 1),
            FieldOfView = 45
        };
    }
}