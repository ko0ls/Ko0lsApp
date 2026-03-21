using AutoCADTools.Presentation.Canvas.Utils;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class UtilsPointTests : IDisposable
{
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  #region IsValid

  [Fact]
  public void IsValid_ReturnsTrue_ForValidPoint()
  {
    var p = new System.Windows.Point(1, 2);

    p.IsValid().Should().BeTrue();
  }

  [Fact]
  public void IsValid_ReturnsFalse_ForNaNPoint()
  {
    var pNaNX = new System.Windows.Point(double.NaN, 1);
    var pNaNY = new System.Windows.Point(1, double.NaN);

    pNaNX.IsValid().Should().BeFalse();
    pNaNY.IsValid().Should().BeFalse();
  }

  #endregion

  #region DistanceTo

  [Fact]
  public void DistanceTo_ReturnsCorrectDistance_ForRightTriangle()
  {
    var p1 = new System.Windows.Point(0, 0);
    var p2 = new System.Windows.Point(3, 4);

    var result = p1.DistanceTo(p2);

    result.Should().Be(5.0);
  }

  [Fact]
  public void DistanceTo_ReturnsZero_ForSamePoint()
  {
    var p = new System.Windows.Point(7, 3);

    p.DistanceTo(p).Should().Be(0);
  }

  [Fact]
  public void DistanceTo_ReturnsCorrectDistance_NegativeCoordinates()
  {
    var p1 = new System.Windows.Point(-3, -4);
    var p2 = new System.Windows.Point(0, 0);

    p1.DistanceTo(p2).Should().Be(5.0);
  }

  [Fact]
  public void DistanceTo_IsSymmetric()
  {
    var p1 = new System.Windows.Point(1, 1);
    var p2 = new System.Windows.Point(4, 5);

    p1.DistanceTo(p2).Should().Be(p2.DistanceTo(p1));
  }

  #endregion

  #region MidPoint

  [Theory]
  [InlineData(0, 0, 2, 2, 1, 1)]
  [InlineData(0, 0, 4, 0, 2, 0)]
  [InlineData(1, 1, 3, 3, 2, 2)]
  [InlineData(-2, -4, 2, 4, 0, 0)]
  public void MidPoint_ReturnsCorrectCoordinates(
    double x1, double y1, double x2, double y2,
    double expectedX, double expectedY)
  {
    var p1 = new System.Windows.Point(x1, y1);
    var p2 = new System.Windows.Point(x2, y2);

    var result = UtilsPoint.MidPoint(p1, p2);

    result.X.Should().Be(expectedX);
    result.Y.Should().Be(expectedY);
  }

  [Fact]
  public void MidPoint_ReturnsSamePoint_ForIdenticalPoints()
  {
    var p = new System.Windows.Point(3, 7);

    var result = UtilsPoint.MidPoint(p, p);

    result.X.Should().Be(3);
    result.Y.Should().Be(7);
  }

  #endregion

  #region IsAlmostEqualTo

  [Fact]
  public void IsAlmostEqualTo_ReturnsTrue_ForSamePoint()
  {
    var p1 = new System.Windows.Point(1.0000000001, 2.0000000001);
    var p2 = new System.Windows.Point(1, 2);

    p1.IsAlmostEqualTo(p2).Should().BeTrue();
  }

  [Fact]
  public void IsAlmostEqualTo_ReturnsFalse_ForDifferentPoints()
  {
    var p1 = new System.Windows.Point(1, 2);
    var p2 = new System.Windows.Point(3, 4);

    p1.IsAlmostEqualTo(p2).Should().BeFalse();
  }

  #endregion

  #region ProjectOnLineBound

  [Fact]
  public void ProjectOnLineBound_ReturnsProjectedPoint_OnLine()
  {
    var point = new System.Windows.Point(1.5, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 0);

    var result = point.ProjectOnLineBound(start, end);

    result.X.Should().Be(1.5);
    result.Y.Should().Be(0);
  }

  [Fact]
  public void ProjectOnLineBound_ReturnsStartPoint_WhenPointBeforeSegment()
  {
    // Point at x=-1 is outside the segment, should clamp to start point (0,0)
    var point = new System.Windows.Point(-1, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 0);

    var result = point.ProjectOnLineBound(start, end);

    result.Should().Be(start);
  }

  [Fact]
  public void ProjectOnLineBound_ReturnsEndPoint_WhenPointAfterSegment()
  {
    // Point at x=5 is outside the segment, should clamp to end point (3,0)
    var point = new System.Windows.Point(5, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 0);

    var result = point.ProjectOnLineBound(start, end);

    result.Should().Be(end);
  }

  [Fact]
  public void ProjectOnLineBound_ReturnsPerpendicularFoot_ForOffLinePoint()
  {
    // Point (0,1) projected onto segment from (0,0) to (4,0) should land at (0,0)
    // because the perpendicular projection falls outside the segment
    var point = new System.Windows.Point(0, 1);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(4, 0);

    var result = point.ProjectOnLineBound(start, end);

    result.X.Should().Be(0);
    result.Y.Should().Be(0);
  }

  [Fact]
  public void ProjectOnLineBound_ReturnsProjectedPoint_ForDiagonalLine()
  {
    // Point (1,1) projected onto diagonal from (0,0) to (4,4)
    var point = new System.Windows.Point(1, 1);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(4, 4);

    var result = point.ProjectOnLineBound(start, end);

    result.X.Should().BeApproximately(1, 0.0001);
    result.Y.Should().BeApproximately(1, 0.0001);
  }

  #endregion

  #region ProjectOnLineUnbound

  [Fact]
  public void ProjectOnLineUnbound_ReturnsProjectedPoint_OnLine()
  {
    var point = new System.Windows.Point(1.5, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 0);

    var result = point.ProjectOnLineUnbound(start, end);

    result.X.Should().BeApproximately(1.5, 0.0001);
    result.Y.Should().BeApproximately(0, 0.0001);
  }

  [Fact]
  public void ProjectOnLineUnbound_ReturnsProjectedPoint_WhenPointBeforeSegment()
  {
    // Point at x=-1 projects freely to x=-1 on the unbounded line
    var point = new System.Windows.Point(-1, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 0);

    var result = point.ProjectOnLineUnbound(start, end);

    result.X.Should().BeApproximately(-1, 0.0001);
    result.Y.Should().BeApproximately(0, 0.0001);
  }

  [Fact]
  public void ProjectOnLineUnbound_ReturnsProjectedPoint_WhenPointAfterSegment()
  {
    // Point at x=5 projects freely to x=5 on the unbounded line
    var point = new System.Windows.Point(5, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(3, 0);

    var result = point.ProjectOnLineUnbound(start, end);

    result.X.Should().BeApproximately(5, 0.0001);
    result.Y.Should().BeApproximately(0, 0.0001);
  }

  #endregion

  #region DistanceToLineBound

  [Fact]
  public void DistanceToLineBound_ReturnsZero_ForPointOnLine()
  {
    var point = new System.Windows.Point(1, 0);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(4, 0);

    point.DistanceToLineBound(start, end).Should().Be(0);
  }

  [Fact]
  public void DistanceToLineBound_ReturnsCorrectDistance_PerpendicularToLine()
  {
    var point = new System.Windows.Point(2, 3);
    var start = new System.Windows.Point(0, 0);
    var end = new System.Windows.Point(4, 0);

    // Horizontal line at y=0, point at y=3 -> distance = 3
    point.DistanceToLineBound(start, end).Should().Be(3);
  }

  [Fact]
  public void DistanceToLineBound_ReturnsCorrectDistance_ForDiagonalLine()
  {
    // Point at origin, line from (1,0) to (0,1) - distance should be sqrt(2)/2
    var point = new System.Windows.Point(0, 0);
    var start = new System.Windows.Point(1, 0);
    var end = new System.Windows.Point(0, 1);

    var result = point.DistanceToLineBound(start, end);

    result.Should().BeApproximately(0.7071, 0.0001);
  }

  #endregion
}
