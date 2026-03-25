using System;
using System.Collections.Generic;
using System.Windows.Media;
using AutoCADTools.Core._3D;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation._3D.Models
{
  public class Layer3D : BindableObject, ILayer
  {
    public Layer3D() { }

    public Layer3D(string name) : this()
    {
      _name = name;
    }

    private readonly HashSet<Guid> _objectIds = new HashSet<Guid>();
    private string _name = "Layer";
    private bool _isVisible = true;
    private Color _color = Colors.White;
    private string _lineType = "";
    private double _lineWeight;

    public Guid Id { get; } = Guid.NewGuid();

    [ChangeTracker]
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    [ChangeTracker]
    public bool IsVisible
    {
      get => _isVisible;
      set => SetProperty(ref _isVisible, value);
    }

    [ChangeTracker]
    public Color Color
    {
      get => _color;
      set => SetProperty(ref _color, value);
    }

    [ChangeTracker]
    public string LineType
    {
      get => _lineType;
      set => SetProperty(ref _lineType, value);
    }

    [ChangeTracker]
    public double LineWeight
    {
      get => _lineWeight;
      set => SetProperty(ref _lineWeight, value);
    }

    public IReadOnlyCollection<Guid> ObjectIds => _objectIds;

    public void Attach(Guid objectId)
    {
      if (_objectIds.Add(objectId))
        OnPropertyChanged(nameof(ObjectIds));
    }

    public void Detach(Guid objectId)
    {
      if (_objectIds.Remove(objectId))
        OnPropertyChanged(nameof(ObjectIds));
    }
  }
}
