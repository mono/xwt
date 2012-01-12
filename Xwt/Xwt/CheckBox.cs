// 
// CheckBox.cs
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
using System.ComponentModel;
using Xwt.Backends;

namespace Xwt
{
	public class CheckBox: Widget
	{
		Widget content;
		EventHandler clicked;
		EventHandler toggled;
		string label = "";
		
		protected new class EventSink: Widget.EventSink, ICheckBoxEventSink
		{
			public void OnClicked ()
			{
				((CheckBox)Parent).OnClicked (EventArgs.Empty);
			}
			public void OnToggled ()
			{
				((CheckBox)Parent).OnToggled (EventArgs.Empty);
			}
		}
		
		static CheckBox ()
		{
			MapEvent (ButtonEvent.Clicked, typeof(CheckBox), "OnClicked");
		}
		
		public CheckBox ()
		{
		}
		
		public CheckBox (string label): this ()
		{
			Label = label;
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new ICheckBoxBackend Backend {
			get { return (ICheckBoxBackend) base.Backend; }
		}
		
		[DefaultValue ("")]
		public string Label {
			get { return label; }
			set {
				label = value;
				Backend.SetContent (label);
				OnPreferredSizeChanged ();
			}
		}
		
		public new Widget Content {
			get { return content; }
			set {
				if (content != null)
					UnregisterChild (content);
				content = value;
				if (content != null)
					RegisterChild (content);
				Backend.SetContent ((IWidgetBackend)GetBackend (content));
				OnPreferredSizeChanged ();
			}
		}
		
		[DefaultValue (false)]
		public bool Active {
			get { return Backend.Active; }
			set { Backend.Active = value; }
		}
		
		[DefaultValue (false)]
		public bool Mixed {
			get { return Backend.Mixed; }
			set { Backend.Mixed = value; }
		}
		
		[DefaultValue (false)]
		public bool AllowMixed {
			get { return Backend.AllowMixed; }
			set { Backend.AllowMixed = value; }
		}
		
		protected virtual void OnClicked (EventArgs e)
		{
			if (clicked != null)
				clicked (this, e);
		}
		
		protected virtual void OnToggled (EventArgs e)
		{
			if (toggled != null)
				toggled (this, e);
		}
		
		public event EventHandler Clicked {
			add {
				OnBeforeEventAdd (CheckBoxEvent.Clicked, clicked);
				clicked += value;
			}
			remove {
				clicked -= value;
				OnAfterEventRemove (CheckBoxEvent.Clicked, clicked);
			}
		}
		
		public event EventHandler Toggled {
			add {
				OnBeforeEventAdd (CheckBoxEvent.Toggled, toggled);
				toggled += value;
			}
			remove {
				toggled -= value;
				OnAfterEventRemove (CheckBoxEvent.Toggled, toggled);
			}
		}
	}
}

