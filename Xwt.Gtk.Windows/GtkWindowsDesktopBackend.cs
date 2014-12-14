using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt.GtkBackend;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Xwt.Gtk.Windows
{
	class GtkWindowsDesktopBackend: GtkDesktopBackend
	{
		Dictionary<string, Gdk.Pixbuf> icons = new Dictionary<string, Gdk.Pixbuf> ();

		public override object GetFileIcon (string filename)
		{
			var normal = GetIcon (filename, 0);
			if (normal == null)
				return null;

			var frames = new List<Gdk.Pixbuf> ();
			frames.Add (normal);

			var small = GetIcon (filename, Win32.SHGFI_SMALLICON);
			if (small != null && !frames.Contains (small))
				frames.Add (small);

			var shell = GetIcon (filename, Win32.SHGFI_SHELLICONSIZE);
			if (shell != null && !frames.Contains (shell))
				frames.Add (shell);

			var large = GetIcon (filename, Win32.SHGFI_LARGEICON);
			if (large != null && !frames.Contains (large))
				frames.Add (large);

			return new GtkImage (frames);
		}

		Gdk.Pixbuf GetIcon (string filename, uint size)
		{
			var shinfo = new Win32.SHFILEINFO ();
			Win32.SHGetFileInfo (filename, Win32.FILE_ATTRIBUTES_NORMAL, ref shinfo, (uint)Marshal.SizeOf (shinfo), Win32.SHGFI_USEFILEATTRIBUTES | Win32.SHGFI_ICON | Win32.SHGFI_ICONLOCATION | Win32.SHGFI_TYPENAME | size);
			if (shinfo.iIcon == 0) {
				Win32.DestroyIcon (shinfo.hIcon);
				return null;
			}
			var icon = Icon.FromHandle (shinfo.hIcon);
			string key = shinfo.iIcon + " - " + shinfo.szDisplayName + " - " + icon.Width;

			Gdk.Pixbuf pix;
			if (!icons.TryGetValue (key, out pix)) {
				pix = CreateFromResource (icon.ToBitmap ());
				icons[key] = pix;
			}
			Win32.DestroyIcon (shinfo.hIcon);
			return pix;
		}

		public Gdk.Pixbuf CreateFromResource (Bitmap bitmap)
		{
			using (var ms = new MemoryStream ()) {
				bitmap.Save (ms, ImageFormat.Png);
				ms.Position = 0;
				return new Gdk.Pixbuf (ms);
			}
		}
	}
}
