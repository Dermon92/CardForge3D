namespace CardForge3D.Models;

public class LayerMask
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Alpha { get; }

    public LayerMask(int width, int height, byte defaultAlpha = 255)
    {
        Width = width;
        Height = height;
        Alpha = new byte[width * height];

        Array.Fill(Alpha, defaultAlpha);
    }

    public byte GetAlpha(int x, int y)
    {
        return Alpha[y * Width + x];
    }

    public void SetAlpha(int x, int y, byte value)
    {
        Alpha[y * Width + x] = value;
    }
}