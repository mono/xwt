using System;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public static class Conversion
	{
		public static Gtk.IconSize ToGtkValue (Xwt.IconSize size)
		{
			switch (size) {
			case IconSize.Small:
				return Gtk.IconSize.Menu;
			case IconSize.Medium:
				return Gtk.IconSize.Button;
			case IconSize.Large:
				return Gtk.IconSize.Dialog;
			}
			return Gtk.IconSize.Dialog;
		}

		public static Gdk.Color ToGtkValue (this Xwt.Drawing.Color color)
		{
			return new Gdk.Color ((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));
		}

		public static Color ToXwtValue (this Gdk.Color color)
		{
			return new Color ((double)color.Red / (double)ushort.MaxValue, (double)color.Green / (double)ushort.MaxValue, (double)color.Blue / (double)ushort.MaxValue);
		}

		public static Pango.EllipsizeMode ToGtkValue (this EllipsizeMode value)
		{
			switch (value) {
			case Xwt.EllipsizeMode.None: return Pango.EllipsizeMode.None;
			case Xwt.EllipsizeMode.Start: return Pango.EllipsizeMode.Start;
			case Xwt.EllipsizeMode.Middle: return Pango.EllipsizeMode.Middle;
			case Xwt.EllipsizeMode.End: return Pango.EllipsizeMode.End;
			}
			throw new NotSupportedException ();
		}

		public static EllipsizeMode ToXwtValue (this Pango.EllipsizeMode value)
		{
			switch (value) {
			case Pango.EllipsizeMode.None: return Xwt.EllipsizeMode.None;
			case Pango.EllipsizeMode.Start: return Xwt.EllipsizeMode.Start;
			case Pango.EllipsizeMode.Middle: return Xwt.EllipsizeMode.Middle;
			case Pango.EllipsizeMode.End: return Xwt.EllipsizeMode.End;
			}
			throw new NotSupportedException ();
		}

		public static ScrollPolicy ToXwtValue (this Gtk.PolicyType p)
		{
			switch (p) {
			case Gtk.PolicyType.Always:
				return ScrollPolicy.Always;
			case Gtk.PolicyType.Automatic:
				return ScrollPolicy.Automatic;
			case Gtk.PolicyType.Never:
				return ScrollPolicy.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}

		public static Gtk.PolicyType ToGtkValue (this ScrollPolicy p)
		{
			switch (p) {
			case ScrollPolicy.Always:
				return Gtk.PolicyType.Always;
			case ScrollPolicy.Automatic:
				return Gtk.PolicyType.Automatic;
			case ScrollPolicy.Never:
				return Gtk.PolicyType.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}

		public static ScrollDirection ToXwtValue(this Gdk.ScrollDirection d)
		{
			switch(d) {
			case Gdk.ScrollDirection.Up:
				return Xwt.ScrollDirection.Up;
			case Gdk.ScrollDirection.Down:
				return Xwt.ScrollDirection.Down;
			case Gdk.ScrollDirection.Left:
				return Xwt.ScrollDirection.Left;
			case Gdk.ScrollDirection.Right:
				return Xwt.ScrollDirection.Right;
			}
			throw new InvalidOperationException("Invalid mouse scroll direction value: " + d);
		}

		public static Gdk.ScrollDirection ToGtkValue(this ScrollDirection d)
		{
			switch (d) {
			case ScrollDirection.Up:
				return Gdk.ScrollDirection.Up;
			case ScrollDirection.Down:
				return Gdk.ScrollDirection.Down;
			case ScrollDirection.Left:
				return Gdk.ScrollDirection.Left;
			case ScrollDirection.Right:
				return Gdk.ScrollDirection.Right;
			}
			throw new InvalidOperationException("Invalid mouse scroll direction value: " + d);
		}

		public static ModifierKeys ToXwtValue (this Gdk.ModifierType s)
		{
			ModifierKeys m = ModifierKeys.None;
			if ((s & Gdk.ModifierType.ShiftMask) != 0)
				m |= ModifierKeys.Shift;
			if ((s & Gdk.ModifierType.ControlMask) != 0)
				m |= ModifierKeys.Control;
			if ((s & Gdk.ModifierType.Mod1Mask) != 0)
				m |= ModifierKeys.Alt;
			if ((s & Gdk.ModifierType.Mod2Mask) != 0)
				m |= ModifierKeys.Command;
			return m;
		}

		public static Gtk.TreeViewGridLines ToGtkValue (this GridLines value)
		{
			switch (value)
			{
				case GridLines.Both:
					return Gtk.TreeViewGridLines.Both;
				case GridLines.Horizontal:
					return Gtk.TreeViewGridLines.Horizontal;
				case GridLines.Vertical:
					return Gtk.TreeViewGridLines.Vertical;
				case GridLines.None:
					return Gtk.TreeViewGridLines.None;
			}
			throw new InvalidOperationException("Invalid GridLines value: " + value);
		}

		public static GridLines ToXwtValue (this Gtk.TreeViewGridLines value)
		{
			switch (value)
			{
				case Gtk.TreeViewGridLines.Both:
					return GridLines.Both;
				case Gtk.TreeViewGridLines.Horizontal:
					return GridLines.Horizontal;
				case Gtk.TreeViewGridLines.Vertical:
					return GridLines.Vertical;
				case Gtk.TreeViewGridLines.None:
					return GridLines.None;
			}
			throw new InvalidOperationException("Invalid TreeViewGridLines value: " + value);
		}

		public static Gtk.Justification ToGtkValue (this Alignment value)
		{
			if (value == Alignment.Center)
				return Gtk.Justification.Center;
			if (value == Alignment.Start)
				return Gtk.Justification.Left;
			// if (value == Alignment.End)
			return Gtk.Justification.Right;
		}

		public static Alignment ToXwtValue (this Gtk.Justification value)
		{
			if (value == Gtk.Justification.Center)
				return Alignment.Center;
			if (value == Gtk.Justification.Left)
				return Alignment.Start;
			// if (value == Gtk.Justification.Right)
			return Alignment.End;
		}
	}
}

