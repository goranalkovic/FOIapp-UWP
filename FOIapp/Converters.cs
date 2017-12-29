using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using FOIapp.Classes;
using Microsoft.Toolkit.Uwp.Helpers;


namespace Converters
{

    public class RequestedThemeConverter : IValueConverter
    {
        public LocalObjectStorageHelper LocalStorage = new LocalObjectStorageHelper();

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return LocalStorage.Read<bool>("uiTheme") ? ElementTheme.Dark : ElementTheme.Light;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsultationLabelVisibleConverter : IValueConverter
    {
        public LocalObjectStorageHelper LocalStorage = new LocalObjectStorageHelper();

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return ((List<TeacherConsultations>)value).Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsultationDayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture = "hr-HR")
        {
            switch (value)
            {
                case 1:
                case 10:
                    return "Ponedjeljak";
                case 2:
                case 20:
                    return "Utorak";
                case 3:
                case 30:
                    return "Srijeda";
                case 4:
                case 40:
                    return "Četvrtak";
                case 5:
                case 50:
                    return "Petak";
                case -1:
                    return "Po dogovoru";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SpecialConsultationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture = "hr-HR")
        {
            switch (value)
            {
                case 10:
                case 20:
                case 30:
                case 40:
                case 50:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsultationFoi2LabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture = "hr-HR")
        {
            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConsultationTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return (string) value != "-1" ? (string) value : "";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
