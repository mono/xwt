// 
// FontBackendHandler.cs
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

using Font = System.Drawing.Font;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.Gdi.Backend {

    public class FontBackendHandler : IFontBackendHandler {

        public object CreateFromName(string fontName, double size) {
            return new Font(fontName, (float)size);
        }

        

        public object Copy(object handle) {
            Font d = (Font)handle;
            return d.Clone();
        }


        public object SetSize(object handle, double size) {
            var d = (Font)handle;
            if (d.Size != (int)size) {
                d = new Font(d.FontFamily, (float)size, d.Style);
            }
            return d;
        }

        public object SetFamily(object handle, string family) {
            var d = (Font)handle;
            if (d.FontFamily.Name != family) {
                d = new Font(family, d.Size, d.Style);
            }
            return d;
        }

        System.Drawing.FontStyle Convert(FontStyle style, FontWeight weight) {
            var result = System.Drawing.FontStyle.Regular;
            if (FontStyle.Italic == style || FontStyle.Oblique == style)
                result |= System.Drawing.FontStyle.Italic;
            if (FontWeight.Heavy == weight || FontWeight.Bold == weight || FontWeight.Ultrabold == weight)
                result |= System.Drawing.FontStyle.Bold;
            return result;
        }

         FontStyle Convert(System.Drawing.FontStyle style) {
             if (style == System.Drawing.FontStyle.Italic)
                 return FontStyle.Italic;
             return FontStyle.Normal;

         }

         FontWeight ConvertW(System.Drawing.FontStyle style) {
             if (style == System.Drawing.FontStyle.Bold)
                 return FontWeight.Bold;
             return FontWeight.Normal;

         }

        public object SetStyle(object handle, FontStyle style) {
            var d = (Font)handle;
            var oldStyle = Convert(d.Style);
            var w = ConvertW(d.Style);

            if (oldStyle != style) {
                d = new Font(d.FontFamily, d.Size, Convert(style, w));
            }
            return d;

        }

        public object SetWeight(object handle, FontWeight weight) {
            var d = (Font)handle;
            var oldW = ConvertW(d.Style);
            var s = Convert(d.Style);

            if (oldW != weight) {
                d = new Font(d.FontFamily, d.Size, Convert(s, weight));
            }
            return d;
        }

        public object SetStretch(object handle, FontStretch stretch) {
           
            return handle;
        }

        public double GetSize(object handle) {
            var d = (Font)handle;
            return d.SizeInPoints;
        }

        public string GetFamily(object handle) {
            var d = (Font)handle;
            return d.FontFamily.Name;
        }

        public FontStyle GetStyle(object handle) {
            var d = (Font)handle;
            return Convert(d.Style);
        }

        public FontWeight GetWeight(object handle) {
            var d = (Font)handle;
            return ConvertW(d.Style);
        }

        public FontStretch GetStretch(object handle) {
            return FontStretch.Normal;
        }



    }
}

