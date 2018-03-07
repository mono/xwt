// 
// CellUtil.cs
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
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Xwt.Backends;

namespace Xwt.Mac
{
	static class CellUtil
	{
		public static CompositeCell CreateCellView (ApplicationContext context, ICellSource source, ICollection<CellView> cells, int column)
		{
			CompositeCell c = new CompositeCell (context, source);
			foreach (var cell in cells)
				c.AddCell ((ICellRenderer) CreateCellView (source, cell, column));
			return c;
		}

		public static void UpdateCellView (CompositeCell cellView, ICellSource source, ICollection<CellView> cells, int column)
		{
			cellView.ClearCells ();
			foreach (var cell in cells)
				cellView.AddCell ((ICellRenderer) CreateCellView (source, cell, column));
		}
		
		static NSView CreateCellView (ICellSource source, CellView cell, int column)
		{
			ICellRenderer cr = null;

			if (cell is ITextCellViewFrontend)
				cr = new TextTableCell ();
			else if (cell is IImageCellViewFrontend)
				cr = new ImageTableCell ();
			else if (cell is ICanvasCellViewFrontend)
				cr = new CanvasTableCell ();
			else if (cell is ICheckBoxCellViewFrontend)
				cr = new CheckBoxTableCell ();
			else if (cell is IRadioButtonCellViewFrontend)
				cr = new RadioButtonTableCell ();
			else
				throw new NotImplementedException ();
			ICellViewFrontend fr = cell;
			CellViewBackend backend = null;
			try {
				//FIXME: although the cell views are based on XwtComponent, they don't implement
				//       the dynamic registration based backend creation and there is no way to
				//       identify whether the frontend has already a valid backend.
				backend = cell.GetBackend () as CellViewBackend;
			} catch (InvalidOperationException) { }

			if (backend == null) {
				cr.Backend = new CellViewBackend (source.TableView, column);
				fr.AttachBackend ((source as ViewBackend).Frontend, cr.Backend);
			} else
				cr.Backend = backend;
			return (NSView)cr;
		}

		public static bool HandleMouseDown (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.ButtonPressed)) {
				CGPoint p = cell.CellView.ConvertPointFromEvent (theEvent);
				if (cell.CellView.Bounds.Contains (p)) {
					cell.Backend.Load (cell);
					cell.CellContainer.SetCurrentEventRow ();
					var offset = cell.Backend.CellBounds.Location;
					ButtonEventArgs args = new ButtonEventArgs {
						X = p.X + offset.X,
						Y = p.Y + offset.Y,
						Button = theEvent.GetPointerButton(),
						MultiplePress = (int)theEvent.ClickCount
					};
					cell.Backend.Context.InvokeUserCode (() => cell.Backend.EventSink.OnButtonPressed (args));
					return args.Handled;
				}
			}
			return false;
		}

		public static bool HandleMouseUp (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.ButtonReleased)) {
				CGPoint p = cell.CellView.ConvertPointFromEvent (theEvent);
				if (cell.CellView.Bounds.Contains (p)) {
					cell.Backend.Load (cell);
					cell.CellContainer.SetCurrentEventRow ();
					var offset = cell.Backend.CellBounds.Location;
					ButtonEventArgs args = new ButtonEventArgs {
						X = p.X + offset.X,
						Y = p.Y + offset.Y,
						Button = theEvent.GetPointerButton (),
						MultiplePress = (int)theEvent.ClickCount
					};
					cell.Backend.Context.InvokeUserCode (() => cell.Backend.EventSink.OnButtonReleased (args));
					return args.Handled;
				}
			}
			return false;
		}

		public static void HandleMouseEntered (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.MouseEntered)) {
				cell.Backend.Load (cell);
				cell.CellContainer.SetCurrentEventRow ();
				cell.Backend.Context.InvokeUserCode (cell.Backend.EventSink.OnMouseEntered);
			}
		}

		public static void HandleMouseExited (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.MouseExited)) {
				cell.Backend.Load (cell);
				cell.CellContainer.SetCurrentEventRow ();
				cell.Backend.Context.InvokeUserCode (cell.Backend.EventSink.OnMouseExited);
			}
		}

		public static bool HandleMouseMoved (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.MouseMoved)) {
				CGPoint p = cell.CellView.ConvertPointFromEvent (theEvent);
				if (cell.CellView.Bounds.Contains (p)) {
					cell.Backend.Load (cell);
					cell.CellContainer.SetCurrentEventRow ();
					var offset = cell.Backend.CellBounds.Location;
					MouseMovedEventArgs args = new MouseMovedEventArgs ((long)TimeSpan.FromSeconds (theEvent.Timestamp).TotalMilliseconds, p.X + offset.X, p.Y + offset.Y);
					cell.Backend.Context.InvokeUserCode (() => cell.Backend.EventSink.OnMouseMoved (args));
					return args.Handled;
				}
			}
			return false;
		}

		public static bool HandleKeyDown (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.KeyPressed)) {
				cell.Backend.Load (cell);
				cell.CellContainer.SetCurrentEventRow ();
				var keyArgs = theEvent.ToXwtKeyEventArgs ();
				cell.Backend.Context.InvokeUserCode (() => cell.Backend.EventSink.OnKeyPressed (keyArgs));
				return keyArgs.Handled;
			}
			return false;
		}

		public static bool HandleKeyUp (this ICellRenderer cell, NSEvent theEvent)
		{
			if (cell.Backend.GetIsEventEnabled (WidgetEvent.KeyReleased)) {
				cell.Backend.Load (cell);
				cell.CellContainer.SetCurrentEventRow ();
				var keyArgs = theEvent.ToXwtKeyEventArgs ();
				cell.Backend.Context.InvokeUserCode (() => cell.Backend.EventSink.OnKeyReleased (keyArgs));
				return keyArgs.Handled;
			}
			return false;
		}
	}
}

