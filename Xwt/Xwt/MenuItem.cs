// 
// MenuItem.cs
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
using System.ComponentModel;
using Xwt.Drawing;

namespace Xwt
{
	public class MenuItem: XwtComponent, ICellContainer
	{
		CellViewCollection cells;
		Menu subMenu;
		EventSink eventSink;
		EventHandler clicked;
		MenuItemType type;
		Image image;
		
		static MenuItem ()
		{
			MapEvent (MenuItemEvent.Clicked, typeof(MenuItem), "OnClicked");
		}
		
		public MenuItem ()
		{
			eventSink = new EventSink () { Parent = this };
		}
		
		public MenuItem (Command command): this ()
		{
			Label = command.Label;
			if (!string.IsNullOrEmpty (command.Icon))
				Image = Image.FromIcon (command.Icon, IconSize.Small);
		}
		
		public MenuItem (string label): this ()
		{
			Label = label;
		}
		
		public MenuItem (string label, MenuItemType type): this ()
		{
			Label = label;
			Type = type;
		}
		
		new IMenuItemBackend Backend {
			get { return (IMenuItemBackend) base.Backend; }
		}
		
		protected override void OnBackendCreated ()
		{
			base.OnBackendCreated ();
			Backend.Initialize (eventSink);
		}
		
		[DefaultValue ("")]
		public string Label {
			get { return Backend.Label; }
			set { Backend.Label = value; }
		}
		
		[DefaultValue (true)]
		public bool Sensitive {
			get { return Backend.Sensitive; }
			set { Backend.Sensitive = value; }
		}
		
		[DefaultValue (true)]
		public bool Visible {
			get { return Backend.Visible; }
			set { Backend.Visible = value; }
		}
		
		[DefaultValue (true)]
		public bool Checked {
			get { return Backend.Checked; }
			set { Backend.Checked = value; }
		}
		
		public Image Image {
			get { return image; }
			set { image = value; Backend.SetImage (XwtObject.GetBackend (value)); }
		}
		
		[DefaultValue (MenuItemType.Normal)]
		public MenuItemType Type {
			get { return type; }
			set { type = value; Backend.SetType (type); }
		}
		
		public void Show ()
		{
			Visible = true;
		}
		
		public void Hide ()
		{
			Visible = false;
		}
		
		public CellViewCollection Cells {
			get {
				if (cells == null)
					cells = new CellViewCollection (this);
				return cells;
			}
		}
		
		public Menu SubMenu {
			get { return subMenu; }
			set {
				subMenu = value;
				Backend.SetSubmenu ((IMenuBackend) GetBackend (subMenu));
			}
		}
		
		public void NotifyCellChanged ()
		{
			throw new NotImplementedException ();
		}
		
		class EventSink: IMenuItemEventSink
		{
			public MenuItem Parent;
			
			public void OnClicked ()
			{
				Parent.OnClicked (EventArgs.Empty);
			}
		}
		
		protected virtual void OnClicked (EventArgs e)
		{
			if (clicked != null)
				clicked (this, e);
		}
		
		public event EventHandler Clicked {
			add {
				OnBeforeEventAdd (MenuItemEvent.Clicked, clicked);
				clicked += value;
			}
			remove {
				clicked -= value;
				OnAfterEventRemove (MenuItemEvent.Clicked, clicked);
			}
		}
	}
	
	public enum MenuItemType
	{
		Normal,
		CheckBox,
		RadioButton
	}
}

