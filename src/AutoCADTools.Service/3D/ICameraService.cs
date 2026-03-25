using System.Windows.Media.Media3D;

namespace AutoCADTools.Service._3D
{
  public interface ICameraService
  {
    PerspectiveCamera Camera { get; }
    double FieldOfView { get; set; }
    bool IsOrthographic { get; set; }
    double Zoom { get; set; }
    Point3D Target { get; set; }

    void Orbit(Vector3D axis, double degrees);
    void Pan(Vector3D direction, double distance);
    void ZoomIn();
    void ZoomOut();
    void ZoomToFit(Rect3D worldBox);
    void ZoomToObjects(System.Collections.Generic.IReadOnlyCollection<Core._3D.IObject3DBase> objs);
    void SetPresetView(Core._3D.Enums.EnumPresetView view);

    event System.EventHandler CameraChanged;
  }
}
