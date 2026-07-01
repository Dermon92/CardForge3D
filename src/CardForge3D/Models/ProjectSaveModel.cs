namespace CardForge3D.Models;




public class ProjectSaveModel
{
    public string SourceImageFile { get; set; } = "";
    public List<ProjectLayerSaveModel> Layers { get; set; } = new();
}

public class ProjectLayerSaveModel
{
    public string Name { get; set; } = "";
    public bool IsVisible { get; set; }
    public double Opacity { get; set; }
    public double LayerThickness { get; set; }
    public int MaskWidth { get; set; }
    public int MaskHeight { get; set; }
    public string MaskFile { get; set; } = "";
}