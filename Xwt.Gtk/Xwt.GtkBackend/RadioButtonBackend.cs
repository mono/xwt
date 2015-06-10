//
// RadioButtonBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc
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
	public class RadioButtonBackend: WidgetBackend, IRadioButtonBackend
	{
		RadioGroup radioGroup;

		public RadioButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			NeedsEventBox = false;
			Widget = new Gtk.RadioButton ("");
			Widget.Show ();
		}

		protected new Gtk.RadioButton Widget {
			get { return (Gtk.RadioButton)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IRadioButtonEventSink EventSink {
			get { return (IRadioButtonEventSink)base.EventSink; }
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

		protected override void OnSetBackgroundColor (Xwt.Drawing.Color color)
		{
			base.OnSetBackgroundColor (color);
			Widget.SetBackgroundColor (color);
			Widget.SetChildBackgroundColor (color);
			EventsRootWidget.SetBackgroundColor (color);
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is RadioButtonEvent) {
				switch ((RadioButtonEvent)eventId) {
				case RadioButtonEvent.ActiveChanged: Widget.Toggled += HandleToggled; break;
				case RadioButtonEvent.Clicked: Widget.Clicked += HandleClicked;; break;
				}
			}
		}

		public override void DisableEvent (object eventId)
		{
			if (eventId is RadioButtonEvent) {
				switch ((RadioButtonEvent)eventId) {
				case RadioButtonEvent.ActiveChanged: Widget.Toggled -= HandleToggled; break;
				case RadioButtonEvent.Clicked: Widget.Clicked -= HandleClicked;; break;
				}
			}
		}
		
		void HandleClicked (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnClicked ();
			});
		}
		
		void HandleToggled (object sender, EventArgs e)
		{
			ApplicationContext.InvokeUserCode (delegate {
				EventSink.OnToggled ();
			});
		}

		#region IRadioButtonBackend implementation


		public void SetContent (IWidgetBackend widget)
		{
			var w = (WidgetBackend)widget;
			if (Widget.Children.Length > 0)
				Widget.Remove (Widget.Children [0]);

			if (w != null)
				Widget.Add (w.Widget);
		}

		public void SetContent (string label)
		{
			Widget.Label = label;
		}

		class RadioGroup
		{
			public Gtk.RadioButton Group;
			public Gtk.RadioButton NullRadio;
		}

		public object Group {
			get {
				if (radioGroup == null)
					radioGroup = new RadioGroup () { Group = Widget };
				return radioGroup;
			}
			set {
				var g = (RadioGroup)value;
				if (g != radioGroup) {
					Widget.Group = g.Group.Group;
					radioGroup = g;
				}
			}
		}

		public bool Active {
			get {
				return Widget.Active;
			}
			set {
				if (Widget.Active && !value) {
					var g = (RadioGroup) Group;
					if (g.NullRadio == null)
						g.NullRadio = new Gtk.RadioButton (g.Group);
					g.NullRadio.Active = true;
				} else
					Widget.Active = value;
			}
		}

		#endregion
	}
}

