using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;
using AutoCADTools.Service._3D;

namespace AutoCADTools.Presentation._3D.Properties3D
{
  public class Properties3DViewModel : BindableObject
  {
    private readonly IObjectManager3D _objectManager;

    public Properties3DViewModel(IObjectManager3D objectManager)
    {
      _objectManager = objectManager;
      _objectManager.SelectionChanged += OnSelectionChanged;

      TransformGroup = new TransformPropertyGroup(this);
      MaterialGroup = new MaterialPropertyGroup(this);
    }

    private void OnSelectionChanged(object sender, Core._3D.Events.SelectionChangedEventArgs e)
    {
      if (_objectManager.SelectedObjects.Count == 0)
        ClearSelection();
      else if (_objectManager.SelectedObjects.Count == 1)
        SetSelection(_objectManager.SelectedObjects[0]);
      else
        SetMultipleSelection(_objectManager.SelectedObjects.Count);
    }

    #region Selected Object

    private IObject3DBase? _selectedObject;
    private int _selectionCount;

    public IObject3DBase? SelectedObject => _selectedObject;

    public bool HasSingleSelection => _selectionCount == 1;
    public bool HasMultipleSelection => _selectionCount > 1;
    public int SelectionCount => _selectionCount;

    public string ObjectName => HasSingleSelection && _selectedObject != null ? _selectedObject.Name : "—";
    public string ObjectType => HasSingleSelection && _selectedObject != null
      ? _selectedObject.ObjectType.ToString() : "Multiple Values";
    public string ObjectId => HasSingleSelection && _selectedObject != null
      ? _selectedObject.Id.ToString().Substring(0, 8) : "—";

    private void SetSelection(IObject3DBase obj)
    {
      _selectedObject = obj;
      _selectionCount = 1;
      NotifyAllProperties();
    }

    private void SetMultipleSelection(int count)
    {
      _selectedObject = null;
      _selectionCount = count;
      NotifyAllProperties();
    }

    private void ClearSelection()
    {
      _selectedObject = null;
      _selectionCount = 0;
      NotifyAllProperties();
    }

    private void NotifyAllProperties()
    {
      OnPropertyChanged(nameof(SelectedObject));
      OnPropertyChanged(nameof(HasSingleSelection));
      OnPropertyChanged(nameof(HasMultipleSelection));
      OnPropertyChanged(nameof(SelectionCount));
      OnPropertyChanged(nameof(ObjectName));
      OnPropertyChanged(nameof(ObjectType));
      OnPropertyChanged(nameof(ObjectId));
      OnPropertyChanged(nameof(TransformGroup));
      OnPropertyChanged(nameof(MaterialGroup));
    }

    #endregion

    #region Property Groups

    public TransformPropertyGroup TransformGroup { get; }
    public MaterialPropertyGroup MaterialGroup { get; }

    [ChangeTracker]
    public bool IsTransformExpanded
    {
      get => _isTransformExpanded;
      set => SetProperty(ref _isTransformExpanded, value);
    }
    private bool _isTransformExpanded = true;

    [ChangeTracker]
    public bool IsMaterialExpanded
    {
      get => _isMaterialExpanded;
      set => SetProperty(ref _isMaterialExpanded, value);
    }
    private bool _isMaterialExpanded = true;

    [ChangeTracker]
    public bool IsIdentityExpanded
    {
      get => _isIdentityExpanded;
      set => SetProperty(ref _isIdentityExpanded, value);
    }
    private bool _isIdentityExpanded;

    #endregion

    #region Layer

    [ChangeTracker]
    public string LayerName
    {
      get => _layerName;
      set
      {
        if (SetProperty(ref _layerName, value) && _selectedObject != null)
          _selectedObject.LayerId = value;
      }
    }
    private string _layerName = "";

    #endregion
  }

  public class TransformPropertyGroup : BindableObject
  {
    private readonly Properties3DViewModel _owner;

    public TransformPropertyGroup(Properties3DViewModel owner)
    {
      _owner = owner;
    }

    [ChangeTracker]
    public double PositionX
    {
      get => _px;
      set { if (SetProperty(ref _px, value)) ApplyPosition(); }
    }
    private double _px;

    [ChangeTracker]
    public double PositionY
    {
      get => _py;
      set { if (SetProperty(ref _py, value)) ApplyPosition(); }
    }
    private double _py;

    [ChangeTracker]
    public double PositionZ
    {
      get => _pz;
      set { if (SetProperty(ref _pz, value)) ApplyPosition(); }
    }
    private double _pz;

    [ChangeTracker]
    public double ScaleX
    {
      get => _sx;
      set { if (SetProperty(ref _sx, value)) ApplyScale(); }
    }
    private double _sx = 1;

    [ChangeTracker]
    public double ScaleY
    {
      get => _sy;
      set { if (SetProperty(ref _sy, value)) ApplyScale(); }
    }
    private double _sy = 1;

    [ChangeTracker]
    public double ScaleZ
    {
      get => _sz;
      set { if (SetProperty(ref _sz, value)) ApplyScale(); }
    }
    private double _sz = 1;

    private void ApplyPosition()
    {
      if (_owner.SelectedObject == null) return;
      _owner.SelectedObject.Transform.Position = new System.Windows.Media.Media3D.Point3D(_px, _py, _pz);
    }

    private void ApplyScale()
    {
      if (_owner.SelectedObject == null) return;
      _owner.SelectedObject.Transform.Scale = new System.Windows.Media.Media3D.Vector3D(_sx, _sy, _sz);
    }
  }

  public class MaterialPropertyGroup : BindableObject
  {
    private readonly Properties3DViewModel _owner;

    public MaterialPropertyGroup(Properties3DViewModel owner)
    {
      _owner = owner;
    }

    [ChangeTracker]
    public Color DiffuseColor
    {
      get => _diffuseColor;
      set { if (SetProperty(ref _diffuseColor, value)) ApplyMaterial(); }
    }
    private Color _diffuseColor = Colors.LightGray;

    [ChangeTracker]
    public double Transparency
    {
      get => _transparency;
      set { if (SetProperty(ref _transparency, value)) ApplyMaterial(); }
    }
    private double _transparency;

    private void ApplyMaterial()
    {
      if (_owner.SelectedObject == null) return;
      var mat = _owner.SelectedObject.Material as Models.Material3D;
      if (mat != null)
        mat.DiffuseColor = _diffuseColor;
    }
  }
}
