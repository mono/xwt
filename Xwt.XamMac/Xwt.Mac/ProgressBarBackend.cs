// 
// ProgressBarBackend.cs
//  
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
// 
// Copyright (c) 2012 Andres G. Aragoneses
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

using AppKit;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class ProgressBarBackend: ViewBackend<ProgressIndicatorView, IWidgetEventSink>, IProgressBarBackend
	{
		public ProgressBarBackend ()
		{
		}

		public override void Initialize ()
		{
			var widget = new ProgressIndicatorView ();
			ViewObject = widget;

			widget.MinValue = 0.0;
			widget.MaxValue = 1.0;
			widget.DoubleValue = 0.0;

			widget.Indeterminate = false;
		}
		
		public void SetFraction (double fraction)
		{
			var widget = (NSProgressIndicator)ViewObject;

			if (fraction == widget.DoubleValue)
				return;

			widget.DoubleValue = fraction;
		}

		public void SetIndeterminate (bool value) {
			var widget = (NSProgressIndicator)ViewObject;

			if (widget.Indeterminate == value)
				return;

			widget.Indeterminate = value;
			if (value)
				widget.StartAnimation (null);
		}
	}
	
	public class ProgressIndicatorView: NSProgressIndicator, IViewObject
	{
		public ProgressIndicatorView ()
		{
		}
		
		public ViewBackend Backend { get; set; }
		
		public NSView View {
			get { return this; }
		}

		public override void ResetCursorRects ()
		{
			base.ResetCursorRects ();
			if (Backend.Cursor != null)
				AddCursorRect (Bounds, Backend.Cursor);
		}
	}
}

