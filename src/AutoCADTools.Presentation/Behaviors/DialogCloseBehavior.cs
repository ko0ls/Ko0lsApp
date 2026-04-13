using System;
using System.Windows;

namespace AutoCADTools.Presentation.Behaviors;

public interface IHasCloseRequest
{
  event Action? CloseRequested;
}

public static class DialogCloseBehavior
{
  public static readonly DependencyProperty DialogResultProperty =
    DependencyProperty.RegisterAttached(
      "DialogResult",
      typeof( bool? ),
      typeof( DialogCloseBehavior ),
      new PropertyMetadata( false, OnDialogResultChanged ) );

  public static bool? GetDialogResult(Window target) => (bool?) target.GetValue(DialogResultProperty);
  public static void SetDialogResult(Window target, bool? value) => target.SetValue(DialogResultProperty, value);

  private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is Window window && e.NewValue is bool result) {
      window.DialogResult = result;
    }
  }
}
