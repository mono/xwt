// 
// GdiConverter.cs
//  
// Author:
//       Lytico 
// 
// Copyright (c) 2012 Lytico (http://limada.sourceforge.net)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Drawing;
using Xwt.Engine;
using Color = Xwt.Drawing.Color;
using Font = Xwt.Drawing.Font;
using FontStyle = Xwt.Drawing.FontStyle;

namespace Xwt.Gdi {

    public static class GdiConverter {

        public static System.Drawing.Color ToGdi(this Color color) {
            return System.Drawing.Color.FromArgb((int)ToArgb(color));
        }

        public static Color ToXwt(this System.Drawing.Color color) {
            return FromArgb((uint)color.ToArgb());
        }

        public static uint ToArgb(Color color) {
            return
                (uint)(color.Alpha * 255) << 24
                | (uint)(color.Red * 255) << 16
                | (uint)(color.Green * 255) << 8
                | (uint)(color.Blue * 255);

        }

        public static Color FromArgb(uint argb) {
            var a = (argb >> 24) / 255d;
            var r = ((argb >> 16) & 0xFF) / 255d;
            var g = ((argb >> 8) & 0xFF) / 255d;
            var b = (argb & 0xFF) / 255d;
            return new Color(r, g, b, a);

        }

        public static Color FromArgb(byte a, Color color) {
            return new Color(color.Red, color.Green, color.Blue, a);
        }

        public static Color FromArgb(byte a, byte r, byte g, byte b) {
            return Color.FromBytes(r, g, b, a);
        }

        public static Color FromArgb(byte r, byte g, byte b) {
            return Color.FromBytes(r, g, b);
        }

        public static System.Drawing.FontStyle ToGdi(this FontStyle value) {
            var result = System.Drawing.FontStyle.Regular;
            if (value == null)
                return result;
            if ((value & FontStyle.Italic) != 0) {
                result |= System.Drawing.FontStyle.Italic;
            }
            //if ((value & FontStyle.Underline) != 0) {
            //    result |= System.Drawing.FontStyle.Underline;
            //}
            if ((value & FontStyle.Oblique) != 0) {
                result |= System.Drawing.FontStyle.Bold;
            }
            return result;
        }

        public static FontStyle ToXwt(this System.Drawing.FontStyle value) {
            var result = FontStyle.Normal;
            if (value == null)
                return result;
            if ((value & System.Drawing.FontStyle.Italic) != 0) {
                result |= FontStyle.Italic;
            }
            //if ((native & System.Drawing.FontStyle.Underline) != 0) {
            //    result |= FontStyle.Underline;
            //}
            if ((value & System.Drawing.FontStyle.Bold) != 0) {
                result |= FontStyle.Oblique;
            }
            return result;
        }

        public static System.Drawing.Font ToGdi (this Font value) {
            return (System.Drawing.Font) WidgetRegistry.GetBackend (value);
        }

        public static Size ToXwt (this SizeF value) {
            return new Size (value.Width, value.Height);
        }

        public static System.Drawing.StringFormat GetDefaultStringFormat() {
            var stringFormat =
                StringFormat.GenericTypographic;
            stringFormat.Trimming = StringTrimming.EllipsisWord;
            //stringFormat.FormatFlags = StringFormatFlags.FitBlackBox;
            stringFormat.FormatFlags = stringFormat.FormatFlags
                                       & ~StringFormatFlags.NoClip
                                       & ~StringFormatFlags.FitBlackBox
                                       & StringFormatFlags.LineLimit
                ;
            return stringFormat;
        }
    }
}