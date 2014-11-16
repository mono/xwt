//
// ColorSelectorBackend.cs
//
// Author:
//       Vsevolod Kukol <v.kukol@rubologic.de>
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

namespace Xwt.GtkBackend
{
	public class ColorSelectorBackend: WidgetBackend, IColorSelectorBackend
	{
		public ColorSelectorBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new Gtk.ColorSelection ();
			base.Widget.Show ();
		}

		protected new Gtk.ColorSelection Widget {
			get { return (Gtk.ColorSelection)base.Widget; }
			set { base.Widget = value; }
		}

		protected new IColorSelectorEventSink EventSink {
			get { return (IColorSelectorEventSink)base.EventSink; }
		}

		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ColorSelectorEvent) {
				switch ((ColorSelectorEvent)eventId) {
					case ColorSelectorEvent.ColorChanged: Widget.ColorChanged += HandleColorChanged;; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ColorSelectorEvent) {
				switch ((ColorSelectorEvent)eventId) {
					case ColorSelectorEvent.ColorChanged: Widget.ColorChanged -= HandleColorChanged; break;
				}
			}
		}

		void HandleColorChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnColorChanged ();
			});
		}

		public Xwt.Drawing.Color Color {
			get {
				var xwtColor = Widget.CurrentColor.ToXwtValue ();
				return xwtColor.WithAlpha ((double)Widget.CurrentAlpha / (double)ushort.MaxValue);
			}
			set {
				Widget.CurrentColor = value.ToGtkValue ();
				Widget.CurrentAlpha = (ushort)((int)(value.Alpha * 255) << 8 | (int)(value.Alpha * 255));
			}
		}

		public bool SupportsAlpha {
			get {
				return Widget.HasOpacityControl;
			}
			set {
				Widget.HasOpacityControl = value;
			}
		}
	}
}

