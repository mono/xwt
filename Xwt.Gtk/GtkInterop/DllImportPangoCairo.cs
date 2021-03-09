using System.Runtime.InteropServices;

namespace Xwt.Interop {
    public static class DllImportPangoCairo {
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate void d_pango_cairo_font_map_set_default(System.IntPtr fontmap);
        public static d_pango_cairo_font_map_set_default pango_cairo_font_map_set_default = FuncLoader.LoadFunction<d_pango_cairo_font_map_set_default>(FuncLoader.GetProcAddress(GLibrary.Load(Library.PangoCairo), "pango_cairo_font_map_set_default"));

#else		
		[System.Runtime.InteropServices.DllImport (GtkInterop.LIBPANGOCAIRO)]
		public static extern void pango_cairo_font_map_set_default (System.IntPtr fontmap);
#endif   
		
	
    }
}