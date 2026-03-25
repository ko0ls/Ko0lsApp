using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D.Enums;

namespace AutoCADTools.Core._3D
{
  /// <summary>
  /// Root contract for every 3D drawable object in the scene.
  /// Combines MVVM change-tracking (BindableObject) with 3D-specific concerns.
  /// </summary>
  public interface IObject3DBase : IRevertibleChangeTracking, INotifyPropertyChanged, INotifyDataErrorInfo
  {
    Guid Id { get; }
    string Name { get; set; }
    Enum3DObjectType ObjectType { get; }
    bool IsVisible { get; set; }
    bool IsLocked { get; set; }
    string LayerId { get; set; }
    string GroupId { get; set; }
    ITransform3D Transform { get; }
    IMaterial Material { get; }
    bool IsSelected { get; set; }
    bool IsMoveOver { get; set; }

    /// <summary>
    /// WPF 3D model node attached to the Viewport3D scene graph.
    /// </summary>
    Model3DGroup WpfModel { get; }

    void ApplyHighlight(EnumHighlightMode mode);
    bool HitTest(Point screenPoint, Ray3D ray, double tolerance);
    Rect3D GetBoundingBox();
    IObject3DBase Clone();
  }
}
