using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace IDC.Helper;

public static class FileDropBehavior
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".png", ".pdf" };

    public static readonly DependencyProperty DropCommandProperty =
        DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(ICommand),
            typeof(FileDropBehavior),
            new PropertyMetadata(onDropCommandChanged));

    public static ICommand GetDropCommand(DependencyObject obj) => (ICommand)obj.GetValue(DropCommandProperty);
    public static void SetDropCommand(DependencyObject obj, ICommand value) => obj.SetValue(DropCommandProperty, value);

    public static readonly DependencyProperty OverlayElementProperty =
            DependencyProperty.RegisterAttached(
                "OverlayElement",
                typeof(UIElement),
                typeof(FileDropBehavior),
                new PropertyMetadata(null));

    public static UIElement GetOverlayElement(DependencyObject obj) => (UIElement)obj.GetValue(OverlayElementProperty);
    public static void SetOverlayElement(DependencyObject obj, UIElement value) => obj.SetValue(OverlayElementProperty, value);

    private static void onDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            if (e.OldValue != null)
            {
                element.AllowDrop = false;
                element.DragEnter -= Element_DragEnter;
                element.DragOver -= Element_DragOver;
                element.DragLeave -= Element_DragLeave;
                element.Drop -= Element_Drop;
            }
            if (e.NewValue != null)
            {
                element.AllowDrop = true;
                element.DragEnter += Element_DragEnter;
                element.DragOver += Element_DragOver;
                element.DragLeave += Element_DragLeave;
                element.Drop += Element_Drop;
            }
        }
    }

    private static bool IsValidFileDrop(System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) return false;
        if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files)
        {
            return files.Any(f => AllowedExtensions.Contains(Path.GetExtension(f)?.ToLower()));
        }
        return false;
    }

    private static void Element_DragEnter(object sender, System.Windows.DragEventArgs e)
    {
        if (sender is DependencyObject d)
        {
            if (IsValidFileDrop(e))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
                VisibleOverlay(sender);
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }
        e.Handled = true;
    }

    private static void Element_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (IsValidFileDrop(e))
        {
            e.Effects = System.Windows.DragDropEffects.Copy;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }
        e.Handled = true;
    }

    private static void Element_DragLeave(object sender, System.Windows.DragEventArgs e)
    {
        HideOverlay(sender);
        e.Handled = true;
    }

    private static void Element_Drop(object sender, System.Windows.DragEventArgs e)
    {
        HideOverlay(sender);

        if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            // 허용된 확장자만 필터링
            var validFiles = files.Where(f => AllowedExtensions.Contains(Path.GetExtension(f)?.ToLower())).ToList();

            if (validFiles.Any())
            {
                var command = GetDropCommand(sender as DependencyObject);
                if (command != null && command.CanExecute(validFiles))
                {
                    command.Execute(validFiles);
                }
            }
        }
    }

    private static void VisibleOverlay(object sender)
    {
        if (sender is DependencyObject d)
        {
            var overlay = GetOverlayElement(d);
            if (overlay != null) overlay.Visibility = Visibility.Visible;
        }
    }

    private static void HideOverlay(object sender)
    {
        if (sender is DependencyObject d)
        {
            var overlay = GetOverlayElement(d);
            if (overlay != null) overlay.Visibility = Visibility.Collapsed;
        }
    }
}
