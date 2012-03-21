// 
// TextLayoutBackend.cs
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

using Xwt.Drawing;

namespace Xwt.Gdi.Backend {

    public class TextLayoutBackend {
        public GdiContext Context { get; set; }
        private double _width;
        private string _text;
        private Xwt.Drawing.Font _font;

        public double Width {
            get { return _width; } 
            set {
                if (_width != value)
                    _size = null;
                _width = value;
            }
        }

        public string Text {
            get { return _text; } 
            set {
                if (_text != value)
                    _size = null;
                _text = value;
            }
        }

        public Font Font {
            get { return _font; }
            set {
                if (!Xwt.Drawing.XwtDrawingExtensions.Equals (_font, value))
                    _size = null;
                _font = value;
            }
        }

        Size? _size = null;
        public Size Size {
            get {
                if (_size == null) {
                    var font = Font.ToGdi ();
                    var size = new System.Drawing.SizeF ((float) Width, 0);
                    _size = Context.Graphics.MeasureString (Text, font, size,Format).ToXwt ();
                }
                return _size.Value;
            }
        }

        System.Drawing.StringFormat _format = null;
        public System.Drawing.StringFormat Format {
            get {
                return _format ?? (_format = GdiConverter.GetDefaultStringFormat ());
            }
        }
    }
}