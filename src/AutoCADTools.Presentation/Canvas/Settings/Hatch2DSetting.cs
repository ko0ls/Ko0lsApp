using System;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Hatch2DSetting
{
  private static readonly Lazy<Hatch2DSetting> _instance = new(() => new Hatch2DSetting());

  public static Hatch2DSetting Instance => _instance.Value;

  public double TextFontSize { get; set; } = 5;
}
