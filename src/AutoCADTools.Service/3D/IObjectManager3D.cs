using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Events;

namespace AutoCADTools.Service._3D
{
  public interface IObjectManager3D
  {
    // Collection access
    IReadOnlyCollection<IObject3DBase> Objects { get; }
    IReadOnlyCollection<ILayer> Layers { get; }
    IReadOnlyCollection<IGroup> Groups { get; }

    // Selection
    IReadOnlyList<IObject3DBase> SelectedObjects { get; }
    event EventHandler<SelectionChangedEventArgs> SelectionChanged;

    // Object CRUD
    void Add(IObject3DBase obj);
    void AddRange(IEnumerable<IObject3DBase> objs);
    void Remove(Guid id);
    void RemoveRange(IEnumerable<Guid> ids);
    IObject3DBase GetObject(Guid id);
    IEnumerable<IObject3DBase> Find(Predicate<IObject3DBase> predicate);
    void Clear();

    // Selection
    void Select(Guid id);
    void SelectMany(IEnumerable<Guid> ids);
    void Deselect(Guid id);
    void DeselectAll();
    void SelectAll();
    void ToggleSelection(Guid id);
    void SelectInRect(Rect3D worldRect, bool crossing);

    // Bulk transform
    void MoveSelected(Vector3D delta);
    void RotateSelected(Vector3D axis, double degrees, Point3D center);
    void ScaleSelected(double factor, Point3D center);

    // Group/Layer bulk ops
    void AssignToLayer(IEnumerable<Guid> objectIds, string layerId);
    void AssignToGroup(IEnumerable<Guid> objectIds, string groupId);
    void DetachFromGroup(IEnumerable<Guid> objectIds);
    void SetVisibility(IEnumerable<Guid> objectIds, bool visible);

    // Layer / Group CRUD
    ILayer CreateLayer(string name);
    void DeleteLayer(Guid id);
    IGroup CreateGroup(string name);
    void DeleteGroup(Guid id);

    // Scene events
    event EventHandler<Object3DEventArgs> ObjectAdded;
    event EventHandler<Object3DEventArgs> ObjectRemoved;
    event EventHandler SceneCleared;
    void NotifyObjectChanged(Guid id);
  }
}
