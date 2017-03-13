// 
// ProgressBarBackend.cs
//  
// Author:
//       Jérémie Laval
// 
// Copyright (c) 2012 Xamarin
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
	public class SpinnerBackend: ViewBackend<ProgressIndicatorView, IWidgetEventSink>, ISpinnerBackend
	{
		bool animating;

		public override void Initialize ()
		{
			var widget = new ProgressIndicatorView ();
			ViewObject = widget;

			widget.Indeterminate = true;
			widget.Style = NSProgressIndicatorStyle.Spinning;
		}

		public bool IsAnimating {
			get {
				return animating;
			}
		}

		public void StartAnimation ()
		{
			animating = true;
			Widget.StartAnimation (Widget);
		}

		public void StopAnimation ()
		{
			animating = false;
			Widget.StopAnimation (Widget);
		}
	}
}

