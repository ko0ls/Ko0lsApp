using System;
using System.Threading;
using Xunit;

namespace AutoCADTools.Test;

/// <summary>
/// Runs a test body on a dedicated STA thread, so WPF UI elements can be created.
/// Usage: call <c>StaThread.Run(() => { /* WPF test code */ });</c>
/// Exceptions thrown inside the lambda are rethrown on the caller's thread.
/// </summary>
public static class StaThread
{
  /// <summary>
  /// Executes <paramref name="testBody"/> on a new STA thread and returns.
  /// Any exception thrown by <paramref name="testBody"/> is captured and rethrown
  /// on the caller's thread.
  /// </summary>
  public static void Run(Action testBody)
  {
    Exception? capturedEx = null;
    var doneEvent = new ManualResetEventSlim(false);

    var thread = new Thread(() =>
    {
      try {
        testBody();
      }
      catch (Exception ex) {
        capturedEx = ex;
      }
      finally {
        doneEvent.Set();
      }
    });

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    doneEvent.Wait();

    if (capturedEx != null) {
      throw capturedEx;
    }
  }
}
