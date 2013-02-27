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
			return Pango.CairoHelper.CreateLayout (SharedContext);
		}

		public override void SetText (object backend, string text)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			tl.SetText (text);
		}

		public override void SetFont (object backend, Xwt.Drawing.Font font)
		{
			Pango.Layout tl = (Pango.Layout)backend;
			tl.FontDescription = (Pango.FontDescription)Toolkit.GetBackend (font);
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
			Pango.Layout tl = (Pango.Layout)backend;
			if (textTrimming == TextTrimming.WordElipsis)
				tl.Ellipsize = Pango.EllipsizeMode.End;
			if (textTrimming == TextTrimming.Word)
				tl.Ellipsize = Pango.EllipsizeMode.None;
			
		}
		
		public override Size GetSize (object backend)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			int w, h;
			tl.GetPixelSize (out w, out h);
			return new Size ((double)w, (double)h);
		}
		
		public override void SetTextAttributes(object backend, IEnumerable<TextAttribute> textAttributes)
		{
			Pango.Layout tl = (Pango.Layout) backend;
			var attrList = new FastPangoAttrList ();
			// TODO: UTF-8 coordinate transformation.
			foreach (var textAttr in textAttributes) {
				var fg = textAttr as TextAttributeForeground;
				if (fg != null) {
					attrList.AddForegroundAttribute (fg.Color.ToGdkColor (), fg.StartIndex, fg.EndIndex);
					continue;
				}
				var bg = textAttr as TextAttributeBackground;
				if (bg != null) {
					attrList.AddBackgroundAttribute (bg.Color.ToGdkColor (), bg.StartIndex, bg.EndIndex);
					continue;
				}

				var style = textAttr as TextAttributeStyle;
				if (style != null) {
					attrList.AddStyleAttribute (ConvertStyle (style.TextStyle), style.StartIndex, style.EndIndex);
					continue;
				}

				var weight = textAttr as TextAttributeWeight;
				if (weight != null) {
					attrList.AddWeightAttribute (ConvertWeight (weight.TextWeight), weight.StartIndex, weight.EndIndex);
					continue;
				}

				var decoration = textAttr as TextAttributeDecoration;
				if (decoration != null) {
					if (decoration.TextDecoration.HasFlag (TextDecoration.Underline))
						attrList.AddUnderlineAttribute (Pango.Underline.Single, decoration.StartIndex, decoration.EndIndex);
					if (decoration.TextDecoration.HasFlag (TextDecoration.Strikethrough))
						attrList.AddStrikethroughAttribute (true, decoration.StartIndex, decoration.EndIndex);
					continue;
				}

			}
			attrList.AssignTo (tl);
		}

		static Pango.Style ConvertStyle (TextStyle textStyle)
		{
			switch (textStyle) {
			case TextStyle.Normal:
				return Pango.Style.Normal;
			case TextStyle.Italic:
				return Pango.Style.Italic;
			case TextStyle.Oblique:
				return Pango.Style.Oblique;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		static Pango.Weight ConvertWeight (TextWeight textWeight)
		{
			switch (textWeight) {
			case TextWeight.Normal:
				return Pango.Weight.Normal;
			case TextWeight.Bold:
				return Pango.Weight.Bold;
			case TextWeight.Heavy:
				return Pango.Weight.Heavy;
			case TextWeight.Light:
				return Pango.Weight.Light;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		/// <summary>
		/// This creates a Pango list and applies attributes to it with *much* less overhead than the GTK# version.
		/// </summary>
		class FastPangoAttrList : IDisposable
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
				if (list != IntPtr.Zero) {
					GC.SuppressFinalize (this);
					Destroy ();
				}
			}
			
			//NOTE: the list destroys all its attributes when the ref count reaches zero
			void Destroy ()
			{
				pango_attr_list_unref (list);
				list = IntPtr.Zero;
			}
			
			~FastPangoAttrList ()
			{
				GLib.Idle.Add (delegate {
					Destroy ();
					return false;
				});
			}
		}
	}
}

