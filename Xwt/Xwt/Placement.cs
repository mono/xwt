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
			var size = Backend.Size;
			double childWidth, childHeight;

			if (child != null) {
				var s = child.Surface.GetPreferredSize (SizeContraint.RequireSize (size.Width), SizeContraint.RequireSize (size.Height));
				childWidth = s.Width;
				childHeight = s.Height;

				var x = XAlign * (size.Width - childWidth - Padding.HorizontalSpacing) + Padding.Left - Padding.Right;
				var y = YAlign * (size.Height - childHeight - Padding.VerticalSpacing) + Padding.Top - Padding.Bottom;

				Backend.SetAllocation (new[] { (IWidgetBackend)GetBackend (child) }, new[] { new Rectangle (x, y, childWidth, childHeight).Round () });

				if (!BackendHost.EngineBackend.HandlesSizeNegotiation)
					child.Surface.Reallocate ();
			}
		}

		protected override Size OnGetPreferredSize (SizeContraint widthConstraint, SizeContraint heightConstraint)
		{
			if (child != null)
				return child.Surface.GetPreferredSize (widthConstraint, heightConstraint);
			else
				return Size.Zero;
		}
	}
}

