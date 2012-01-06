// 
// WPFEngine.cs
//  
// Author:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
// 
// Copyright (c) 2011 Carlos Alberto Cortez
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
using System.Windows;

using Xwt.Backends;
using Xwt.Drawing;
using Xwt.Engine;

namespace Xwt.WPFBackend
{
	public class WPFEngine : Xwt.Backends.EngineBackend
	{
		System.Windows.Application application;

		public override void InitializeApplication ()
		{
			application = new System.Windows.Application ();

			WidgetRegistry.RegisterBackend (typeof (Window), typeof (WindowBackend));
			WidgetRegistry.RegisterBackend (typeof (Menu), typeof (MenuBackend));
			WidgetRegistry.RegisterBackend (typeof (MenuItem), typeof (MenuItemBackend));
			WidgetRegistry.RegisterBackend (typeof (Box), typeof (BoxBackend));

			WidgetRegistry.RegisterBackend (typeof (Font), typeof (FontBackendHandler));
		}

		public override void RunApplication ()
		{
			application.Run ();
		}

		public override void Invoke (Action action)
		{
			application.Dispatcher.BeginInvoke (action, new object [0]);
		}

		public override object TimeoutInvoke (Func<bool> action, TimeSpan timeSpan)
		{
			throw new NotImplementedException ();
		}

		public override void CancelTimeoutInvoke (object id)
		{
			throw new NotImplementedException ();
		}

		public override IWindowFrameBackend GetBackendForWindow (object nativeWindow)
		{
			return new WindowFrameBackend () {
				Window = (System.Windows.Window) nativeWindow
			};
		}

		public override object GetNativeWidget (Widget w)
		{
			var backend = (IWpfWidgetBackend) WidgetRegistry.GetBackend (w);
			return backend.Widget;
		}

		public override object GetNativeParentWindow (Widget w)
		{
			var backend = (IWpfWidgetBackend) WidgetRegistry.GetBackend (w);

			FrameworkElement e = backend.Widget;
			while ((e = e.Parent as FrameworkElement) != null)
				if (e is System.Windows.Window)
					return e;

			return null;
		}
	}
}

