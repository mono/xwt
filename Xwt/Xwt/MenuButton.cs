// 
// MenuButton.cs
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

namespace Xwt
{
	public class MenuButton: Widget
	{
		ButtonStyle style = ButtonStyle.Normal;
		Image image;
		string label;
		Menu menu;
		Func<Menu> creator;
		
		protected new class EventSink: Widget.EventSink, IMenuButtonEventSink
		{
			public IMenuBackend OnCreateMenu ()
			{
				return ((MenuButton)Parent).CreateMenu ();
			}
		}
		
		public MenuButton ()
		{
		}
		
		public MenuButton (string label): this ()
		{
			Label = label;
		}
		
		public MenuButton (Image img, string label): this ()
		{
			Label = label;
			Image = img;
		}
		
		public MenuButton (Image img): this ()
		{
			Image = img;
		}
		
		protected override Widget.EventSink CreateEventSink ()
		{
			return new EventSink ();
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.SetButtonStyle (style);
		}
		
		new IMenuButtonBackend Backend {
			get { return (IMenuButtonBackend) base.Backend; }
		}
				
		public string Label {
			get { return label; }
			set {
				label = value;
				Backend.SetContent (label, image);
				OnPreferredSizeChanged ();
			}
		}
		
		public Image Image {
			get { return image; }
			set {
				image = value;
				Backend.SetContent (label, XwtObject.GetBackend (value)); 
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
		
		public Menu Menu {
			get { return menu; }
			set { menu = value; }
		}
		
		public Func<Menu> MenuSource {
			get { return creator; }
			set { creator = value; }
		}
		
		IMenuBackend CreateMenu ()
		{
			return ((IMenuBackend)WidgetRegistry.GetBackend (OnCreateMenu()));
		}
		
		protected virtual Menu OnCreateMenu ()
		{
			if (menu != null)
				return menu;
			if (creator != null)
				return creator ();
			return null;
		}
	}
}

