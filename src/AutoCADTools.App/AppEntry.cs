#nullable enable

using AutoCADTools.App.Utils;
using AutoCADTools.Core.Utils;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Microsoft.Extensions.DependencyInjection;

[assembly: ExtensionApplication(typeof(AutoCADTools.App.AppEntry))]

namespace AutoCADTools.App
{
    public class AppEntry : IExtensionApplication
    {
        private static ServiceProvider? _serviceProvider;
        private static Presentation.Canvas.CanvasViewModel? _instanceVm;

        public static void RegisterViewModel(Presentation.Canvas.CanvasViewModel vm)
        {
            _instanceVm = vm;
        }

        public static Presentation.Canvas.CanvasViewModel? CanvasViewModel => _instanceVm;

        public void Initialize()
        {
            try
            {
                var services = new ServiceCollection();
                services.AddTransient<Presentation.Canvas.CanvasViewModel>();
                _serviceProvider = services.BuildServiceProvider();

                if (ComponentManager.Ribbon == null)
                    ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
                else
                {
                    var editor = Application.DocumentManager.MdiActiveDocument.Editor;
                    CreatePanel();
                    editor.WriteMessage("\nAutoCADTools loaded successfully.");
                }
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument?.Editor
                    .WriteMessage($"\nAutoCADTools initialization failed: {ex.Message}");
            }
        }

        public void Terminate()
        {
            _serviceProvider?.Dispose();
        }

        private static void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
        {
            try
            {
                if (ComponentManager.Ribbon == null) return;

                CreatePanel();

                ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
            }
            catch (System.Exception ex)
            {
                MessageUtils.Error(ex.Message);
            }
        }

        private static void CreatePanel()
        {
            RibbonUtils.CreatePanel("Ko0ls Tools", "Ko0ls Tab")
                .AddButton("Draw Line", "KOOLS_CMD_LINE", "Draw a line", iconKey: "line")
                .AddButton("Draw Circle", "KOOLS_CMD_CIRCLE", "Draw a circle", iconKey: "circle")
                .AddButton("Draw Arc", "KOOLS_CMD_ARC", "Draw an arc", iconKey: "arc")
                .Build();
        }

        // ── Stub command methods ────────────────────────────────────────

        [CommandMethod("KOOLS_CMD_LINE")]
        public void CmdLine()
        {
            // TODO: implement
        }

        [CommandMethod("KOOLS_CMD_CIRCLE")]
        public void CmdCircle()
        {
            // TODO: implement
        }

        [CommandMethod("KOOLS_CMD_ARC")]
        public void CmdArc()
        {
            // TODO: implement
        }
    }
}