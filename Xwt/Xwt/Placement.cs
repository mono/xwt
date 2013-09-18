//
// Placement.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
using System;
using Xwt.Backends;

namespace Xwt
{
	[BackendType (typeof(IBoxBackend))]
	public class Placement : Widget
	{
		Widget child;
		WidgetSpacing padding;

		protected new class WidgetBackendHost : Widget.WidgetBackendHost<Placement,IBoxBackend>
		{
		}

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IBoxBackend Backend {
			get { return (IBoxBackend) BackendHost.Backend; }
		}

		public Placement ()
		{
		}

		public Widget Child {
			get {
				return child;
			}
			set {
				if (child != null) {
					UnregisterChild (child);
					Backend.Remove ((IWidgetBackend)GetBackend (child));
				}
				child = value;
				if (child != null) {
					RegisterChild (child);
					Backend.Add ((IWidgetBackend)GetBackend (child));
				}
				OnPreferredSizeChanged ();
			}
		}

		public double XAlign {
			get;
			set;
		}

		public double YAlign {
			get;
			set;
		}

		public WidgetSpacing Padding {
			get { return padding; }
			set {
				padding = value;
				UpdatePadding ();
			}
		}

		public double PaddingLeft {
			get { return padding.Left; }
			set {
				padding.Left = value;
				UpdatePadding (); 
			}
		}

		public double PaddingRight {
			get { return padding.Right; }
			set {
				padding.Right = value;
				UpdatePadding (); 
			}
		}

		public double PaddingTop {
			get { return padding.Top; }
			set {
				padding.Top = value;
				UpdatePadding (); 
			}
		}

		public double PaddingBottom {
			get { return padding.Bottom; }
			set {
				padding.Bottom = value;
				UpdatePadding (); 
			}
		}

		void UpdatePadding ()
		{
			OnPreferredSizeChanged();
		}

		protected override void OnReallocate ()
		{
			if (child != null) {
				var size = Backend.Size;
				var availableWidth = size.Width - child.Margin.HorizontalSpacing - Padding.HorizontalSpacing;
				var availableHeight = size.Height - child.Margin.VerticalSpacing - Padding.VerticalSpacing;
				var childSize = child.Surface.GetPreferredSize (availableWidth, availableHeight);
				var x = child.Margin.Left + Padding.Left + XAlign * (availableWidth - childSize.Width);
				var y = child.Margin.Top + Padding.Top + YAlign * (availableHeight - childSize.Height);

				Backend.SetAllocation (new[] { (IWidgetBackend)GetBackend (child) }, new[] { new Rectangle (x, y, childSize.Width, childSize.Height).Round () });
			}
		}

		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (child != null) {
				var s = new Size (child.Margin.HorizontalSpacing + Padding.HorizontalSpacing, child.Margin.VerticalSpacing + Padding.VerticalSpacing);

				if (widthConstraint.IsConstrained) {
					widthConstraint = widthConstraint.AvailableSize - s.Width;
					if (widthConstraint.AvailableSize <= 0)
						return s;
				}
				if (heightConstraint.IsConstrained) {
					heightConstraint = heightConstraint.AvailableSize - child.Margin.VerticalSpacing - Padding.VerticalSpacing;
					if (heightConstraint.AvailableSize <= 0)
						return s;
				}
				return s + child.Surface.GetPreferredSize (widthConstraint, heightConstraint);
			}
			else
				return new Size (Padding.HorizontalSpacing, Padding.VerticalSpacing);
		}
	}
}

