//
// PopoverBackend.cs
//
// Author:
//       Alan McGovern <alan@xamarin.com>
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

using System;
using Xwt.Backends;
using System.Windows.Media;

namespace Xwt.WPFBackend
{
	public class PopoverBackend : Backend, IPopoverBackend
	{
		public Xwt.Popover.Position ActualPosition {
			get; set;
		}

		System.Windows.Controls.Border Border {
			get; set;
		}

		IPopoverEventSink EventSink {
			get; set;
		}

		new Popover Frontend {
			get { return (Popover)base.frontend; }
		}

		System.Windows.Controls.Primitives.Popup NativeWidget {
			get; set;
		}

		public PopoverBackend ()
		{
			Border = new System.Windows.Controls.Border {
				BorderBrush = Brushes.Black,
				CornerRadius = new System.Windows.CornerRadius (15),
				Padding = new System.Windows.Thickness (15),
				BorderThickness = new System.Windows.Thickness (1)
			};

			NativeWidget = new System.Windows.Controls.Primitives.Popup {
				AllowsTransparency = true,
				Child = Border,
				Placement = System.Windows.Controls.Primitives.PlacementMode.Custom,
				StaysOpen = false,
				Margin = new System.Windows.Thickness (10),
			};

			NativeWidget.CustomPopupPlacementCallback = (popupSize, targetSize, offset) => {
				var location = new System.Windows.Point (targetSize.Width / 2 - popupSize.Width / 2, 0);
				if (ActualPosition == Popover.Position.Top)
					location.Y = targetSize.Height;
				else
					location.Y = -popupSize.Height;

				return new[] {
					new System.Windows.Controls.Primitives.CustomPopupPlacement (location, System.Windows.Controls.Primitives.PopupPrimaryAxis.Horizontal)
				};
			};
		}

		public void Initialize (IPopoverEventSink sink)
		{
			EventSink = sink;
		}

		public override void EnableEvent (object eventId)
		{
			if (eventId is PopoverEvent)
				if ((PopoverEvent)eventId == PopoverEvent.Closed)
					NativeWidget.Closed +=new EventHandler(NativeWidget_Closed);
		}

		public override void DisableEvent (object eventId)
		{
			if (eventId is PopoverEvent)
				if ((PopoverEvent) eventId == PopoverEvent.Closed)
					NativeWidget.Closed += new EventHandler (NativeWidget_Closed);
		}

		public void Show (Xwt.Popover.Position orientation, Xwt.Widget reference, Xwt.Rectangle positionRect, Widget child)
		{
			ActualPosition = orientation;
			Border.Child = (System.Windows.FrameworkElement)Context.Toolkit.GetNativeWidget (child);
			NativeWidget.PlacementTarget = (System.Windows.FrameworkElement)Context.Toolkit.GetNativeWidget (reference);
			NativeWidget.IsOpen = true;
		}

		void NativeWidget_Closed (object sender, EventArgs e)
		{
			EventSink.OnClosed ();
		}

		public void Hide ()
		{
			NativeWidget.IsOpen = false;
		}

		public void Dispose ()
		{
			if (NativeWidget != null)
				NativeWidget.Closed -= NativeWidget_Closed;
		}
	}
}