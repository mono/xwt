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
	public class StatusIconBackend : MenuBackend, IStatusIconBackend
	{
		// https://developer.apple.com/library/mac/documentation/Cocoa/Reference/ApplicationKit/Classes/NSStatusBar_Class/Reference/Reference.html#//apple_ref/doc/constant_group/Status_Bar_Item_Length
		static int NSVariableStatusItemLength = -1;
		
		NSStatusItem status_item;
		
		public override void InitializeBackend (object frontend)
		{
			base.InitializeBackend (frontend);
			status_item = NSStatusBar.SystemStatusBar.CreateStatusItem (NSVariableStatusItemLength);
			status_item.Menu = this;
		}
		
		public void SetContent (object imageBackend) {
			if (imageBackend == null) {
				throw new ArgumentNullException ("imageBackend");
			}
			if (status_item == null) {
				throw new InvalidOperationException ("backend was not initialized");
			}
			status_item.Image = (NSImage)imageBackend;
		}
	}
}

