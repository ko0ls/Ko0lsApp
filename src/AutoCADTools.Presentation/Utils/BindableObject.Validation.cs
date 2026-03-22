using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace AutoCADTools.Presentation.Utils;

public abstract partial class BindableObject
{
  #region Validation

  protected async Task DebounceCheckAsync(BindableObject bindableObject)
  {
    try {
      _debounceCts?.Cancel();
      _debounceCts?.Dispose();
    }
    finally {
      _debounceCts = new CancellationTokenSource();
    }
    try {
      await Task.Delay(_debounceTime, _debounceCts.Token);
      await ValidateDataAsync();
    }
    catch (OperationCanceledException) {
      // Task bị hủy, không làm gì cả
    }
#if DEBUG
    catch (Exception e) {
      Console.WriteLine(e);
    }
#else
    catch {
      // Ignore
    }
#endif
  }

  protected async Task ValidateDataAsync()
  {
#if DEBUG
    var sw = new System.Diagnostics.Stopwatch();
    sw.Start();
#endif
    try {
      IsValidModel = await Task.Run(CheckValidData);
      CommandManager.InvalidateRequerySuggested();
    }
    catch (OperationCanceledException) {
      // Task bị hủy, không làm gì cả
    }
#if DEBUG
    catch {
      // Ignore
    }
    sw.Stop();
    Console.WriteLine($@"CheckValidData took: {sw.ElapsedMilliseconds} ms");
#else
    catch {
      // Ignore
    }
#endif
  }

  public virtual bool CheckValidData()
  {
    return true;
  }

  #endregion

  #region DataErrorInfo

  protected Dictionary<string, List<string>> PropErrors { get; set; } = null!;

  public IEnumerable GetErrors(string? propertyName)
  {
    if (propertyName == null) return Enumerable.Empty<string>();
    return PropErrors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
  }

  [JsonIgnore]
  public bool HasErrors
  {
    get => PropErrors.Values.Any(list => list.Count > 0);
  }

  public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

  protected void Validate(object? value, [CallerMemberName] string propertyName = "")
  {
    Task.Run(() => DataValidation(value, propertyName));
  }

  protected void DataValidation(object? value, [CallerMemberName] string propertyName = "")
  {
    List<string>? capturedErrors = null;
    lock (_lock) {
      var validationContext = new ValidationContext(this, null, null);
      validationContext.MemberName = propertyName;
      var validationResults = new List<ValidationResult>();
      Validator.TryValidateObject(this, validationContext, validationResults, true);

      foreach (var propError in PropErrors.ToList()) {
        if (!validationResults.All(r => r.MemberNames.All(m => m != propError.Key))) continue;
        PropErrors.Remove(propError.Key);
        capturedErrors ??= new List<string>();
        capturedErrors.Add(propError.Key);
      }

      var q = from r in validationResults from m in r.MemberNames group r by m into g select g;

      foreach (var prop in q) {
        var messages = prop.Select(r => r.ErrorMessage!).ToList();

        PropErrors.Remove(prop.Key);

        PropErrors.Add(prop.Key, messages);
        capturedErrors ??= new List<string>();
        capturedErrors.Add(prop.Key);
      }
    }

    if (capturedErrors != null) {
      foreach (var key in capturedErrors) {
        RaiseErrorsChanged(key);
      }
    }
  }

  private void RaiseErrorsChanged(string propertyName)
  {
    if (ErrorsChanged == null) return;
    if (Application.Current?.Dispatcher != null &&
        Application.Current.Dispatcher.CheckAccess()) {
      ErrorsChanged.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
    else {
      Application.Current?.Dispatcher?.Invoke(() =>
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)));
    }
  }

  private static readonly object _lock = new();

  #endregion
}
