using System.Windows.Media.Media3D;

namespace AutoCADTools.Presentation._3D.Viewport3D.Selection
{
  public class SelectionHitResult
  {
    public SelectionHitResult(Core._3D.IObject3DBase obj, Point3D worldPoint, Vector3D normal, double distance)
    {
      Object = obj;
      WorldPoint = worldPoint;
      Normal = normal;
      Distance = distance;
    }

    public Core._3D.IObject3DBase Object { get; }
    public Point3D WorldPoint { get; }
    public Vector3D Normal { get; }
    public double Distance { get; }
  }
}
