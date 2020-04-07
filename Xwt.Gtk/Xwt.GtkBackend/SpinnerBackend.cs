//
// SpinnerBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
using System;
using System.Runtime.InteropServices;
using  static Xwt.Interop.DllImportGtk;

using Xwt.Backends;

namespace Xwt.GtkBackend
{
	public class SpinnerBackend : WidgetBackend, ISpinnerBackend
	{
		public SpinnerBackend ()
		{
			Widget = new Spinner ();
			Widget.Show ();
		}

		protected new Spinner Widget {
			get { return (Spinner)base.Widget; }
			set { base.Widget = value; }
		}

		public void StartAnimation ()
		{
			Widget.Start ();
		}

		public void StopAnimation ()
		{
			Widget.Stop ();
		}

		public bool IsAnimating {
			get {
				return Widget.Active;
			}
		}
	}

	public class Spinner : Gtk.Widget
	{
		#if !XWT_GTK3
		[Obsolete]
		protected Spinner(GLib.GType gtype) : base(gtype) {}
		#endif

		public Spinner () : base(IntPtr.Zero)
		{
			if (GetType () != typeof(Spinner))
			{
				this.CreateNativeObject (new string[0], new GLib.Value[0]);
				return;
			}
			this.Raw = gtk_spinner_new ();
		}

		[GLib.Property ("active")]
		public bool Active {
			get {
				GLib.Value val = GetProperty ("active");
				bool ret = (bool) val;
				val.Dispose ();
				return ret;
			}
			set {
				GLib.Value val = new GLib.Value(value);
				SetProperty("active", val);
				val.Dispose ();
			}
		}

		public void Start()
		{
			gtk_spinner_start(Handle);
		}


		public void Stop ()
		{
			gtk_spinner_stop(Handle);
		}
	
		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gtk_spinner_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
	}
}
