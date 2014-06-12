// 
// StatusIcon.cs
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

using Xwt.Drawing;
using Xwt.Backends;

namespace Xwt
{
	[BackendType (typeof(IStatusIconBackend))]
	public class StatusIcon : XwtComponent
	{
		Image image;
		Menu menu;
		
		internal StatusIcon ()
		{
		}

		public Menu Menu {
			get { return menu; }
			set { menu = value; Backend.SetMenu (Menu.Backend); }
		}

		public Image Image {
			get { return image; }
			set { image = value; Backend.SetImage (image != null ? image.GetImageDescription (BackendHost.ToolkitEngine) : ImageDescription.Null); }
		}

		IStatusIconBackend Backend {
			get { return (IStatusIconBackend) BackendHost.Backend; }
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			// Don't dispose the backend if this object is being finalized
			// The backend has to handle the finalizing on its own
			if (disposing && BackendHost.BackendCreated)
				Backend.Dispose ();
		}
	}
}

