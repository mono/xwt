using System;
using System.Runtime.InteropServices;

namespace Xwt.Interop {
    public static class DllImportGObj {
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_g_object_get_data(IntPtr source, string name);
        public static d_g_object_get_data g_object_get_data = FuncLoader.LoadFunction<d_g_object_get_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "g_object_get_data"));
#else
		[DllImport (GtkInterop.LIBGOBJECT, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr g_object_get_data (IntPtr source, string name);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        public delegate IntPtr d_g_signal_stop_emission_by_name(IntPtr raw, string name);
        public static d_g_signal_stop_emission_by_name g_signal_stop_emission_by_name = FuncLoader.LoadFunction<d_g_signal_stop_emission_by_name>(FuncLoader.GetProcAddress(GLibrary.Load(Library.GObject), "g_signal_stop_emission_by_name"));
#else
		[DllImport(GtkInterop.LIBGOBJECT, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr g_signal_stop_emission_by_name(IntPtr raw, string name);
#endif        
    }
}