using System.IO;
using System.Windows;

namespace AutoCADTools.Core.Utils
{
  public static class MessageUtils
  {
    public static MessageBoxResult Notification(string content, string title = "通知")
    {
      return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static MessageBoxResult Notification_YesNo(string content, string title = "通知")
    {
      return MessageBox.Show(content, title, MessageBoxButton.YesNo, MessageBoxImage.Information);
    }

    public static MessageBoxResult Notification_OKCancel(string content, string title = "通知")
    {
      return MessageBox.Show(content, title, MessageBoxButton.OKCancel, MessageBoxImage.Information);
    }

    public static MessageBoxResult Warning(string content, string title = "警告")
    {
      return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static MessageBoxResult Warning_YesNo(string content, string title = "警告")
    {
      return MessageBox.Show(content, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
    }

    public static MessageBoxResult Warning_OKCancel(string content, string title = "警告")
    {
      return MessageBox.Show(content, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
    }

    public static MessageBoxResult Error(string content, string title = "エラーメッセージ")
    {
      return MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static MessageBoxResult Error_YesNo(string content, string title = "エラーメッセージ")
    {
      return MessageBox.Show(content, title, MessageBoxButton.YesNo, MessageBoxImage.Error);
    }

    public static MessageBoxResult Error_OKCancel(string content, string title = "エラーメッセージ")
    {
      return MessageBox.Show(content, title, MessageBoxButton.OKCancel, MessageBoxImage.Error);
    }

    public static bool IsFileLocked(FileInfo file)
    {
      try {
        using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
        stream.Close();
      }
      catch {
        return true;
      }

      return false;
    }
  }
}