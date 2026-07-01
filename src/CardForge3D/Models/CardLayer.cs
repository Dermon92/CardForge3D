namespace CardForge3D.Models;

public class CardLayer
{
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public double Opacity { get; set; } = 1.0;

    public string VisibilityIcon => IsVisible ? "#" : "—";

    public CardLayer(string name)
    {
        Name = name;
    }
}