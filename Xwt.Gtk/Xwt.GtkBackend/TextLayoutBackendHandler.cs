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
using System.Runtime.InteropServices;

namespace Xwt.GtkBackend
{
	public class GtkTextLayoutBackendHandler: TextLayoutBackendHandler
	{
		static Cairo.Context SharedContext;
		
		public double Heigth = -1;

		internal class PangoBackend : IDisposable
		{
			Pango.Layout layout;
			public Pango.Layout Layout {
				get {
					if (hasUnassignedAttributes) {
						attributes.AssignTo (layout);
						hasUnassignedAttributes = false;
					}
					return layout;
				}
				set {
					layout = value;
				}
			}

			FastPangoAttrList attributes;
			bool hasUnassignedAttributes = false;
			public FastPangoAttrList Attributes {
				get {
					if (attributes == null)
						attributes = new FastPangoAttrList ();
					hasUnassignedAttributes = true;
					return attributes;
				}
				private set {
					attributes = value;
				}
			}

			string text = String.Empty;
			public string Text {
				get {
					return text;
				}
				set {
					text = value ?? String.Empty;;
					indexer = null;
					if (attributes != null) {
						attributes.Dispose ();
						attributes = null;
					}
				}
			}
			
			TextIndexer indexer;
			public TextIndexer TextIndexer {
				get {
					if (indexer == null)
						indexer = new TextIndexer (Text);
					return indexer;
				}
			}

			public void ClearAttributes ()
			{
				if (attributes != null) {
					attributes.Dispose ();
					attributes = new FastPangoAttrList ();
					hasUnassignedAttributes = true;
				}
			}

			public void Dispose ()
			{
				if (layout != null) {
					layout.Dispose ();
					layout = null;
				}
				if (attributes != null) {
					attributes.Dispose ();
					attributes = null;
				}
			}
		}

		static GtkTextLayoutBackendHandler ()
		{
			using (Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1)) {
				SharedContext = new Cairo.Context (sf);
			}
		}

		public static void DisposeResources ()
		{
			((IDisposable)SharedContext).Dispose ();
		}
		
		public override object Create ()
		{
			return new PangoBackend {
				Layout = Pango.CairoHelper.CreateLayout (SharedContext)
			};
		}

		public override void SetText (object backend, string text)
		{
			var tl = (PangoBackend) backend;
			text = text ?? String.Empty;
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
			var tl = (PangoBackend) backend;
			tl.Layout.Width = (int) (value * Pango.Scale.PangoScale);
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

		public override void AddAttribute (object backend, TextAttribute attribute)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddAttribute (tl.TextIndexer, attribute);
		}

		public override void ClearAttributes (object backend)
		{
			var tl = (PangoBackend) backend;
			tl.ClearAttributes ();
		}

		public override int GetIndexFromCoordinates (object backend, double x, double y)
		{
			var tl = (PangoBackend) backend;
			int index, trailing;
			tl.Layout.XyToIndex ((int)x, (int)y, out index, out trailing);
			return tl.TextIndexer.ByteIndexToIndex (index);
		}

		public override Point GetCoordinateFromIndex (object backend, int index)
		{
			var tl = (PangoBackend) backend;
			var pos = tl.Layout.IndexToPos (tl.TextIndexer.IndexToByteIndex (index));
			return new Point (pos.X / Pango.Scale.PangoScale, pos.Y / Pango.Scale.PangoScale);
		}

		public override double GetBaseline (object backend)
		{
			var tl = (PangoBackend) backend;
			// Just get the first line
			using (var iter = tl.Layout.Iter)
				return Pango.Units.ToPixels (iter.Baseline);
		}

		public override void Dispose (object backend)
		{
			var tl = (IDisposable) backend;
			tl.Dispose ();
		}
	}
}
