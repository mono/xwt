//
// MessageService.cs
//
// Author:
//   Mike Krüger <mkrueger@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Gtk;
using System.Collections.Generic;

namespace Xwt.GtkBackend
{
	public static class MessageService
	{
		public static Gtk.Window RootWindow { get; set; }
		
		/// <summary>
		/// Places, runs and destroys a transient dialog.
		/// </summary>
		public static int ShowCustomDialog (Gtk.Dialog dialog)
		{
			return ShowCustomDialog (dialog, null);
		}
		
		public static int ShowCustomDialog (Gtk.Dialog dialog, Gtk.Window parent)
		{
			try {
				return RunCustomDialog (dialog, parent);
			} finally {
				if (dialog != null)
					dialog.Destroy ();
			}
		}
		
		public static int RunCustomDialog (Gtk.Dialog dialog)
		{
			return RunCustomDialog (dialog, null);
		}
		
		/// <summary>
		/// Places and runs a transient dialog. Does not destroy it, so values can be retrieved from its widgets.
		/// </summary>
		public static int RunCustomDialog (Gtk.Dialog dialog, Gtk.Window parent)
		{
			if (parent == null) {
				if (dialog.TransientFor != null)
					parent = dialog.TransientFor;
				else
					parent = GetDefaultParent (dialog);
			}
			dialog.TransientFor = parent;
			dialog.DestroyWithParent = true;
			return GtkWorkarounds.RunDialogWithNotification (dialog);
		}
		
		//make sure modal children are parented on top of other modal children
		static Gtk.Window GetDefaultParent (Gtk.Window child)
		{
			if (child.Modal) {
				return GetDefaultModalParent ();
			} else {
				return RootWindow;
			}
		}
		
		/// <summary>
		/// Gets a default parent for modal dialogs.
		/// </summary>
		public static Gtk.Window GetDefaultModalParent ()
		{
			foreach (Gtk.Window w in Gtk.Window.ListToplevels ())
				if (w.Visible && w.HasToplevelFocus && w.Modal)
					return w;
			return RootWindow;
		}
	}
}
