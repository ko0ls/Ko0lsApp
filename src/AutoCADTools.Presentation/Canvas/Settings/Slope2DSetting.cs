using System;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Annotation;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Slope2DSetting
{
  private static readonly Lazy<Slope2DSetting> _instance = new(() => new Slope2DSetting());

  public static Slope2DSetting Instance => _instance.Value;

  public int LineWeightId { get; set; } = 2;
  public double ArrowLength { get; set; } = 10;
  public EnumArrowHeadStyle ArrowStyle { get; set; } = EnumArrowHeadStyle.ArrowOpen;
  public Brush? LineColor { get; set; } = Brushes.Black;
  public Brush? TextColor { get; set; } = Brushes.Black;
  public double TextFontSize { get; set; } = 5;
}
