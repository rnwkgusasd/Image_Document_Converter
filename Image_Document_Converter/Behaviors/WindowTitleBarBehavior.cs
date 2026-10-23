using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace IDC.Behaviors;

public static class WindowTitleBarBehavior
{
    // IsDraggable 첨부 속성 정의
    public static readonly DependencyProperty IsDraggableProperty =
        DependencyProperty.RegisterAttached(
            "IsDraggable",
            typeof(bool),
            typeof(WindowTitleBarBehavior),
            new PropertyMetadata(false, OnIsDraggableChanged));

    public static bool GetIsDraggable(DependencyObject obj) => (bool)obj.GetValue(IsDraggableProperty);
    public static void SetIsDraggable(DependencyObject obj, bool value) => obj.SetValue(IsDraggableProperty, value);

    private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            }
            else
            {
                element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            }
        }
    }

    private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            Window window = Window.GetWindow(element);
            if (window == null) return;

            if (e.ClickCount == 2)
            {
                // 더블클릭 시 최대화 / 복원 토글
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
            else if (e.ClickCount == 1)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    var downPoint = e.GetPosition(element);
                    MouseButtonEventHandler upHandler = null;
                    System.Windows.Input.MouseEventHandler moveHandler = null;

                    moveHandler = (s, args) =>
                    {
                        var currentPoint = args.GetPosition(element);
                        double dx = Math.Abs(currentPoint.X - downPoint.X);
                        double dy = Math.Abs(currentPoint.Y - downPoint.Y);

                        // 5픽셀 이상 움직였을 때만 드래그로 판정하여 Normal로 전환
                        if (dx > 5 || dy > 5)
                        {
                            element.MouseMove -= moveHandler;
                            element.MouseLeftButtonUp -= upHandler;

                            // 1. 현재 창의 DPI 배율 가져오기 (다중 모니터별 상이한 배율 대응)
                            DpiScale dpiScale = System.Windows.Media.VisualTreeHelper.GetDpi(window);

                            // 2. 마우스의 현재 물리 픽셀 좌표 획득 후 모니터 판정
                            var mouseScreenPos = element.PointToScreen(currentPoint);
                            Screen currentScreen = Screen.FromPoint(
                                new System.Drawing.Point((int)mouseScreenPos.X, (int)mouseScreenPos.Y)
                            );

                            // 3. WinForms WorkingArea(물리 픽셀)를 WPF의 DIPs(디바이스 독립 픽셀)로 환산
                            double workAreaLeft = currentScreen.WorkingArea.Left / dpiScale.DpiScaleX;
                            double workAreaTop = currentScreen.WorkingArea.Top / dpiScale.DpiScaleY;
                            double workAreaWidth = currentScreen.WorkingArea.Width / dpiScale.DpiScaleX;
                            double workAreaHeight = currentScreen.WorkingArea.Height / dpiScale.DpiScaleY;

                            // 4. 원래 크기(RestoreBounds) 확인 및 모니터 크기 맞춤 보정
                            double restoreWidth = window.RestoreBounds.Width;
                            double restoreHeight = window.RestoreBounds.Height;

                            if (restoreWidth <= 0 || double.IsNaN(restoreWidth)) restoreWidth = 800;
                            if (restoreHeight <= 0 || double.IsNaN(restoreHeight)) restoreHeight = 600;

                            if (restoreWidth > workAreaWidth) restoreWidth = workAreaWidth * 0.8;
                            if (restoreHeight > workAreaHeight) restoreHeight = workAreaHeight * 0.8;

                            // 5. 타이틀바 내에서 마우스가 위치한 가로 비율 계산
                            double ratio = currentPoint.X / element.ActualWidth;
                            if (double.IsNaN(ratio) || ratio < 0 || ratio > 1) ratio = 0.5;

                            // 6. Normal 상태로 변경 및 크기 지정
                            window.WindowState = WindowState.Normal;
                            window.Width = restoreWidth;
                            window.Height = restoreHeight;

                            // 7. 마우스 물리 좌표를 DIPs 좌표로 변환하여 창 위치 계산
                            double mouseDipsX = mouseScreenPos.X / dpiScale.DpiScaleX;
                            double mouseDipsY = mouseScreenPos.Y / dpiScale.DpiScaleY;

                            double newLeft = mouseDipsX - (restoreWidth * ratio);
                            double newTop = mouseDipsY - currentPoint.Y;

                            // 8. 모니터 작업 영역을 벗어나지 않도록 클램핑(Clamping)
                            if (newLeft < workAreaLeft)
                                newLeft = workAreaLeft;
                            if (newLeft + restoreWidth > workAreaLeft + workAreaWidth)
                                newLeft = (workAreaLeft + workAreaWidth) - restoreWidth;
                            if (newTop < workAreaTop)

                                newTop = workAreaTop;

                            window.Left = newLeft;
                            window.Top = newTop;

                            // 9. 창 이동 시작
                            window.DragMove();
                        }
                    };

                    upHandler = (s, args) =>
                    {
                        element.MouseMove -= moveHandler;
                        element.MouseLeftButtonUp -= upHandler;
                    };

                    element.MouseMove += moveHandler;
                    element.MouseLeftButtonUp += upHandler;
                }
                else
                {
                    window.DragMove();
                }
            }
        }
    }
}