#nullable enable

using System.Windows;
using AutoCADTools.Core.Localization;

namespace AutoCADTools.Core.Utils
{
  public static class MessageUtils
  {
    public static MessageBoxResult Notification(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Notification".GetString(), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static MessageBoxResult Notification_YesNo(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Notification".GetString(), MessageBoxButton.YesNo, MessageBoxImage.Information);
    }

    public static MessageBoxResult Notification_OKCancel(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Notification".GetString(), MessageBoxButton.OKCancel, MessageBoxImage.Information);
    }

    public static MessageBoxResult Warning(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Warning".GetString(), MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public static MessageBoxResult Warning_YesNo(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Warning".GetString(), MessageBoxButton.YesNo, MessageBoxImage.Warning);
    }

    public static MessageBoxResult Warning_OKCancel(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Warning".GetString(), MessageBoxButton.OKCancel, MessageBoxImage.Warning);
    }

    public static MessageBoxResult Error(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Error".GetString(), MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static MessageBoxResult Error_YesNo(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Error".GetString(), MessageBoxButton.YesNo, MessageBoxImage.Error);
    }

    public static MessageBoxResult Error_OKCancel(string content, string? title = null)
    {
      return MessageBox.Show(content, title ?? "Message.Title.Error".GetString(), MessageBoxButton.OKCancel, MessageBoxImage.Error);
    }
  }
}