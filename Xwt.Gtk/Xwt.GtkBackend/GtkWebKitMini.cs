//
// GtkWebKitMini.cs
//
// Author:
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2014 Vsevolod Kukol
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;

namespace Xwt.GtkBackend.WebKit
{


	public class WebView : Gtk.Container
	{
		public WebView(IntPtr raw) : base(raw)
		{
		}

		public WebView () : base (IntPtr.Zero)
		{
			Raw = webkit_web_view_new();
		}

		public void LoadUri(string uri) {
			IntPtr native_uri = GLib.Marshaller.StringToPtrGStrdup (uri);
			webkit_web_view_load_uri(Handle, native_uri);
			GLib.Marshaller.Free (native_uri);
		}

		public string Uri { 
			get {
				IntPtr raw_ret = webkit_web_view_get_uri(Handle);
				string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
				return ret;
			}
		}

		public double LoadProgress { 
			get {
				double ret = webkit_web_view_get_progress(Handle);
				return ret;
			}
		}

		public bool FullContentZoom { 
			get {
				bool raw_ret = webkit_web_view_get_full_content_zoom(Handle);
				bool ret = raw_ret;
				return ret;
			}
			set {
				webkit_web_view_set_full_content_zoom(Handle, value);
			}
		}

		public void StopLoading() {
			webkit_web_view_stop_loading(Handle);
		}

		public void Reload() {
			webkit_web_view_reload(Handle);
		}

		public bool CanGoBack() {
			bool raw_ret = webkit_web_view_can_go_back(Handle);
			bool ret = raw_ret;
			return ret;
		}

		public void GoBack() {
			webkit_web_view_go_back(Handle);
		}

		public void GoForward() {
			webkit_web_view_go_forward(Handle);
		}

		public bool CanGoForward() {
			bool raw_ret = webkit_web_view_can_go_forward(Handle);
			bool ret = raw_ret;
			return ret;
		}

		public void LoadHtmlString(string content, string base_uri) {
			IntPtr native_content = GLib.Marshaller.StringToPtrGStrdup (content);
			IntPtr native_base_uri = GLib.Marshaller.StringToPtrGStrdup (base_uri);
			webkit_web_view_load_string (Handle, native_content, IntPtr.Zero, IntPtr.Zero, native_base_uri);
			GLib.Marshaller.Free (native_content);
			GLib.Marshaller.Free (native_base_uri);
		}

		public string Title { 
			get {
				IntPtr raw_ret = webkit_web_view_get_title(Handle);
				string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
				return ret;
			}
		}

		[GLib.Signal("load-finished")]
		public event EventHandler<GLib.SignalArgs> LoadFinished {
			add {
				this.AddSignalHandler ("load-finished", value, typeof(GLib.SignalArgs));
			}
			remove {
				this.RemoveSignalHandler ("load-finished", value);
			}
		}

		[GLib.Signal("load-started")]
		public event EventHandler<GLib.SignalArgs> LoadStarted {
			add {
				this.AddSignalHandler ("load-started", value, typeof(GLib.SignalArgs));
			}
			remove {
				this.RemoveSignalHandler ("load-started", value);
			}
		}

		[GLib.Signal("navigation-requested")]
		public event EventHandler<NavigationRequestedArgs> NavigationRequested {
			add {
				this.AddSignalHandler ("navigation-requested", value, typeof(NavigationRequestedArgs));
			}
			remove {
				this.RemoveSignalHandler ("navigation-requested", value);
			}
		}

		[GLib.Signal("title-changed")]
		public event EventHandler<TitleChangedArgs> TitleChanged {
			add {
				this.AddSignalHandler ("title-changed", value, typeof(TitleChangedArgs));
			}
			remove {
				this.RemoveSignalHandler ("title-changed", value);
			}
		}

		static WebView ()
		{
			Initialize ();
		}

		static bool initialized = false;
		internal static void Initialize ()
		{
			if (initialized)
				return;

			initialized = true;
			GLib.GType.Register (WebView.GType, typeof (WebView));
			GLib.GType.Register (NetworkRequest.GType, typeof (NetworkRequest));
		}

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = webkit_web_view_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_web_view_new();

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_web_view_get_type();

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_load_uri(IntPtr raw, IntPtr uri);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_web_view_get_uri(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern bool webkit_web_view_get_full_content_zoom(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_set_full_content_zoom(IntPtr raw, bool full_content_zoom);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_stop_loading(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_reload(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern bool webkit_web_view_can_go_back(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_go_back(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern bool webkit_web_view_can_go_forward(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_go_forward(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_web_view_load_string (IntPtr raw, IntPtr content, IntPtr mime_type, IntPtr encoding, IntPtr base_uri);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_web_view_get_title(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern double webkit_web_view_get_progress(IntPtr raw);

	}

	public sealed class NetworkRequest : GLib.Object {

		public NetworkRequest(IntPtr raw) : base(raw) {}

		public NetworkRequest (string uri) : base (IntPtr.Zero)
		{
			IntPtr native_uri = GLib.Marshaller.StringToPtrGStrdup (uri);
			Raw = webkit_network_request_new(native_uri);
			GLib.Marshaller.Free (native_uri);
		}

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = webkit_network_request_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		public string Uri { 
			get {
				IntPtr raw_ret = webkit_network_request_get_uri(Handle);
				string ret = GLib.Marshaller.Utf8PtrToString (raw_ret);
				return ret;
			}
			set {
				IntPtr native_value = GLib.Marshaller.StringToPtrGStrdup (value);
				webkit_network_request_set_uri(Handle, native_value);
				GLib.Marshaller.Free (native_value);
			}
		}


		static NetworkRequest ()
		{
			WebView.Initialize ();
		}

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_network_request_new(IntPtr uri);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_network_request_get_type();

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern IntPtr webkit_network_request_get_uri(IntPtr raw);

		[DllImport (GtkInterop.LIBWEBKIT)]
		static extern void webkit_network_request_set_uri(IntPtr raw, IntPtr uri);
	}

	public enum NavigationResponse {

		Accept,
		Ignore,
		Download,
	}

	public class LoadProgressChangedArgs : GLib.SignalArgs
	{
		public int Progress
		{
			get {
				return (int)Args [0];
			}
		}
	}

	public class NavigationRequestedArgs : GLib.SignalArgs
	{
		public IntPtr Frame
		{
			get {
				return (IntPtr) Args [0];
			}
		}

		public NetworkRequest Request
		{
			get {
				return (NetworkRequest) Args [1];
			}
		}
	}

	public class TitleChangedArgs : GLib.SignalArgs
	{
		public string Title
		{
			get {
				return (string)Args [1];
			}
		}
	}
}

