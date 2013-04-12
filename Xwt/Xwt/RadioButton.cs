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
using System.Windows.Markup;
using System.Linq;

namespace Xwt
{
	[BackendType (typeof(IRadioButtonBackend))]
	[ContentProperty("Content")]
	public class RadioButton: Widget
	{
		Widget content;
		EventHandler clicked;
		EventHandler activeChanged;
		string label = "";
		RadioButtonGroup radioGroup;
		
		protected new class WidgetBackendHost: Widget.WidgetBackendHost, ICheckBoxEventSink
		{
			public void OnClicked ()
			{
				((RadioButton)Parent).OnClicked (EventArgs.Empty);
			}
			public void OnToggled ()
			{
				((RadioButton)Parent).OnActiveChanged (EventArgs.Empty);
			}
		}
		
		static RadioButton ()
		{
			MapEvent (ButtonEvent.Clicked, typeof(RadioButton), "OnClicked");
			MapEvent (ButtonEvent.Clicked, typeof(RadioButton), "OnActiveChanged");
		}
		
		public RadioButton ()
		{
		}
		
		public RadioButton (string label): this ()
		{
			Label = label;
		}
		
		public RadioButton (Widget content): this ()
		{
			Content = content;
		}
		
		protected override BackendHost CreateBackendHost ()
		{
			return new WidgetBackendHost ();
		}
		
		IRadioButtonBackend Backend {
			get { return (IRadioButtonBackend) BackendHost.Backend; }
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
		
		[DefaultValue (null)]
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
		
		public RadioButtonGroup Group {
			get {
				if (radioGroup == null)
					Group = new RadioButtonGroup ();
				return radioGroup;
			}
			set {
				if (radioGroup != null)
					radioGroup.Items.Remove (this);
				radioGroup = value;
				if (radioGroup == null)
					radioGroup = new RadioButtonGroup ();

				if (radioGroup.GroupBackend == null)
					radioGroup.GroupBackend = Backend.CreateRadioGroup ();
				Backend.SetRadioGroup (radioGroup.GroupBackend);
				radioGroup.Items.Add (this);
			}
		}
		
		[DefaultValue (false)]
		public bool Active {
			get { return Backend.Active; }
			set { Backend.Active = value; }
		}
		
		protected virtual void OnClicked (EventArgs e)
		{
			if (clicked != null)
				clicked (this, e);
		}
		
		protected virtual void OnActiveChanged (EventArgs e)
		{
			if (activeChanged != null)
				activeChanged (this, e);
		}
		
		public event EventHandler Clicked {
			add {
				BackendHost.OnBeforeEventAdd (RadioButtonEvent.Clicked, clicked);
				clicked += value;
			}
			remove {
				clicked -= value;
				BackendHost.OnAfterEventRemove (RadioButtonEvent.Clicked, clicked);
			}
		}
		
		public event EventHandler ActiveChanged {
			add {
				BackendHost.OnBeforeEventAdd (RadioButtonEvent.ActiveChanged, activeChanged);
				activeChanged += value;
			}
			remove {
				activeChanged -= value;
				BackendHost.OnAfterEventRemove (RadioButtonEvent.ActiveChanged, activeChanged);
			}
		}
	}
}

