// 
// Button.cs
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

namespace Xwt
{
	public class Button: Widget
	{
		EventHandler clicked;
		ButtonStyle style = ButtonStyle.Normal;
		ButtonType type = ButtonType.Normal;
		Image image;
		string label;
		ContentPosition imagePosition = ContentPosition.Left;
		
		protected new class EventSink: Widget.EventSink, IButtonEventSink
		{
			public void OnClicked ()
			{
				((Button)Parent).OnClicked (EventArgs.Empty);
			}
		}
		
		static Button ()
		{
			MapEvent (ButtonEvent.Clicked, typeof(Button), "OnClicked");
		}
		
		public Button ()
		{
		}
		
		public Button (string label): this ()
		{
			Label = label;
		}
		
		public Button (Image img, string label): this ()
		{
			Label = label;
			Image = img;
		}
		
		public Button (Image img): this ()
		{
			Image = img;
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		new IButtonBackend Backend {
			get { return (IButtonBackend) base.Backend; }
		}
		
		public string Label {
			get { return label; }
			set {
				label = value;
				Backend.SetContent (label, XwtObject.GetBackend (image), imagePosition);
				OnPreferredSizeChanged ();
			}
		}
		
		public Image Image {
			get { return image; }
			set {
				image = value;
				Backend.SetContent (label, XwtObject.GetBackend (value), imagePosition); 
				OnPreferredSizeChanged ();
			}
		}
		
		public ContentPosition ImagePosition {
			get { return imagePosition; }
			set {
				imagePosition = value;
				Backend.SetContent (label, XwtObject.GetBackend (image), imagePosition); 
				OnPreferredSizeChanged ();
			}
		}
		
		public ButtonStyle Style {
			get { return style; }
			set {
				style = value;
				Backend.SetButtonStyle (style);
				OnPreferredSizeChanged ();
			}
		}
		
		public ButtonType Type {
			get { return type; }
			set {
				type = value;
				Backend.SetButtonType (type);
				OnPreferredSizeChanged ();
			}
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.SetButtonStyle (style);
		}
		
		protected virtual void OnClicked (EventArgs e)
		{
			if (clicked != null)
				clicked (this, e);
		}
		
		public event EventHandler Clicked {
			add {
				OnBeforeEventAdd (ButtonEvent.Clicked, clicked);
				clicked += value;
			}
			remove {
				clicked -= value;
				OnAfterEventRemove (ButtonEvent.Clicked, clicked);
			}
		}
	}
}

