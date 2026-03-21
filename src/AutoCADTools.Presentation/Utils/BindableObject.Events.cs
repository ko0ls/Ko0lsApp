using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoCADTools.Presentation.Utils;

public abstract partial class BindableObject
{
  #region Events

  public event PropertyChangedEventHandler? PropertyChanged;

  /// <summary>
  /// Fires the PropertyChanged event notifying that the specified property value changed
  /// </summary>
  protected void NotifyPropertyChanged(object? currentValue, [CallerMemberName] string propertyName = "")
  {
    if (PropertyChanged == null) return;
    if (PropErrors != null) Validate(currentValue, propertyName);
    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  /// <summary>
  /// Fires the PropertyChanged event notifying that the specified property value changed
  /// </summary>
  protected virtual void OnPropertyChanged(object? currentValue, [CallerMemberName] string propertyName = "") => NotifyPropertyChanged(currentValue, propertyName);

  protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
  {
    var oldValue = field;
    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    field = value;
    OnPropertyChanged(value, propertyName);
    ModelPropertyChanged?.Invoke(this, new ModelPropertyChangedEventArgs { PropertyName = propertyName, Value = value, OldValue = oldValue });
    return true;
  }

  #endregion

  #region Public Property Changed Event

  public delegate void ModelPropertyChangedHandler(object sender, ModelPropertyChangedEventArgs e);
  public event ModelPropertyChangedHandler? ModelPropertyChanged;

  #endregion
}