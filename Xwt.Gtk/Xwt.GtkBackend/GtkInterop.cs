//
// PangoUtils.cs
//
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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
using Gtk;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Xwt.Drawing;

namespace Xwt.GtkBackend
{
	public static class GtkInterop
	{
		internal const string LIBATK          = "libatk-1.0-0.dll";
		internal const string LIBGLIB         = "libglib-2.0-0.dll";
		internal const string LIBGOBJECT      = "libgobject-2.0-0.dll";
		internal const string LIBPANGO        = "libpango-1.0-0.dll";
		internal const string LIBPANGOCAIRO   = "libpangocairo-1.0-0.dll";
		internal const string LIBFONTCONFIG   = "fontconfig";

		#if XWT_GTK3
		internal const string LIBGTK          = "libgtk-3-0.dll";
		internal const string LIBGDK          = "libgdk-3-0.dll";
		internal const string LIBGTKGLUE      = "gtksharpglue-3";
		internal const string LIBGLIBGLUE     = "glibsharpglue-3";
		internal const string LIBWEBKIT       = "libwebkitgtk-3.0-0.dll";
		#else
		internal const string LIBGTK          = "libgtk-win32-2.0-0.dll";
		internal const string LIBGDK          = "libgdk-win32-2.0-0.dll";
		internal const string LIBGTKGLUE      = "gtksharpglue-2";
		internal const string LIBGLIBGLUE     = "glibsharpglue-2";
		internal const string LIBWEBKIT       = "libwebkitgtk-1.0-0.dll";
		#endif
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

		public void AddAttributes (TextIndexer indexer, IEnumerable<TextAttribute> attrs)
		{
			foreach (var attr in attrs)
				AddAttribute (indexer, attr);
		}

		public void AddAttribute (TextIndexer indexer, TextAttribute attr)
		{
			var start = (uint) indexer.IndexToByteIndex (attr.StartIndex);
			var end = (uint) indexer.IndexToByteIndex (attr.StartIndex + attr.Count);

			if (attr is BackgroundTextAttribute) {
				var xa = (BackgroundTextAttribute)attr;
				AddBackgroundAttribute (xa.Color.ToGtkValue (), start, end);
			}
			else if (attr is ColorTextAttribute) {
				var xa = (ColorTextAttribute)attr;
				AddForegroundAttribute (xa.Color.ToGtkValue (), start, end);
			}
			else if (attr is FontWeightTextAttribute) {
				var xa = (FontWeightTextAttribute)attr;
				AddWeightAttribute ((Pango.Weight)(int)xa.Weight, start, end);
			}
			else if (attr is FontStyleTextAttribute) {
				var xa = (FontStyleTextAttribute)attr;
				AddStyleAttribute ((Pango.Style)(int)xa.Style, start, end);
			}
			else if (attr is UnderlineTextAttribute) {
				var xa = (UnderlineTextAttribute)attr;
				AddUnderlineAttribute (xa.Underline ? Pango.Underline.Single : Pango.Underline.None, start, end);
			}
			else if (attr is StrikethroughTextAttribute) {
				var xa = (StrikethroughTextAttribute)attr;
				AddStrikethroughAttribute (xa.Strikethrough, start, end);
			}
			else if (attr is FontTextAttribute) {
				var xa = (FontTextAttribute)attr;
				AddFontAttribute ((Pango.FontDescription)Toolkit.GetBackend (xa.Font), start, end);
			}
			else if (attr is LinkTextAttribute) {
				AddUnderlineAttribute (Pango.Underline.Single, start, end);
				AddForegroundAttribute (Colors.Blue.ToGtkValue (), start, end);
			}
		}

		public IntPtr Handle {
			get { return list; }
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

		public void AddFontAttribute (Pango.FontDescription font, uint start, uint end)
		{
			Add (pango_attr_font_desc_new (font.Handle), start, end);
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

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_style_new (Pango.Style style);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_stretch_new (Pango.Stretch stretch);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_weight_new (Pango.Weight weight);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_foreground_new (ushort red, ushort green, ushort blue);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_background_new (ushort red, ushort green, ushort blue);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_underline_new (Pango.Underline underline);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_strikethrough_new (bool strikethrough);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_font_desc_new (IntPtr desc);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern IntPtr pango_attr_list_new ();

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern void pango_attr_list_unref (IntPtr list);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern void pango_attr_list_insert (IntPtr list, IntPtr attr);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		static extern void pango_layout_set_attributes (IntPtr layout, IntPtr attrList);

		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
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

	internal class TextIndexer
	{
		int[] indexToByteIndex;
		int[] byteIndexToIndex;

		public TextIndexer (string text)
		{
			SetupTables (text);
		}

		public int IndexToByteIndex (int i)
		{
			if (i >= indexToByteIndex.Length)
				return i;
			return indexToByteIndex[i];
		}

		public int ByteIndexToIndex (int i)
		{
			return byteIndexToIndex[i];
		}

		public void SetupTables (string text)
		{
			if (text == null) {
				this.indexToByteIndex = new int[0];
				this.byteIndexToIndex = new int[0];
				return;
			}

			var arr = text.ToCharArray ();
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
}
