﻿//
// PopupWindowBackend.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corporation
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

namespace Xwt.GtkBackend
{
	public class PopupWindowBackend : WindowBackend, IPopupWindowBackend
	{
		PopupWindow.PopupType windowType;

		public override void Initialize ()
		{
			Window = new GtkPopoverWindow (windowType == PopupWindow.PopupType.Tooltip ? Gtk.WindowType.Popup : Gtk.WindowType.Toplevel);

			switch (windowType) {
			case PopupWindow.PopupType.Tooltip:
				Window.TypeHint = Gdk.WindowTypeHint.Tooltip;
				Window.Decorated = false;
				break;
			case PopupWindow.PopupType.Menu:
				Window.TypeHint = Gdk.WindowTypeHint.PopupMenu;
				Window.Decorated = false;
				break;
			}

			Window.SkipPagerHint = true;
			Window.SkipTaskbarHint = true;
			Window.Add (CreateMainLayout ());
		}

		void IPopupWindowBackend.Initialize (IWindowFrameEventSink sink, PopupWindow.PopupType type)
		{
			windowType = type;
			((IWindowFrameBackend)this).Initialize (sink);
		}
	}
}
