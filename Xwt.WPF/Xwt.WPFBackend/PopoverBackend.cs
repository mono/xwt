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
using System.Windows;

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

		public Xwt.Drawing.Color BackgroundColor {
			get {
				return Border.Background.ToXwtColor ();
			}
			set {
				Border.Background = new SolidColorBrush (value.ToWpfColor ());
			}
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
				Padding = new Thickness (15, 10, 15, 15),
				BorderThickness = new Thickness (1),
				Margin = new System.Windows.Thickness (10),
				Effect = new System.Windows.Media.Effects.DropShadowEffect () {
					Color = Colors.Black,
					Direction = 270,
					BlurRadius = 15,
					Opacity = .15,
					ShadowDepth = 1,
				}
			};
			Border.SetResourceReference (System.Windows.Controls.Border.BorderBrushProperty,
			                             SystemColors.ActiveBorderBrushKey);
			BackgroundColor = Xwt.Drawing.Color.FromBytes (230, 230, 230, 230);

			NativeWidget = new System.Windows.Controls.Primitives.Popup {
				AllowsTransparency = true,
				Child = Border,
				Placement = System.Windows.Controls.Primitives.PlacementMode.Custom,
				StaysOpen = false,
			};
			NativeWidget.Closed += NativeWidget_Closed;
		}

		public void Initialize (IPopoverEventSink sink)
		{
			EventSink = sink;
		}

		public void Show (Xwt.Popover.Position orientation, Xwt.Widget reference, Xwt.Rectangle positionRect, Widget child)
		{
			ActualPosition = orientation;
			Border.Child = (System.Windows.FrameworkElement)Context.Toolkit.GetNativeWidget (child);
			NativeWidget.CustomPopupPlacementCallback = (popupSize, targetSize, offset) => {
				System.Windows.Point location;
				if (ActualPosition == Popover.Position.Top)
					location = new System.Windows.Point (positionRect.Left - popupSize.Width / 2,
					                                     positionRect.Height > 0 ? positionRect.Bottom : targetSize.Height);
				else
					location = new System.Windows.Point (positionRect.Left - popupSize.Width / 2,
					                                     positionRect.Top - popupSize.Height);

				return new[] {
					new System.Windows.Controls.Primitives.CustomPopupPlacement (location, System.Windows.Controls.Primitives.PopupPrimaryAxis.Horizontal)
				};
			};
			NativeWidget.PlacementTarget = (System.Windows.FrameworkElement)Context.Toolkit.GetNativeWidget (reference);
			NativeWidget.IsOpen = true;
		}

		void NativeWidget_Closed (object sender, EventArgs e)
		{
			Border.Child = null;
			EventSink.OnClosed ();
		}

		public void Hide ()
		{
			NativeWidget.IsOpen = false;
			Border.Child = null;
		}

		public void Dispose ()
		{
			if (NativeWidget != null)
				NativeWidget.Closed -= NativeWidget_Closed;
		}
	}
}