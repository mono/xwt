//
// XwtTest.cs
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
using System.Threading;
using NUnit.Framework;

namespace Xwt
{
	public class XwtTest
	{
		public void Run (Action a)
		{
			Exception ex = null;
			Application.Invoke (delegate {
				try {
					a ();
				} catch (Exception e) {
					ex = e;
				}
				Application.Exit ();
			});
			Application.Run ();
			if (ex != null)
				throw new Exception ("Exception in gui event loop", ex);
		}
		
		protected void WaitForEvents (int ms = 1)
		{
			DateTime t = DateTime.Now;
			do {
				Application.MainLoop.DispatchPendingEvents ();
				System.Threading.Thread.Sleep (20);
			} while ((DateTime.Now - t).TotalMilliseconds < ms);
		}

		public void ShowWindow (Window win)
		{
			var ev = new ManualResetEvent (false);

			win.Shown += delegate {
				ev.Set ();
			};

			win.Show ();
			ev.WaitForEvent ();
			Application.MainLoop.DispatchPendingEvents ();
		}

		public void CheckWidgetRender (string refImageName, Xwt.Widget widget, int winWidth = 300, int winHeight = 100)
		{
			using (var win = new Window { Width = winWidth, Height = winHeight }) {
				win.Content = widget;
				ShowWindow (win);
				var img = Toolkit.CurrentEngine.RenderWidget (widget);
				ReferenceImageManager.CheckImage (refImageName, img);
			}
		}
	}
	
	static class EventHelper
	{
		public static void WaitForEvent (this ManualResetEvent ev)
		{
			DateTime t = DateTime.Now;
			do {
				Application.MainLoop.DispatchPendingEvents ();
				if (ev.WaitOne (100))
					return;
			} while ((DateTime.Now - t).TotalMilliseconds < 1000);

			Assert.Fail ("Event not fired");
		}
	}
}

