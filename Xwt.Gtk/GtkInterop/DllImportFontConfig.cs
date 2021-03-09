using System.Runtime.InteropServices;

namespace Xwt.Interop {
    public static class DllImportFontConfig {
#if XWT_GTKSHARP3
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_FcConfigAppFontAddFile(System.IntPtr config, string fontPath);

        public static d_FcConfigAppFontAddFile FcConfigAppFontAddFile = FuncLoader.LoadFunction<d_FcConfigAppFontAddFile>(FuncLoader.GetProcAddress(GLibrary.Load(Library.Fontconfig), "FcConfigAppFontAddFile"));
#else
		[System.Runtime.InteropServices.DllImport (GtkInterop.LIBFONTCONFIG)]
		public static extern bool FcConfigAppFontAddFile (System.IntPtr config, string fontPath);
#endif
    }
}