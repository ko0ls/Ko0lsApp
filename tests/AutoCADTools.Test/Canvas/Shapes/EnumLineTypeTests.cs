using AutoCADTools.Presentation.Canvas.Shapes;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class EnumLineTypeTests
{
  [Fact]
  public void HasExpectedValues()
  {
    Enum.GetValues(typeof(EnumLineType)).Length.Should().Be(3);
    Enum.IsDefined(typeof(EnumLineType), EnumLineType.Solid).Should().BeTrue();
    Enum.IsDefined(typeof(EnumLineType), EnumLineType.Dash).Should().BeTrue();
    Enum.IsDefined(typeof(EnumLineType), EnumLineType.DashDot).Should().BeTrue();
  }
}
