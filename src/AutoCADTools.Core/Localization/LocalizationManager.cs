#nullable enable

using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace AutoCADTools.Core.Localization
{
  public static class LocalizationManager
  {
    private static readonly ResourceManager _resourceManager =
      new ResourceManager(
        "AutoCADTools.Core.Localization.Resources.Strings",
        typeof(LocalizationManager).Assembly);

    private static string _currentCulture = "en";

    public static event EventHandler? LanguageChanged;

    public static string CurrentCulture => _currentCulture;

    public static void SetLanguage(string cultureName)
    {
      var culture = new CultureInfo(cultureName);
      _currentCulture = cultureName;
      Thread.CurrentThread.CurrentUICulture = culture;
      Thread.CurrentThread.CurrentCulture = culture;
      LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string GetString(this string key)
    {
      // GetString is called from the WPF UI thread where CultureInfo.CurrentUICulture
      // is set by AppEntry.OnLanguageChanged before this returns.
      return _resourceManager.GetString(key) ?? key;
    }
  }
}
