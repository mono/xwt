// 
// Util.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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
using Xwt.Drawing;
using Xwt.Engine;
using System.Collections.Generic;
using System.Linq;

namespace Xwt.GtkBackend
{
	public static class Util
	{
		static uint targetIdCounter = 0;
		static Dictionary<string, Gtk.TargetEntry[]> dragTargets = new Dictionary<string, Gtk.TargetEntry[]> ();
		static Dictionary<string, string> atomToType = new Dictionary<string, string> ();

		public static Cairo.Color ToCairoColor (this Color col)
		{
			return new Cairo.Color (col.Red, col.Green, col.Blue, col.Alpha);
		}
		
		public static void SetDragData (TransferDataSource data, Gtk.DragDataGetArgs args)
		{
			foreach (var t in data.DataTypes) {
				object val = data.GetValue (t);
				if (val == null)
					continue;
				if (val is string)
					args.SelectionData.Text = (string)data.GetValue (t);
				else if (val is Xwt.Drawing.Image)
					args.SelectionData.SetPixbuf ((Gdk.Pixbuf) WidgetRegistry.GetBackend (val));
				else {
					var at = Gdk.Atom.Intern (t, false);
					args.SelectionData.Set (at, 0, TransferDataSource.SerializeValue (val));
				}
			}
		}
		
		internal static string AtomToType (string targetName)
		{
			string type;
			atomToType.TryGetValue (targetName, out type);
			return type;
		}
		
		internal static string[] GetDragTypes (Gdk.Atom[] dropTypes)
		{
			List<string> types = new List<string> ();
			foreach (var dt in dropTypes) {
				string type;
				if (atomToType.TryGetValue (dt.ToString (), out type))
					types.Add (type);
			}
			return types.ToArray ();
		}
		
		public static Gtk.TargetList BuildTargetTable (string[] types)
		{
			var tl = new Gtk.TargetList ();
			foreach (var tt in types)
				tl.AddTable (CreateTargetEntries (tt));
			return tl;
		}
		
		static Gtk.TargetEntry[] CreateTargetEntries (string type)
		{
			lock (dragTargets) {
				Gtk.TargetEntry[] entries;
				if (dragTargets.TryGetValue (type, out entries))
					return entries;
				
				uint id = targetIdCounter++;
				
				switch (type) {
				case TransferDataType.Uri: {
						Gtk.TargetList list = new Gtk.TargetList ();
						list.AddUriTargets (id);
						entries = (Gtk.TargetEntry[])list;
						break;
					}
				case TransferDataType.Text: {
						Gtk.TargetList list = new Gtk.TargetList ();
						list.AddTextTargets (id);
						//HACK: work around gtk_selection_data_set_text causing crashes on Mac w/ QuickSilver, Clipbard History etc.
						if (Platform.IsMac) {
							list.Remove ("COMPOUND_TEXT");
							list.Remove ("TEXT");
							list.Remove ("STRING");
						}
						entries = (Gtk.TargetEntry[])list;
						break;
					}
				case TransferDataType.Rtf: {
						Gdk.Atom atom;
						if (Platform.IsMac)
							atom = Gdk.Atom.Intern ("NSRTFPboardType", false); //TODO: use public.rtf when dep on MacOS 10.6
						else
							atom = Gdk.Atom.Intern ("text/rtf", false);
						entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (atom, 0, id) };
						break;
					}
				default:
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (Gdk.Atom.Intern ("application/" + type, false), 0, id) };
					break;
				}
				foreach (var a in entries.Select (e => e.Target))
					atomToType [a] = type;
				return dragTargets [type] = entries;
			}
		}	
		
		static Dictionary<string,string> icons;

		public static string ToGtkStock (string id)
		{
			if (icons == null) {
				icons = new Dictionary<string, string> ();
				icons [StockIcons.ZoomIn] = Gtk.Stock.ZoomIn;
				icons [StockIcons.ZoomOut] = Gtk.Stock.ZoomOut;
				icons [StockIcons.Zoom100] = Gtk.Stock.Zoom100;
				icons [StockIcons.ZoomFit] = Gtk.Stock.ZoomFit;
			}
			string res;
			icons.TryGetValue (id, out res);
			return res;
		}
		
		public static Gtk.IconSize ToGtkSize (Xwt.IconSize size)
		{
			switch (size) {
			case IconSize.Small:
				return Gtk.IconSize.Menu;
			case IconSize.Medium:
				return Gtk.IconSize.Button;
			case IconSize.Large:
				return Gtk.IconSize.Dialog;
			}
			return Gtk.IconSize.Dialog;
		}
	}
}

