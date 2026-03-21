using System;
using System.Windows.Media;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Line2DSetting
{
  private static readonly Lazy<Line2DSetting> _instance = new(() => new Line2DSetting());

  public static Line2DSetting Instance => _instance.Value;

  public DoubleCollection DashDotSpaces { get; } = new DoubleCollection([12, 3, 3, 3]);
  public DoubleCollection DashSpaces { get; } = new DoubleCollection([4, 2, 4, 2]);
}
