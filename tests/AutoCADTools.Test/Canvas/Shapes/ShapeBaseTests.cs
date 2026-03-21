using AutoCADTools.Presentation.Canvas.Shapes;
using FluentAssertions;
using Xunit;

namespace AutoCADTools.Test;

public class ShapeBaseTests : IDisposable
{
  public void Dispose()
  {
    GC.SuppressFinalize(this);
  }

  #region Default State

  [Fact]
  public void IsSelected_Default_IsFalse()
  {
    var shape = new ConcreteShape();

    shape.IsSelected.Should().BeFalse();
  }

  [Fact]
  public void IsMoveOver_Default_IsFalse()
  {
    var shape = new ConcreteShape();

    shape.IsMoveOver.Should().BeFalse();
  }

  #endregion

  #region IsSelected

  [Fact]
  public void IsSelected_WhenSetToTrue_RaisesPropertyChanged()
  {
    var shape = new ConcreteShape();
    var raised = false;
    shape.PropertyChanged += (_, e) =>
    {
      if (e.PropertyName == nameof(ShapeBase.IsSelected)) raised = true;
    };

    shape.IsSelected = true;

    raised.Should().BeTrue();
  }

  [Fact]
  public void IsSelected_WhenSetToFalse_RaisesPropertyChanged()
  {
    var shape = new ConcreteShape();
    shape.IsSelected = true;
    var raiseCount = 0;
    shape.PropertyChanged += (_, e) =>
    {
      if (e.PropertyName == nameof(ShapeBase.IsSelected)) raiseCount++;
    };

    shape.IsSelected = false;

    raiseCount.Should().Be(1);
  }

  [Fact]
  public void IsSelected_DoesNotRaise_WhenSetToSameValue()
  {
    var shape = new ConcreteShape();
    shape.IsSelected = true;
    var raiseCount = 0;
    shape.PropertyChanged += (_, e) =>
    {
      if (e.PropertyName == nameof(ShapeBase.IsSelected)) raiseCount++;
    };

    shape.IsSelected = true;

    raiseCount.Should().Be(0);
  }

  #endregion

  #region IsMoveOver

  [Fact]
  public void IsMoveOver_WhenSetToTrue_RaisesPropertyChanged()
  {
    var shape = new ConcreteShape();
    var raised = false;
    shape.PropertyChanged += (_, e) =>
    {
      if (e.PropertyName == nameof(ShapeBase.IsMoveOver)) raised = true;
    };

    shape.IsMoveOver = true;

    raised.Should().BeTrue();
  }

  [Fact]
  public void IsMoveOver_WhenSetToFalse_AfterTrue_DoesNotTriggerResetHighlight_ThroughIsSelectedFalse()
  {
    var shape = new ConcreteShape();
    shape.IsSelected = true; // highlight active via IsSelected
    shape.IsMoveOver = true; // highlight already on
    shape.ResetCounter();

    shape.IsMoveOver = false;

    // IsMoveOver=false, IsSelected=true -> ResetHighLight NOT called
    shape.ResetHighLightCount.Should().Be(0);
  }

  #endregion

  #region Methods

  [Fact]
  public void MakeHighLight_ByDefault_DoesNotThrow()
  {
    var shape = new ConcreteShape();

    var act = () => shape.MakeHighLight();

    act.Should().NotThrow();
  }

  [Fact]
  public void ResetHighLight_ByDefault_DoesNotThrow()
  {
    var shape = new ConcreteShape();

    var act = () => shape.ResetHighLight();

    act.Should().NotThrow();
  }

  [Fact]
  public void CheckMoveOver_ByDefault_ReturnsFalse()
  {
    var shape = new ConcreteShape();
    var point = new System.Windows.Point(0, 0);

    shape.CheckMoveOver(point).Should().BeFalse();
  }

  #endregion

  #region Highlight Behavior

  [Fact]
  public void IsSelected_True_IncrementsMakeHighLightCount()
  {
    var shape = new ConcreteShape();

    shape.IsSelected = true;

    shape.MakeHighLightCount.Should().Be(1);
  }

  [Fact]
  public void IsSelected_False_IncrementsResetHighLightCount()
  {
    var shape = new ConcreteShape();
    shape.IsSelected = true;

    shape.IsSelected = false;

    shape.ResetHighLightCount.Should().Be(1);
  }

  [Fact]
  public void IsMoveOver_True_IncrementsMakeHighLightCount()
  {
    var shape = new ConcreteShape();

    shape.IsMoveOver = true;

    shape.MakeHighLightCount.Should().Be(1);
  }

  [Fact]
  public void IsMoveOver_True_ThenFalse_WithIsSelectedFalse_IncrementsResetHighLightCount()
  {
    var shape = new ConcreteShape();
    shape.IsMoveOver = true;

    shape.IsMoveOver = false;

    shape.ResetHighLightCount.Should().Be(1);
  }

  [Fact]
  public void IsMoveOver_True_ThenFalse_WithIsSelectedTrue_DoesNotIncrementResetHighLightCount()
  {
    var shape = new ConcreteShape();
    shape.IsSelected = true;
    shape.IsMoveOver = true;

    shape.IsMoveOver = false;

    // ResetHighLight should NOT be called when IsMoveOver goes false
    // if IsSelected is still true (highlight should remain on)
    shape.ResetHighLightCount.Should().Be(0);
  }

  #endregion

  #region Helper

  private class ConcreteShape : ShapeBase
  {
    public int MakeHighLightCount { get; private set; }
    public int ResetHighLightCount { get; private set; }
    public void ResetCounter() { ResetHighLightCount = 0; }

    public override void MakeHighLight()
    {
      MakeHighLightCount++;
    }

    public override void ResetHighLight()
    {
      ResetHighLightCount++;
    }
  }

  #endregion
}
