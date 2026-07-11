namespace FamilySuper.Host.WPF.Components;

public class AnnotationPosition
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public FaceNormal FaceNormal { get; set; } = new();
}

public class FaceNormal
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}

public class ModelInfo
{
    public Position Center { get; set; } = new();
    public Position Size { get; set; } = new();
    public double Scale { get; set; }
}

public class Position
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
}