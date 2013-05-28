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

		public static ImageDescription ToImageDescription (this Image img)
		{
			return img != null ? img.ImageDescription : ImageDescription.Null;
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
	}
}

