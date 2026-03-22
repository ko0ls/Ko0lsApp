using System;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Arrow2DSetting
{
  private static readonly Lazy<Arrow2DSetting> _instance = new(() => new Arrow2DSetting());

  public static Arrow2DSetting Instance => _instance.Value;

  public double DotWidth { get; set; } = 2;
  public double DiagonalLength { get; set; } = 4;
}
