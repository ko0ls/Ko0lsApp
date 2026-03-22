using System;
using System.Windows.Media;
using AutoCADTools.Presentation.Canvas.Annotation;

namespace AutoCADTools.Presentation.Canvas.Settings;

public class Dimension2DSetting
{
  private static readonly Lazy<Dimension2DSetting> _instance = new(() => new Dimension2DSetting());

  public static Dimension2DSetting Instance => _instance.Value;

  public int LineWeightId { get; set; } = 2;
  public Brush? DimLineColor { get; set; } = Brushes.Gray;
  public EnumArrowHeadStyle TickMarkType { get; set; } = EnumArrowHeadStyle.Dot;
  public double DimensionLineExtension { get; set; } = 0;
  public double WitnessLineExtension { get; set; } = 1;
  public double WitnessLineGapToElement { get; set; } = 0;
  public double SnapDistance { get; set; } = 10;
  public Brush? TextColor { get; set; } = Brushes.Black;
  public double TextOffset { get; set; } = 0;
  public double TextLeaderOffset { get; set; } = 4;
  public double TextFontSize { get; set; } = 5;
  public int RoundingDigit { get; set; } = 3;
  public EnumTextPlacementVertical TextPlacementVertical { get; set; } = EnumTextPlacementVertical.Above;
  public EnumTextPlacementHorizontal TextPlacementHorizontal { get; set; } = EnumTextPlacementHorizontal.Centered;
  public EnumTextPlacement TextPlacement { get; set; } = EnumTextPlacement.OverDimWithLeader;
}
