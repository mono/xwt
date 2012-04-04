// 
// StatusIconBackend.cs
//  
// Author:
//       Andres G. Aragoneses <knocte@gmail.com>
// 
// Copyright (c) 2012 Andres G. Aragoneses
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
using System.Collections.ObjectModel;
using Xwt.Backends;
using Xwt.Drawing;
using MonoMac.AppKit;

namespace Xwt.Mac
{
	public class StatusIconBackend : IStatusIconBackend
	{
		// https://developer.apple.com/library/mac/documentation/Cocoa/Reference/ApplicationKit/Classes/NSStatusBar_Class/Reference/Reference.html#//apple_ref/doc/constant_group/Status_Bar_Item_Length
		static int NSVariableStatusItemLength = -1;
		
		public void InitializeBackend (object frontend)
		{
		}
		
		public void EnableEvent (object eventId) { throw new NotImplementedException (); }
		public void DisableEvent (object eventId) { throw new NotImplementedException (); }
		
		public void SetComponents (string pathToImage, Collection<MenuItem> menuItems)
		{
			if (String.IsNullOrEmpty (pathToImage)) {
				throw new ArgumentNullException ("pathToImage");
			}
			if (menuItems == null) {
				throw new ArgumentNullException ("menuItems");
			}
			if (menuItems.Count == 0) {
				throw new ArgumentException ("menuItems must contain at least one item", "menuItems");
			}
			
			var image = new NSImage (pathToImage);
			var status_item = NSStatusBar.SystemStatusBar.CreateStatusItem (NSVariableStatusItemLength);
			status_item.Image = image;
			
			status_item.Menu = new NSMenu();
			foreach (var menuItem in menuItems) {
				status_item.Menu.AddItem (new NSMenuItem (menuItem.Label));
			}
		}
		
		public void Initialize (IWidgetEventSink eventSink)
		{
		}
		public void Dispose () { throw new NotImplementedException (); }
		
		public bool Visible { get; set; }
		public bool Sensitive { get; set; }
		public bool CanGetFocus { get; set; }
		public bool HasFocus { get { throw new NotImplementedException (); } }
		public Size Size { get { throw new NotImplementedException (); } }
		public Point ConvertToScreenCoordinates (Point widgetCoordinates) { throw new NotImplementedException (); }
		
		public void SetMinSize (double width, double height) { throw new NotImplementedException (); }
		public void SetNaturalSize (double width, double height) { throw new NotImplementedException (); }
		
		public void SetFocus () { throw new NotImplementedException (); }
		
		public void UpdateLayout () { throw new NotImplementedException (); }
		public WidgetSize GetPreferredWidth () { throw new NotImplementedException (); }
		public WidgetSize GetPreferredHeightForWidth (double width) { throw new NotImplementedException (); }
		public WidgetSize GetPreferredHeight () { throw new NotImplementedException (); }
		public WidgetSize GetPreferredWidthForHeight (double height) { throw new NotImplementedException (); }
		
		public object NativeWidget { get { throw new NotImplementedException (); } }
		
		public void DragStart (DragStartData data) { throw new NotImplementedException (); }
		public void SetDragSource (TransferDataType[] types, DragDropAction dragAction) { throw new NotImplementedException (); }
		
		public void SetDragTarget (TransferDataType[] types, DragDropAction dragAction) { throw new NotImplementedException (); }
		
		public object Font { get; set; }
		public Color BackgroundColor { get; set; }
		public string TooltipText { get; set; }

		public void SetCursor (CursorType cursorType) { throw new NotImplementedException (); }
	}
}

