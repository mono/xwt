// 
// SelectColorDialog.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc
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

using Xwt.Backends;

namespace Xwt
{
	public sealed class SelectColorDialog
	{
		Color color = Colors.Transparent;
		string title = "";
		bool supportsAlpha;
		
		public SelectColorDialog ()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.SelectColorDialog"/> class.
		/// </summary>
		/// <param name='title'>
		/// Title of the dialog
		/// </param>
		public SelectColorDialog (string title)
		{
			this.title = title;
		}

		/// <summary>
		/// Gets or sets the title of the dialog
		/// </summary>
		public string Title {
			get { return title ?? ""; }
			set { title = value ?? ""; }
		}

		/// <summary>
		/// Gets or sets the selected color
		/// </summary>
		public Color Color {
			get { return color; }
			set { color = value; }
		}
		
		public bool SupportsAlpha {
			get { return supportsAlpha; }
			set { supportsAlpha = value; }
		}
		
		/// <summary>
		/// Shows the dialog.
		/// </summary>
		public bool Run ()
		{
			return Run (null);
		}

		/// <summary>
		/// Shows the dialog.
		/// </summary>
		public bool Run (WindowFrame parentWindow)
		{
			var backend = Toolkit.CurrentEngine.Backend.CreateBackend<ISelectColorDialogBackend> ();
			try {
				if (color != Colors.Transparent)
					backend.Color = color;
				return backend.Run ((IWindowFrameBackend)Toolkit.CurrentEngine.GetSafeBackend (parentWindow), title, supportsAlpha);
			} finally {
				color = backend.Color;
				backend.Dispose ();
			}
		}
	}
}

