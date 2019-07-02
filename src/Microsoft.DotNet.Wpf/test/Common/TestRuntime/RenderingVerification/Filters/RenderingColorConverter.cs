// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
        
using System;
using System.Drawing;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text.RegularExpressions;
using Microsoft.Test.RenderingVerification;

namespace Microsoft.Test.RenderingVerification.Filters
{
    /// <summary>
    /// Summary description for RenderingColorConverter.
    /// </summary>
    [BrowsableAttribute(false)]
    public class RenderingColorEditor: UITypeEditor
    {
        /// <summary>
        /// Default 'ctor
        /// </summary>
        public RenderingColorEditor() { }

        /// <summary>
        /// Indicates whether the UITypeEditor provides a form-based (modal) dialog, 
        /// drop down dialog, or no UI outside of the properties window.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return  UITypeEditorEditStyle.DropDown;
        }

        /// <summary>
        /// Displays the UI for value selection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            IColor color = (IColor)context.PropertyDescriptor.GetValue(context.Instance);
            ColorDialog colorDialog = new ColorDialog();

            colorDialog.Color = color.ToColor();
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                color.ARGB = colorDialog.Color.ToArgb();
                colorDialog.Dispose();
            }
            return (ColorByte)color;
        }
    }

    /// <summary>
    /// Summary description for NormalizedColorConverter.
    /// </summary>
    [BrowsableAttribute(false)]
    public class RenderingColorConverter : ExpandableObjectConverter
    {
        /// <summary>
        /// Returns whether this converter can convert the object to the specified type
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context. </param>
        /// <param name="destinationType">A Type that represents the type you want to convert to.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType is IColor)
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }
        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
        /// <param name="culture">A CultureInfo object. If a null reference (Nothing in Visual Basic) is passed, the current culture is assumed.</param>
        /// <param name="value">The Object to convert.</param>
        /// <param name="destinationType">The Type to convert the value parameter to. </param>
        /// <returns>An Object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is IColor)
            {
                if (((IColor)value).IsEmpty)
                {
                    return ((IColor)value).ToString();
                }
                byte A  = (byte)value.GetType().InvokeMember("A", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
                byte R  = (byte)value.GetType().InvokeMember("R", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
                byte G  = (byte)value.GetType().InvokeMember("G", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
                byte B  = (byte)value.GetType().InvokeMember("B", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
                return "ARGB ( " + A + " / " + R + " / " + G + " / " + B + ")";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context. </param>
        /// <param name="sourceType">A Type that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and culture information.
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that provides a format context.</param>
        /// <param name="culture">The CultureInfo to use as the current culture.</param>
        /// <param name="value">The Object to convert.</param>
        /// <returns>An Object that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    IColor color = new ColorDouble();
                    if (((string)value).Trim().ToLower() != ColorDouble.Empty.ToString().ToLower())
                    { 
                        string ARGB = Regex.Replace((string)value, @"ARGB\s*\(\s*(.+?)\s*/\s*(.+?)\s*/\s*(.+?)\s*/\s*(.+?)\)", "$1_$2_$3_$4", RegexOptions.IgnoreCase); 
#if CLR_VERSION_BELOW_2
                        string[] channels = ARGB.Split(new char[] { '_' });
#else
                        string[] channels = ARGB.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
#endif
                        if (channels.Length != 4)
                        { 
                            throw new FormatException("NormalizedColor param not formatted as expected");
                        }
                        color.A = byte.Parse(channels[0]);
                        color.R = byte.Parse(channels[1]);
                        color.G = byte.Parse(channels[2]);
                        color.B = byte.Parse(channels[3]);
                    }
                    return color;
                }
                catch 
                {
                    throw new ArgumentException("Cannot convert '" + value.ToString() +"' to type 'NormalizedColor'");
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

}
