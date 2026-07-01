using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CardForge3D.Models;

public class CardLayer : INotifyPropertyChanged
{
    private string _name;
    private bool _isVisible = true;
    private double _opacity = 1.0;
    private bool _isEditing;

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged();
        }
    }
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(VisibilityIcon));
        }
    }

    public double Opacity
    {
        get => _opacity;
        set
        {
            _opacity = value;
            OnPropertyChanged();
        }
    }

    public string VisibilityIcon => IsVisible ? "#" : "—";

    public CardLayer(string name)
    {
        _name = name;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}