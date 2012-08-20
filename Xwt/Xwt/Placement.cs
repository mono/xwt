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
	public class Placement : Widget, ISpacingListener
	{
		Widget child;

		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}

		IBoxBackend Backend {
			get { return (IBoxBackend) BackendHost.Backend; }
		}

		public Placement ()
		{
			Padding = new WidgetSpacing (this);
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
			get;
			private set;
		}

		protected override void OnReallocate ()
		{
			var size = Backend.Size;
			var childHeight = child.Surface.GetPreferredHeight ().NaturalSize;
			var childWidth = child.Surface.GetPreferredWidth ().NaturalSize;

			var x = XAlign * (size.Width - childWidth - Padding.HorizontalSpacing) + Padding.Left - Padding.Right;
			var y = YAlign * (size.Height - childHeight - Padding.VerticalSpacing) + Padding.Top - Padding.Bottom;

			Backend.SetAllocation (new[] { (IWidgetBackend)GetBackend (child) }, new[] { new Rectangle (x, y, childWidth, childHeight) });

			if (!Application.EngineBackend.HandlesSizeNegotiation)
				child.Surface.Reallocate ();
		}

		protected override WidgetSize OnGetPreferredHeight ()
		{
			WidgetSize s = new WidgetSize ();
			if (child != null) {
				s += child.Surface.GetPreferredHeight ();
				s += Margin.VerticalSpacing;
			}
			return s;
		}

		protected override WidgetSize OnGetPreferredWidth ()
		{
			WidgetSize s = new WidgetSize ();
			if (child != null) {
				s += child.Surface.GetPreferredWidth ();
				s += Margin.HorizontalSpacing;
			}
			return s;
		}

		protected override WidgetSize OnGetPreferredHeightForWidth (double width)
		{
			WidgetSize s = new WidgetSize ();
			if (child != null) {
				s += child.Surface.GetPreferredHeightForWidth (width);
				s += Margin.VerticalSpacing;
			}
			return s;
		}

		protected override WidgetSize OnGetPreferredWidthForHeight (double height)
		{
			WidgetSize s = new WidgetSize ();
			if (child != null) {
				s += child.Surface.GetPreferredWidthForHeight (height);
				s += Margin.HorizontalSpacing;
			}
			return s;
		}

		public void OnSpacingChanged (WidgetSpacing spacing)
		{
			OnPreferredSizeChanged ();
		}
	}
}

