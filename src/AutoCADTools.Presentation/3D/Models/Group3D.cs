using System;
using System.Collections.Generic;
using AutoCADTools.Core._3D;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation._3D.Models
{
  public class Group3D : BindableObject, IGroup
  {
    public Group3D()
    {
      _name = "Group";
      _isExpanded = true;
      _memberIds = new HashSet<Guid>();
    }

    public Group3D(string name) : this()
    {
      _name = name;
    }

    private readonly HashSet<Guid> _memberIds;
    private string _name;
    private bool _isExpanded;

    public Guid Id { get; } = Guid.NewGuid();

    [ChangeTracker]
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    [ChangeTracker]
    public bool IsExpanded
    {
      get => _isExpanded;
      set => SetProperty(ref _isExpanded, value);
    }

    public IReadOnlyCollection<Guid> MemberIds => _memberIds;

    public void AddMember(Guid objectId)
    {
      if (_memberIds.Add(objectId))
        OnPropertyChanged(nameof(MemberIds));
    }

    public void RemoveMember(Guid objectId)
    {
      if (_memberIds.Remove(objectId))
        OnPropertyChanged(nameof(MemberIds));
    }

    public void Clear()
    {
      _memberIds.Clear();
      OnPropertyChanged(nameof(MemberIds));
    }
  }
}
