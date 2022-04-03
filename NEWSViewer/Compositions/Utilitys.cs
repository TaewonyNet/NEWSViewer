using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NEWSViewer.Compositions
{
    public class Utilitys
    {

        public static T GetParentControl<T>(DependencyObject obj) where T : DependencyObject
        {
            if ((null == obj) || !(obj is FrameworkElement))
            {
                return default(T);
            }

            DependencyObject dobj = (obj as FrameworkElement);
            while (null != dobj)
            {
                if (dobj is FrameworkElement)
                {
                    dobj = ((FrameworkElement)dobj).Parent;
                    if ((dobj != null)
                        && (typeof(T) == dobj.GetType()))
                    {
                        return (T)dobj;
                    }
                }
            }
            return default(T);
        }

        public static DependencyObject GetChildrenControl(DependencyObject dobj, Type type)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dobj); i++)
            {
                DependencyObject ret = VisualTreeHelper.GetChild(dobj, i);
                if (ret.GetType() == type)
                {
                    return ret;
                }
                else
                {
                    DependencyObject child = GetChildrenControl(ret, type);
                    if (null != child)
                    {
                        return child;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 하위 컨트롤의 특정타입만 찾아온다.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> GetChildElement<T>(object obj)
        {
            List<T> result = new List<T>();
            if (obj is FrameworkElement)
            {
                FrameworkElement element = obj as FrameworkElement;
                if (element is T)
                {
                    result.Add((T)obj);
                }
                else if (element is Panel)
                {
                    foreach (UIElement child in (element as Panel).Children)
                    {
                        foreach (T findelement in GetChildElement<T>(child))
                        {
                            result.Add(findelement);
                        }
                    }
                }
                else if (element is System.Windows.Controls.Primitives.Selector)
                {
                    foreach (object item in (element as System.Windows.Controls.Primitives.Selector).Items)
                    {
                        if (item is UIElement)
                        {
                            UIElement child = item as UIElement;
                            foreach (T findelement in GetChildElement<T>(child))
                            {
                                result.Add(findelement);
                            }
                        }
                    }
                }
                else if (element is ContentPresenter)
                {
                    foreach (T findelement in GetChildElement<T>((element as ContentPresenter).Content))
                    {
                        result.Add(findelement);
                    }
                }
                else if (element is ContentControl)
                {
                    foreach (T findelement in GetChildElement<T>((element as ContentControl).Content))
                    {
                        result.Add(findelement);
                    }
                }
                else if (element is Decorator)
                {
                    foreach (T findelement in GetChildElement<T>((element as Decorator).Child))
                    {
                        result.Add(findelement);
                    }
                }
                else if (element is ItemsControl)
                {
                    foreach (T findelement in GetChildElement<T>((element as ItemsControl).Items))
                    {
                        result.Add(findelement);
                    }
                }
            }
            return result;
        }
    }
}
