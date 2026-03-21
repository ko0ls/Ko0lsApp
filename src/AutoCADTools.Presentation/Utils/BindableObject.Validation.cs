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

public abstract partial class BindableObject
{
    #region Validation

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
}
