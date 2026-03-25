using System;
using System.Collections.Generic;

namespace AutoCADTools.Core._3D.Events
{
  public class SelectionChangedEventArgs : EventArgs
  {
    public IReadOnlyList<IObject3DBase> Added { get; }
    public IReadOnlyList<IObject3DBase> Removed { get; }

    public SelectionChangedEventArgs(
      IReadOnlyList<IObject3DBase> added,
      IReadOnlyList<IObject3DBase> removed)
    {
      Added = added;
      Removed = removed;
    }
  }
}
