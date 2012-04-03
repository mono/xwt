// 
// DesignerSurface.cs
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
using System.Xml;
using System.Xaml;
using Xwt.Drawing;

namespace Xwt.Design
{
	public class DesignerSurface: Widget
	{
		Widget widget;
		
		public DesignerSurface ()
		{
		}
		
		IDesignerSurfaceBackend Backend {
			get { return (IDesignerSurfaceBackend) BackendHost.Backend; }
		}
		
		public void Load (XmlReader r)
		{
			object o = XamlServices.Load (r);
			if (!(o is Widget))
				throw new InvalidOperationException ("Invalid object type. Expected Xwt.Widget, found: " + o.GetType ());
			widget = (Widget)o;
			Backend.Load (widget);
		}
		
		public void Load (Widget widget)
		{
			this.widget = widget;
			Backend.Load (widget);
		}
		
		public void Save (XmlWriter w)
		{
			XamlServices.Save (w, widget);
		}
	}
}

