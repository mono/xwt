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
using System.Collections.Generic;

namespace Xwt
{
	[BackendType (typeof(IRadioButtonBackend))]
	[ContentProperty("Content")]
	public class RadioButton: Widget
	{
		Widget content;
		string label = "";
		RadioButtonGroup radioGroup;

		EventHandler clicked;
		EventHandler activeChanged;
		EventHandler activeSet;

		protected new class WidgetBackendHost: Widget.WidgetBackendHost, IRadioButtonEventSink
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
			MapEvent (RadioButtonEvent.Clicked, typeof(RadioButton), "OnClicked");
			MapEvent (RadioButtonEvent.ActiveChanged, typeof(RadioButton), "OnActiveChanged");
			MapEvent (RadioButtonEvent.ActiveChanged, typeof(RadioButton), "OnActivated");
		}
		
		public RadioButton ()
		{
		}
		
		public RadioButton (string label)
		{
			VerifyConstructorCall (this);
			Label = label;
		}
		
		public RadioButton (Widget content)
		{
			VerifyConstructorCall (this);
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
					Group = new RadioButtonGroup () { GroupBackend = Backend.Group };
				return radioGroup;
			}
			set {
				if (radioGroup != null)
					radioGroup.Remove (this);
				radioGroup = value;
				if (radioGroup == null)
					radioGroup = new RadioButtonGroup ();

				if (radioGroup.GroupBackend == null)
					radioGroup.GroupBackend = Backend.Group;
				else
					Backend.Group = radioGroup.GroupBackend;
				radioGroup.Add (this);
			}
		}
		
		[DefaultValue (false)]
		public bool Active {
			get { return Backend.Active; }
			set {
				if (!value && Active && radioGroup != null)
					radioGroup.ResetSelection ();
				Backend.Active = value;
			}
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

		protected virtual void OnActivated (EventArgs e)
		{
			if (activeSet != null)
				activeSet (this, e);
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

		public event EventHandler Activated {
			add {
				if (activeSet == null)
					ActiveChanged += HandleActiveChanged;
				activeSet += value;
			}
			remove {
				activeSet -= value;
				if (activeSet == null)
					ActiveChanged -= HandleActiveChanged;
			}
		}

		void HandleActiveChanged (object sender, EventArgs e)
		{
			if (Active)
				OnActivated (e);
		}
	}

	
	public class RadioButtonGroup
	{
		internal object GroupBackend;

		List<RadioButton> items = new List<RadioButton> ();
		EventHandler activeEvent;
		RadioButton activeRadioButton;
		bool eventsEnabled;

		public RadioButton ActiveRadioButton {
			get {
				if (eventsEnabled)
					return activeRadioButton;
				else {
					EnableActiveEvent ();
					return activeRadioButton = items.FirstOrDefault (r => r.Active);
				}
			}
		}

		internal void Add (RadioButton r)
		{
			items.Add (r);
			if (eventsEnabled)
				r.ActiveChanged += HandleActiveChanged;
		}

		internal void Remove (RadioButton r)
		{
			items.Remove (r);
			if (eventsEnabled)
				r.ActiveChanged -= HandleActiveChanged;
		}

		public event EventHandler ActiveRadioButtonChanged {
			add {
				if (!eventsEnabled)
					EnableActiveEvent ();
				activeEvent += value; 
			}
			remove {
				activeEvent -= value;
			}
		}

		public void ClearActive ()
		{
			if (ActiveRadioButton != null)
				ActiveRadioButton.Active = false;
		}

		void EnableActiveEvent ()
		{
			if (!eventsEnabled) {
				eventsEnabled = true;
				foreach (var b in items)
					b.ActiveChanged += HandleActiveChanged;
			}
		}

		void HandleActiveChanged (object sender, EventArgs e)
		{
			if (((RadioButton)sender).Active)
				SetActive ((RadioButton)sender);
		}

		void SetActive (RadioButton r)
		{
			var old = activeRadioButton;
			activeRadioButton = r;
			if (old != r && activeEvent != null)
				activeEvent (this, EventArgs.Empty);
		}

		internal void ResetSelection ()
		{
			SetActive (null);
		}
	}
}

