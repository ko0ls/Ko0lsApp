using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AutoCADTools.Core._3D.Models
{
  public class Layer3D : ILayer
  {
    public Layer3D()
    {
      ObjectIdsList = new HashSet<Guid>();
    }

    public Layer3D(string name) : this()
    {
      Name = name;
    }

    private readonly HashSet<Guid> ObjectIdsList;

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Layer";
    public bool IsVisible { get; set; } = true;
    public Color Color { get; set; } = Colors.White;
    public string LineType { get; set; } = "";
    public double LineWeight { get; set; }

    public IReadOnlyCollection<Guid> ObjectIds => ObjectIdsList;

    public void Attach(Guid objectId) => ObjectIdsList.Add(objectId);
    public void Detach(Guid objectId) => ObjectIdsList.Remove(objectId);
  }
}
