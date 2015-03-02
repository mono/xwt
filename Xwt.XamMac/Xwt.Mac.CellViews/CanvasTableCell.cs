//
// CanvasTableCell.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using Xwt.Backends;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.CoreGraphics;
using MonoMac.AppKit;
#else
using CoreGraphics;
using AppKit;
#endif

namespace Xwt.Mac
{
	class CanvasTableCell: NSCell, ICellRenderer
	{
		public CanvasTableCell (IntPtr p): base (p)
		{
		}

		public CanvasTableCell ()
		{
		}

		public CompositeCell CellContainer { get; set; }

		public void CopyFrom (object other)
		{
			var ob = (CanvasTableCell)other;
			Backend = ob.Backend;
		}

		public void Fill ()
		{
		}
		
		ICanvasCellViewFrontend Frontend {
			get { return (ICanvasCellViewFrontend) Backend.Frontend; }
		}

		public CellViewBackend Backend { get; set; }


		public override CGSize CellSizeForBounds (CGRect bounds)
		{
			var size = new CGSize ();
			Frontend.ApplicationContext.InvokeUserCode (delegate {
				var s = Frontend.GetRequiredSize ();
				size = new SizeF ((float)s.Width, (float)s.Height);
			});
			if (size.Width > bounds.Width)
				size.Width = bounds.Width;
			if (size.Height > bounds.Height)
				size.Height = bounds.Height;
			return size;
		}

		public override void DrawInteriorWithFrame (CGRect cellFrame, NSView inView)
		{
			CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;
			
			var backend = new CGContextBackend {
				Context = ctx,
				InverseViewTransform = ctx.GetCTM ().Invert ()
			};
			Frontend.ApplicationContext.InvokeUserCode (delegate {
				Frontend.Draw (backend, new Rectangle (cellFrame.X, cellFrame.Y, cellFrame.Width, cellFrame.Height));
			});
		}
	}
}

