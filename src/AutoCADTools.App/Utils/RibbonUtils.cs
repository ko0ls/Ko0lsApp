#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AutoCADTools.Core.Localization;
using Autodesk.Windows;

namespace AutoCADTools.App.Utils;

/// <summary>
///   Provides fluent builder utilities for creating AutoCAD Ribbon tabs, panels,
///   and buttons using the <c>Autodesk.Windows</c> API (AdWindows/RibbonControl).
///
///   <para>
///   <b>How the AutoCAD Ribbon API works:</b><br />
///   The ribbon is accessed via <c>Autodesk.Windows.ComponentManager.Ribbon</c>,
///   which exposes a <c>Tabs</c> collection of <c>Autodesk.Windows.RibbonTab</c>
///   objects.  Each tab owns a <c>Panels</c> collection of
///   <c>Autodesk.Windows.RibbonPanel</c>.  A panel wraps a
///   <c>Autodesk.Windows.RibbonPanelSource</c> (the content object) whose
///   <c>Items</c> property holds a <c>Autodesk.Windows.RibbonItemCollection</c>
///   of UI elements — most commonly <c>Autodesk.Windows.RibbonButton</c>.
///   Button clicks are wired to AutoCAD commands registered via
///   <c>[Autodesk.AutoCAD.Runtime.CommandMethod]</c> using the button's
///   <c>CommandHandler</c> property (an <c>ICommand</c> that calls
///   <c>Editor.RunCommand(commandName)</c>).
///   </para>
///
///   <para>
///   <b>Notes on AutoCAD 2023 vs 2024:</b><br />
///   The AdWindows assembly (<c>AdWindows.dll</c>) is the host for the ribbon
///   API and is shipped with both AutoCAD 2023 and 2024.  The public-facing
///   types in <c>Autodesk.Windows</c> (RibbonControl, RibbonTab, RibbonPanel,
///   RibbonPanelSource, RibbonButton) are stable across these versions; no API
///   divergence has been observed.
///   </para>
///
///   <para>
///   <b>Recommended improvements for future iterations:</b><br />
///   <list type="number">
///     <item>
///       <b>Centralised Ribbon definition (JSON/attribute-driven):</b> define
///       tabs, panels, and buttons in a JSON file (or via custom attributes on
///       command methods) and load at startup.  This keeps UI layout separate
///       from code and makes localisation easier.
///     </item>
///     <item>
///       <b>Icon loading from embedded resources:</b> replace the placeholder
///       <c>iconKey</c> with a <c>ResourceManager</c> lookup that loads
///       <c>BitmapImage</c>/<c>BitmapSource</c> from embedded .png resources
///       keyed by that string, with a fallback for missing icons.
///     </item>
///     <item>
///       <b>Convention-based command registration:</b> scan assemblies at
///       startup for methods decorated with a custom attribute
///       (e.g. <c>[RibbonButton(...)]</c>) that carries display name, tooltip,
///       icon key, and command name.  An <c>IRibbonCommandRegistry</c> can then
///       build the ribbon automatically with zero per-button boilerplate.
///     </item>
///     <item>
///       <b>Panel layout customisation:</b> extend <c>PanelBuilder</c> with
///       <c>AddSeparator()</c>, <c>AddRowPanel()</c>, and <c>AddTextBox()</c>
///       to cover advanced UI scenarios (RibbonRowPanel, slide-out panels).
///     </item>
///   </list>
///   </para>
/// </summary>
public static class RibbonUtils
{
  // ─────────────────────────────────────────────────────────────────
  // Public entry points
  // ─────────────────────────────────────────────────────────────────

  /// <summary>
  ///   Begins building a ribbon panel with the given source name, optionally
  ///   placing it inside an existing tab (identified by <c>tabName</c>) or
  ///   creating a new tab first.
  /// </summary>
  /// <param name="panelSourceName">
  ///   The <c>RibbonPanelSource.Name</c> (used as the panel's internal key).
  /// </param>
  /// <param name="tabName">
  ///   The display name of the <c>RibbonTab</c>.  If a tab with this name
  ///   already exists, the panel is added to it; otherwise a new tab is created.
  /// </param>
  /// <returns>
  ///   A <see cref="PanelBuilder"/> that can be used to add buttons and finally
  ///   call <see cref="PanelBuilder.Build"/> to attach the panel to AutoCAD's
  ///   ribbon.
  /// </returns>
  public static PanelBuilder CreatePanel(string panelSourceName, string tabName)
  {
    return new PanelBuilder(panelSourceName, tabName);
  }

  /// <summary>
  ///   Creates a new ribbon tab and panel, adds them to
  ///   <c>Autodesk.Windows.ComponentManager.Ribbon</c>, and returns the
  ///   underlying <c>RibbonPanelSource</c>.
  /// </summary>
  /// <remarks>
  ///   Convenience overload that calls <see cref="CreatePanel"/> followed by
  ///   <see cref="PanelBuilder.Build"/> internally.
  /// </remarks>
  /// <param name="panelSourceName">The <c>Name</c> of the panel source.</param>
  /// <param name="tabName">The display name of the tab.</param>
  /// <returns>The newly created <c>RibbonPanelSource</c>.</returns>
  public static RibbonPanelSource CreateAndAddTab(
    string panelSourceName,
    string tabName)
  {
    return CreatePanel(panelSourceName, tabName).Build();
  }

  // ─────────────────────────────────────────────────────────────────
  // PanelBuilder — fluent builder for a single ribbon panel
  // ─────────────────────────────────────────────────────────────────

  /// <summary>
  ///   Fluent builder for populating a <c>RibbonPanelSource</c> with buttons
  ///   before attaching it to the AutoCAD ribbon.
  /// </summary>
  public class PanelBuilder
  {
    private readonly string _panelSourceName;
    private readonly string _tabName;
    private readonly List<RibbonButton> _buttons = new List<RibbonButton>();

    /// <summary>
    ///   Initialises a new <see cref="PanelBuilder"/>.
    /// </summary>
    /// <param name="panelSourceName">The panel source's internal name.</param>
    /// <param name="tabName">The display name of the tab.</param>
    public PanelBuilder(string panelSourceName, string tabName)
    {
      _panelSourceName = panelSourceName ?? throw new ArgumentNullException(nameof(panelSourceName));
      _tabName = tabName ?? throw new ArgumentNullException(nameof(tabName));
    }

    /// <summary>
    ///   Appends a <c>RibbonButton</c> to the panel under construction.
    /// </summary>
    /// <param name="text">The button's display label (shown on the ribbon).</param>
    /// <param name="commandName">
    ///   The name of an AutoCAD command registered via
    ///   <c>[Autodesk.AutoCAD.Runtime.CommandMethod("CMD_...")]</c>.
    /// </param>
    /// <param name="tooltip">
    ///   Long description shown in the tooltip dialog when the button is hovered.
    /// </param>
    /// <param name="iconKey">
    ///   An arbitrary string key used to locate the button's icon.  Currently
    ///   a placeholder — the real implementation should resolve this to an
    ///   embedded resource or palette file.  Pass <c>null</c> for no icon.
    /// </param>
    /// <param name="largeIcon">
    ///   If <c>true</c>, the button is rendered in large (32x32) style;
    ///   if <c>false</c>, standard small-button style.  Defaults to <c>false</c>.
    /// </param>
    /// <returns>This <see cref="PanelBuilder"/> for call-chaining.</returns>
    public PanelBuilder AddButton(
      string? text,
      string? commandName,
      string? tooltip = null,
      string? iconKey = null,
      bool largeIcon = false)
    {
      var button = CreateRibbonButton(text, commandName, tooltip, iconKey, largeIcon);
      _buttons.Add(button);
      return this;
    }

    /// <summary>
    ///   Wires the built panel into the AutoCAD ribbon and returns the
    ///   underlying <c>RibbonPanelSource</c>.
    /// </summary>
    /// <returns>The fully constructed <c>RibbonPanelSource</c>.</returns>
    public RibbonPanelSource Build()
    {
      var ribbonControl = ComponentManager.Ribbon;
      if (ribbonControl == null)
      {
        throw new InvalidOperationException(
          "Error.RibbonNotInitialized".GetString());
      }

      RibbonTab tab = FindOrCreateTab(ribbonControl, _tabName);

      var panelSource = new RibbonPanelSource
      {
        Name = _panelSourceName,
        Title = _panelSourceName,
      };

      foreach (var button in _buttons)
      {
        panelSource.Items.Add(button);
      }

      var ribbonPanel = new RibbonPanel
      {
        Source = panelSource,
      };

      tab.Panels.Add(ribbonPanel);
      return panelSource;
    }

    // ──────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────

    private static RibbonTab FindOrCreateTab(RibbonControl ribbonControl, string tabName)
    {
      foreach (var existingTab in ribbonControl.Tabs)
      {
        if (existingTab.Title == tabName)
          return existingTab;
      }

      var newTab = new RibbonTab
      {
        Title = tabName,
        Name = tabName,
        Id = tabName,
      };
      ribbonControl.Tabs.Add(newTab);
      return newTab;
    }

    private static RibbonButton CreateRibbonButton(
      string? text,
      string? commandName,
      string? tooltip,
      string? iconKey,
      bool largeIcon)
    {
      var icon = TryLoadIcon(iconKey);

      var button = new RibbonButton
      {
        Text = text ?? string.Empty,
        ToolTip = tooltip,
        CommandHandler = new AutoCADCommandHandler(commandName),
        IsEnabled = true,
        ShowImage = true,
        ShowText = true,
        Image = icon,
        LargeImage = largeIcon ? icon : null,
      };

      if (largeIcon)
      {
        button.Size = RibbonItemSize.Large;
      }

      return button;
    }

    /// <summary>
    ///   Attempts to load a <c>BitmapImage</c> icon for the given key from
    ///   the bundle's <c>Resources\Icons</c> folder.
    /// </summary>
    /// <param name="iconKey">An application-defined key (e.g. <c>"swallow_foundation"</c>).</param>
    /// <returns>The loaded icon, or <c>null</c> if not found.</returns>
    private static BitmapImage? TryLoadIcon(string? iconKey)
    {
      if (string.IsNullOrEmpty(iconKey))
        return null;

      try
      {
        var iconPath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "Resources", "Icons", $"{iconKey}.png");
        if (!File.Exists(iconPath))
          return null;

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
      }
      catch
      {
        return null;
      }
    }
  }

  // ─────────────────────────────────────────────────────────────────
  // AutoCADCommandHandler — ICommand that runs an AutoCAD command
  // ─────────────────────────────────────────────────────────────────

  /// <summary>
  ///   An <c>ICommand</c> that dispatches an AutoCAD command registered via
  ///   <c>[CommandMethod]</c> using <c>Autodesk.Windows.RibbonCommandHandler.Execute</c>,
  ///   which is the official AdWindows API for ribbon button command dispatch.
  /// </summary>
  private class AutoCADCommandHandler : ICommand
  {
    private readonly string? _commandName;

    public AutoCADCommandHandler(string? commandName)
    {
      _commandName = commandName;
    }

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter) => !string.IsNullOrEmpty(_commandName);

    public void Execute(object? parameter)
    {
      if (string.IsNullOrEmpty(_commandName))
        return;

      try
      {
        // SendStringToExecute on Document (not Editor) is the correct AutoCAD API
        // for dispatching commands from ribbon button clicks.
        Autodesk.AutoCAD.ApplicationServices.Core.Application
          .DocumentManager.MdiActiveDocument
          ?.SendStringToExecute(_commandName + "\n", true, false, true);
      }
      catch (Exception ex)
      {
        var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application
          .DocumentManager.MdiActiveDocument?.Editor;
        ed?.WriteMessage($"\n{"Error.CommandExecutionFailed".GetString().Replace("{0}", _commandName ?? "").Replace("{1}", ex.Message)}");
      }
    }
  }
}
