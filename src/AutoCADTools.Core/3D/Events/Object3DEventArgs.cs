using System;

namespace AutoCADTools.Core._3D.Events
{
  public class Object3DEventArgs : EventArgs
  {
    public IObject3DBase Object { get; }

    public Object3DEventArgs(IObject3DBase obj)
    {
      Object = obj;
    }
  }
}
