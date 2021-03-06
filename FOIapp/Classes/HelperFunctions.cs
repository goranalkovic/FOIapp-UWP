﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Helpers
{

    static class FunctionsAndInterfaces
    {
        public static string connectionString = "Data Source=.;Initial Catalog=bp2-db;Integrated Security=True";
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var random = new Random();
            var copy = source.ToArray();
            
            for (var i = copy.Length - 1; i >= 0; i--)
            {
                var index = random.Next(i + 1);
                yield return copy[index];
                copy[index] = copy[i];
            }
        }

        public static T FindControl<T>(UIElement parent, Type targetType, string ControlName) where T : FrameworkElement
        {

            if (parent == null) return null;

            if (parent.GetType() == targetType && ((T)parent).Name == ControlName)
            {
                return (T)parent;
            }
            T result = null;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                if (FindControl<T>(child, targetType, ControlName) != null)
                {
                    result = FindControl<T>(child, targetType, ControlName);
                    break;
                }
            }
            return result;
        }

        public static T GetAncestorOfType<T>(FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && !(parent is T))
                return (T)GetAncestorOfType<T>((FrameworkElement)parent);
            return (T)parent;
        }
    }
    class FluentDesign
    {

        public static Brush GenerateAcrylicBrush(Color acrylicColor, double acrylicOpacity = 0.7, bool inAppAcrylic = false)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
                return new AcrylicBrush
                {
                    BackgroundSource = inAppAcrylic ? AcrylicBackgroundSource.Backdrop : AcrylicBackgroundSource.HostBackdrop,
                    TintColor = acrylicColor,
                    FallbackColor = acrylicColor,
                    TintOpacity = acrylicOpacity
                };

            return new SolidColorBrush(acrylicColor);
        }

        public static Brush GenerateAcrylicBrush(Color acrylicColor, Color alternateColor, double acrylicOpacity = 0.7, bool inAppAcrylic = false)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
                return new AcrylicBrush
                {
                    BackgroundSource = inAppAcrylic ? AcrylicBackgroundSource.Backdrop : AcrylicBackgroundSource.HostBackdrop,
                    TintColor = acrylicColor,
                    FallbackColor = alternateColor,
                    TintOpacity = acrylicOpacity
                };

            return new SolidColorBrush(alternateColor);
        }

        public static void SetTransparentTitleBar(Color buttonColor)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = buttonColor;
        }
    }   
}