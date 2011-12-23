// 
// Window.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Backends;

namespace Xwt
{
	public class Window: WindowFrame
	{
		Widget child;
		WidgetSpacing padding;
		Menu mainMenu;
		
		protected new class EventSink: WindowFrame.EventSink, ISpacingListener
		{
			public void OnSpacingChanged (WidgetSpacing source)
			{
				var w = (Window) Parent;
				w.Backend.SetPadding (w.padding.Left, w.padding.Top, w.padding.Right, w.padding.Bottom);
			}
		}
		
		protected override WindowFrame.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		public Window ()
		{
			padding = new WidgetSpacing ((EventSink)WindowEventSink);
			padding.SetAll (6);
		}
		
		public Window (string title): base (title)
		{
			padding = new WidgetSpacing ((EventSink)WindowEventSink);
		}
		
		new IWindowBackend Backend {
			get { return (IWindowBackend) base.Backend; } 
		}
		
		public WidgetSpacing Padding {
			get { return padding; }
		}
		
		public Menu MainMenu {
			get {
				return mainMenu;
			}
			set {
				mainMenu = value;
				Backend.SetMainMenu ((IMenuBackend)GetBackend (mainMenu));
			}
		}
		
		public Widget Content {
			get {
				return child;
			}
			set {
				if (child != null)
					child.SetParentWindow (null);
				this.child = value;
				child.SetParentWindow (this);
				Backend.SetChild ((IWidgetBackend)GetBackend (child));
				AdjustSize ();
			}
		}
		
		protected override void OnReallocate ()
		{
			if (child != null) {
				((IWidgetSurface)child).Reallocate ();
			}
		}
		
		void AdjustSize ()
		{
			IWidgetSurface s = child;
			var w = s.GetPreferredWidth ().MinSize;
			if (w > Width)
				Width = w;
			var h = s.GetPreferredHeightForWidth (Width).MinSize;
			if (h > Height)
				Height = h;
		}
	}
}

