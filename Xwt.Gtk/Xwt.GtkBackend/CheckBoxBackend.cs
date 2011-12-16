// 
// CheckBoxBackend.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
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

namespace Xwt.GtkBackend
{
	public class CheckBoxBackend: WidgetBackend, ICheckBoxBackend
	{
		public CheckBoxBackend ()
		{
		}

		public override void Initialize ()
		{
			Widget = new Gtk.CheckButton ();
			Widget.Show ();
		}
		
		protected new Gtk.CheckButton Widget {
			get { return (Gtk.CheckButton)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new ICheckBoxEventSink EventSink {
			get { return (ICheckBoxEventSink)base.EventSink; }
		}
		
		public bool Active {
			get {
				return Widget.Active;
			}
			set {
				Widget.Active = value;
			}
		}
		
		public void SetContent (string label)
		{
			Widget.Label = label;
		}
		
		public void SetContent (IWidgetBackend widget)
		{
			throw new NotImplementedException ();
		}
		
		public override object Font {
			get {
				return base.Font;
			}
			set {
				var fd = (Pango.FontDescription) value;
				foreach (var c in Widget.Children)
					c.ModifyFont (fd);
			}
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is CheckBoxEvent) {
				switch ((CheckBoxEvent)eventId) {
				case CheckBoxEvent.Clicked: Widget.Clicked += HandleWidgetClicked; break;
				case CheckBoxEvent.Toggled: Widget.Toggled += HandleWidgetActivated; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CheckBoxEvent) {
				switch ((CheckBoxEvent)eventId) {
				case CheckBoxEvent.Clicked: Widget.Clicked -= HandleWidgetClicked; break;
				case CheckBoxEvent.Toggled: Widget.Toggled -= HandleWidgetActivated; break;
				}
			}
		}

		void HandleWidgetActivated (object sender, EventArgs e)
		{
			EventSink.OnToggled ();
		}

		void HandleWidgetClicked (object sender, EventArgs e)
		{
			EventSink.OnClicked ();
		}
	}
}

