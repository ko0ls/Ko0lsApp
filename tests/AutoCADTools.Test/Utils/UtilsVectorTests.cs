using AutoCADTools.Presentation.Canvas.Utils;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class UtilsVectorTests : IDisposable
{
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  #region IsValid

  [Fact]
  public void IsValid_ReturnsTrue_ForValidVector()
  {
    var v = new System.Windows.Vector(1, 2);

    v.IsValid().Should().BeTrue();
  }

  [Fact]
  public void IsValid_ReturnsFalse_ForNaNVector()
  {
    var vNaNX = new System.Windows.Vector(double.NaN, 1);
    var vNaNY = new System.Windows.Vector(1, double.NaN);

    vNaNX.IsValid().Should().BeFalse();
    vNaNY.IsValid().Should().BeFalse();
  }

  [Fact]
  public void IsValid_ReturnsTrue_ForZeroVector()
  {
    var v = new System.Windows.Vector(0, 0);

    v.IsValid().Should().BeTrue();
  }

  #endregion

  #region CreateVector

  [Fact]
  public void CreateVector_ReturnsUnitVector_DirectionOnly()
  {
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 4);

    var result = UtilsVector.CreateVector(start, end);

    result.Length.Should().BeApproximately(1.0, 0.0001);
  }

  [Fact]
  public void CreateVector_ReturnsCorrectDirection_ForPositiveQuadrant()
  {
    var start = new System.Windows.Point(1, 1);
    var end = new System.Windows.Point(4, 5);

    var result = UtilsVector.CreateVector(start, end);

    result.X.Should().BeApproximately(0.6, 0.0001);
    result.Y.Should().BeApproximately(0.8, 0.0001);
  }

  [Fact]
  public void CreateVector_ReturnsNegativeComponents_ForReverseDirection()
  {
    var start = new System.Windows.Point(4, 5);
    var end = new System.Windows.Point(1, 1);

    var result = UtilsVector.CreateVector(start, end);

    result.X.Should().BeApproximately(-0.6, 0.0001);
    result.Y.Should().BeApproximately(-0.8, 0.0001);
  }

  #endregion

  #region GetAngle

  [Fact]
  public void GetAngle_ReturnsZero_ForPositiveXAxis()
  {
    var v = new System.Windows.Vector(1, 0);

    v.GetAngle().Should().BeApproximately(0, 0.0001);
  }

  [Fact]
  public void GetAngle_ReturnsNinety_ForPositiveYAxis()
  {
    var v = new System.Windows.Vector(0, 1);

    v.GetAngle().Should().BeApproximately(90, 0.0001);
  }

  [Fact]
  public void GetAngle_ReturnsNegativeNinety_ForNegativeYAxis()
  {
    var v = new System.Windows.Vector(0, -1);

    v.GetAngle().Should().BeApproximately(-90, 0.0001);
  }

  [Fact]
  public void GetAngle_ReturnsNegativeAngle_InFourthQuadrant()
  {
    var v = new System.Windows.Vector(1, -1);

    v.GetAngle().Should().BeApproximately(-45, 0.0001);
  }

  [Fact]
  public void GetAngle_ReturnsOneEighty_ForNegativeXAxis()
  {
    var v = new System.Windows.Vector(-1, 0);

    v.GetAngle().Should().BeApproximately(180, 0.0001);
  }

  #endregion

  #region AngleTo

  [Fact]
  public void AngleTo_ReturnsZero_ForParallelVectors()
  {
    var v1 = new System.Windows.Vector(1, 0);
    var v2 = new System.Windows.Vector(3, 0);

    v1.AngleTo(v2).Should().BeApproximately(0, 0.0001);
  }

  [Fact]
  public void AngleTo_ReturnsCorrectAngle_BetweenPerpendicularVectors()
  {
    var v1 = new System.Windows.Vector(1, 0);
    var v2 = new System.Windows.Vector(0, 1);

    v1.AngleTo(v2).Should().BeApproximately(90, 0.0001);
  }

  [Fact]
  public void AngleTo_ReturnsCorrectAngle_BetweenNonParallelVectors()
  {
    var v1 = new System.Windows.Vector(1, 0);
    var v2 = new System.Windows.Vector(1, 1);

    v1.AngleTo(v2).Should().BeApproximately(45, 0.0001);
  }

  [Fact]
  public void AngleTo_ReturnsSymmetric()
  {
    var v1 = new System.Windows.Vector(1, 0);
    var v2 = new System.Windows.Vector(0, 1);

    v1.AngleTo(v2).Should().Be(v2.AngleTo(v1));
  }

  [Fact]
  public void AngleTo_Returns180_ForOppositeVectors()
  {
    var v1 = new System.Windows.Vector(1, 0);
    var v2 = new System.Windows.Vector(-1, 0);

    v1.AngleTo(v2).Should().BeApproximately(180, 0.0001);
  }

  #endregion
}
