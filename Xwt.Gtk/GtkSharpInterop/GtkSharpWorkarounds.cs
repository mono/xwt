using System;
using System.Runtime.InteropServices;
using Gdk;
using Gtk;
using Xwt.CairoBackend;

namespace Xwt.GtkBackend {
    public static class GtkSharpWorkarounds {
        
        public static Color ToGdkColor(this Gdk.RGBA color)=>new Color((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));

        static void TestGetting_abi_info(Gtk.Style it, Gtk.StateType state) {
            unsafe {
                
                var test = "black";
                var raw_ptr1 = it.Handle + (int)Gtk.Style.abi_info.GetFieldOffset(test);
                var z1 =  Marshal.PtrToStructure(raw_ptr1, typeof(Gdk.Color));  
                var raw_ptr2 = (IntPtr*)(((byte*)it.Handle) + Gtk.Style.abi_info.GetFieldOffset(test));
                var z2 =  Gdk.Color.New ((*raw_ptr2));
                var z3 =   Marshal.PtrToStructure((*raw_ptr2), typeof(Gdk.Color));  ;
                
                var bg = "bg";
                var ptr_bg = it.Handle + (int)Gtk.Style.abi_info.GetFieldOffset(bg);
                // seems to work:
                var bgRes =  Marshal.PtrToStructure<Gtk.Style.GtkStyle_bgAlign>(ptr_bg);
                
                // crash:
                if (false) {
                    var ptr_bg1 = (IntPtr*) (((byte*) it.Handle) + Gtk.Style.abi_info.GetFieldOffset(bg));
                    var bgRes1 = Marshal.PtrToStructure(*ptr_bg1, typeof(Gtk.Style.GtkStyle_bgAlign));
                }
            }
        }
        public static Gdk.Color Background(this Gtk.Style it, Gtk.StateType state) {
            var ctx = it.Context;
            var color = ctx.GetBackgroundColor(state.ToGtk3StateFlags());
            var col = color.ToGdkColor();

            if (false) {
                TestGetting_abi_info(it, state);
            }

            return col;
        }
        
        public static Gdk.Color Dark(this Gtk.Style it, Gtk.StateType state)
        {
            // TODO
            return it.Foreground(state);
            
        }
        
        public static Gdk.Color Foreground(this Gtk.Style it, Gtk.StateType state)
        {
            var ctx = it.Context;
            var color = ctx.GetColor(state.ToGtk3StateFlags());
            return color.ToGdkColor();
        }
        
        public static Gdk.Color Base(this Gtk.Style it, Gtk.StateType state)
        {
           // TODO
           return it.Foreground(state);
        }

        public static void ModifyBase(this Gtk.Widget it, Gtk.StateType state, Gdk.Color color) {
            // TODO
        }

        /*
Pattern:

#endif
#if XWT_GTKSHARP3
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate IntPtr d_funcname(IntPtr raw, ...);
		public static d_funcname funcname = FuncLoader.LoadFunction<d_funcname>(FuncLoader.GetProcAddress(GLibrary.Load(Library.*), "funcname"));
#else

         
         
         */
    
    }
}