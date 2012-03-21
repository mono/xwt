// 
// ImageHandler.cs
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

using Xwt.Backends;
using SD=System.Drawing;
using System.Drawing.Drawing2D;
using System;

namespace Xwt.Gdi.Backend {

    public class ImageHandler : ImageBackendHandler {

        public override object LoadFromStream (System.IO.Stream stream) {
            return SD.Image.FromStream(stream);
        }

        public override object LoadFromIcon (string id, IconSize size) {
            throw new System.NotImplementedException ();
        }

        public override Size GetSize (object handle) {
            var image = (SD.Image)handle;
            return new Size(image.Width, image.Height);
        }

        public override object Resize(object handle, double width, double height) {
            var source = (SD.Image)handle;
            SD.Image result = new SD.Bitmap(source, (int)width, (int)height);
            return result;

            // try if this has better quality:
            result = new SD.Bitmap((int)width, (int)height, source.PixelFormat);

            using (var g = SD.Graphics.FromImage(result)) {
                g.SetQuality(GdiConverter.CopyHighQuality);
                g.DrawImage(source, 0, 0, source.Width, source.Height);
                g.Flush();
            }
            return result;
        }

        public override object Copy (object handle) {
            var source = (SD.Image)handle;
            SD.Image result = new SD.Bitmap(source, source.Width, source.Height);
            return result;
        }

        public override void CopyArea (object srcHandle, int srcX, int srcY, int width, int height, object destHandle, int destX, int destY) {
            var source = (SD.Image)srcHandle;
            var dest = (SD.Image)destHandle;

            using (var g = SD.Graphics.FromImage(dest)) {
                g.SetQuality(GdiConverter.CopyHighQuality);
                var sr = new SD.Rectangle(srcX, srcY, width, height);
                var dr = new SD.Rectangle(destX, destY, width, height);
                g.DrawImage(source,dr,sr,SD.GraphicsUnit.Pixel);
                g.Flush();
            }

        }

        public override object Crop (object handle, int srcX, int srcY, int width, int height) {
            var source = (SD.Image)handle;
            SD.Image result = new SD.Bitmap(source, source.Width, source.Height);

            using (var g = SD.Graphics.FromImage(result)) {
                g.SetQuality(GdiConverter.CopyHighQuality);
                var sr = new SD.Rectangle(srcX, srcY, width, height);
                var dr = new SD.Rectangle(0, 0, width, height);
                g.DrawImage(source, dr, sr, SD.GraphicsUnit.Pixel);
                g.Flush();
            }
            return result;
        }

        public override object ChangeOpacity (object backend, double opacity) {
            throw new System.NotImplementedException ();
        }
    }
}