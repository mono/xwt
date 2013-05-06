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
		bool allowMixed;
		bool internalActiveUpdate;
		bool toggleEventEnabled;
		
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
				return Widget.Active && !Widget.Inconsistent;
			}
			set {
				Widget.Active = value;
			}
		}
		
		public bool AllowMixed {
			get {
				return allowMixed;
			}
			set {
				if (value)
					UpdateToggleEventStatus (true);
				allowMixed = value;
				if (!value)
					UpdateToggleEventStatus (false);
			}
		}
		
		public bool Mixed {
			get { return Widget.Inconsistent; }
			set {
				internalActiveUpdate = true;
				Widget.Inconsistent = value;
				Widget.Active = value;
				internalActiveUpdate = false;
			}
		}
		
		public void SetContent (string label)
		{
			Widget.Label = label;
		}
		
		public void SetContent (IWidgetBackend widget)
		{
			var w = (WidgetBackend)widget;
			if (Widget.Children.Length > 0)
				Widget.Remove(Widget.Children[0]);

			if (w != null)
				Widget.Add(w.Widget);
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
				case CheckBoxEvent.Toggled: UpdateToggleEventStatus (true); toggleEventEnabled = true; break;
				case CheckBoxEvent.Clicked: Widget.Clicked += HandleWidgetClicked; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is CheckBoxEvent) {
				switch ((CheckBoxEvent)eventId) {
				case CheckBoxEvent.Toggled: toggleEventEnabled = false; UpdateToggleEventStatus (false); break;
				case CheckBoxEvent.Clicked: Widget.Clicked -= HandleWidgetClicked; break;
				}
			}
		}
		
		void UpdateToggleEventStatus (bool enable)
		{
			if (enable && !toggleEventEnabled && !allowMixed)
				Widget.Toggled += HandleWidgetActivated;
			else if (!enable && !toggleEventEnabled && !allowMixed)
				Widget.Toggled -= HandleWidgetActivated;
		}
		
		void HandleWidgetActivated (object sender, EventArgs e)
		{
			if (internalActiveUpdate)
				return;
			
			if (allowMixed) {
				if (!Widget.Active) {
					if (Widget.Inconsistent)
						Widget.Inconsistent = false;
					else {
						Widget.Inconsistent = true;
						internalActiveUpdate = true;
						Widget.Active = true;
						internalActiveUpdate = false;
					}
				}
			}
			
			if (toggleEventEnabled) {
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnToggled ();
				});
			}
		}

		void HandleWidgetClicked (object sender, EventArgs e)
		{
			if (internalActiveUpdate)
				return;
			
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnClicked ();
			});
		}
	}
}

