﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NEWSViewer.Compositions
{
    #region ZeroToVisibility
    public class ZeroToVisibility : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (true == value.ToString().Equals("0"))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion
    #region NonzeroToVisibility
    public class NonzeroToVisibility : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (false == value.ToString().Equals("0"))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion
    #region NotNullToVisibility
    public class NotNullToVisibility : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        } 

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion

    #region ArticleHighlightColorConveter
    public class ArticleHighlightColorConveter : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                // 읽기 여부
                if (value.Equals(false))
                {
                    return new SolidColorBrush(Global.Instance.NoReadColor);
                }
                else
                {
                    return new SolidColorBrush(Global.Instance.ReadColor);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion

    #region DateTimeStringConveter
    public class DateTimeStringConveter : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is DateTime)
            {
                var date = (DateTime)value;
                return date.ToString("yyyy-MM-dd HH:mm");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion

    #region TrueParamConverter
    public class TrueParamConverter : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool)
            {
                if (value.Equals(false))
                {
                    return parameter;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion

    #region TrueFontBoldConverter
    public class TrueFontBoldConverter : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool)
            {
                if (value.Equals(false))
                {
                    return FontWeights.Bold;
                }
                return FontWeights.Normal;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
    #endregion


    #region StringToBoolConverter
    public class StringToBoolConverter : IValueConverter
    {
        #region IValueConverter 멤버

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.Equals("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.Equals(true))
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            return null;
        }

        #endregion
    }
    #endregion
}
