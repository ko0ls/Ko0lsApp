using System;
using System.Windows.Media;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Span2DSetting
{
  private static readonly Lazy<Span2DSetting> _instance = new(() => new Span2DSetting());

  public static Span2DSetting Instance => _instance.Value;

  public int LineWeightId { get; set; } = 5;
  public Brush? LineColor { get; set; } = Brushes.Black;
  public Brush? FillColor { get; set; } = Brushes.Transparent;
  public double TextFontSize { get; set; } = 5;
  public Brush? TextColor { get; set; } = Brushes.Black;
  public double PileSymbolWidth { get; set; } = 10;
}
