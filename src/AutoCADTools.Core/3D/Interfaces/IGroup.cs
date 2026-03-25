using System;
using System.Collections.Generic;

namespace AutoCADTools.Core._3D
{
  public interface IGroup
  {
    Guid Id { get; }
    string Name { get; set; }
    bool IsExpanded { get; set; }
    IReadOnlyCollection<Guid> MemberIds { get; }
    void AddMember(Guid objectId);
    void RemoveMember(Guid objectId);
    void Clear();
  }
}
