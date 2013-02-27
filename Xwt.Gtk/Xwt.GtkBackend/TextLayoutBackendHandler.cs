// 
// TextLayoutBackendHandler.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
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
using Xwt.Drawing;

using Xwt.CairoBackend;
using System.Collections.Generic;

namespace Xwt.GtkBackend
{
	public class GtkTextLayoutBackendHandler: TextLayoutBackendHandler
	{
		static Cairo.Context SharedContext;
		
		public double Heigth = -1;

		class PangoBackend
		{
			public Pango.Layout Layout { get; set; }

			int[] indexToByteIndex;
			int[] byteIndexToIndex;
			
			string text;
			public string Text {
				get {
					return text;
				}
				set {
					text = value;
					if (indexToByteIndex != null)
						SetupTables();
				}
			}
			
			public int IndexToByteIndex (int i)
			{
				if (indexToByteIndex == null)
					SetupTables();
				return indexToByteIndex[i];
			}
			
			public int ByteIndexToIndex (int i)
			{
				if (byteIndexToIndex == null)
					SetupTables();
				return byteIndexToIndex[i];
			}
			
			public void SetupTables()
			{
				var arr = Text.ToCharArray ();
				int byteIndex = 0;
				int[] indexToByteIndex = new int[arr.Length];
				var byteIndexToIndex = new List<int> ();
				for (int i = 0; i < arr.Length; i++) {
					indexToByteIndex[i] = byteIndex;
					byteIndex += System.Text.Encoding.UTF8.GetByteCount (arr, i, 1);
					while (byteIndexToIndex.Count < byteIndex)
						byteIndexToIndex.Add (i);
				}
				this.indexToByteIndex = indexToByteIndex;
				this.byteIndexToIndex = byteIndexToIndex.ToArray ();
			}
		}

		
		static GtkTextLayoutBackendHandler ()
		{
			Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
			SharedContext = new Cairo.Context (sf);
		}
		
		public override object Create (Context context)
		{
			CairoContextBackend c = (CairoContextBackend) Toolkit.GetBackend (context);
			return Pango.CairoHelper.CreateLayout (c.Context);
		}
		
		public override object Create (ICanvasBackend canvas)
		{
			return new PangoBackend {
				Layout = Pango.CairoHelper.CreateLayout (SharedContext)
			};
		}

		public override void SetText (object backend, string text)
		{
			var tl = (PangoBackend) backend;
			tl.Layout.SetText (text);
			tl.Text = text;
		}

		public override void SetFont (object backend, Xwt.Drawing.Font font)
		{
			var tl = (PangoBackend)backend;
			tl.Layout.FontDescription = (Pango.FontDescription)Toolkit.GetBackend (font);
		}
		
		public override void SetWidth (object backend, double value)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			tl.Width = (int) (value * Pango.Scale.PangoScale);
		}
		
		public override void SetHeight (object backend, double value)
		{
			this.Heigth = value;
		}
		
		public override void SetTrimming (object backend, TextTrimming textTrimming)
		{
			var tl = (PangoBackend)backend;
			if (textTrimming == TextTrimming.WordElipsis)
				tl.Layout.Ellipsize = Pango.EllipsizeMode.End;
			if (textTrimming == TextTrimming.Word)
				tl.Layout.Ellipsize = Pango.EllipsizeMode.None;
			
		}
		
		public override Size GetSize (object backend)
		{
			var tl = (PangoBackend)backend;
			int w, h;
			tl.Layout.GetPixelSize (out w, out h);
			return new Size ((double)w, (double)h);
		}

		public override void DisposeBackend (object backend)
		{
			var tl = (PangoBackend)backend;
			tl.Layout.Dispose ();
		}

	}
}

