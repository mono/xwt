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
using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	class CanvasTableCell: NSView, ICellRenderer
	{
		public CompositeCell CellContainer { get; set; }

		public CellViewBackend Backend { get; set; }

		public void CopyFrom (object other)
		{
			var ob = (CanvasTableCell)other;
			Backend = ob.Backend;
		}

		public void Fill ()
		{
			Hidden = !Frontend.Visible;
		}
		
		ICanvasCellViewFrontend Frontend {
			get { return (ICanvasCellViewFrontend) Backend.Frontend; }
		}

		public override CGSize FittingSize {
			get {
				var size = CGSize.Empty;
				Frontend.ApplicationContext.InvokeUserCode (delegate {
					var s = Frontend.GetRequiredSize ();
					size = new CGSize ((nfloat)s.Width, (nfloat)s.Height);
				});
				return size;
			}
		}

		public override void DrawRect (CGRect dirtyRect)
		{
			Backend.Load (this);
			Frontend.ApplicationContext.InvokeUserCode (delegate {
				CGContext ctx = NSGraphicsContext.CurrentContext.GraphicsPort;

				var backend = new CGContextBackend {
					Context = ctx,
					InverseViewTransform = ctx.GetCTM ().Invert ()
				};
				var bounds = Backend.CellBounds;
				backend.Context.ClipToRect (dirtyRect);
				backend.Context.TranslateCTM ((nfloat)(-bounds.X), (nfloat)(-bounds.Y));
				Frontend.Draw (backend, new Rectangle (bounds.X, bounds.Y, bounds.Width, bounds.Height));
			});
		}
	}
}

