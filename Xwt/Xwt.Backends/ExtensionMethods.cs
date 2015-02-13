//
// ExtensionMethods.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using Xwt.Drawing;

namespace Xwt.Backends
{
	public static class ExtensionMethods
	{
		public static object GetBackend (this XwtComponent obj)
		{
			if (obj != null)
				return ((IFrontend)obj).Backend;
			else
				return null;
		}

		public static IWidgetBackend GetBackend (this Widget obj)
		{
			if (obj != null)
				return (IWidgetBackend)((IFrontend)obj).Backend;
			else
				return null;
		}

		public static object GetBackend (this XwtObject obj)
		{
			if (obj != null)
				return ((IFrontend)obj).Backend;
			else
				return null;
		}

		public static ImageDescription ToImageDescription (this Image img, ApplicationContext ctx)
		{
			return img != null ? img.GetImageDescription (ctx.Toolkit) : ImageDescription.Null;
		}

		public static double GetValue (this WidgetPlacement al)
		{
			switch (al) {
			case WidgetPlacement.Center:
				return 0.5;
			case WidgetPlacement.End:
				return 1;
			default:
				return 0;
			}
		}

		public static Widget GetInternalParent (this Widget widget)
		{
			return widget.InternalParent;
		}

		public static void RaiseKeyPressed (this CellView cellView, KeyEventArgs args)
		{
			cellView.OnKeyPressed (args);
		}

		public static void RaiseKeyReleased (this CellView cellView, KeyEventArgs args)
		{
			cellView.OnKeyReleased (args);
		}

		public static void RaiseMouseEntered (this CellView cellView)
		{
			cellView.OnMouseEntered ();
		}

		public static void RaiseMouseExited (this CellView cellView)
		{
			cellView.OnMouseExited ();
		}

		public static void RaiseMouseMoved (this CellView cellView, MouseMovedEventArgs args)
		{
			cellView.OnMouseMoved (args);
		}

		public static void RaiseButtonPressed (this CellView cellView, ButtonEventArgs args)
		{
			cellView.OnButtonPressed (args);
		}

		public static void RaiseButtonReleased (this CellView cellView, ButtonEventArgs args)
		{
			cellView.OnButtonReleased (args);
		}

		public static void SetFileSource (this Image image, string file)
		{
			image.NativeRef.SetFileSource (file);
		}

		public static void SetResourceSource (this Image image, System.Reflection.Assembly asm, string name)
		{
			image.NativeRef.SetResourceSource (asm, name);
		}

		public static void SetStreamSource (this Image image, Func<System.IO.Stream[]> imageLoader)
		{
			image.NativeRef.SetStreamSource (imageLoader);
		}

		public static void SetStockSource (this Image image, string stockID)
		{
			image.NativeRef.SetStockSource (stockID);
		}
	}
}

