using System;
using System.Collections.Generic;

namespace AutoCADTools.Core._3D.Models
{
  public class Group3D : IGroup
  {
    public Group3D()
    {
      MemberIdsList = new HashSet<Guid>();
    }

    public Group3D(string name) : this()
    {
      Name = name;
    }

    private readonly HashSet<Guid> MemberIdsList;

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Group";
    public bool IsExpanded { get; set; } = true;

    public IReadOnlyCollection<Guid> MemberIds => MemberIdsList;

    public void AddMember(Guid objectId) => MemberIdsList.Add(objectId);
    public void RemoveMember(Guid objectId) => MemberIdsList.Remove(objectId);
    public void Clear() => MemberIdsList.Clear();
  }
}
