using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoCADTools.Presentation.Utils.HelperTracking;
using Newtonsoft.Json;

namespace AutoCADTools.Presentation.Utils;

public abstract partial class BindableObject
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
}