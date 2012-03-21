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
using Xwt.Engine;

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
			get {
				return Widget.Text;
			}
			set {
				Widget.Text = value;
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
				Widget.ModifyBase (Gtk.StateType.Normal, Util.ToGdkColor (value));
			}
		}

		Pango.Layout layout;
		
		void HandleWidgetExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			// The Entry's GdkWindow is the top level window onto which
			// the frame is drawn; the actual text entry is drawn into a
			// separate window, so we can ensure that for themes that don't
			// respect HasFrame, we never ever allow the base frame drawing
			// to happen
			if (args.Event.Window == Widget.GdkWindow)
				return;
			
			if (Widget.Text.Length > 0)
				return;
			
			if (layout == null) {
				layout = new Pango.Layout (Widget.PangoContext);
				layout.FontDescription = Widget.PangoContext.FontDescription.Copy ();
			}
			
			int wh, ww;
			args.Event.Window.GetSize (out ww, out wh);
			
			int width, height;
			layout.SetText (placeHolderText);
			layout.GetPixelSize (out width, out height);
			Gdk.GC gc = new Gdk.GC (args.Event.Window);
			gc.Copy (Widget.Style.TextGC (Gtk.StateType.Normal));
			Color color_a = Util.ToXwtColor (Widget.Style.Base (Gtk.StateType.Normal));
			Color color_b = Util.ToXwtColor (Widget.Style.Text (Gtk.StateType.Normal));
			gc.RgbFgColor = Util.ToGdkColor (color_b.BlendWith (color_a, 0.5));
			
			args.Event.Window.DrawLayout (gc, 2, (wh - height) / 2 + 1, layout);
			gc.Dispose ();
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
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed: Widget.Changed += HandleChanged; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is TextEntryEvent) {
				switch ((TextEntryEvent)eventId) {
				case TextEntryEvent.Changed: Widget.Changed -= HandleChanged; break;
				}
			}
		}

		void HandleChanged (object sender, EventArgs e)
		{
			Toolkit.Invoke (delegate {
				EventSink.OnChanged ();
			});
		}
	}
}

