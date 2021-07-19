// 
// PanedBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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
using AppKit;
using CoreGraphics;
using Foundation;
using Xwt.Backends;

namespace Xwt.Mac
{
	public class PanedBackend : ViewBackend<NSSplitView, IPanedEventSink>, IPanedBackend
	{
		SplitViewDelegate viewDelegate;
		NSView view1;
		NSView view2;

		class SplitViewDelegate : NSSplitViewDelegate
		{
			public PanedBackend PanedBackend;

			public override void DidResizeSubviews(NSNotification notification)
			{
				PanedBackend.DidResizeSubviews();
			}
		}

		public PanedBackend()
		{
		}

		#region IPanedBackend implementation
		public void Initialize(Orientation dir)
		{
			ViewObject = new CustomSplitView();
			if (dir == Orientation.Horizontal)
				Widget.IsVertical = true;
			viewDelegate = new SplitViewDelegate() { PanedBackend = this };
			Widget.Delegate = viewDelegate;
		}

		public void SetPanel(int panel, IWidgetBackend widget, bool resize, bool shrink)
		{
			ViewBackend view = (ViewBackend)widget;
			var w = GetWidgetWithPlacement(view);
			RemovePanel(panel);
			Widget.AddSubview(w);
			Widget.AdjustSubviews();
			if (panel == 1)
				view1 = w;
			else
				view2 = w;
			view.NotifyPreferredSizeChanged();
		}

		void DidResizeSubviews()
		{
			EventSink.OnPositionChanged();
		}

		public void UpdatePanel(int panel, bool resize, bool shrink)
		{
		}

		public void RemovePanel(int panel)
		{
			if (panel == 1)
			{
				if (view1 != null)
				{
					view1.RemoveFromSuperview();
					RemoveChildPlacement(view1);
					view1 = null;
				}
			}
			else
			{
				if (view2 != null)
				{
					view2.RemoveFromSuperview();
					RemoveChildPlacement(view2);
					view2 = null;
				}
			}
		}

		public override void ReplaceChild(NSView oldChild, NSView newChild)
		{
			if (view1 != null)
				view1.RemoveFromSuperview();
			if (view2 != null)
				view2.RemoveFromSuperview();

			if (oldChild == view1)
				view1 = newChild;
			else
				view2 = newChild;

			if (view1 != null)
				Widget.AddSubview(view1);
			if (view2 != null)
				Widget.AddSubview(view2);
		}

		private double position;
		public double Position
		{
			get
			{
				return position;
			}
			set
			{

				position = value;
				this.DidResizeSubviews();
			}
		}
		#endregion
	}

	class CustomSplitView : NSSplitView, IViewObject
	{

		readonly double initDividerPosition = 220;
		public event EventHandler<CGRect> OnFrameChanged;
		bool needsDividerSet;

		public double Position { get; set; }

		public CustomSplitView(double initDividerPosition = 0)
		{
			Position = initDividerPosition;
		}

		public NSView View
		{
			get
			{
				return this;
			}
		}

		private ViewBackend backend;
		public ViewBackend Backend
		{
			get
			{
				return backend;
			}
			set
			{
				if (value != null)
				{
					foreach (var view in ArrangedSubviews)
					{
						RemoveArrangedSubview(view);
					}

					backend = value;

					SetDividerPosition();
				}
			}
		}

		public override CGRect Frame
		{
			get
			{
				return base.Frame;
			}

			set
			{
				base.Frame = value;
				if (Frame.Width == 0)
				{
					needsDividerSet = true;
				}
				else
				{
					SetDividerPosition();
				}
			}
		}

		void SetDividerPosition()
		{
			SetPositionOfDivider((nfloat)initDividerPosition, 0);
			needsDividerSet = false;
		}
	}
}

