// 
// Table.cs
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

namespace Xwt
{
	public class Table
	{
/*		new ITableBackend Backend {
			get { return (ITableBackend) base.Backend; }
		}
		
		public Table ()
		{
		}
		
		public void Attach (Widget w, int left, int right, int top, int bottom)
		{
			TablePlacement p = new TablePlacement (w);
			p.LeftAttach = left;
			p.RightAttach = right;
			p.TopAttach = top;
			p.BottomAttach = bottom;
			OnAdd (w, p);
		}
		
		protected override void OnAdd (Widget child, ChildPlacement placement)
		{
			if (placement == null)
				placement = new TablePlacement (child);

			base.OnAdd (child, placement);
			Backend.Attach ((IWidgetBackend)GetBackend (child), (TablePlacement) placement);
		}
		
		internal protected override void OnChildChanged (ChildPlacement placement, object hint)
		{
			Backend.Update ((IWidgetBackend)GetBackend (placement.Child), (TablePlacement) placement, (TablePlacementChange) hint);
		}*/
	}
	
/*	public class TablePlacement: ChildPlacement
	{
		int leftAttach;
		int rightAttach;
		int topAttach;
		int bottomAttach;
		AttachOptions xOptions = AttachOptions.Fill;
		AttachOptions yOptions = AttachOptions.Fill;
		int xPadding;
		int yPadding;
		
		internal TablePlacement (Widget child): base (child)
		{
		}
		
		public int LeftAttach {
			get {
				return this.leftAttach;
			}
			set {
				leftAttach = value;
				NotifyChanged (TablePlacementChange.LeftAttach);
			}
		}

		public int RightAttach {
			get {
				return this.rightAttach;
			}
			set {
				rightAttach = value;
				NotifyChanged (TablePlacementChange.RightAttach);
			}
		}

		public int TopAttach {
			get {
				return this.topAttach;
			}
			set {
				topAttach = value;
				NotifyChanged (TablePlacementChange.TopAttach);
			}
		}

		public int BottomAttach {
			get {
				return this.bottomAttach;
			}
			set {
				bottomAttach = value;
				NotifyChanged (TablePlacementChange.BottomAttach);
			}
		}

		public AttachOptions XOptions {
			get {
				return this.xOptions;
			}
			set {
				xOptions = value;
				NotifyChanged (TablePlacementChange.XOptions);
			}
		}

		public AttachOptions YOptions {
			get {
				return this.yOptions;
			}
			set {
				yOptions = value;
				NotifyChanged (TablePlacementChange.YOptions);
			}
		}

		public int XPadding {
			get {
				return this.xPadding;
			}
			set {
				xPadding = value;
				NotifyChanged (TablePlacementChange.XPadding);
			}
		}

		public int YPadding {
			get {
				return this.yPadding;
			}
			set {
				yPadding = value;
				NotifyChanged (TablePlacementChange.YPadding);
			}
		}
	}
	
	public enum AttachOptions
	{
		Shrink,
		Expand,
		Fill
	}*/
}

