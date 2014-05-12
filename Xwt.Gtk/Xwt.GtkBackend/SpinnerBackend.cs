//
// SpinnerBackend.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
using System;
using System.Runtime.InteropServices;

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

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gtk_spinner_new();

		public Spinner () : base(IntPtr.Zero)
		{
			if (GetType () != typeof(Spinner))
			{
				this.CreateNativeObject (new string[0], new GLib.Value[0]);
				return;
			}
			this.Raw = Spinner.gtk_spinner_new ();
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

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_spinner_start(IntPtr raw);

		public void Start()
		{
			gtk_spinner_start(Handle);
		}

		[DllImport (GtkInterop.LIBGTK, CallingConvention = CallingConvention.Cdecl)]
		static extern void gtk_spinner_stop(IntPtr raw);

		public void Stop ()
		{
			gtk_spinner_stop(Handle);
		}
		
		[DllImport(GtkInterop.LIBGTK)]
		static extern IntPtr gtk_spinner_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gtk_spinner_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}
	}
}
