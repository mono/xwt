// 
// PopupWindow.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Vsevolod Kukol <sevoku@microsoft.com>
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

using Xwt.Backends;

namespace Xwt
{
	[BackendType (typeof (IPopupWindowBackend))]
	public class PopupWindow : Window
	{
		public enum PopupType
		{
			Tooltip,
			Menu
		}

		readonly PopupType type;

		public PopupType Type { get { return type; } }

		public PopupWindow () : this (PopupType.Tooltip)
		{
		}

		public PopupWindow (PopupType type) : base (0)
		{
			this.type = type;
			switch (type) {
			case PopupType.Tooltip:
			case PopupType.Menu:
				ShowInTaskbar = false;
				InitialLocation = WindowLocation.CenterParent;
				Resizable = false;
				break;
			}
		}

		protected new class WindowBackendHost : Window.WindowBackendHost
		{
			new PopupWindow Parent { get { return (PopupWindow)base.Parent; } }

			protected override void OnBackendCreated ()
			{
				((IPopupWindowBackend)Backend).Initialize (this, Parent.Type);
				base.OnBackendCreated ();
			}
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WindowBackendHost ();
		}

		IPopupWindowBackend Backend {
			get { return (IPopupWindowBackend)BackendHost.Backend; }
		}

		bool shown;
		internal override void AdjustSize ()
		{
			if (Resizable || !shown) {
				base.AdjustSize ();
				shown = true;
				return;
			}
			Size mMinSize, mDecorationsSize;
			Backend.GetMetrics (out mMinSize, out mDecorationsSize);

			var ws = mDecorationsSize;
			if (Content != null) {
				IWidgetSurface s = Content.Surface;
				ws += s.GetPreferredSize (true);
			}
			ws.Width += Padding.HorizontalSpacing;
			ws.Height += Padding.VerticalSpacing;

			if (ws.Width<mMinSize.Width)
				ws.Width = mMinSize.Width;
			if (ws.Height<mMinSize.Height)
				ws.Height = mMinSize.Height;

			Backend.SetSize (ws.Width, ws.Height);
		}
	}
}

