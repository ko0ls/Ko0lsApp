using AutoCADTools.Core;

namespace AutoCADTools.Presentation.Drawing;

public interface IFoundationDrawer
{
  void RefreshDrawing(SwallowFoundationModel? model);
}
