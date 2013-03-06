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
					if (hasUnassignedAttributes)
						attributes.AssignTo (layout);
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
					if (attributes != null) {
						attributes.Dispose ();
						attributes = null;
					}
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

			public void Dispose ()
			{
				Layout.Dispose ();
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
		
		public override object Create (Context context)
		{
			CairoContextBackend c = (CairoContextBackend) Toolkit.GetBackend (context);
			return new PangoBackend {
				Layout = Pango.CairoHelper.CreateLayout (c.Context)
			};
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

		public override void SetForeground (object backend, Color color, int startIndex, int count)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddForegroundAttribute (color.ToGdkColor (), (uint)tl.IndexToByteIndex (startIndex), (uint)tl.IndexToByteIndex (startIndex + count));
		}
		
		public override void SetBackgound (object backend, Color color, int startIndex, int count)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddBackgroundAttribute (color.ToGdkColor (), (uint)tl.IndexToByteIndex (startIndex), (uint)tl.IndexToByteIndex (startIndex + count));
		}
		
		public override void SetFontWeight (object backend, FontWeight weight, int startIndex, int count)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddWeightAttribute ((Pango.Weight)(int)weight, (uint)tl.IndexToByteIndex (startIndex), (uint)tl.IndexToByteIndex (startIndex + count));
		}
		
		public override void SetFontStyle (object backend, FontStyle style, int startIndex, int count)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddStyleAttribute ((Pango.Style)(int)style, (uint)tl.IndexToByteIndex (startIndex), (uint)tl.IndexToByteIndex (startIndex + count));
		}
		
		public override void SetUnderline (object backend, int startIndex, int count)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddUnderlineAttribute (Pango.Underline.Single, (uint)tl.IndexToByteIndex (startIndex), (uint)tl.IndexToByteIndex (startIndex + count));
		}
		
		public override void SetStrikethrough (object backend, int startIndex, int count)
		{
			var tl = (PangoBackend) backend;
			tl.Attributes.AddStrikethroughAttribute (true, (uint)tl.IndexToByteIndex (startIndex), (uint)tl.IndexToByteIndex (startIndex + count));
		}

		/// <summary>
		/// This creates a Pango list and applies attributes to it with *much* less overhead than the GTK# version.
		/// </summary>
		internal class FastPangoAttrList : IDisposable
		{
			IntPtr list;
			
			public FastPangoAttrList ()
			{
				list = pango_attr_list_new ();
			}
			
			public void AddStyleAttribute (Pango.Style style, uint start, uint end)
			{
				Add (pango_attr_style_new (style), start, end);
			}
			
			public void AddWeightAttribute (Pango.Weight weight, uint start, uint end)
			{
				Add (pango_attr_weight_new (weight), start, end);
			}
			
			public void AddForegroundAttribute (Gdk.Color color, uint start, uint end)
			{
				Add (pango_attr_foreground_new (color.Red, color.Green, color.Blue), start, end);
			}
			
			public void AddBackgroundAttribute (Gdk.Color color, uint start, uint end)
			{
				Add (pango_attr_background_new (color.Red, color.Green, color.Blue), start, end);
			}
			
			public void AddUnderlineAttribute (Pango.Underline underline, uint start, uint end)
			{
				Add (pango_attr_underline_new (underline), start, end);
			}
			
			public void AddStrikethroughAttribute (bool strikethrough, uint start, uint end)
			{
				Add (pango_attr_strikethrough_new (strikethrough), start, end);
			}

			void Add (IntPtr attribute, uint start, uint end)
			{
				unsafe {
					PangoAttribute *attPtr = (PangoAttribute *) attribute;
					attPtr->start_index = start;
					attPtr->end_index = end;
				}
				pango_attr_list_insert (list, attribute);
			}
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_style_new (Pango.Style style);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_stretch_new (Pango.Stretch stretch);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_weight_new (Pango.Weight weight);

			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_foreground_new (ushort red, ushort green, ushort blue);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_background_new (ushort red, ushort green, ushort blue);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_underline_new (Pango.Underline underline);

			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_strikethrough_new (bool strikethrough);

			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern IntPtr pango_attr_list_new ();
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern void pango_attr_list_unref (IntPtr list);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern void pango_attr_list_insert (IntPtr list, IntPtr attr);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern void pango_layout_set_attributes (IntPtr layout, IntPtr attrList);
			
			[DllImport (PangoUtil.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
			static extern void pango_attr_list_splice (IntPtr attr_list, IntPtr other, Int32 pos, Int32 len);
			
			public void Splice (Pango.AttrList attrs, int pos, int len)
			{
				pango_attr_list_splice (list, attrs.Handle, pos, len);
			}
			
			public void AssignTo (Pango.Layout layout)
			{
				pango_layout_set_attributes (layout.Handle, list);
			}
			
			[StructLayout (LayoutKind.Sequential)]
			struct PangoAttribute
			{
				public IntPtr klass;
				public uint start_index;
				public uint end_index;
			}
			
			public void Dispose ()
			{
				pango_attr_list_unref (list);
				list = IntPtr.Zero;
			}
		}

		public override int GetIndexFromCoordinates (object backend, double x, double y)
		{
			var tl = (PangoBackend) backend;
			int index, trailing;
			tl.Layout.XyToIndex ((int)x, (int)y, out index, out trailing);
			return tl.ByteIndexToIndex (index);
		}

		public override Point GetCoordinateFromIndex (object backend, int index)
		{
			var tl = (PangoBackend) backend;
			var pos = tl.Layout.IndexToPos (tl.IndexToByteIndex (index));
			return new Point (pos.X, pos.Y);
		}

		public override void DisposeBackend (object backend)
		{
			var tl = (IDisposable) backend;
			tl.Dispose ();
		}
	}
}
