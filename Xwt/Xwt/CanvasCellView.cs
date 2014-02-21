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

		protected virtual Size OnGetRequiredSize ()
		{
			return new Size ();
		}
		
		#region ICanvasCellRenderer implementation

		void ICanvasCellViewFrontend.Draw (object ctxBackend, Rectangle cellArea)
		{
			using (var ctx = new Context (ctxBackend, Toolkit.CurrentEngine)) {
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
			return OnGetRequiredSize ();
		}

		ApplicationContext ICanvasCellViewFrontend.ApplicationContext {
			get { return new ApplicationContext (Toolkit.CurrentEngine); }
		}

		#endregion
	}
}

