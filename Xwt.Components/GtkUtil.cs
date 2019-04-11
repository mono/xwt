//
// GtkMacInterop.cs
//
// Author:
//       Jérémie Laval <jeremie.laval@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using Gdk;

namespace Xwt.Components
{
	internal class GtkUtil
	{
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

		// Analysis disable InconsistentNaming
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

		public static Gdk.EventKey CreateKeyEventFromKeyCode (ushort keyCode, Gdk.ModifierType state, Gdk.EventType eventType, Gdk.Window win, uint time)
		{
			return CreateKeyEvent (0, keyCode, state, eventType, win, time);
		}

		static Gdk.EventKey CreateKeyEvent (uint keyval, int keyCode, Gdk.ModifierType state, Gdk.EventType eventType, Gdk.Window win, uint? time)
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
				time = time ?? Gtk.Global.CurrentEventTime
			};

			IntPtr ptr = GLib.Marshaller.StructureToPtrAlloc (nativeEvent);
			return new EventKeyWrapper (ptr);
		}
	}
}