using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace IDC.Helper;

public static class GridColumnHelper
{
    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached(
            "IsVisible",
            typeof(bool),
            typeof(GridColumnHelper),
            new PropertyMetadata(true, OnIsVisibleChanged));

    public static bool GetIsVisible(DependencyObject obj) => (bool)obj.GetValue(IsVisibleProperty);
    public static void SetIsVisible(DependencyObject obj, bool value) => obj.SetValue(IsVisibleProperty, value);

    private static readonly DependencyProperty CachedWidthProperty =
        DependencyProperty.RegisterAttached(
            "CachedWidth",
            typeof(GridLength),
            typeof(GridColumnHelper),
            new PropertyMetadata(default(GridLength)));

    private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColumnDefinition colDef)
        {
            bool isVisible = (bool)e.NewValue;
            if (!isVisible)
            {
                // 숨기기 전 사용자가 조절했던 현재 너비를 캐시에 저장 (0보다 클 때만)
                if (colDef.Width.Value > 0)
                {
                    colDef.SetValue(CachedWidthProperty, colDef.Width);
                }
                colDef.Width = new GridLength(0);
            }
            else
            {
                // 저장해둔 캐시 크기가 있다면 복원, 없으면 기존 너비 유지
                if (colDef.ReadLocalValue(CachedWidthProperty) is GridLength cachedWidth && cachedWidth.Value > 0)
                {
                    colDef.Width = cachedWidth;
                }
            }
        }
    }
}
