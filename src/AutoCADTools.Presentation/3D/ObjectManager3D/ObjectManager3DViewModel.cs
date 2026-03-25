using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AutoCADTools.Core._3D;
using AutoCADTools.Core._3D.Enums;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;
using AutoCADTools.Service._3D;

namespace AutoCADTools.Presentation._3D.ObjectManager3D
{
  public class ObjectManager3DViewModel : BindableObject
  {
    private readonly IObjectManager3D _objectManager;

    public ObjectManager3DViewModel(IObjectManager3D objectManager)
    {
      _objectManager = objectManager;
      TreeItems = new ObservableCollection<ObjectTreeItem>();

      _objectManager.ObjectAdded += (s, e) => RebuildTree();
      _objectManager.ObjectRemoved += (s, e) => RebuildTree();
      _objectManager.SceneCleared += (s, e) => RebuildTree();

      RebuildTree();

      SelectItemCommand = new RelayCommand<ObjectTreeItem>(null, item =>
      {
        if (item == null) return;
        if (item.Object != null)
        {
          _objectManager.DeselectAll();
          _objectManager.Select(item.Object.Id);
        }
        else if (item.Layer != null)
        {
          _objectManager.DeselectAll();
          foreach (var objId in item.Layer.ObjectIds)
            _objectManager.Select(objId);
        }
      });

      DeleteSelectedCommand = new RelayCommand(() =>
      {
        var ids = _objectManager.SelectedObjects.Select(o => o.Id).ToList();
        foreach (var id in ids)
          _objectManager.Remove(id);
      }, () => _objectManager.SelectedObjects.Count > 0);

      ToggleVisibilityCommand = new RelayCommand<ObjectTreeItem>(null, item =>
      {
        if (item?.Object != null)
          item.Object.IsVisible = !item.Object.IsVisible;
        else if (item?.Layer != null)
          item.Layer.IsVisible = !item.Layer.IsVisible;
      });

      CreateLayerCommand = new RelayCommand<string>(null, name =>
      {
        _objectManager.CreateLayer(name ?? "New Layer");
        RebuildTree();
      });

      CreateGroupCommand = new RelayCommand<string>(null, name =>
      {
        _objectManager.CreateGroup(name ?? "New Group");
        RebuildTree();
      });
    }

    public ObservableCollection<ObjectTreeItem> TreeItems { get; }

    [ChangeTracker]
    public string SearchText
    {
      get => _searchText;
      set { if (SetProperty(ref _searchText, value)) ApplyFilter(); }
    }
    private string _searchText = "";

    [ChangeTracker]
    public ObjectTreeItem? SelectedTreeItem { get; set; }

    public ICommand SelectItemCommand { get; }
    public ICommand DeleteSelectedCommand { get; }
    public ICommand ToggleVisibilityCommand { get; }
    public ICommand CreateLayerCommand { get; }
    public ICommand CreateGroupCommand { get; }

    private void RebuildTree()
    {
      TreeItems.Clear();

      // Layers
      foreach (var layer in _objectManager.Layers)
      {
        var layerItem = new ObjectTreeItem
        {
          Name = layer.Name,
          ItemType = ObjectTreeItemType.Layer,
          Layer = layer,
          IsExpanded = true
        };

        // Groups under layer
        foreach (var group in _objectManager.Groups)
        {
          var groupItem = new ObjectTreeItem
          {
            Name = group.Name,
            ItemType = ObjectTreeItemType.Group,
            Group = group,
            IsExpanded = true
          };

          // Objects under group
          foreach (var obj in _objectManager.Objects)
          {
            if (!string.IsNullOrEmpty(obj.GroupId) && obj.GroupId == group.Id.ToString())
            {
              var objItem = new ObjectTreeItem
              {
                Name = obj.Name,
                ItemType = ObjectTreeItemType.Object,
                Object = obj
              };
              if (obj.IsSelected)
                objItem.IsSelected = true;
              groupItem.Children.Add(objItem);
            }
          }

          if (groupItem.Children.Count > 0)
            layerItem.Children.Add(groupItem);
        }

        // Objects not in any group
        foreach (var obj in _objectManager.Objects)
        {
          if (string.IsNullOrEmpty(obj.GroupId))
          {
            var objItem = new ObjectTreeItem
            {
              Name = obj.Name,
              ItemType = ObjectTreeItemType.Object,
              Object = obj
            };
            if (obj.IsSelected)
              objItem.IsSelected = true;
            layerItem.Children.Add(objItem);
          }
        }

        TreeItems.Add(layerItem);
      }
    }

    private void ApplyFilter()
    {
      // Filter logic — simple text search
      if (string.IsNullOrEmpty(_searchText))
      {
        RebuildTree();
        return;
      }

      // TODO: apply search filter
    }
  }

  public enum ObjectTreeItemType
  {
    Layer,
    Group,
    Object
  }

  public class ObjectTreeItem : BindableObject
  {
    public ObjectTreeItem()
    {
      Children = new ObservableCollection<ObjectTreeItem>();
    }

    public ObjectTreeItemType ItemType { get; set; }
    public string Name { get; set; } = "";
    public IObject3DBase? Object { get; set; }
    public ILayer? Layer { get; set; }
    public IGroup? Group { get; set; }

    public ObservableCollection<ObjectTreeItem> Children { get; }

    [ChangeTracker]
    public bool IsExpanded
    {
      get => _isExpanded;
      set => SetProperty(ref _isExpanded, value);
    }
    private bool _isExpanded = true;

    [ChangeTracker]
    public bool IsSelected
    {
      get => _isSelected;
      set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;
  }
}
