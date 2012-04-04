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
		bool shown;
		
		protected new class WindowBackendHost: WindowFrame.WindowBackendHost, ISpacingListener
		{
			public void OnSpacingChanged (WidgetSpacing source)
			{
				var w = (Window) Parent;
				w.Backend.SetPadding (w.padding.Left, w.padding.Top, w.padding.Right, w.padding.Bottom);
			}
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WindowBackendHost ();
		}
		
		public Window ()
		{
			padding = new WidgetSpacing ((WindowBackendHost)BackendHost);
			padding.SetAll (6);
		}
		
		public Window (string title): base (title)
		{
			padding = new WidgetSpacing ((WindowBackendHost)BackendHost);
		}
		
		IWindowBackend Backend {
			get { return (IWindowBackend) BackendHost.Backend; } 
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
				Backend.SetMainMenu ((IMenuBackend)WidgetRegistry.GetBackend (mainMenu));
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
				Backend.SetChild ((IWidgetBackend)WidgetRegistry.GetBackend (child));
				Widget.QueueWindowSizeNegotiation (this);
			}
		}
		
		protected override void OnReallocate ()
		{
			if (child != null && !Application.EngineBackend.HandlesSizeNegotiation) {
				child.Surface.Reallocate ();
			}
		}

		bool widthSet;
		bool heightSet;
		Rectangle initialBounds;

		internal override void SetSize (double width, double height)
		{
			if (width != -1)
				widthSet = true;
			if (height != -1)
				heightSet = true;
			base.SetSize (width, height);
		}

		internal override Rectangle BackendBounds
		{
			get
			{
				return shown ? base.BackendBounds : initialBounds;
			}
			set
			{
				if (shown)
					base.BackendBounds = value;
				else
					initialBounds = value;
			}
		}

		internal void AdjustSize ()
		{
			if (child == null)
				return;
			
			IWidgetSurface s = child.Surface;

			var size = shown ? Size : initialBounds.Size;

			var w = s.GetPreferredWidth ();

			if (!shown && !widthSet)
				size.Width = w.NaturalSize + padding.HorizontalSpacing;

			var h = s.GetPreferredHeightForWidth (size.Width - padding.HorizontalSpacing);

			if (!shown && !heightSet)
				size.Height = h.NaturalSize + padding.VerticalSpacing;

			if (w.MinSize + padding.HorizontalSpacing > size.Width)
				size.Width = w.MinSize + padding.HorizontalSpacing;
			if (h.MinSize + padding.VerticalSpacing > size.Height)
				size.Height = h.MinSize + padding.VerticalSpacing;

			if (!Application.EngineBackend.HandlesSizeNegotiation || !shown) {
	
				shown = true;
	
				if (size != Size)
					Size = size;
	
				Backend.SetMinSize (new Size (w.MinSize + padding.HorizontalSpacing, h.MinSize + padding.VerticalSpacing));
			}
		}
	}
}

