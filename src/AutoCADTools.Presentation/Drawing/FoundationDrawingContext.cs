using System.Windows.Controls;

namespace AutoCADTools.Presentation.Drawing;

public sealed class FoundationDrawingContext
{
  public global::System.Windows.Controls.Canvas Canvas { get; }
  public double Scale { get; }
  public double LineThickness { get; }

  public FoundationDrawingContext(global::System.Windows.Controls.Canvas canvas, double scale, double lineThickness)
  {
    Canvas = canvas;
    Scale = scale;
    LineThickness = lineThickness;
  }
}
