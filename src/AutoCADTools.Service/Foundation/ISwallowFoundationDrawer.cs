using AutoCADTools.Core;
using Autodesk.AutoCAD.Geometry;

namespace AutoCADTools.Service.Foundation;

public interface ISwallowFoundationDrawer
{
  void Draw(SwallowFoundationModel model, Point3d basePoint, string sectionName);
}
