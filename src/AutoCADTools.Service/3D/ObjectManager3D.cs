using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Events;
using AutoCADTools.Core._3D.Models;

namespace AutoCADTools.Service._3D
{
  public class ObjectManager3D : IObjectManager3D
  {
    private readonly Dictionary<Guid, IObject3DBase> _objects = new Dictionary<Guid, IObject3DBase>();
    private readonly HashSet<Guid> _selectedIds = new HashSet<Guid>();
    private readonly List<ILayer> _layers = new List<ILayer>();
    private readonly List<IGroup> _groups = new List<IGroup>();
    private readonly List<IObject3DBase> _selectedObjectsList = new List<IObject3DBase>();

    public ObjectManager3D()
    {
      _layers.Add(new Layer3D("Default"));
    }

    public IReadOnlyCollection<IObject3DBase> Objects => _objects.Values.ToList().AsReadOnly();
    public IReadOnlyCollection<ILayer> Layers => _layers.AsReadOnly();
    public IReadOnlyCollection<IGroup> Groups => _groups.AsReadOnly();
    public IReadOnlyList<IObject3DBase> SelectedObjects => _selectedObjectsList.AsReadOnly();

    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    public event EventHandler<Object3DEventArgs> ObjectAdded;
    public event EventHandler<Object3DEventArgs> ObjectRemoved;
    public event EventHandler SceneCleared;

    #region Object CRUD

    public void Add(IObject3DBase obj)
    {
      if (_objects.ContainsKey(obj.Id)) return;
      _objects[obj.Id] = obj;
      ObjectAdded?.Invoke(this, new Object3DEventArgs(obj));
    }

    public void AddRange(IEnumerable<IObject3DBase> objs)
    {
      foreach (var obj in objs)
        Add(obj);
    }

    public void Remove(Guid id)
    {
      if (!_objects.TryGetValue(id, out var obj)) return;
      _objects.Remove(id);
      Deselect(id);
      ObjectRemoved?.Invoke(this, new Object3DEventArgs(obj));
    }

    public void RemoveRange(IEnumerable<Guid> ids)
    {
      foreach (var id in ids)
        Remove(id);
    }

    public IObject3DBase GetObject(Guid id)
    {
      _objects.TryGetValue(id, out var obj);
      return obj;
    }

    public IEnumerable<IObject3DBase> Find(Predicate<IObject3DBase> predicate)
    {
      return _objects.Values.Where(o => predicate(o));
    }

    public void Clear()
    {
      _objects.Clear();
      _selectedIds.Clear();
      _selectedObjectsList.Clear();
      SceneCleared?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Selection

    private void RebuildSelectedList()
    {
      _selectedObjectsList.Clear();
      foreach (var id in _selectedIds)
      {
        if (_objects.TryGetValue(id, out var obj))
          _selectedObjectsList.Add(obj);
      }
    }

    private void RaiseSelectionChanged(List<IObject3DBase> added, List<IObject3DBase> removed)
    {
      SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
        added.AsReadOnly(), removed.AsReadOnly()));
    }

    public void Select(Guid id)
    {
      if (_selectedIds.Add(id))
      {
        RebuildSelectedList();
        RaiseSelectionChanged(_selectedObjectsList, new List<IObject3DBase>());
      }
    }

    public void SelectMany(IEnumerable<Guid> ids)
    {
      var added = new List<IObject3DBase>();
      foreach (var id in ids)
      {
        if (_selectedIds.Add(id) && _objects.TryGetValue(id, out var obj))
          added.Add(obj);
      }
      if (added.Count > 0)
      {
        RebuildSelectedList();
        RaiseSelectionChanged(added, new List<IObject3DBase>());
      }
    }

    public void Deselect(Guid id)
    {
      if (_selectedIds.Remove(id))
      {
        RebuildSelectedList();
        RaiseSelectionChanged(new List<IObject3DBase>(), _selectedObjectsList);
      }
    }

    public void DeselectAll()
    {
      if (_selectedIds.Count == 0) return;
      var removed = _selectedObjectsList.ToList();
      _selectedIds.Clear();
      _selectedObjectsList.Clear();
      RaiseSelectionChanged(new List<IObject3DBase>(), removed);
    }

    public void SelectAll()
    {
      var added = new List<IObject3DBase>();
      foreach (var kvp in _objects)
      {
        if (_selectedIds.Add(kvp.Key))
          added.Add(kvp.Value);
      }
      if (added.Count > 0)
      {
        RebuildSelectedList();
        RaiseSelectionChanged(added, new List<IObject3DBase>());
      }
    }

    public void ToggleSelection(Guid id)
    {
      if (_selectedIds.Contains(id))
        Deselect(id);
      else
        Select(id);
    }

    public void SelectInRect(Rect3D worldRect, bool crossing)
    {
      var found = new List<IObject3DBase>();
      foreach (var obj in _objects.Values)
      {
        var bb = obj.GetBoundingBox();
        var intersects = RectsIntersect(bb, worldRect);
        if (crossing ? intersects : RectContains(worldRect, bb))
        {
          found.Add(obj);
        }
      }

      var added = new List<IObject3DBase>();
      foreach (var obj in found)
      {
        if (_selectedIds.Add(obj.Id))
          added.Add(obj);
      }
      if (added.Count > 0)
      {
        RebuildSelectedList();
        RaiseSelectionChanged(added, new List<IObject3DBase>());
      }
    }

    private static bool RectsIntersect(Rect3D a, Rect3D b)
    {
      return a.X <= b.X + b.SizeX && a.X + a.SizeX >= b.X
          && a.Y <= b.Y + b.SizeY && a.Y + a.SizeY >= b.Y
          && a.Z <= b.Z + b.SizeZ && a.Z + a.SizeZ >= b.Z;
    }

    private static bool RectContains(Rect3D outer, Rect3D inner)
    {
      return outer.X <= inner.X
          && outer.Y <= inner.Y
          && outer.Z <= inner.Z
          && outer.X + outer.SizeX >= inner.X + inner.SizeX
          && outer.Y + outer.SizeY >= inner.Y + inner.SizeY
          && outer.Z + outer.SizeZ >= inner.Z + inner.SizeZ;
    }

    #endregion

    #region Bulk Transform

    public void MoveSelected(Vector3D delta)
    {
      foreach (var obj in _selectedObjectsList)
        obj.Transform.Translate(delta);
      NotifySelectedChanged();
    }

    public void RotateSelected(Vector3D axis, double degrees, Point3D center)
    {
      foreach (var obj in _selectedObjectsList)
      {
        obj.Transform.Translate(new Vector3D(-center.X, -center.Y, -center.Z));
        obj.Transform.Rotate(axis, degrees);
        obj.Transform.Translate(new Vector3D(center.X, center.Y, center.Z));
      }
      NotifySelectedChanged();
    }

    public void ScaleSelected(double factor, Point3D center)
    {
      foreach (var obj in _selectedObjectsList)
      {
        obj.Transform.Translate(new Vector3D(-center.X, -center.Y, -center.Z));
        obj.Transform.Scale = new Vector3D(factor, factor, factor);
        obj.Transform.Translate(new Vector3D(center.X, center.Y, center.Z));
      }
      NotifySelectedChanged();
    }

    private void NotifySelectedChanged()
    {
      foreach (var obj in _selectedObjectsList)
        NotifyObjectChanged(obj.Id);
    }

    #endregion

    #region Bulk Assignment

    public void AssignToLayer(IEnumerable<Guid> objectIds, string layerId)
    {
      foreach (var id in objectIds)
      {
        if (_objects.TryGetValue(id, out var obj))
          obj.LayerId = layerId;
      }
    }

    public void AssignToGroup(IEnumerable<Guid> objectIds, string groupId)
    {
      foreach (var id in objectIds)
      {
        if (_objects.TryGetValue(id, out var obj))
          obj.GroupId = groupId;
      }
    }

    public void DetachFromGroup(IEnumerable<Guid> objectIds)
    {
      foreach (var id in objectIds)
      {
        if (_objects.TryGetValue(id, out var obj))
          obj.GroupId = "";
      }
    }

    public void SetVisibility(IEnumerable<Guid> objectIds, bool visible)
    {
      foreach (var id in objectIds)
      {
        if (_objects.TryGetValue(id, out var obj))
          obj.IsVisible = visible;
      }
    }

    #endregion

    #region Layer / Group CRUD

    public ILayer CreateLayer(string name)
    {
      var layer = new Layer3D(name);
      _layers.Add(layer);
      return layer;
    }

    public void DeleteLayer(Guid id)
    {
      _layers.RemoveAll(l => l.Id == id);
    }

    public IGroup CreateGroup(string name)
    {
      var group = new Group3D(name);
      _groups.Add(group);
      return group;
    }

    public void DeleteGroup(Guid id)
    {
      _groups.RemoveAll(g => g.Id == id);
    }

    #endregion

    public void NotifyObjectChanged(Guid id)
    {
      if (_objects.TryGetValue(id, out var obj))
        ObjectAdded?.Invoke(this, new Object3DEventArgs(obj));
    }
  }
}
