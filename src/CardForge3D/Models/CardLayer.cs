using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CardForge3D.Models;

public class CardLayer : INotifyPropertyChanged
{
    private string _name;
    private bool _isVisible = true;
    private double _opacity = 1.0;
    private bool _isEditing;
    private ImageSource? _imageSource;
    private LayerMask? _mask;
    private ImageSource? _maskImageSource;
    private double _layerThickness = 0.3;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            NotifyPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged(nameof(VisibilityIcon));
        }
    }

    public double Opacity
    {
        get => _opacity;
        set
        {
            _opacity = value;
            NotifyPropertyChanged();
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            NotifyPropertyChanged();
        }
    }

    public ImageSource? ImageSource
    {
        get => _imageSource;
        set
        {
            _imageSource = value;
            NotifyPropertyChanged();
        }
    }

    public LayerMask? Mask
    {
        get => _mask;
        set
        {
            _mask = value;
            NotifyPropertyChanged();
        }
    }

    public ImageSource? MaskImageSource
    {
        get => _maskImageSource;
        set
        {
            _maskImageSource = value;
            NotifyPropertyChanged();
        }
    }

    public double LayerThickness
    {
        get => _layerThickness;
        set
        {
            _layerThickness = value;
            NotifyPropertyChanged();
        }
    }

    public string VisibilityIcon => IsVisible ? "#" : "-";

    public CardLayer(string name)
    {
        _name = name;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}