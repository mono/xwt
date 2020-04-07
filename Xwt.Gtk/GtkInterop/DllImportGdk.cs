using System;
using System.Runtime.InteropServices;

namespace Xwt.Interop {
	public static class DllImportGdk {
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_gdk_pixbuf_get_from_window(IntPtr win, int src_x, int src_y, int width, int height);
        public static d_gdk_pixbuf_get_from_window gdk_pixbuf_get_from_window = FuncLoader.LoadFunction<d_gdk_pixbuf_get_from_window>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_pixbuf_get_from_window"));

#else		
		[DllImport (GtkInterop.LIBGDK)]
		public static extern IntPtr gdk_pixbuf_get_from_window(IntPtr win, int src_x, int src_y, int width, int height);
#endif
        
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate bool d_gdk_keymap_add_virtual_modifiers(IntPtr keymap, ref Gdk.ModifierType state);
        public static d_gdk_keymap_add_virtual_modifiers gdk_keymap_add_virtual_modifiers = FuncLoader.LoadFunction<d_gdk_keymap_add_virtual_modifiers>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_keymap_add_virtual_modifiers"));
#else		
		//introduced in GTK 2.20
		[DllImport (GtkInterop.LIBGDK, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool gdk_keymap_add_virtual_modifiers (IntPtr keymap, ref Gdk.ModifierType state);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_gdk_quartz_set_fix_modifiers(bool fix);
        public static d_gdk_quartz_set_fix_modifiers gdk_quartz_set_fix_modifiers = FuncLoader.LoadFunction<d_gdk_quartz_set_fix_modifiers>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_quartz_set_fix_modifiers"));
#else		
		//Custom patch in Mono Mac w/GTK+ 2.24.8+
		[DllImport (GtkInterop.LIBGDK, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool gdk_quartz_set_fix_modifiers (bool fix);
#endif        
    }
}