#nullable enable
using System;

namespace AutoCADTools.Core;

public class Class1
{
}

/// <summary>Bridge between Presentation and App layers to avoid circular dependencies.
/// Presentation raises <see cref="OnAppSettingsSaved"/> when the user saves settings.
/// AppEntry subscribes to it during Initialize().</summary>
public static class AppProxy
{
  public static event Action? SettingsSaved;

  /// <summary>Called by Presentation when settings are saved.</summary>
  public static void NotifySettingsSaved() => SettingsSaved?.Invoke();
}