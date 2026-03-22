using System;
using System.Windows.Media;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Grid2DSetting
{
  private static readonly Lazy<Grid2DSetting> _instance = new(() => new Grid2DSetting());

  public static Grid2DSetting Instance => _instance.Value;

  public double EllipseWidth { get; set; } = 10;
  public double EllipseHeight { get; set; } = 8;
  public double TextFontSize { get; set; } = 5;
  public int LineWeightId { get; set; } = 3;
  public Brush? LineColor { get; set; } = Brushes.Magenta;
  public Brush? SymbolColor { get; set; } = Brushes.Black;
}
