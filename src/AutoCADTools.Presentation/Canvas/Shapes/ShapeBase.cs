using System.Windows;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation.Canvas.Shapes;

public abstract partial class ShapeBase : BindableObject
{
  private bool _isMoveOver;
  private bool _isSelected;

  protected ShapeBase()
  {
    ModelPropertyChanged += OnModelPropertyChanged;
  }

  [ChangeTracker]
  public bool IsMoveOver
  {
    get => _isMoveOver;
    set => SetProperty(ref _isMoveOver, value);
  }

  [ChangeTracker]
  public bool IsSelected
  {
    get => _isSelected;
    set => SetProperty(ref _isSelected, value);
  }

  private void OnModelPropertyChanged(object sender, ModelPropertyChangedEventArgs e)
  {
    switch (e.PropertyName) {
      case nameof(IsMoveOver):
        if (e.Value is bool moveOverValue) OnIsMoveOverChanged(moveOverValue);
        break;
      case nameof(IsSelected):
        if (e.Value is bool selectedValue) OnIsSelectedChanged(selectedValue);
        break;
    }
  }

  private void OnIsMoveOverChanged(bool value)
  {
    if (value) {
      MakeHighLight();
    }
    else if (!IsSelected) {
      ResetHighLight();
    }
  }

  private void OnIsSelectedChanged(bool value)
  {
    if (value) {
      MakeHighLight();
    }
    else {
      ResetHighLight();
    }
  }

  public virtual void MakeHighLight()
  {
  }

  public virtual void ResetHighLight()
  {
  }

  public virtual bool CheckMoveOver(Point currentPoint)
  {
    return false;
  }
}