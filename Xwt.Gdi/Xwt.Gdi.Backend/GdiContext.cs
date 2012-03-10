// 
// Colors.cs
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

namespace Xwt.Gdi.Backend
{
	public class GdiContext:IDisposable
	{

		public Graphics Graphics;

		public GraphicsState State { get; set; }

		private PointF _current;

		public PointF Current {
			get {
				if (Path.PointCount == 0)
					return _current;
				return Path.GetLastPoint ();
			}
			set { _current = value;}
		}

		GraphicsPath _path = null;

		public GraphicsPath Path {
			get { return _path ?? (_path = new GraphicsPath ()); }
			set {
				if (_path != value && _path != null)
					_path.Dispose ();
				_path = value;
			}
		}

		public Color Color { get; set; }

		public double LineWidth { get; set; }

		public double[] LineDash { get; set; }

		public object Pattern { get; set; }

		SolidBrush _brush = null;

		public Brush Brush {
			get {
				if (_brush == null)
					_brush = new SolidBrush (this.Color);
				else
					_brush.Color = this.Color;
				return _brush;
			}
			set { _brush = value as SolidBrush; }
		}

		Pen _pen = null;

		public Pen Pen {
			get {
				if (_pen == null)
					_pen = new Pen (Color);
				else
					_pen.Color = Color;

				_pen.Width = (float)LineWidth;
				return _pen;
			}
			set { _pen = value; }
		}

		public void Dispose ()
		{
			if (_path != null)
				Path.Dispose ();
			if (_pen != null)
				_pen.Dispose ();
			if (_brush != null)
				_brush.Dispose ();
		}

		public Xwt.Drawing.Font Font { get; set; }
	}
}