using System.Windows;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation.Canvas.Annotation;

public abstract class AnnotationBase : BindableObject
{
  private bool _isMoveOver;
  private bool _isSelected;

  protected AnnotationBase()
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

  protected virtual void OnIsMoveOverChanged(bool value)
  {
  }

  protected virtual void OnIsSelectedChanged(bool value)
  {
  }

  public virtual void MakeHighLight()
  {
    IsSelected = true;
  }

  public virtual void ResetHighLight()
  {
    IsSelected = false;
  }

  public virtual bool CheckMoveOver(Point point)
  {
    return false;
  }

  private void OnModelPropertyChanged(object sender, ModelPropertyChangedEventArgs e)
  {
    switch (e.PropertyName) {
      case nameof(IsMoveOver):
        if (e.Value is bool moveOverValue)
          OnIsMoveOverChanged(moveOverValue);
        break;
      case nameof(IsSelected):
        if (e.Value is bool selectedValue)
          OnIsSelectedChanged(selectedValue);
        break;
    }
  }
}