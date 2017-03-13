//
// GtkWin32Interop.cs
//
// Author:
//       Marius Ungureanu <marius.ungureanu@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
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

using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Xwt.Gtk.Windows
{
	public static class GtkWin32Interop
	{
		const string LIBGDK = "libgdk-win32-2.0-0.dll";
		const string LIBGOBJECT = "libgobject-2.0-0.dll";
		const string USER32 = "user32.dll";

		public enum ExtendedWindowStyles
		{
			// Hides it from alt-tab.
			WS_EX_TOOLWINDOW = 0x80,
		}

		public enum GWLParameter
		{
			// Sets a new extended style.
			GWL_EXSTYLE = -20,

			// Sets a new application handle instance.
			GWL_HINSTANCE = -6,

			// Sets a new window handle as the parent.
			GWL_HWNDPARENT = -8,

			// Sets a new identifier of the window.
			GWL_ID = -12,

			// Sets a new window style.
			GWL_STYLE = -16,

			// Sets a new user data associated with the window. Initially zero.
			GWL_USERDATA = -21,

			// Sets a new address of the window procedure.
			GWL_WNDPROC = -4,
		}

		[StructLayout (LayoutKind.Sequential)]
		struct NativeEventKeyStruct
		{
			public Gdk.EventType type;
			public IntPtr window;
			public sbyte send_event;
			public uint time;
			public uint state;
			public uint keyval;
			public int length;
			public IntPtr str;
			public ushort hardware_keycode;
			public byte group;
			public uint is_modifier;
		}

		class EventKeyWrapper : Gdk.EventKey
		{
			IntPtr ptr;

			public EventKeyWrapper (IntPtr ptr) : base (ptr)
			{
				this.ptr = ptr;
			}

			~EventKeyWrapper ()
			{
				Marshal.FreeHGlobal (ptr);
			}
		}

		[DllImport (LIBGDK, CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr gdk_win32_window_get_impl_hwnd (IntPtr window);

		public static IntPtr HWndGet (Gdk.Window window)
		{
			return gdk_win32_window_get_impl_hwnd (window.Handle);
		}

		[DllImport (USER32, EntryPoint="SetWindowLongPtr")]
		static extern IntPtr SetWindowLongPtr64 (IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport(USER32, EntryPoint="SetWindowLong")]
		static extern IntPtr SetWindowLongPtr32 (IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		public static IntPtr SetWindowLongPtr (IntPtr hWnd, int nIndex, IntPtr dwNewLong)
		{
			if (IntPtr.Size == 4)
				return SetWindowLongPtr32 (hWnd, nIndex, dwNewLong);
			return SetWindowLongPtr64 (hWnd, nIndex, dwNewLong);
		}

		[DllImport (USER32, EntryPoint="GetWindowLongPtr")]
		static extern IntPtr GetWindowLongPtr64 (IntPtr hWnd, int nIndex);

		[DllImport(USER32, EntryPoint="GetWindowLong")]
		static extern IntPtr GetWindowLongPtr32 (IntPtr hWnd, int nIndex);

		public static IntPtr GetWindowLongPtr (IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 4)
				return GetWindowLongPtr32 (hWnd, nIndex);
			return GetWindowLongPtr64 (hWnd, nIndex);
		}

		public static global::Gtk.Widget ControlToGtkWidget (System.Windows.FrameworkElement fe)
		{
			return new GtkWPFWidget (fe);
		}

		internal static Gdk.EventKey ConvertKeyEvent (System.Windows.Input.ModifierKeys mod, System.Windows.Input.Key key, Gdk.Window rootWindow)
		{
			var state = Gdk.ModifierType.None;
			if ((mod & System.Windows.Input.ModifierKeys.Control) != 0)
				state |= Gdk.ModifierType.ControlMask;
			if ((mod & System.Windows.Input.ModifierKeys.Shift) != 0)
				state |= Gdk.ModifierType.ShiftMask;
			if ((mod & System.Windows.Input.ModifierKeys.Windows) != 0)
				state |= Gdk.ModifierType.MetaMask;
			if ((mod & System.Windows.Input.ModifierKeys.Alt) != 0)
				state |= Gdk.ModifierType.Mod1Mask;

			return CreateKeyEventFromKeyCode ((ushort)KeyInterop.VirtualKeyFromKey (key), state, Gdk.EventType.KeyPress, rootWindow);
		}

		public static Gdk.EventKey CreateKeyEventFromKeyCode (ushort keyCode, Gdk.ModifierType state, Gdk.EventType eventType, Gdk.Window win)
		{
			return CreateKeyEvent (0, keyCode, state, eventType, win);
		}

		static Gdk.EventKey CreateKeyEvent (uint keyval, int keyCode, Gdk.ModifierType state, Gdk.EventType eventType, Gdk.Window win)
		{
			int effectiveGroup, level;
			Gdk.ModifierType cmods;
			if (keyval == 0)
				Gdk.Keymap.Default.TranslateKeyboardState ((uint)keyCode, state, 0, out keyval, out effectiveGroup, out level, out cmods);

			Gdk.KeymapKey [] keyms = Gdk.Keymap.Default.GetEntriesForKeyval (keyval);
			if (keyms.Length == 0)
				return null;

			var nativeEvent = new NativeEventKeyStruct {
				type = eventType,
				send_event = 1,
				window = win != null ? win.Handle : IntPtr.Zero,
				state = (uint)state,
				keyval = keyval,
				group = (byte)keyms [0].Group,
				hardware_keycode = keyCode == -1 ? (ushort)keyms [0].Keycode : (ushort)keyCode,
				length = 0,
				time = global::Gtk.Global.CurrentEventTime
			};

			IntPtr ptr = GLib.Marshaller.StructureToPtrAlloc (nativeEvent);
			return new EventKeyWrapper (ptr);
		}

		class GtkWPFWidget : global::Gtk.Widget
		{
			FrameworkElement wpfControl;

			internal System.Windows.Forms.Integration.ElementHost wpfWidgetHost {
				get;
				private set;
			}

			public GtkWPFWidget (System.Windows.FrameworkElement wpfControl)
			{
				CanFocus = true;
				this.wpfControl = wpfControl;
				wpfWidgetHost = new System.Windows.Forms.Integration.ElementHost {
					BackColor = System.Drawing.Color.Transparent,
					Child = wpfControl,
				};
				wpfControl.GotKeyboardFocus += OnGotFocus;
				wpfControl.KeyDown += OnKeyDown;
				WidgetFlags |= global::Gtk.WidgetFlags.NoWindow;
			}

			void OnKeyDown (object sender, System.Windows.Input.KeyEventArgs e)
			{
				if (e.Handled)
					return;
				var toplevel = Toplevel?.GdkWindow;
				if (toplevel == null)
					return;
				var key = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;
				var keyEvent = ConvertKeyEvent (e.KeyboardDevice.Modifiers, key, toplevel);
				global::Gtk.Main.DoEvent (keyEvent);
				e.Handled = true;
			}

			void OnGotFocus (object sender, KeyboardFocusChangedEventArgs e)
			{
				GrabFocus ();
			}

			protected virtual void RepositionWpfWindow ()
			{
				int scale = (int)GtkBackend.GtkWorkarounds.GetScaleFactor (this);
				RepositionWpfWindow (scale, scale);
			}

			protected void RepositionWpfWindow (int hscale, int vscale)
			{
				int x, y;
				if (TranslateCoordinates (Toplevel, 0, 0, out x, out y)) {
					wpfWidgetHost.Left = x * hscale;
					wpfWidgetHost.Top = y * vscale;
				} else {
					wpfWidgetHost.Left = Allocation.Left * hscale;
					wpfWidgetHost.Top = Allocation.Top * vscale;
				}
				wpfWidgetHost.Width = (Allocation.Width + 1) * hscale;
				wpfWidgetHost.Height = (Allocation.Height + 1) * vscale;
				wpfWidgetHost.Show ();
			}

			protected override void OnRealized ()
			{
				base.OnRealized ();

				// Initial size setting.
				RepositionWpfWindow ();
			}

			protected override void OnSizeAllocated (Gdk.Rectangle allocation)
			{
				base.OnSizeAllocated (allocation);

				// Needed for window full screening.
				RepositionWpfWindow ();
			}

			void OnWindowConfigured (object sender, global::Gtk.ConfigureEventArgs args)
			{
				if (Visible)
					RepositionWpfWindow ();
			}

			void OnWindowSetFocus (object o, global::Gtk.SetFocusArgs args)
			{
				var focused = System.Windows.Input.Keyboard.FocusedElement as FrameworkElement;
				if (focused != null && (focused == wpfControl || wpfControl.IsAncestorOf (focused))) {
					System.Windows.Input.Keyboard.ClearFocus ();
					var toplevelWindow = Toplevel as global::Gtk.Window;
					if (toplevelWindow != null)
						toplevelWindow.Present ();
				}
			}

			protected override void OnDestroyed ()
			{
				base.OnDestroyed ();

				wpfControl.GotKeyboardFocus -= OnGotFocus;
				wpfControl.KeyDown -= OnKeyDown;
				wpfWidgetHost.Dispose ();
			}

			protected override void OnHierarchyChanged (global::Gtk.Widget previous_toplevel)
			{
				base.OnHierarchyChanged (previous_toplevel);
				var previousWindow = previous_toplevel as global::Gtk.Window;
				if (previousWindow != null) {
					previousWindow.ConfigureEvent -= OnWindowConfigured;
					previousWindow.SetFocus -= OnWindowSetFocus;
				}
				var toplevel = Toplevel as global::Gtk.Window;
				if (toplevel != null) {
					toplevel.ConfigureEvent += OnWindowConfigured;
					toplevel.SetFocus += OnWindowSetFocus;
					var window = toplevel.GdkWindow;
					if (window != null) {
						IntPtr gtkWindowPtr = GtkWin32Interop.HWndGet (window);
						IntPtr wpfWindowPtr = wpfWidgetHost.Handle;
						GtkWin32Interop.SetWindowLongPtr (wpfWindowPtr, (int)GtkWin32Interop.GWLParameter.GWL_HWNDPARENT, gtkWindowPtr);
					}
				}
			}

			protected override void OnShown ()
			{
				base.OnShown ();
				wpfWidgetHost.Show ();
			}

			protected override void OnHidden ()
			{
				base.OnHidden ();
				wpfWidgetHost.Hide ();
			}

			protected override void OnUnmapped ()
			{
				base.OnUnmapped ();
				wpfWidgetHost.Hide ();
			}

			protected override void OnUnrealized ()
			{
				base.OnUnrealized ();
				wpfWidgetHost.Hide ();
			}
		}
	}
}
