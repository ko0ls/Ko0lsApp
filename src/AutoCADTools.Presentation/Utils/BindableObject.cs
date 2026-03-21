using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation.Utils;

public abstract partial class BindableObject : IRevertibleChangeTracking, INotifyPropertyChanged, INotifyDataErrorInfo
{
  protected BindableObject()
  {
    TrackedDictionary = new Dictionary<string, ITrackedChange>();
    PropErrors = new Dictionary<string, List<string>>();
  }

  protected class TrackedChange : ITrackedChange
  {
    public TrackedChange(Func<object?, object?, bool> hasChangedFunc, PropertyInfo propertyInfo)
    {
      HasChangedFunc = hasChangedFunc;
      Property = propertyInfo;
    }

    public string PropertyName => Property.Name;
    public object? OriginalValue { get; set; }
    public object? CurrentValue { get; set; }

    public enum TrackingStatus
    {
      Unchecked,
      Checked
    }

    public PropertyInfo Property { get; set; }
    public TrackingStatus Status { get; set; } = TrackingStatus.Unchecked;

    public bool HasChanges => Status == TrackingStatus.Checked && DetectChange(CurrentValue);

    public DateTime? LastChecked { get; set; }

    private Func<object?, object?, bool> HasChangedFunc { get; set; }
    public bool DetectChange(object? currentValue)
    {
      var hasChanged = HasChangedFunc(OriginalValue, currentValue);
      IsDirty = hasChanged;
      return hasChanged;
    }

    public bool IsDirty { get; set; }

    public void AcceptChange()
    {
      if (CurrentValue != null) OriginalValue = CurrentValue is string ? CurrentValue : CurrentValue.Clone();
      RejectChange();
    }

    public void RejectChange()
    {
      CurrentValue = OriginalValue is string ? OriginalValue : OriginalValue.Clone();
      Status = TrackingStatus.Unchecked;
      LastChecked = null;
    }
  }
}