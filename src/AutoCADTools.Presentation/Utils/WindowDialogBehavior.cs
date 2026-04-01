#nullable enable

using System;
using System.Windows;

namespace AutoCADTools.Presentation.Utils;

public interface IHasCloseRequest
{
  event Action? CloseRequested;
}

public static class WindowDialogBehavior
{
  public static readonly DependencyProperty IsAttachedProperty =
    DependencyProperty.RegisterAttached(
      "IsAttached",
      typeof( bool ),
      typeof( WindowDialogBehavior ),
      new PropertyMetadata( false, OnIsAttachedChanged ) );

  public static bool GetIsAttached( DependencyObject obj )
  {
    return (bool)obj.GetValue( IsAttachedProperty );
  }

  public static void SetIsAttached( DependencyObject obj, bool value )
  {
    obj.SetValue( IsAttachedProperty, value );
  }

  private static void OnIsAttachedChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
  {
    if (d is Window window && (bool)e.NewValue)
    {
      window.ContentRendered += OnWindowReady;
    }
  }

  private static void OnWindowReady( object sender, EventArgs e )
  {
    if (sender is Window window)
    {
      window.ContentRendered -= OnWindowReady;

      if (window.DataContext is IHasCloseRequest vm)
      {
        vm.CloseRequested += () => { window.DialogResult = true; };
      }
    }
  }
}
