// 
// GdiContext.cs
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
using System.Drawing.Drawing2D;
using System;
using System.Collections.Generic;


namespace Xwt.Gdi.Backend {
    public class GdiContext : IDisposable {
        public GdiContext () {
            Color = Color.Black;
            LineWidth = 1;
        }

        public GdiContext (GdiContext c)
            : this () {
            c.SaveTo (this);
        }
        public Graphics Graphics;
        public GraphicsState State { get; set; }
        public GraphicsContainer State1 { get; set; }

        private PointF _current;
        public PointF Current {
            get { return _current; }
            set { _current = value; }
        }

        GraphicsPath _path = null;
        public GraphicsPath Path {
            get {
                if (_path == null) {
                    _path = new GraphicsPath ();
                    _transformed = false;
                }
                return _path;
            }
            set {
                if (_path != value && _path != null)
                    _path.Dispose ();
                if (_path != value)
                    _transformed = false;
                _path = value;
            }
        }

        Matrix _matrix = null;
        public Matrix Matrix {
            get {

                if (_matrix == null) {
                    _matrix = new Matrix ();
                }
                return _matrix;

            }
            set {
                if (_matrix != value && _matrix != null)
                    _matrix.Dispose ();
                _matrix = value;
                if (_path != null)
                    _path.Transform(Matrix);
            }
        }

        public Color Color { get; set; }
        public double LineWidth { get; set; }
        public double[] LineDash { get; set; }
        public object Pattern { get; set; }

        Brush _brush = null;
        public Brush Brush {
            get {
                if (_brush == null)
                    _brush = new SolidBrush (this.Color);
                else if(_brush is SolidBrush)
                    ((SolidBrush)_brush).Color = this.Color;
                return _brush;
            }
            set { _brush = value; }
        }

        Pen _pen = null;
        public Pen Pen {
            get {
                if (_pen == null)
                    _pen = new Pen (Color);
                else
                    _pen.Color = Color;

                _pen.Width = (float) LineWidth;
                return _pen;
            }
            set { _pen = value; }
        }

        public void Dispose () {
            if (_path != null)
                Path.Dispose ();
            if (_pen != null)
                _pen.Dispose ();
            if (_brush != null)
                _brush.Dispose ();
            if (_matrix != null)
                _matrix.Dispose ();
            if (this.contexts != null) {
                foreach (var c in this.contexts)
                    c.Dispose();
                contexts.Clear ();
            }
        }

        public Xwt.Drawing.Font Font { get; set; }

        public bool HasTransform {
            get { return _matrix != null; }
        }

        private bool _transformed = false;
        public void Transform () {
            if (!_transformed && _path != null) {

                // this transforms path.points immedeatly:
                Path.Transform (Matrix);
                Matrix.Reset ();

                _transformed = true;
            }
        }

        public void Rotate (float p) {
            Matrix.Rotate (p, MatrixOrder.Append);
        }

        public void ResetTransform () {
            if (_matrix != null) {
                Matrix.Reset ();
            }
            Graphics.ResetTransform ();
            _transformed = false;
        }

        public void Translate (float x, float y) {
            Graphics.TranslateTransform (x, y);
        }

        public void TranslatePath (float x, float y) {
            Matrix.Translate (x, y);
        }

        public void SaveTo (GdiContext c) {
            c.Font = this.Font;
            c.Pen = this._pen;
            c.LineDash = this.LineDash;
            c.Brush = this._brush;
            c.Matrix = this._matrix;
            c.Path = this._path;
            c.Current = this.Current;
            //return c;
        }

        protected Stack<GdiContext> contexts;

        public void Save () {
            if (this.contexts == null)
                this.contexts = new Stack<GdiContext> ();

            this.contexts.Push (new GdiContext (this) { State = Graphics.Save () });
        }


        public void Restore () {
            if (this.contexts == null || this.contexts.Count == 0)
                throw new InvalidOperationException ();

            var c = this.contexts.Pop ();

            c.SaveTo (this);
            Graphics.Restore (c.State);

        }



    }
}