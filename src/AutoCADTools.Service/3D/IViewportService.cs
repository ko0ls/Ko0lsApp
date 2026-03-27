using System.Windows;
using System.Windows.Input;
using AutoCADTools.Core._3D.Enums;

namespace AutoCADTools.Service._3D
{
  public interface IViewportService
  {
    ICameraService CameraService { get; }
    EnumViewportMode CurrentMode { get; set; }
    Point LastMousePosition { get; }
    bool IsShiftDown { get; }

    void OnMouseMove(Point screenPt, bool isLeftBtnDown, bool isRightBtnDown, bool isMiddleBtnDown);
    void OnMouseWheel(int delta);
    void OnKeyDown(Key key);
    void OnKeyUp(Key key);

    event System.EventHandler<EnumViewportMode> ModeChanged;
  }
}
