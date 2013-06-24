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
		Color bgColor, borderColor;

		public ReliefFrame ()
		{
			bgColor = base.BackgroundColor;
		}

		public ReliefFrame (Widget content)
			: this ()
		{
			Content = content;
		}

		public new Color BackgroundColor
		{
			get { return bgColor; }
			set
			{
				bgColor = value;
				QueueDraw ();
			}
		}

		public Color BorderColor {
			get { return borderColor; }
			set {
				borderColor = value;
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
			var s = new Size (2 * padding, 2 * padding);
			if (Content != null)
				s += Content.Surface.GetPreferredSize (widthConstraint - 2 * padding, heightConstraint - 2 * padding, true);
			return s;
		}

		protected override void OnReallocate ()
		{
			base.OnReallocate ();
			UpdateChildBounds ();
		}

		void UpdateChildBounds ()
		{
			var r = Content.Surface.GetPlacementInRect (new Rectangle (padding, padding, Math.Max (Size.Width - (2 * padding), 0), Math.Max (Size.Height - (2 * padding), 0)));
			SetChildBounds (Content, r);
		}

		static void CreateRoundedBoxPath (Context ctx, double x, double y, double w, double h, double radius)
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

		void DrawFilledBox (Context ctx, double x, double y, double w, double h)
		{
			ctx.NewPath ();
			ctx.MoveTo (x,     y);
			ctx.LineTo (x + w, y);
			ctx.LineTo (x + w, y + h);
			ctx.LineTo (x,     y + h);
			ctx.LineTo (x,     y);
			ctx.Fill ();
		}

		void DrawFilledQuarterCircle (Context ctx, double x, double y, double thickness, double radius,
		                              double xDirection, double yDirection, Color outerColor, Color innerColor)
		{
			var w = radius * xDirection;
			var h = radius * yDirection;

			// approximate (quite close) the arc using a bezier curve
			double arc = ArcToBezier * radius;

			ctx.NewPath ();
			ctx.MoveTo (x,     y);
			ctx.LineTo (x + w, y);
			ctx.CurveTo (x + w, y + (arc * yDirection), x + (arc * xDirection), y + h, x, y + h);
			ctx.LineTo (x,     y);

			using (var pattern = new RadialGradient (x, y, radius - thickness, x, y, radius - 1)) {
				pattern.AddColorStop (0, innerColor);
				pattern.AddColorStop (1, outerColor);

				ctx.Pattern = pattern;
				ctx.Fill ();
			}
		}

		void DrawGradientBorder (Context ctx, double x, double y, double w, double h, double thickness, double radius,
		                         Color outerColor, Color innerColor)
		{
			// test limits (without using multiplication)
			if (radius > w - radius)
				radius = w / 2;
			if (radius > h - radius)
				radius = h / 2;

			var r = x + w;
			var b = y + h;
			var r2 = radius + radius;

			// top-left corner
			DrawFilledQuarterCircle (ctx, x + radius, y + radius, thickness, radius, -1, -1, outerColor, innerColor);

			// top edge
			using (var topGrad = new LinearGradient (0, y + 1, 0, y + thickness)) {
				topGrad.AddColorStop (0, outerColor);
				topGrad.AddColorStop (1, innerColor);
				ctx.Pattern = topGrad;
				DrawFilledBox (ctx, x + radius, y, w - r2, radius);
			}

			// top-right corner
			DrawFilledQuarterCircle (ctx, r - radius, y + radius, thickness, radius, 1, -1, outerColor, innerColor);

			// right edge
			using (var rightGrad = new LinearGradient (r - thickness, 0, r - 1, 0)) {
				rightGrad.AddColorStop (0, innerColor);
				rightGrad.AddColorStop (1, outerColor);
				ctx.Pattern = rightGrad;
				DrawFilledBox (ctx, r, y + radius, -radius, h - r2);
			}

			// bottom-right corner
			DrawFilledQuarterCircle (ctx, r - radius, b - radius, thickness, radius, 1, 1, outerColor, innerColor);

			// bottom edge
			using (var bottomGrad = new LinearGradient (0, b - thickness, 0, b - 1)) {
				bottomGrad.AddColorStop (0, innerColor);
				bottomGrad.AddColorStop (1, outerColor);
				ctx.Pattern = bottomGrad;
				DrawFilledBox (ctx, x + radius, b, w - r2, -radius);
			}

			// bottom-left corner
			DrawFilledQuarterCircle (ctx, x + radius, b - radius, thickness, radius, -1, 1, outerColor, innerColor);

			// left edge
			using (var leftGrad = new LinearGradient (1, 0, thickness, 0)) {
				leftGrad.AddColorStop (0, outerColor);
				leftGrad.AddColorStop (1, innerColor);
				ctx.Pattern = leftGrad;
				DrawFilledBox (ctx, x, y + radius, radius, h - r2);
			}
		}

		protected override void OnDraw (Context ctx, Rectangle dirtyRect)
		{
			double radius = CornerRadius ?? Math.Min (Size.Width, Size.Height) / 6.4;
			double thickness = Math.Min (4, radius);

			ctx.Save ();

			// Background
			ctx.SetColor (bgColor);
			CreateRoundedBoxPath (ctx, 0, 0, Size.Width, Size.Height, radius);
			ctx.Fill ();

			// Border
			DrawGradientBorder (ctx, 0, 0, Size.Width, Size.Height, thickness, radius, borderColor, bgColor);

			ctx.Restore ();
		}
	}
}
