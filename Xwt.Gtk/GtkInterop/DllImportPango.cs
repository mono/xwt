using System;
using System.Runtime.InteropServices;

namespace Xwt.Interop {
    public static class DllImportPango {
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_style_new(Pango.Style style);
        public static d_pango_attr_style_new pango_attr_style_new = FuncLoader.LoadFunction<d_pango_attr_style_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_style_new"));

#else		
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_style_new (Pango.Style style);
#endif

#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_stretch_new(Pango.Stretch stretch);
        public static d_pango_attr_stretch_new pango_attr_stretch_new = FuncLoader.LoadFunction<d_pango_attr_stretch_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_stretch_new"));

#else			
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_stretch_new (Pango.Stretch stretch);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_weight_new(Pango.Weight weight);
        public static d_pango_attr_weight_new pango_attr_weight_new = FuncLoader.LoadFunction<d_pango_attr_weight_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_weight_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_weight_new (Pango.Weight weight);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_foreground_new(ushort red, ushort green, ushort blue);
        public static d_pango_attr_foreground_new pango_attr_foreground_new = FuncLoader.LoadFunction<d_pango_attr_foreground_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_foreground_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_foreground_new (ushort red, ushort green, ushort blue);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_background_new(ushort red, ushort green, ushort blue);
        public static d_pango_attr_background_new pango_attr_background_new = FuncLoader.LoadFunction<d_pango_attr_background_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_background_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_background_new (ushort red, ushort green, ushort blue);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_underline_new(Pango.Underline underline);
        public static d_pango_attr_underline_new pango_attr_underline_new = FuncLoader.LoadFunction<d_pango_attr_underline_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_underline_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_underline_new (Pango.Underline underline);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_strikethrough_new(bool strikethrough);
        public static d_pango_attr_strikethrough_new pango_attr_strikethrough_new = FuncLoader.LoadFunction<d_pango_attr_strikethrough_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_strikethrough_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_strikethrough_new (bool strikethrough);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_font_desc_new(IntPtr desc);
        public static d_pango_attr_font_desc_new pango_attr_font_desc_new = FuncLoader.LoadFunction<d_pango_attr_font_desc_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_font_desc_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_font_desc_new (IntPtr desc);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_list_new();
        public static d_pango_attr_list_new pango_attr_list_new = FuncLoader.LoadFunction<d_pango_attr_list_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_list_new"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_list_new ();
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_list_unref(IntPtr list);
        public static d_pango_attr_list_unref pango_attr_list_unref = FuncLoader.LoadFunction<d_pango_attr_list_unref>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_list_unref"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern void pango_attr_list_unref (IntPtr list);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_list_insert(IntPtr list, IntPtr attr);
        public static d_pango_attr_list_insert pango_attr_list_insert = FuncLoader.LoadFunction<d_pango_attr_list_insert>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_list_insert"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern void pango_attr_list_insert (IntPtr list, IntPtr attr);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_layout_set_attributes(IntPtr layout, IntPtr attrList);
        public static d_pango_layout_set_attributes pango_layout_set_attributes = FuncLoader.LoadFunction<d_pango_layout_set_attributes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_layout_set_attributes"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern void pango_layout_set_attributes (IntPtr layout, IntPtr attrList);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_list_splice(IntPtr attr_list, IntPtr other, Int32 pos, Int32 len);
        public static d_pango_attr_list_splice pango_attr_list_splice = FuncLoader.LoadFunction<d_pango_attr_list_splice>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_list_splice"));

#else	
		[DllImport (GtkInterop.LIBPANGO, CallingConvention=CallingConvention.Cdecl)]
		public static extern void pango_attr_list_splice (IntPtr attr_list, IntPtr other, Int32 pos, Int32 len);
#endif

#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_pango_attr_size_new_absolute(int size);
        public static d_pango_attr_size_new_absolute pango_attr_size_new_absolute = FuncLoader.LoadFunction<d_pango_attr_size_new_absolute>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_size_new_absolute"));

#else			
		[DllImport (GtkInterop.LIBPANGO, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr pango_attr_size_new_absolute (int size);
#endif

#if XWT_GTKSHARP3
	    [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	    public delegate IntPtr d_pango_attribute_copy(IntPtr raw);
	    public static d_pango_attribute_copy pango_attribute_copy = FuncLoader.LoadFunction<d_pango_attribute_copy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attribute_copy"));
#else
		[DllImport (GtkInterop.LIBPANGO, CallingConvention = CallingConvention.Cdecl)]
		public  static extern IntPtr pango_attribute_copy (IntPtr raw);
#endif
#if XWT_GTKSHARP3
	    [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	    public delegate IntPtr d_pango_attr_iterator_get(IntPtr raw, int type);
	    public static d_pango_attr_iterator_get pango_attr_iterator_get = FuncLoader.LoadFunction<d_pango_attr_iterator_get>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Pango), "pango_attr_iterator_get"));
#else
		[DllImport (GtkInterop.LIBPANGO, CallingConvention = CallingConvention.Cdecl)]
		public  static extern IntPtr pango_attr_iterator_get (IntPtr raw, int type);
#endif		
    }
}