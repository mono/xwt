using System;
using System.Runtime.InteropServices;
using Xwt.GtkBackend;

namespace Xwt.Interop {
    public static class DllImportWebkit {
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_network_request_new(IntPtr uri);

        public static d_webkit_network_request_new webkit_network_request_new = FuncLoader.LoadFunction<d_webkit_network_request_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_network_request_new"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_network_request_new(IntPtr uri);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_network_request_get_type();

        public static d_webkit_network_request_get_type webkit_network_request_get_type = FuncLoader.LoadFunction<d_webkit_network_request_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_network_request_get_type"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_network_request_get_type();
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_network_request_get_uri(IntPtr raw);

        public static d_webkit_network_request_get_uri webkit_network_request_get_uri = FuncLoader.LoadFunction<d_webkit_network_request_get_uri>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_network_request_get_uri"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_network_request_get_uri(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_network_request_set_uri(IntPtr raw, IntPtr uri);

        public static d_webkit_network_request_set_uri webkit_network_request_set_uri = FuncLoader.LoadFunction<d_webkit_network_request_set_uri>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_network_request_set_uri"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_network_request_set_uri(IntPtr raw, IntPtr uri);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_web_view_new();

        public static d_webkit_web_view_new webkit_web_view_new = FuncLoader.LoadFunction<d_webkit_web_view_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_new"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_web_view_new();
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_web_view_get_type();

        public static d_webkit_web_view_get_type webkit_web_view_get_type = FuncLoader.LoadFunction<d_webkit_web_view_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_get_type"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_web_view_get_type();
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_load_uri(IntPtr raw, IntPtr uri);

        public static d_webkit_web_view_load_uri webkit_web_view_load_uri = FuncLoader.LoadFunction<d_webkit_web_view_load_uri>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_load_uri"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_load_uri(IntPtr raw, IntPtr uri);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_web_view_get_uri(IntPtr raw);

        public static d_webkit_web_view_get_uri webkit_web_view_get_uri = FuncLoader.LoadFunction<d_webkit_web_view_get_uri>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_get_uri"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_web_view_get_uri(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_webkit_web_view_get_full_content_zoom(IntPtr raw);

        public static d_webkit_web_view_get_full_content_zoom webkit_web_view_get_full_content_zoom = FuncLoader.LoadFunction<d_webkit_web_view_get_full_content_zoom>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_get_full_content_zoom"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern bool webkit_web_view_get_full_content_zoom(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_set_full_content_zoom(IntPtr raw, bool full_content_zoom);

        public static d_webkit_web_view_set_full_content_zoom webkit_web_view_set_full_content_zoom = FuncLoader.LoadFunction<d_webkit_web_view_set_full_content_zoom>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_set_full_content_zoom"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_set_full_content_zoom(IntPtr raw, bool full_content_zoom);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_stop_loading(IntPtr raw);

        public static d_webkit_web_view_stop_loading webkit_web_view_stop_loading = FuncLoader.LoadFunction<d_webkit_web_view_stop_loading>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_stop_loading"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_stop_loading(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_reload(IntPtr raw);

        public static d_webkit_web_view_reload webkit_web_view_reload = FuncLoader.LoadFunction<d_webkit_web_view_reload>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_reload"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_reload(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_webkit_web_view_can_go_back(IntPtr raw);

        public static d_webkit_web_view_can_go_back webkit_web_view_can_go_back = FuncLoader.LoadFunction<d_webkit_web_view_can_go_back>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_can_go_back"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern bool webkit_web_view_can_go_back(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_go_back(IntPtr raw);

        public static d_webkit_web_view_go_back webkit_web_view_go_back = FuncLoader.LoadFunction<d_webkit_web_view_go_back>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_go_back"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_go_back(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_webkit_web_view_can_go_forward(IntPtr raw);

        public static d_webkit_web_view_can_go_forward webkit_web_view_can_go_forward = FuncLoader.LoadFunction<d_webkit_web_view_can_go_forward>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_can_go_forward"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern bool webkit_web_view_can_go_forward(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_go_forward(IntPtr raw);

        public static d_webkit_web_view_go_forward webkit_web_view_go_forward = FuncLoader.LoadFunction<d_webkit_web_view_go_forward>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_go_forward"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_go_forward(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_webkit_web_view_load_string(IntPtr raw, IntPtr content, IntPtr mime_type, IntPtr encoding, IntPtr base_uri);

        public static d_webkit_web_view_load_string webkit_web_view_load_string = FuncLoader.LoadFunction<d_webkit_web_view_load_string>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_load_string"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern void webkit_web_view_load_string (IntPtr raw, IntPtr content, IntPtr mime_type, IntPtr encoding, IntPtr base_uri);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_webkit_web_view_get_title(IntPtr raw);

        public static d_webkit_web_view_get_title webkit_web_view_get_title = FuncLoader.LoadFunction<d_webkit_web_view_get_title>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_get_title"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern IntPtr webkit_web_view_get_title(IntPtr raw);
#endif
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate double d_webkit_web_view_get_progress(IntPtr raw);

        public static d_webkit_web_view_get_progress webkit_web_view_get_progress = FuncLoader.LoadFunction<d_webkit_web_view_get_progress>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Webkit), "webkit_web_view_get_progress"));
#else
        [DllImport (GtkInterop.LIBWEBKIT)]
        public static extern double webkit_web_view_get_progress(IntPtr raw);
#endif
    }
}