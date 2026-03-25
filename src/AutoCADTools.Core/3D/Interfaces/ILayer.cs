using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AutoCADTools.Core._3D
{
  public interface ILayer
  {
    Guid Id { get; }
    string Name { get; set; }
    bool IsVisible { get; set; }
    Color Color { get; set; }
    string LineType { get; set; }
    double LineWeight { get; set; }
    IReadOnlyCollection<Guid> ObjectIds { get; }
    void Attach(Guid objectId);
    void Detach(Guid objectId);
  }
}
