//
// CanvasCellView.cs
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
using Xwt.Drawing;
using Xwt.Backends;

namespace Xwt
{
	public class CanvasCellView: CellView, ICanvasCellViewFrontend
	{
		public CanvasCellView ()
		{
		}

		protected void QueueDraw ()
		{
			((ICanvasCellViewBackend)BackendHost.Backend).QueueDraw ();
		}

		/// <summary>
		/// Signals that the size of the cell may have changed, and the row
		/// that contains it may need to be resized
		/// </summary>
		protected void QueueResize ()
		{
			((ICanvasCellViewBackend)BackendHost.Backend).QueueResize ();
		}

		/// <summary>
		/// Called when the cell needs to be redrawn
		/// </summary>
		/// <param name='ctx'>
		/// Drawing context
		/// </param>
		protected virtual void OnDraw (Context ctx, Rectangle cellArea)
		{
		}

		protected virtual Rectangle OnGetDrawingAreaForBounds (Rectangle cellBounds)
		{
			return cellBounds;
		}

		[Obsolete("Use OnGetRequiredSize (SizeConstraint widthConstraint)")]
		protected virtual Size OnGetRequiredSize ()
		{
			return new Size ();
		}

		protected virtual Size OnGetRequiredSize (SizeConstraint widthConstraint)
		{
			#pragma warning disable 618
			return OnGetRequiredSize ();
			#pragma warning restore 618
		}
		
		#region ICanvasCellRenderer implementation

		void ICanvasCellViewFrontend.Draw (object ctxBackend, Rectangle cellArea)
		{
			using (var ctx = new Context (ctxBackend, BackendHost.ToolkitEngine)) {
				ctx.Reset (null);
				OnDraw (ctx, cellArea);
			}
		}

		Rectangle ICanvasCellViewFrontend.GetDrawingAreaForBounds (Rectangle cellBounds)
		{
			return OnGetDrawingAreaForBounds (cellBounds);
		}

		Size ICanvasCellViewFrontend.GetRequiredSize ()
		{
			return OnGetRequiredSize (SizeConstraint.Unconstrained);
		}

		Size ICanvasCellViewFrontend.GetRequiredSize (SizeConstraint widthConstraint)
		{
			return OnGetRequiredSize (widthConstraint);
		}

		ApplicationContext ICanvasCellViewFrontend.ApplicationContext {
			get { return new ApplicationContext (BackendHost.ToolkitEngine); }
		}

		#endregion

		protected bool IsHighlighted {
			get { return ((ICanvasCellViewBackend)BackendHost.Backend).IsHighlighted; }
		}
	}
}

