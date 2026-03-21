using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoCADTools.Presentation.Utils.HelperTracking;

namespace AutoCADTools.Presentation.Utils;

public abstract class BindableObject : IRevertibleChangeTracking, INotifyPropertyChanged, INotifyDataErrorInfo
{
    #region Data

    protected Dictionary<string, ITrackedChange> TrackedDictionary { get; set; }
    private bool _isChanged;
    private bool _isValidModel;
    private CancellationTokenSource? _debounceCts;
    private int _debounceTime = 50;
    private bool _isAnyPropsDirty;

    [JsonIgnore]
    public bool IsChanged
    {
        get
        {
            if (!IsTracking) return _isChanged;
            var trackedChanges = UpdateTracked();
            _isChanged = trackedChanges.Where(x => x.Property.Name != string.Empty).Any(x => x.DetectChange(GetValue(x.Property)));
            return _isChanged;
        }
        private set => _isChanged = value;
    }

    [JsonIgnore]
    public bool IsValidModel
    {
        get => _isValidModel;
        set => SetProperty(ref _isValidModel, value);
    }

    private bool IsTracking => TrackedDictionary.Count > 0;

    public bool IsAnyPropsDirty
    {
        get => _isAnyPropsDirty;
        set => SetProperty(ref _isAnyPropsDirty, value);
    }

    #endregion

    #region Constructors

    protected BindableObject()
    {
        TrackedDictionary = new Dictionary<string, ITrackedChange>();
        PropErrors = new Dictionary<string, List<string>>();
    }

    #endregion

    #region Functions

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
        if (!IsAnyPropsDirty)
        {
            IsAnyPropsDirty = TrackedDictionary.Select(x => x.Value).OfType<TrackedChange>().Any(x => x.IsDirty);
        }
    }

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
        foreach (var change in trackedChanges)
        {
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
        foreach (var change in trackedChanges)
        {
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
        foreach (var prop in properties)
        {
            if (prop.Name != string.Empty)
            {
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
        if (currentValue != null)
        {
            var underlyingType = Nullable.GetUnderlyingType(currentValue.GetType());
            isValueType = underlyingType?.IsValueType ?? currentValue.GetType().IsValueType;
        }

        if (!IsTrackedChange(property.Name))
        {
            bool HasChangedFunc(object? original, object? current) => HasValuesChanged(original, current);
            currentChange = new TrackedChange(HasChangedFunc, property)
            {
                OriginalValue = currentValue == null ? null :
                    isValueType || currentValue is string ? currentValue : currentValue.Clone()
            };

            TrackedDictionary.Add(property.Name, currentChange);
        }
        else
        {
            currentChange = GetTrackedChange(property.Name);
            if (currentChange == null) return;
            currentChange.CurrentValue = isValueType || currentValue is string ? currentValue : currentValue.Clone();
            currentChange.Status = TrackedChange.TrackingStatus.Checked;
            currentChange.LastChecked = DateTime.Now;
        }

        if (currentChange.HasChanges)
        {
            IsChanged = true;
            currentChange.IsDirty = true;
        }
        else
        {
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
        try
        {
            if (currentValue is IEnumerable<double> currentValueDouble && originalValue is IEnumerable<double> originalValueDouble)
            {
                return currentValueDouble.Count() != originalValueDouble.Count() ||
                       !currentValueDouble.SequenceEqual(originalValueDouble);
            }
            if (currentValue is ObservableCollection<double> currentValueDouble1 && originalValue is ObservableCollection<double>
                originalValueDouble1)
            {
                return currentValueDouble1.Count != originalValueDouble1.Count ||
                       !currentValueDouble1.SequenceEqual(originalValueDouble1);
            }
            var collectionCurrent = ((IEnumerable<T>)currentValue).ToList();
            var collectionOrigin = ((IEnumerable<T>)originalValue).ToList();
            var res = collectionCurrent.Count != collectionOrigin.Count ||
                      !collectionCurrent.SequenceEqual(collectionOrigin);
            return res;
        }
        catch
        {
            var res = !EqualityComparer<T>.Default.Equals(originalValue, currentValue);
            return res;
        }
    }

    protected IEnumerable<TrackedChange> GetTrackedChanges() => TrackedDictionary.Select(pair => (TrackedChange)pair.Value);

    protected TrackedChange? GetTrackedChange(string propertyName) => IsTrackedChange(propertyName) ? (TrackedChange)TrackedDictionary[propertyName] : null;

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
        if (!IsTracking)
        {
            BeginChanges();
        }
    }

    private void ThrowIfTrackingStarted()
    {
        if (IsTracking)
        {
            throw new InvalidOperationException("Change tracking has already started");
        }
    }

    protected async Task DebounceCheckAsync(BindableObject bindableObject)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(_debounceTime, _debounceCts.Token);
            await ValidateDataAsync(bindableObject);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    protected async Task ValidateDataAsync(BindableObject sender)
    {
        var sw = new Stopwatch();
        sw.Start();
        try
        {
            IsValidModel = await Task.Run(() => CheckValidData(sender));
            CommandManager.InvalidateRequerySuggested();
        }
        catch (OperationCanceledException)
        {
            // Task bị hủy, không làm gì cả
        }
        catch
        {
            // Ignore
        }
        sw.Stop();
        Console.WriteLine($@"CheckValidData took: {sw.ElapsedMilliseconds} ms");
    }

    public virtual bool CheckValidData(BindableObject sender)
    {
        return true;
    }

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Classes

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

        public PropertyInfo Property { get; set; } = null!;
        public TrackingStatus Status { get; set; } = TrackingStatus.Unchecked;

        public bool HasChanges => Status == TrackingStatus.Checked && DetectChange(CurrentValue);

        public DateTime? LastChecked { get; set; }

        private Func<object?, object?, bool> HasChangedFunc { get; set; } = null!;
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

    #endregion

    #region DataErrorInfo

    protected Dictionary<string, List<string>> PropErrors { get; set; } = null!;

    public IEnumerable? GetErrors(string? propertyName)
    {
        if (propertyName == null) return null;
        PropErrors.TryGetValue(propertyName, out var errors);
        return errors;
    }

    [JsonIgnore]
    public bool HasErrors
    {
        get
        {
            var propErrorsCount = PropErrors.Values.FirstOrDefault(r => r.Any());
            return propErrorsCount != null;
        }
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    protected void Validate(object? value, [CallerMemberName] string propertyName = "")
    {
        Task.Run(() => DataValidation(value, propertyName));
    }

    protected void DataValidation(object? value, [CallerMemberName] string propertyName = "")
    {
        lock (_lock)
        {
            var validationContext = new ValidationContext(this, null, null);
            validationContext.MemberName = propertyName;
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(this, validationContext, validationResults, true);

            foreach (var propError in PropErrors.ToList())
            {
                if (!validationResults.All(r => r.MemberNames.All(m => m != propError.Key))) continue;
                PropErrors.Remove(propError.Key);
                OnPropertyErrorsChanged(propError.Key);
            }

            var q = from r in validationResults from m in r.MemberNames group r by m into g select g;

            foreach (var prop in q)
            {
                var messages = prop.Select(r => r.ErrorMessage).ToList();

                if (PropErrors.ContainsKey(prop.Key))
                {
                    PropErrors.Remove(prop.Key);
                }

                PropErrors.Add(prop.Key, messages);
                OnPropertyErrorsChanged(prop.Key);
            }
        }
    }

    private void OnPropertyErrorsChanged(string p) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(p));

    private static readonly object _lock = new();

    #endregion

    #region Public Property Changed Event

    public delegate void ModelPropertyChangedHandler(object sender, ModelPropertyChangedEventArgs e);
    public event ModelPropertyChangedHandler? ModelPropertyChanged;

    #endregion
}
