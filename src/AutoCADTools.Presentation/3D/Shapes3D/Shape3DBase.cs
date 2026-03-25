using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation._3D.Shapes3D
{
  public abstract class Shape3DBase : BindableObject, IObject3DBase
  {
    protected Shape3DBase()
    {
      _name = GetType().Name;
      _isVisible = true;
      _transform = new Models.Transform3D();
      _material = new Models.Material3D();
    }

    #region Identity

    public Guid Id { get; } = Guid.NewGuid();
    public abstract Enum3DObjectType ObjectType { get; }

    private string _name;
    [ChangeTracker]
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    #endregion

    #region Visibility / Locking

    private bool _isVisible;
    [ChangeTracker]
    public bool IsVisible
    {
      get => _isVisible;
      set
      {
        if (SetProperty(ref _isVisible, value))
          RebuildModel();
      }
    }

    private bool _isLocked;
    [ChangeTracker]
    public bool IsLocked
    {
      get => _isLocked;
      set => SetProperty(ref _isLocked, value);
    }

    #endregion

    #region Layer / Group

    private string _layerId = "";
    [ChangeTracker]
    public string LayerId
    {
      get => _layerId;
      set => SetProperty(ref _layerId, value);
    }

    private string _groupId = "";
    [ChangeTracker]
    public string GroupId
    {
      get => _groupId;
      set => SetProperty(ref _groupId, value);
    }

    #endregion

    #region Selection / Hover

    private bool _isSelected;
    [ChangeTracker]
    public bool IsSelected
    {
      get => _isSelected;
      set
      {
        if (SetProperty(ref _isSelected, value))
          ApplyHighlight(value ? EnumHighlightMode.Selected : EnumHighlightMode.None);
      }
    }

    private bool _isMoveOver;
    [ChangeTracker]
    public bool IsMoveOver
    {
      get => _isMoveOver;
      set
      {
        if (SetProperty(ref _isMoveOver, value))
          ApplyHighlight(value ? EnumHighlightMode.Hover : EnumHighlightMode.None);
      }
    }

    #endregion

    #region Transform & Material

    private readonly Models.Transform3D _transform;
    public ITransform3D Transform => _transform;

    private readonly Models.Material3D _material;
    public IMaterial Material => _material;

    #endregion

    #region WPF Model

    private Model3DGroup? _cachedModel;

    public Model3DGroup WpfModel
    {
      get
      {
        if (_cachedModel == null)
        {
          _cachedModel = new Model3DGroup();
          RebuildModel();
        }
        return _cachedModel;
      }
    }

    /// <summary>
    /// Subclasses override to build the geometry mesh.
    /// </summary>
    protected abstract Model3D BuildCoreModel();

    /// <summary>
    /// Returns the world-space bounding box of this shape.
    /// </summary>
    protected abstract Rect3D ComputeBoundingBox();

    /// <summary>
    /// Performs ray vs. shape intersection test.
    /// </summary>
    protected abstract bool CoreHitTest(Ray3D ray, double tolerance);

    public bool HitTest(Point screenPoint, Ray3D ray, double tolerance)
    {
      if (!_isVisible) return false;
      return CoreHitTest(ray, tolerance);
    }

    public Rect3D GetBoundingBox() => ComputeBoundingBox();

    /// <summary>
    /// Rebuilds the WPF model from current geometry, material, and transform.
    /// </summary>
    protected void RebuildModel()
    {
      if (_cachedModel == null) return;

      _cachedModel.Children.Clear();

      if (!_isVisible) return;

      var model = BuildCoreModel();
      ApplyHighlightToModel(model as GeometryModel3D, GetCurrentHighlight());
      _cachedModel.Children.Add(model);
    }

    #endregion

    #region Highlight

    private EnumHighlightMode _currentHighlight = EnumHighlightMode.None;

    private EnumHighlightMode GetCurrentHighlight()
    {
      if (_isSelected) return EnumHighlightMode.Selected;
      if (_isMoveOver) return EnumHighlightMode.Hover;
      return EnumHighlightMode.None;
    }

    public void ApplyHighlight(EnumHighlightMode mode)
    {
      if (_currentHighlight == mode) return;
      _currentHighlight = mode;

      if (_cachedModel != null && _cachedModel.Children.Count > 0)
        ApplyHighlightToModel(_cachedModel.Children[0] as GeometryModel3D, mode);
    }

    private void ApplyHighlightToModel(GeometryModel3D? model, EnumHighlightMode mode)
    {
      if (model == null) return;

      var mat = _material as Models.Material3D;
      if (mat == null) return;

      var color = mat.DiffuseColor;

      switch (mode)
      {
        case EnumHighlightMode.Hover:
          color = BlendColor(color, Colors.Yellow, 0.3);
          break;
        case EnumHighlightMode.Selected:
          color = BlendColor(color, Colors.DodgerBlue, 0.4);
          break;
      }

      model.Material = new DiffuseMaterial(new SolidColorBrush(color));
      model.BackMaterial = mat.BackFaceCulling
        ? (Material)new DiffuseMaterial(new SolidColorBrush(color))
        : null;
    }

    private static Color BlendColor(Color baseColor, Color overlay, double factor)
    {
      return Color.FromArgb(
        baseColor.A,
        (byte)(baseColor.R + (overlay.R - baseColor.R) * factor),
        (byte)(baseColor.G + (overlay.G - baseColor.G) * factor),
        (byte)(baseColor.B + (overlay.B - baseColor.B) * factor));
    }

    #endregion

    #region Clone

    public IObject3DBase Clone()
    {
      return ObjectCloner.Clone<IObject3DBase>(this) ?? this;
    }

    #endregion

    #region BindableObject Required Members

    public override void AcceptChanges()
    {
      _transform.AcceptChanges();
      _material.AcceptChanges();
      base.AcceptChanges();
    }

    public override void RejectChanges()
    {
      _transform.RejectChanges();
      _material.RejectChanges();
      base.RejectChanges();
    }

    #endregion
  }
}
