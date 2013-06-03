using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.Drawing;

namespace Xwt
{
	public class ReliefFrame : Canvas
	{
		// http://graphics.stanford.edu/courses/cs248-98-fall/Final/q1.html
		const double ArcToBezier = 0.55228475;
		double padding = 12.0;
		double? radius = null;
		Color bgcolor;

		public ReliefFrame ()
		{
			bgcolor = base.BackgroundColor;
		}

		public ReliefFrame (Widget content)
			: this ()
		{
			Content = content;
		}

		public new Color BackgroundColor
		{
			get { return bgcolor; }
			set
			{
				bgcolor = value;
				QueueDraw ();
			}
		}

		public new Widget Content
		{
			get { return Children.SingleOrDefault (); }
			set
			{
				if (Content != null)
					RemoveChild (Content);
				if (value != null)
					AddChild (value);
			}
		}

		public double? CornerRadius
		{
			get { return radius; }
			set
			{
				if (radius == value)
					return;

				radius = value;
				QueueDraw ();
			}
		}

		public double Padding
		{
			get { return padding; }
			set
			{
				if (padding == value)
					return;

				padding = value;
				OnPreferredSizeChanged ();
			}
		}

		protected override Size OnGetPreferredSize (SizeConstraint widthConstraint, SizeConstraint heightConstraint)
		{
			if (widthConstraint.IsConstrained)
				widthConstraint.AvailableSize -= 2 * padding;
			if (heightConstraint.IsConstrained)
				heightConstraint.AvailableSize -= 2 * padding;

			var s = base.OnGetPreferredSize (widthConstraint, heightConstraint);
			s.Width += 2 * padding;
			s.Height += 2 * padding;
			return s;
		}

		protected override void OnReallocate ()
		{
			base.OnReallocate ();
			UpdateChildBounds ();
		}

		void UpdateChildBounds ()
		{
			SetChildBounds (Content, new Rectangle (padding, padding, Math.Max (Size.Width - (2 * padding), 0), Math.Max (Size.Height - (2 * padding), 0)));
		}

		static void DrawBorder (Context ctx, double x, double y, double w, double h, double radius)
		{
			ctx.NewPath ();

			// test limits (without using multiplication)
			if (radius > w - radius)
				radius = w / 2;
			if (radius > h - radius)
				radius = h / 2;

			// approximate (quite close) the arc using a bezier curve
			double arc = ArcToBezier * radius;

			// top-left corner
			ctx.MoveTo (x + radius, y);

			// top edge
			ctx.LineTo (x + w - radius, y);

			// top-right corner
			ctx.CurveTo (x + w - radius + arc, y, x + w, y + arc, x + w, y + radius);

			// right edge
			ctx.LineTo (x + w, y + h - radius);

			// bottom-right corner
			ctx.CurveTo (x + w, y + h - radius + arc, x + w + arc - radius, y + h, x + w - radius, y + h);

			// bottom edge
			ctx.LineTo (x + radius, y + h);

			// bottom-left corner
			ctx.CurveTo (x + radius - arc, y + h, x, y + h - arc, x, y + h - radius);

			// left edge
			ctx.LineTo (x, y + radius);

			// top-left corner
			ctx.CurveTo (x, y + radius - arc, x + radius - arc, y, x + radius, y);
		}

		static Color GetColor (Color color, double percent)
		{
			return new Color (color.Red * percent, color.Green * percent, color.Blue * percent);
		}

		RadialGradient GetCornerGradient (double x, double y, double radius, double thickness)
		{
			var gradient = new RadialGradient (x, y, radius - thickness, x, y, radius + thickness);
			gradient.AddColorStop (0, BackgroundColor);
			gradient.AddColorStop (1, GetColor (BackgroundColor, 0.75));
			return gradient;
		}

		LinearGradient GetBottomEdgeGradient (double bottom, double thickness)
		{
			var gradient = new LinearGradient (0, bottom - thickness, 0, bottom + thickness);
			gradient.AddColorStop (0, BackgroundColor);
			gradient.AddColorStop (1, GetColor (BackgroundColor, 0.75));
			return gradient;
		}

		LinearGradient GetTopEdgeGradient (double top, double thickness)
		{
			var gradient = new LinearGradient (0, top - thickness, 0, top + thickness);
			gradient.AddColorStop (0, GetColor (BackgroundColor, 0.75));
			gradient.AddColorStop (1, BackgroundColor);
			return gradient;
		}

		LinearGradient GetRightEdgeGradient (double right, double thickness)
		{
			var gradient = new LinearGradient (right - thickness, 0, right + thickness, 0);
			gradient.AddColorStop (0, BackgroundColor);
			gradient.AddColorStop (1, GetColor (BackgroundColor, 0.75));
			return gradient;
		}

		LinearGradient GetLeftEdgeGradient (double left, double thickness)
		{
			var gradient = new LinearGradient (left - thickness, 0, left + thickness, 0);
			gradient.AddColorStop (0, GetColor (BackgroundColor, 0.75));
			gradient.AddColorStop (1, BackgroundColor);
			return gradient;
		}

		void DrawBorder (Context ctx, double x, double y, double w, double h, double radius, double thickness)
		{
			// test limits (without using multiplication)
			if (radius > w - radius)
				radius = w / 2;
			if (radius > h - radius)
				radius = h / 2;

			// approximate (quite close) the arc using a bezier curve
			double arc = ArcToBezier * radius;

			ctx.SetLineWidth (thickness);

			// top-left corner
			ctx.NewPath ();
			ctx.MoveTo (x, y + radius);
			ctx.CurveTo (x, y + radius - arc, x + radius - arc, y, x + radius, y);
			ctx.Pattern = GetCornerGradient (x + radius, y + radius, radius, thickness / 2);
			ctx.Stroke ();

			// top edge
			ctx.NewPath ();
			ctx.MoveTo (x + radius - 0.5, y);
			ctx.LineTo (x + w - radius + 0.5, y);
			ctx.Pattern = GetTopEdgeGradient (y, thickness / 2);
			ctx.Stroke ();

			// top-right corner
			ctx.NewPath ();
			ctx.MoveTo (x + w - radius, y);
			ctx.CurveTo (x + w - radius + arc, y, x + w, y + arc, x + w, y + radius);
			ctx.Pattern = GetCornerGradient (x + w - radius, y + radius, radius, thickness / 2);
			ctx.Stroke ();

			// right edge
			ctx.NewPath ();
			ctx.MoveTo (x + w, y + radius - 0.5);
			ctx.LineTo (x + w, y + h - radius + 0.5);
			ctx.Pattern = GetRightEdgeGradient (x + w, thickness / 2);
			ctx.Stroke ();

			// bottom-right corner
			ctx.NewPath ();
			ctx.MoveTo (x + w, y + h - radius);
			ctx.CurveTo (x + w, y + h - radius + arc, x + w + arc - radius, y + h, x + w - radius, y + h);
			ctx.Pattern = GetCornerGradient (x + w - radius, y + h - radius, radius, thickness / 2);
			ctx.Stroke ();

			// bottom edge
			ctx.NewPath ();
			ctx.MoveTo (x + w - radius + 0.5, y + h);
			ctx.LineTo (x + radius - 0.5, y + h);
			ctx.Pattern = GetBottomEdgeGradient (y + h, thickness / 2);
			ctx.Stroke ();

			// bottom-left corner
			ctx.NewPath ();
			ctx.MoveTo (x + radius, y + h);
			ctx.CurveTo (x + radius - arc, y + h, x, y + h - arc, x, y + h - radius);
			ctx.Pattern = GetCornerGradient (x + radius, y + h - radius, radius, thickness / 2);
			ctx.Stroke ();

			// left edge
			ctx.NewPath ();
			ctx.MoveTo (x, y + h - radius + 0.5);
			ctx.LineTo (x, y + radius - 0.5);
			ctx.Pattern = GetLeftEdgeGradient (x, thickness / 2);
			ctx.Stroke ();
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			double radius = CornerRadius ?? Math.Min (Size.Width, Size.Height) / 6.4;
			double h = Size.Height - 1.0;
			double w = Size.Width - 1.0;
			double x = 0.5;
			double y = 0.5;

			ctx.Save ();

			// Background
			DrawBorder (ctx, x, y, w, h, radius);
			ctx.SetColor (BackgroundColor);
			ctx.Fill ();

			// Border
			h = Size.Height - 6;
			w = Size.Width - 6;
			x = y = 3;
			DrawBorder (ctx, x, y, w, h, radius, 6);

			ctx.Restore ();
		}
	}
}
