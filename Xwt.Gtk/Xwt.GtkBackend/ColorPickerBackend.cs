//
// ColorPickerBackend.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
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
using Cairo;

namespace Xwt.GtkBackend
{
	public class ColorPickerBackend: WidgetBackend, IColorPickerBackend
	{
		public ColorPickerBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new Gtk.ColorButton ();
			base.Widget.Show ();
		}

		protected new Gtk.ColorButton Widget {
			get { return (Gtk.ColorButton)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IColorPickerEventSink EventSink {
			get { return (IColorPickerEventSink)base.EventSink; }
		}

		public Xwt.Drawing.Color Color {
			get {
				var xwtColor = Widget.Color.ToXwtValue ();
				return xwtColor.WithAlpha ((double)Widget.Alpha / (double)ushort.MaxValue);
			}
			set {
				Widget.Color = value.ToGtkValue ();
				Widget.Alpha = (ushort)((int)(value.Alpha * 255) << 8 | (int)(value.Alpha * 255));
			}
		}

		public bool SupportsAlpha {
			get {
				return Widget.UseAlpha;
			}
			set {
				Widget.UseAlpha = value;
			}
		}

		public string Title {
			get {
				return Widget.Title;
			}
			set {
				Widget.Title = value;
			}
		}

		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
				case ButtonStyle.Normal:
					Widget.Relief = Gtk.ReliefStyle.Normal;
					break;
				case ButtonStyle.Flat:
					Widget.Relief = Gtk.ReliefStyle.None;
					break;
				case ButtonStyle.Borderless:
					Widget.Relief = Gtk.ReliefStyle.None;
					break;
			}
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ColorPickerEvent) {
				switch ((ColorPickerEvent)eventId) {
					case ColorPickerEvent.ColorChanged: Widget.ColorSet += HandleColorSet; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ColorPickerEvent) {
				switch ((ColorPickerEvent)eventId) {
					case ColorPickerEvent.ColorChanged: Widget.ColorSet -= HandleColorSet; break;
				}
			}
		}

		void HandleColorSet (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (EventSink.OnColorChanged);
		}
	}
}

