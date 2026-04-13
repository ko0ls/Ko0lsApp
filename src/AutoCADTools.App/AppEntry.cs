#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AutoCADTools.App.Utils;
using AutoCADTools.Core;
using AutoCADTools.Core.Localization;
using AutoCADTools.Core.Utils;
using AutoCADTools.Presentation.Utils;
using AutoCADTools.Storage;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Microsoft.Extensions.DependencyInjection;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: ExtensionApplication(typeof(AutoCADTools.App.AppEntry))]

namespace AutoCADTools.App
{
  public class AppEntry : IExtensionApplication
  {
    private static ServiceProvider? _serviceProvider;
    private static ISettingsRepository? _settingsRepository;
    private static Presentation.Canvas.CanvasViewModel? _instanceVm;

    // HACK: Force Microsoft.Xaml.Behaviors.Wpf to be loaded before any XAML is parsed.
    // Without this, the WPF XAML parser fails to locate the assembly when
    // <i:Interaction.Behaviors> is used in Presentation class library.
    // Ref: https://github.com/microsoft/XamlBehaviorsWpf/issues/86
#pragma warning disable IDE0051, CS0414
    private static readonly object XamlBehaviorsWarmup =
      new Microsoft.Xaml.Behaviors.EventTrigger();
#pragma warning restore IDE0051, CS0414

    public static void RegisterViewModel(Presentation.Canvas.CanvasViewModel vm)
    {
      _instanceVm = vm;
    }

    public static Presentation.Canvas.CanvasViewModel? CanvasViewModel => _instanceVm;

  // ── Template caches: pre-loaded from temp.dwt once at startup ───────────────

  private static readonly HashSet<string> _loadedBlocks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  private static readonly HashSet<string> _loadedDimStyles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  private static readonly HashSet<string> _loadedLayers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  private static readonly HashSet<string> _loadedTextStyles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

  private static bool _preloadRan;    // guards PreLoadFromTemplate against double-run
  private static bool _settingsSaved; // true once PreLoadFromTemplate succeeds

  public static IReadOnlyCollection<string> LoadedBlocks => _loadedBlocks;
  public static IReadOnlyCollection<string> LoadedDimStyles => _loadedDimStyles;
  public static IReadOnlyCollection<string> LoadedLayers => _loadedLayers;
  public static IReadOnlyCollection<string> LoadedTextStyles => _loadedTextStyles;

  /// <summary>Pre-load blocks, dim styles, layers, and text styles from temp.dwt.
  /// Idempotent — returns immediately on subsequent calls. Call once at startup.</summary>
  private static void PreLoadFromTemplate()
  {
    // Guard against double-run: if the HashSets are already populated, skip.
    if (_preloadRan) return;
    _preloadRan = true;

    var doc = Application.DocumentManager.MdiActiveDocument;
    if (doc == null) return;

    var templatePath = System.IO.Path.Combine(
      System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
      ?? string.Empty, "Assets", "TemplateAutocad", "temp.dwt");
    if (!System.IO.File.Exists(templatePath)) {
      doc.Editor.WriteMessage("\nWarning: temp.dwt template not found — some blocks/dimstyles may be missing.");
      return;
    }

    using (doc.LockDocument())
    using (var tr = doc.Database.TransactionManager.StartTransaction())
    {
      try
      {
        using var srcDb = new Database(false, true);
        srcDb.ReadDwgFile(templatePath, FileOpenMode.OpenForReadAndReadShare, true, string.Empty);

        using var trSrc = srcDb.TransactionManager.StartTransaction();

        // ── 1. Blocks ────────────────────────────────────────────────────────────
        var srcBt = (BlockTable)trSrc.GetObject(srcDb.BlockTableId, OpenMode.ForRead);
        var blockIds = new ObjectIdCollection();
        foreach (var entry in srcBt)
        {
          var btr = (BlockTableRecord)trSrc.GetObject(entry, OpenMode.ForRead);
          if (btr.IsLayout || btr.IsAnonymous || btr.IsFromOverlayReference) continue;
          if (_loadedBlocks.Contains(btr.Name)) continue;
          blockIds.Add(btr.ObjectId);
          _loadedBlocks.Add(btr.Name); // track now; confirm after clone
        }
        if (blockIds.Count > 0) {
          var mapping = new IdMapping();
          doc.Database.WblockCloneObjects(blockIds, doc.Database.BlockTableId, mapping, DuplicateRecordCloning.Replace, false);
        }

        // ── 2. Dim Styles ──────────────────────────────────────────────────────
        var srcDst = (DimStyleTable)trSrc.GetObject(srcDb.DimStyleTableId, OpenMode.ForRead);
        var dstIds = new ObjectIdCollection();
        foreach (var entry in srcDst)
        {
          var dsr = (DimStyleTableRecord)trSrc.GetObject(entry, OpenMode.ForRead);
          if (_loadedDimStyles.Contains(dsr.Name)) continue;
          dstIds.Add(dsr.ObjectId);
          _loadedDimStyles.Add(dsr.Name);
        }
        if (dstIds.Count > 0) {
          var mapping = new IdMapping();
          doc.Database.WblockCloneObjects(dstIds, doc.Database.DimStyleTableId, mapping, DuplicateRecordCloning.Ignore, false);
        }

        // ── 3. Layers ───────────────────────────────────────────────────────────
        var srcLt = (LayerTable)trSrc.GetObject(srcDb.LayerTableId, OpenMode.ForRead);
        var ltIds = new ObjectIdCollection();
        foreach (var entry in srcLt)
        {
          var ltr = (LayerTableRecord)trSrc.GetObject(entry, OpenMode.ForRead);
          if (ltr.IsHidden) continue;
          if (_loadedLayers.Contains(ltr.Name)) continue;
          ltIds.Add(ltr.ObjectId);
          _loadedLayers.Add(ltr.Name);
        }
        if (ltIds.Count > 0) {
          var mapping = new IdMapping();
          doc.Database.WblockCloneObjects(ltIds, doc.Database.LayerTableId, mapping, DuplicateRecordCloning.Ignore, false);
        }

        // ── 4. Text Styles ─────────────────────────────────────────────────────
        var srcTst = (TextStyleTable)trSrc.GetObject(srcDb.TextStyleTableId, OpenMode.ForRead);
        var tstIds = new ObjectIdCollection();
        foreach (var entry in srcTst)
        {
          var tstr = (TextStyleTableRecord)trSrc.GetObject(entry, OpenMode.ForRead);
          if (_loadedTextStyles.Contains(tstr.Name)) continue;
          tstIds.Add(tstr.ObjectId);
          _loadedTextStyles.Add(tstr.Name);
        }
        if (tstIds.Count > 0) {
          var mapping = new IdMapping();
          doc.Database.WblockCloneObjects(tstIds, doc.Database.TextStyleTableId, mapping, DuplicateRecordCloning.Ignore, false);
        }

        trSrc.Commit();
        tr.Commit();
        _settingsSaved = true;
      }
      catch (System.Exception ex) {
        // Roll back transaction and log the error so the user knows what failed.
        tr.Abort();
        Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(
          $"\nTemplate pre-load failed: {ex.Message}");
      }
    }
  }

  public void Initialize()
    {
      try {
        var services = new ServiceCollection();
        services.AddTransient<Presentation.Canvas.CanvasViewModel>();

        // 3D system
        services.AddSingleton<Service._3D.IObjectManager3D, Service._3D.ObjectManager3D>();
        services.AddSingleton<Service._3D.ICameraService, Service._3D.CameraService>();
        services.AddSingleton<Service._3D.IViewportService, Service._3D.ViewportService>();
        services.AddSingleton<Presentation._3D.Viewport3D.Selection.SelectionService>();
        services.AddTransient<Presentation._3D.Viewport3D.Viewport3DViewModel>();
        services.AddTransient<Presentation._3D.Properties3D.Properties3DViewModel>();
        services.AddTransient<Presentation._3D.ObjectManager3D.ObjectManager3DViewModel>();
        services.AddTransient<Presentation._3D.Viewport3DWindowViewModel>();
        services.AddTransient<Presentation._3D.MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        _settingsRepository = new SettingsRepository();
        try {
          var lang = _settingsRepository.GetLanguage();
          LocalizationManager.SetLanguage(lang);
        }
        catch {
          LocalizationManager.SetLanguage("en");
        }

        // Trigger LocalizationService singleton init so LanguageChanged is subscribed
        _ = LocalizationService.Instance;

        if (ComponentManager.Ribbon == null)
          ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        else {
          var doc = Application.DocumentManager.MdiActiveDocument;
          if (doc == null) return;
          CreatePanel();
          LocalizationManager.LanguageChanged += OnLanguageChanged;

          // Pre-load blocks / dim styles / layers / text styles from temp.dwt
          AppProxy.SettingsSaved += PreLoadFromTemplate;
          PreLoadFromTemplate(); // runs once; idempotent on subsequent calls

          doc.Editor.WriteMessage($"\n{"App.Loaded".GetString()}");
        }
      }
      catch (System.Exception ex) {
        Application.DocumentManager.MdiActiveDocument?.Editor
          .WriteMessage($"\n{"App.InitializationFailed".GetString().Replace("{0}", ex.Message)}");
      }
    }

    public void Terminate()
    {
      _serviceProvider?.Dispose();
    }

    private static void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
    {
      try {
        if (ComponentManager.Ribbon == null) return;

        CreatePanel();
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        AppProxy.SettingsSaved += () => {
          _settingsSaved = true;
          PreLoadFromTemplate();
        };
        PreLoadFromTemplate();
        _settingsSaved = true;

        ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
      }
      catch (System.Exception ex) {
        MessageUtils.Error(ex.Message);
      }
    }

    private static void OnLanguageChanged(object? sender, EventArgs e)
    {
      RebuildRibbon();
    }

    private static void RebuildRibbon()
    {
      if (ComponentManager.Ribbon == null) return;
      const string tabName = "Ko0ls Tab";
      foreach (var tab in ComponentManager.Ribbon.Tabs) {
        if (tab.Title == tabName) {
          foreach (var panel in tab.Panels.ToList()) {
            tab.Panels.Remove(panel);
          }
          break;
        }
      }
      CreatePanel();
    }

    private static void CreatePanel()
    {
      RibbonUtils.CreatePanel("Panel.Settings.Title".GetString(), "Ko0ls Tab")
        .AddButton("Command.Settings".GetString(), "KOOLS_CMD_SETTINGS", "Command.Settings".GetString(), iconKey: "settings")
        .Build();

      RibbonUtils.CreatePanel("Panel.Draw.Title".GetString(), "Ko0ls Tab")
        .AddButton("SwallowFoundation.Button.Draw".GetString(), "KOOLS_CMD_VMD", "Swallow Foundation Window", iconKey: "swallow_foundation")
        .Build();
    }

    // ── Stub command methods ────────────────────────────────────────

    private static void WriteMessageSafely(string msg)
    {
      var doc = Application.DocumentManager.MdiActiveDocument;
      if (doc != null) {
        doc.Editor.WriteMessage(msg);
      }
    }

    [CommandMethod("KOOLS_CMD_SETTINGS")]
    public void CmdSettings()
    {
      var vm = new Presentation.ViewModels.SettingViewModel(_settingsRepository!);
      var window = new Presentation.Views.SettingsWindow(vm);
      Application.ShowModalWindow(window);
    }

    [CommandMethod("KOOLS_CMD_3DVIEW")]
    public void Cmd3DView()
    {
      if (_serviceProvider == null) return;
      try {
        var mainVm = new Presentation._3D.MainViewModel(
          _serviceProvider.GetRequiredService<Presentation.Canvas.CanvasViewModel>(),
          _serviceProvider.GetRequiredService<Presentation._3D.Viewport3DWindowViewModel>());
        var window = new Presentation._3D.MainView(mainVm);
        Application.ShowModalWindow(window);
      }
      catch (System.Exception ex) {
        WriteMessageSafely($"\nMain View error: {ex.Message}");
      }
    }

    [CommandMethod("KOOLS_CMD_VMD")]
    public void CmdSwallowFoundation()
    {
      if (Application.DocumentManager.MdiActiveDocument == null) return;
      if (!_settingsSaved) {
        WriteMessageSafely("\nPlease run Settings first before drawing.");
        return;
      }
      try {
        var window = new Presentation.Views.SwallowFoundationWindow();
        var result = window.ShowDialog();

        if (result != true) return;
        var vm = (Presentation.ViewModels.SwallowFoundationViewModel) window.DataContext!;
        var model = vm.ToModel();
        {
          var drawer = new Foundation.SwallowFoundationDrawer();
          drawer.Draw(model);
        }
      }
      catch (System.Exception ex) {
        WriteMessageSafely($"\nSwallow foundation error: {ex.Message}");
      }
    }
  }
}