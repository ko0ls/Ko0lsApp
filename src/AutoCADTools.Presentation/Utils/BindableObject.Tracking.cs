using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation.Utils;

public abstract partial class BindableObject
{
  #region ChangeTracking

  public void AddEventsForPropsDirty()
  {
    ModelPropertyChanged -= OnModelPropertyChanged;
    ModelPropertyChanged += OnModelPropertyChanged;
  }

  private void OnModelPropertyChanged(object sender, ModelPropertyChangedEventArgs e)
  {
    if (!TrackedDictionary.TryGetValue(e.PropertyName, out var value) ||
        value is not TrackedChange trackedChange) return;
    IsAnyPropsDirty = trackedChange.DetectChange(e.Value);
    if (!IsAnyPropsDirty) {
      IsAnyPropsDirty = TrackedDictionary.Select(x => x.Value).OfType<TrackedChange>().Any(x => x.IsDirty);
    }
  }

  /// <summary>
  /// Starts the tracking operation on the object. Changes made to the
  /// object will start being tracked the moment this function is called
  /// </summary>
  /// <exception>Changes have already begun</exception>
  public void BeginChanges()
  {
    ThrowIfTrackingStarted();
    Track(GetTrackableProperties());
    IsChanged = false;
  }

  /// <summary>
  /// Ends the tracking operation on the object. Any pending tracked changes made to the
  /// object since either <see cref="BeginChanges"/> was called, or since <see cref="AcceptChanges"/>
  /// was last called will be lost the moment this function is called
  /// </summary>
  /// <exception>Change tracking has not started</exception>
  public virtual void EndChanges()
  {
    if (!TrackedDictionary.Any()) return;
    TrackedDictionary.Clear();
    IsChanged = false;
    IsAnyPropsDirty = false;
  }

  /// <summary>
  /// Sets the current object state as its default state by accepting the modifications.
  /// Commits all the changes made to this object since either <see cref="BeginChanges"/>
  /// was called, or since <see cref="AcceptChanges"/> was last called
  /// </summary>
  /// <exception>Change tracking has not started</exception>
  public virtual void AcceptChanges()
  {
    ThrowIfTrackingNotStarted();
    var trackedChanges = UpdateTracked();
    foreach (var change in trackedChanges) {
      change.AcceptChange();
    }

    UpdateTracked();
    IsChanged = false;
  }

  /// <summary>
  /// Resets the current objects state by rejecting the modifications. Rejects
  /// all changes made to the object since either <see cref="BeginChanges"/>
  /// was called, or since <see cref="AcceptChanges"/> was last called
  /// </summary>
  /// <exception>Change tracking has not started</exception>
  public virtual void RejectChanges()
  {
    ThrowIfTrackingNotStarted();
    var trackedChanges = GetTrackedChanges();
    foreach (var change in trackedChanges) {
      SetValue(change.Property, change.OriginalValue);
      change.RejectChange();
    }

    IsChanged = false;
  }

  /// <summary>
  /// Returns a list containing information of all the properties with changes
  /// applied to it since either <see cref="BeginChanges"/> was called, or
  /// since <see cref="AcceptChanges"/> was last called
  /// </summary>
  /// <returns>A list containing the changes made to the object</returns>
  /// <exception>Change tracking has not started</exception>
  public virtual List<ITrackedChange> GetChanges()
  {
    ThrowIfTrackingNotStarted();
    var trackedChanges = UpdateTracked();
    return trackedChanges.Where(change => change.HasChanges).Cast<ITrackedChange>().ToList();
  }

  /// <summary>
  /// Updates the tracking status of all the properties being tracked
  /// </summary>
  /// <returns>A list containing properties being tracked</returns>
  protected IEnumerable<TrackedChange> UpdateTracked()
  {
    var trackedChanges = GetTrackedChanges().ToList();
    Track(trackedChanges.Select(x => x.Property));
    return trackedChanges;
  }

  /// <summary>
  /// Returns a list containing information of all the properties being tracked
  /// </summary>
  /// <returns>A list containing property tracking information</returns>
  /// <exception>Change tracking has not started</exception>
  public List<ITrackedChange> GetTracked()
  {
    ThrowIfTrackingNotStarted();
    return TrackedDictionary.Values.ToList();
  }

  /// <summary>
  /// Keeps track of the original and current values of the provided properties
  /// </summary>
  /// <param name="properties">The properties to track</param>
  protected void Track(IEnumerable<PropertyInfo> properties)
  {
    foreach (var prop in properties) {
      if (prop.Name != string.Empty) {
        Track(prop, GetValue(prop));
      }
    }
  }

  /// <summary>
  /// Keeps track of the original and current value of the provided property
  /// </summary>
  protected void Track(PropertyInfo property, object? currentValue)
  {
    TrackedChange? currentChange;
    var isValueType = false;
    if (currentValue != null) {
      var underlyingType = Nullable.GetUnderlyingType(currentValue.GetType());
      isValueType = underlyingType?.IsValueType ?? currentValue.GetType().IsValueType;
    }

    if (!IsTrackedChange(property.Name)) {
      bool HasChangedFunc(object? original, object? current) => HasValuesChanged(original, current);
      currentChange = new TrackedChange(HasChangedFunc, property) {
        OriginalValue = currentValue == null ? null :
              isValueType || currentValue is string ? currentValue : currentValue.Clone()
      };

      TrackedDictionary.Add(property.Name, currentChange);
    }
    else {
      currentChange = GetTrackedChange(property.Name);
      if (currentChange == null) return;
      currentChange.CurrentValue = isValueType || currentValue is string ? currentValue : currentValue.Clone();
      currentChange.Status = TrackedChange.TrackingStatus.Checked;
      currentChange.LastChecked = DateTime.Now;
    }

    if (currentChange.HasChanges) {
      IsChanged = true;
      currentChange.IsDirty = true;
    }
    else {
      currentChange.IsDirty = false;
    }
  }

  /// <summary>
  /// Compare two object. Note: if T is collection of custom class,
  /// override two functions are Equals and GetHashCode
  /// to override compare function
  /// </summary>
  protected virtual bool HasValuesChanged<T>(T originalValue, T currentValue)
  {
    if (currentValue == null) return originalValue != null;
    if (originalValue == null) return true;
    try {
      if (currentValue is IEnumerable<double> currentValueDouble && originalValue is IEnumerable<double> originalValueDouble) {
        return currentValueDouble.Count() != originalValueDouble.Count() ||
               !currentValueDouble.SequenceEqual(originalValueDouble);
      }
      if (currentValue is System.Collections.ObjectModel.ObservableCollection<double> currentValueDouble1 && originalValue is System.Collections.ObjectModel.ObservableCollection<double>
          originalValueDouble1) {
        return currentValueDouble1.Count != originalValueDouble1.Count ||
               !currentValueDouble1.SequenceEqual(originalValueDouble1);
      }
      var collectionCurrent = ((IEnumerable<T>) currentValue).ToList();
      var collectionOrigin = ((IEnumerable<T>) originalValue).ToList();
      var res = collectionCurrent.Count != collectionOrigin.Count ||
                !collectionCurrent.SequenceEqual(collectionOrigin);
      return res;
    }
    catch {
      var res = !EqualityComparer<T>.Default.Equals(originalValue, currentValue);
      return res;
    }
  }

  protected IEnumerable<TrackedChange> GetTrackedChanges() => TrackedDictionary.Select(pair => (TrackedChange) pair.Value);

  protected TrackedChange? GetTrackedChange(string propertyName) => IsTrackedChange(propertyName) ? (TrackedChange) TrackedDictionary[propertyName] : null;

  private bool IsTrackedChange(string propertyName) => TrackedDictionary.ContainsKey(propertyName);

  protected virtual object? GetValue(PropertyInfo property) => property.GetValue(this, property.GetIndexParameters().Length == 1 ? new object?[] { null } : null);

  protected virtual void SetValue(PropertyInfo property, object? value)
  {
    if (!property.CanWrite) return;
    property.SetValue(this, value);
  }

  /// <summary>
  /// Get all properties can read, can write, dont's have attribute "ChangeTrackerIgnore" and property is not ViewModalBase class
  /// </summary>
  protected virtual IEnumerable<PropertyInfo> GetTrackableProperties()
  {
    return GetType().GetProperties().Where(p =>
        p.DeclaringType != typeof(BindableObject) && p is { CanRead: true, CanWrite: true } &&
        p.GetCustomAttributes<ChangeTrackerAttribute>(false).Any());
  }

  private void ThrowIfTrackingNotStarted()
  {
    if (!IsTracking) {
      BeginChanges();
    }
  }

  private void ThrowIfTrackingStarted()
  {
    if (IsTracking) {
      throw new InvalidOperationException("Change tracking has already started");
    }
  }

  #endregion
}