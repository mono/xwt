// 
// SelectColorDialogBackend.cs
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
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public class SelectColorDialogBackend: ISelectColorDialogBackend
	{
		Gtk.ColorSelectionDialog dlg;
		Color color;
		string defaultTitle;
		
		public SelectColorDialogBackend ()
		{
			dlg = new Gtk.ColorSelectionDialog (null);
			defaultTitle = dlg.Title;
		}

		#region ISelectColorDialogBackend implementation
		public bool Run (IWindowFrameBackend parent, string title, bool supportsAlpha)
		{
			if (!String.IsNullOrEmpty (title))
				dlg.Title = title;
			else
				dlg.Title = defaultTitle;

			dlg.ColorSelection.HasOpacityControl = supportsAlpha;
			
			dlg.ColorSelection.CurrentColor = color.ToGtkValue ();
			if (supportsAlpha)
				dlg.ColorSelection.CurrentAlpha = (ushort) (((double)ushort.MaxValue) * color.Alpha);
		
			var p = (WindowFrameBackend) parent;
			int result = MessageService.RunCustomDialog (dlg, p != null ? p.Window : null);
			
			if (result == (int) Gtk.ResponseType.Ok) {
				color = dlg.ColorSelection.CurrentColor.ToXwtValue ();
				if (supportsAlpha)
					color = color.WithAlpha ((double)dlg.ColorSelection.CurrentAlpha / (double)ushort.MaxValue);
				return true;
			}
			else
				return false;
		}

		public void Dispose ()
		{
			dlg.Destroy ();
		}

		public Color Color {
			get {
				return color;
			}
			set {
				color = value;
			}
		}
		
		#endregion
	}
}

