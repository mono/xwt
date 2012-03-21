// 
// ImageGraphics.cs
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


using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Xwt.Gdi.Backend {
    public class ImageGraphics : IGdiGraphicsBackend, IDisposable {

        public ImageGraphics(int width, int height, Drawing.ImageFormat format) {
            var sdFormat = PixelFormat.DontCare;
            if (format == Drawing.ImageFormat.ARGB32)
                sdFormat = PixelFormat.Format32bppPArgb;
            else if (format == Drawing.ImageFormat.RGB24)
                sdFormat = PixelFormat.Format24bppRgb;
            this.Image = new Bitmap(width, height, sdFormat);
        }

        public Image Image { get; protected set; }

        System.Drawing.Graphics _graphics = null;
        public System.Drawing.Graphics Graphics {
            get {
                if (Image == null)
                    return null;
                if(_graphics==null) {
                    _graphics = Graphics.FromImage(Image);
                    _graphics.SetQuality(GdiConverter.PaintHighQuality);
                }
                return _graphics;
            }
        }

        protected void Dispose(bool disposing) {
            if (_graphics != null) {
                _graphics.Dispose();
                _graphics = null;
            }
            if (Image != null)
                Image.Dispose();
            Image = null;
        }

        public void Dispose() {
            Dispose(true);
        }

        ~ImageGraphics() {
            Dispose(false);
        }

    }
}