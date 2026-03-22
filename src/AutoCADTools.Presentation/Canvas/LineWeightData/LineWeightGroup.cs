using System.Collections.Generic;

namespace AutoCADTools.Presentation.Canvas.LineWeightData;

public class LineWeightGroup
{
  public double Scale { get; set; }
  public List<LineWeight> LineWeights { get; set; } = new List<LineWeight>();
}
