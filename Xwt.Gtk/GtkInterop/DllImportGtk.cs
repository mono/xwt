using System;
using System.Runtime.InteropServices;
using Xwt.GtkBackend;

namespace Xwt.Interop {
	public static class DllImportGtk {

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr d_gdk_pixbuf_get_from_surface(IntPtr surface, int src_x, int src_y, int width, int height);

		public static d_gdk_pixbuf_get_from_surface gdk_pixbuf_get_from_surface = FuncLoader.LoadFunction<d_gdk_pixbuf_get_from_surface>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gdk), "gdk_pixbuf_get_from_surface"));

#else
		[DllImport (GtkInterop.LIBGDK, CallingConvention = CallingConvention.Cdecl)]
	    public static extern IntPtr gdk_pixbuf_get_from_surface (IntPtr surface, int src_x, int src_y, int width, int height);
#endif
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_font_selection_get_preview_entry(IntPtr raw);
		public static d_gtk_font_selection_get_preview_entry gtk_font_selection_get_preview_entry = FuncLoader.LoadFunction<d_gtk_font_selection_get_preview_entry>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_font_selection_get_preview_entry"));
		
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public private static extern IntPtr gtk_font_selection_get_preview_entry (IntPtr raw);
#endif	
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_font_chooser_dialog_new(IntPtr title, IntPtr parent);
		public static d_gtk_font_chooser_dialog_new gtk_font_chooser_dialog_new = FuncLoader.LoadFunction<d_gtk_font_chooser_dialog_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_font_chooser_dialog_new"));

#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_font_chooser_dialog_new(IntPtr title, IntPtr parent);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_font_chooser_widget_get_type();
		public static d_gtk_font_chooser_widget_get_type gtk_font_chooser_widget_get_type = FuncLoader.LoadFunction<d_gtk_font_chooser_widget_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_font_chooser_widget_get_type"));

#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr gtk_font_chooser_widget_get_type();
#endif		

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate bool d_gdk_event_get_scroll_deltas(IntPtr eventScroll, out double deltaX, out double deltaY);
		public static d_gdk_event_get_scroll_deltas  gdk_event_get_scroll_deltas = FuncLoader.LoadFunction<d_gdk_event_get_scroll_deltas>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gdk_event_get_scroll_deltas"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool gdk_event_get_scroll_deltas (IntPtr eventScroll, out double deltaX, out double deltaY);
#endif

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_scrolled_window_set_overlay_policy(IntPtr sw, Gtk.PolicyType hpolicy, Gtk.PolicyType vpolicy);
		public static d_gtk_scrolled_window_set_overlay_policy  gtk_scrolled_window_set_overlay_policy = FuncLoader.LoadFunction<d_gtk_scrolled_window_set_overlay_policy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_scrolled_window_set_overlay_policy"));
#else
		[DllImport (GtkInterop.LIBGTK)]
		public static extern void gtk_scrolled_window_set_overlay_policy (IntPtr sw, Gtk.PolicyType hpolicy, Gtk.PolicyType vpolicy);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_scrolled_window_get_overlay_policy(IntPtr sw, out Gtk.PolicyType hpolicy, out Gtk.PolicyType vpolicy);
		public static d_gtk_scrolled_window_get_overlay_policy  gtk_scrolled_window_get_overlay_policy = FuncLoader.LoadFunction<d_gtk_scrolled_window_get_overlay_policy>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_scrolled_window_get_overlay_policy"));
#else
		[DllImport (GtkInterop.LIBGTK)]
		static extern void gtk_scrolled_window_get_overlay_policy (IntPtr sw, out Gtk.PolicyType hpolicy, out Gtk.PolicyType vpolicy);
#endif
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate bool d_gtk_tree_view_get_tooltip_context(IntPtr raw, ref int x, ref int y, bool keyboard_tip, out IntPtr model, out IntPtr path, IntPtr iter);
		public static d_gtk_tree_view_get_tooltip_context  gtk_tree_view_get_tooltip_context = FuncLoader.LoadFunction<d_gtk_tree_view_get_tooltip_context>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_tree_view_get_tooltip_context"));

#else

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool gtk_tree_view_get_tooltip_context (IntPtr raw, ref int x, ref int y, bool keyboard_tip, out IntPtr model, out IntPtr path, IntPtr iter);
#endif
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_image_menu_item_set_always_show_image(IntPtr menuitem, bool alwaysShow);
		public static d_gtk_image_menu_item_set_always_show_image  gtk_image_menu_item_set_always_show_image = FuncLoader.LoadFunction<d_gtk_image_menu_item_set_always_show_image>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_image_menu_item_set_always_show_image"));

#else

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern void gtk_image_menu_item_set_always_show_image (IntPtr menuitem, bool alwaysShow);
#endif
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_icon_source_set_scale(IntPtr source, double scale);
		public static d_gtk_icon_source_set_scale  gtk_icon_source_set_scale = FuncLoader.LoadFunction<d_gtk_icon_source_set_scale>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_source_set_scale"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_icon_source_set_scale (IntPtr source, double scale);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_icon_source_set_scale_wildcarded(IntPtr source, bool setting);
		public static d_gtk_icon_source_set_scale_wildcarded  gtk_icon_source_set_scale_wildcarded = FuncLoader.LoadFunction<d_gtk_icon_source_set_scale_wildcarded>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_source_set_scale_wildcarded"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_icon_source_set_scale_wildcarded (IntPtr source, bool setting);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate double d_gtk_widget_get_scale_factor(IntPtr widget);
		public static d_gtk_widget_get_scale_factor gtk_widget_get_scale_factor = FuncLoader.LoadFunction<d_gtk_widget_get_scale_factor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_widget_get_scale_factor"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern double gtk_widget_get_scale_factor (IntPtr widget);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate double d_gdk_screen_get_monitor_scale_factor(IntPtr widget, int monitor);
		public static d_gdk_screen_get_monitor_scale_factor gdk_screen_get_monitor_scale_factor = FuncLoader.LoadFunction<d_gdk_screen_get_monitor_scale_factor>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gdk_screen_get_monitor_scale_factor"));
#else
		[DllImport (GtkInterop.LIBGDK, CallingConvention = CallingConvention.Cdecl)]
		static extern double gdk_screen_get_monitor_scale_factor (IntPtr widget, int monitor);
#endif

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_icon_set_render_icon_scaled(IntPtr handle, IntPtr style, int direction, int state, int size, IntPtr widget, IntPtr intPtr, ref double scale);
		public static d_gtk_icon_set_render_icon_scaled gtk_icon_set_render_icon_scaled = FuncLoader.LoadFunction<d_gtk_icon_set_render_icon_scaled>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_icon_set_render_icon_scaled"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_icon_set_render_icon_scaled (IntPtr handle, IntPtr style, int direction, int state, int size, IntPtr widget, IntPtr intPtr, ref double scale);
#endif	
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_message_dialog_get_message_area(IntPtr raw);
		public static d_gtk_message_dialog_get_message_area gtk_message_dialog_get_message_area = FuncLoader.LoadFunction<d_gtk_message_dialog_get_message_area>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_message_dialog_get_message_area"));
#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr gtk_message_dialog_get_message_area(IntPtr raw);
#endif


#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_binding_set_find(string setName);
		public static d_gtk_binding_set_find gtk_binding_set_find = FuncLoader.LoadFunction<d_gtk_binding_set_find>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_binding_set_find"));
#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_binding_set_find (string setName);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_binding_entry_remove(IntPtr bindingSet, uint keyval, Gdk.ModifierType modifiers);
		public static d_gtk_binding_entry_remove gtk_binding_entry_remove = FuncLoader.LoadFunction<d_gtk_binding_entry_remove>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_binding_entry_remove"));
#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_binding_entry_remove (IntPtr bindingSet, uint keyval, Gdk.ModifierType modifiers);
#endif		

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_object_set_data(IntPtr raw, IntPtr key, IntPtr data);
		public static d_gtk_object_set_data gtk_object_set_data = FuncLoader.LoadFunction<d_gtk_object_set_data>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_object_set_data"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern void gtk_object_set_data (IntPtr raw, IntPtr key, IntPtr data);
#endif
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gdk_x11_drawable_get_xid(IntPtr window);
		public static d_gdk_x11_drawable_get_xid gdk_x11_drawable_get_xid = FuncLoader.LoadFunction<d_gdk_x11_drawable_get_xid>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gdk_x11_drawable_get_xid"));
#else
		[DllImport (GtkInterop.LIBGTK)]
		public static extern IntPtr gdk_x11_drawable_get_xid (IntPtr window);
#endif
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate bool d_gtk_selection_data_set_uris(IntPtr raw, IntPtr[] uris);
		public static d_gtk_selection_data_set_uris gtk_selection_data_set_uris = FuncLoader.LoadFunction<d_gtk_selection_data_set_uris>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_selection_data_set_uris"));
#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public  static extern bool gtk_selection_data_set_uris(IntPtr raw, IntPtr[] uris);
#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_selection_data_get_uris(IntPtr raw);
		public static d_gtk_selection_data_get_uris gtk_selection_data_get_uris = FuncLoader.LoadFunction<d_gtk_selection_data_get_uris>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_selection_data_get_uris"));

#else
		[DllImport(GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public  static extern IntPtr gtk_selection_data_get_uris(IntPtr raw);
#endif		
		
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_label_set_attributes(IntPtr label, IntPtr attrList);
		public static d_gtk_label_set_attributes gtk_label_set_attributes = FuncLoader.LoadFunction<d_gtk_label_set_attributes>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_label_set_attributes"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public extern void gtk_label_set_attributes (IntPtr label, IntPtr attrList);
		
#endif		

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_spinner_new();
		public static d_gtk_spinner_new  gtk_spinner_new = FuncLoader.LoadFunction<d_gtk_spinner_new>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_spinner_new"));
#else
		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr gtk_spinner_new();
#endif		

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_spinner_start(IntPtr raw);
		public static d_gtk_spinner_start gtk_spinner_start = FuncLoader.LoadFunction<d_gtk_spinner_start>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_spinner_start"));

#else

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern void gtk_spinner_start(IntPtr raw);
#endif

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate void d_gtk_spinner_stop(IntPtr raw);
		public static d_gtk_spinner_stop gtk_spinner_stop = FuncLoader.LoadFunction<d_gtk_spinner_stop>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_spinner_stop"));

#else

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		public static extern void gtk_spinner_stop(IntPtr raw);
#endif		

#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_gtk_spinner_get_type();
		public static d_gtk_spinner_get_type gtk_spinner_get_type = FuncLoader.LoadFunction<d_gtk_spinner_get_type>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Gtk), "gtk_spinner_get_type"));

#else

		[DllImport(GtkInterop.LIBGTK)]
		public static extern IntPtr gtk_spinner_get_type();
#endif		
	}
	
}