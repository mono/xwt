//
// CustomWidgetBackend.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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

using System.Windows.Controls;
using Xwt.Backends;
using SW = System.Windows;

namespace Xwt.WPFBackend
{
	public class CustomWidgetBackend
		: WidgetBackend, ICustomWidgetBackend
	{
		Widget child;

		public CustomWidgetBackend()
		{
			Widget = new ExUserControl ();
		}

		public void SetContent (IWidgetBackend widget)
		{
			if (widget != null) {
				SetChildPlacement (widget);
				child = (Widget)((WidgetBackend)widget).Frontend;
				UserControl.Content = widget.NativeWidget;
			}
			else {
				child = null;
				UserControl.Content = null;
			}
		}

		protected UserControl UserControl
		{
			get { return (UserControl) Widget; }
		}

		// The size of the container is the size of the child,
		// so we redirect size calculations to the child.

		public override Size GetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			return child != null ? child.Surface.GetPreferredSize (widthConstraint, heightConstraint) : Size.Zero;
		}
	}

	class ExUserControl : UserControl, IWpfWidget
	{
		public WidgetBackend Backend
		{
			get;
			set;
		}

		protected override SW.Size ArrangeOverride (SW.Size arrangeBounds)
		{
			var s = base.ArrangeOverride (arrangeBounds);
			Backend.Frontend.Surface.Reallocate ();
			return s;
		}

		protected override SW.Size MeasureOverride (SW.Size constraint)
		{
			var s = base.MeasureOverride (constraint);
			return Backend.MeasureOverride (constraint, s);
		}
	}
}