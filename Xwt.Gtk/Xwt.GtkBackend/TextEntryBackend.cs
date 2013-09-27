// 
// TextEntryBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Drawing;


namespace Xwt.GtkBackend
{
	public class TextEntryBackend: WidgetBackend, ITextEntryBackend
	{
		string placeHolderText;
		
		public override void Initialize ()
		{
			Widget = new Gtk.Entry ();
			Widget.Show ();
		}
		
		protected new Gtk.Entry Widget {
			get { return (Gtk.Entry)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ITextEntryEventSink EventSink {
			get { return (ITextEntryEventSink)base.EventSink; }
		}

		public string Text {
			get { return Widget.Text; }
			set { Widget.Text = value ?? ""; } // null value causes GTK error
		}

		public Alignment TextAlignment {
			get {
				if (Widget.Xalign == 0)
					return Alignment.Start;
				else if (Widget.Xalign == 1)
					return Alignment.End;
				else
					return Alignment.Center;
			}
			set {
				switch (value) {
				case Alignment.Start: Widget.Xalign = 0; break;
				case Alignment.End: Widget.Xalign = 1; break;
				case Alignment.Center: Widget.Xalign = 0.5f; break;
				}
			}
		}
		
		public string PlaceholderText {
			get { return placeHolderText; }
			set {
				if (placeHolderText != value) {
					if (placeHolderText == null)
						Widget.ExposeEvent += HandleWidgetExposeEvent;
					else if (value == null)
						Widget.ExposeEvent -= HandleWidgetExposeEvent;
				}
				placeHolderText = value;
			}
		}
		
		public override Color BackgroundColor {
			get {
				return base.BackgroundColor;
			}
			set {
				base.BackgroundColor = value;
				Widget.ModifyBase (Gtk.StateType.Normal, value.ToGdkColor ());
			}
		}

		Pango.Layout layout;
		
		void HandleWidgetExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			RenderPlaceholderText (Widget, args, placeHolderText, ref layout);
		}

		internal static void RenderPlaceholderText (Gtk.Entry entry, Gtk.ExposeEventArgs args, string placeHolderText, ref Pango.Layout layout)
		{
			// The Entry's GdkWindow is the top level window onto which
			// the frame is drawn; the actual text entry is drawn into a
			// separate window, so we can ensure that for themes that don't
			// respect HasFrame, we never ever allow the base frame drawing
			// to happen
			if (args.Event.Window == entry.GdkWindow)
				return;
			
			if (entry.Text.Length > 0)
				return;
			
			if (layout == null) {
				layout = new Pango.Layout (entry.PangoContext);
				layout.FontDescription = entry.PangoContext.FontDescription.Copy ();
			}
			
			int wh, ww;
			args.Event.Window.GetSize (out ww, out wh);
			
			int width, height;
			layout.SetText (placeHolderText);
			layout.GetPixelSize (out width, out height);
			using (var gc = new Gdk.GC (args.Event.Window)) {
				gc.Copy (entry.Style.TextGC (Gtk.StateType.Normal));
				Color color_a = entry.Style.Base (Gtk.StateType.Normal).ToXwtColor ();
				Color color_b = entry.Style.Text (Gtk.StateType.Normal).ToXwtColor ();
				gc.RgbFgColor = color_b.BlendWith (color_a, 0.5).ToGdkColor ();

				args.Event.Window.DrawLayout (gc, 2, (wh - height) / 2 + 1, layout);
			}
		}
		
		public bool ReadOnly {
			get {
				return !Widget.IsEditable;
			}
			set {
				Widget.IsEditable = !value;
			}
		}
		
		public bool ShowFrame {
			get {
				return Widget.HasFrame;
			}
			set {
				Widget.HasFrame = value;
			}
		}

		public bool MultiLine {
			get; set;
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed: Widget.Changed += HandleChanged; break;
				case TextEntryEvent.Activated: Widget.Activated += HandleActivated; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed: Widget.Changed -= HandleChanged; break;
				case TextEntryEvent.Activated: Widget.Activated -= HandleActivated; break;
				}
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnChanged ();
			});
		}

		void HandleActivated (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnActivated ();
			});
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				var l = layout;
				if (l != null) {
					l.Dispose ();
					layout = null;
				}
			}
			base.Dispose (disposing);
		}
	}
}

