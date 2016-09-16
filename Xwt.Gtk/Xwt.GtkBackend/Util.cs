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

using Xwt.Backends;
using System.Collections.Generic;
using System.Linq;

namespace Xwt.GtkBackend
{
	public static class Util
	{
		static uint targetIdCounter = 0;
		static Dictionary<TransferDataType, Gtk.TargetEntry[]> dragTargets = new Dictionary<TransferDataType, Gtk.TargetEntry[]> ();
		static Dictionary<string, TransferDataType> atomToType = new Dictionary<string, TransferDataType> ();
		static Size[] iconSizes = new Size[7];
		
		static Util ()
		{
			for (int i = 0; i < iconSizes.Length; i++) {
				int w, h;
				if (!Gtk.Icon.SizeLookup ((Gtk.IconSize)i, out w, out h))
					w = h = -1;
				iconSizes[i].Width = w;
				iconSizes[i].Height = h;
			}
			if (Platform.IsWindows) {
				// Workaround for an issue in GTK for Windows. In windows Menu-sized icons are not 16x16, but 14x14
				iconSizes[(int)Gtk.IconSize.Menu].Width = 16;
				iconSizes[(int)Gtk.IconSize.Menu].Height = 16;
			}
		}

		public static void SetDragData (TransferDataSource data, Gtk.DragDataGetArgs args)
		{
			foreach (var t in data.DataTypes) {
				object val = data.GetValue (t);
				SetSelectionData (args.SelectionData, t.Id, val);
			}
		}
		
		public static void SetSelectionData (Gtk.SelectionData data, string atomType, object val)
		{
			if (val == null)
				return;
			if (val is string)
				data.Text = (string)val;
			else if (val is Xwt.Drawing.Image) {
				var bmp = ((Image)val).ToBitmap ();
				data.SetPixbuf (((GtkImage)Toolkit.GetBackend (bmp)).Frames[0].Pixbuf);
			}
			else if (val is Uri)
				data.SetUris(new string[] { ((Uri)val).AbsolutePath });
			else {
				var at = Gdk.Atom.Intern (atomType, false);
				data.Set (at, 0, TransferDataSource.SerializeValue (val));
			}
		}
		
		public static bool GetSelectionData (ApplicationContext context, Gtk.SelectionData data, TransferDataStore target)
		{
			TransferDataType type = Util.AtomToType (data.Target.Name);
			if (type == null || data.Length <= 0)
				return false;

			if (type == TransferDataType.Text)
				target.AddText (data.Text);
			else if (data.TargetsIncludeImage (false))
				target.AddImage (context.Toolkit.WrapImage (data.Pixbuf));
			else if (type == TransferDataType.Uri)
				target.AddUris (data.GetUris().Where(u => !string.IsNullOrEmpty(u)).Select(u => new Uri(u)).ToArray());
			else
				target.AddValue (type, data.Data);
			return true;
		}
		
		internal static TransferDataType AtomToType (string targetName)
		{
			TransferDataType type;
			atomToType.TryGetValue (targetName, out type);
			return type;
		}
		
		internal static TransferDataType[] GetDragTypes (Gdk.Atom[] dropTypes)
		{
			List<TransferDataType> types = new List<TransferDataType> ();
			foreach (var dt in dropTypes) {
				TransferDataType type;
				if (atomToType.TryGetValue (dt.Name, out type))
					types.Add (type);
			}
			return types.ToArray ();
		}
		
		public static Gtk.TargetList BuildTargetTable (TransferDataType[] types)
		{
			var tl = new Gtk.TargetList ();
			foreach (var tt in types)
				tl.AddTable (CreateTargetEntries (tt));
			return tl;
		}
		
		static Gtk.TargetEntry[] CreateTargetEntries (TransferDataType type)
		{
			lock (dragTargets) {
				Gtk.TargetEntry[] entries;
				if (dragTargets.TryGetValue (type, out entries))
					return entries;
				
				uint id = targetIdCounter++;
				
				if (type == TransferDataType.Uri) {
					Gtk.TargetList list = new Gtk.TargetList ();
					list.AddUriTargets (id);
					entries = (Gtk.TargetEntry[])list;
				}
				else if (type == TransferDataType.Text) {
					Gtk.TargetList list = new Gtk.TargetList ();
					list.AddTextTargets (id);
					//HACK: work around gtk_selection_data_set_text causing crashes on Mac w/ QuickSilver, Clipbard History etc.
					if (Platform.IsMac) {
						list.Remove ("COMPOUND_TEXT");
						list.Remove ("TEXT");
						list.Remove ("STRING");
					}
					entries = (Gtk.TargetEntry[])list;
				} else if (type == TransferDataType.Rtf) {
					Gdk.Atom atom;
					if (Platform.IsMac) {
						atom = Gdk.Atom.Intern ("NSRTFPboardType", false); //TODO: use public.rtf when dep on MacOS 10.6
					} else if (Platform.IsWindows) {
						atom = Gdk.Atom.Intern ("Rich Text Format", false);
					} else {
						atom = Gdk.Atom.Intern ("text/rtf", false);
					}
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (atom, 0, id) };
				}
				else if (type == TransferDataType.Html) {
					Gdk.Atom atom;
					if (Platform.IsMac) {
						atom = Gdk.Atom.Intern ("Apple HTML pasteboard type", false); //TODO: use public.rtf when dep on MacOS 10.6
					} else if (Platform.IsWindows) {
						atom = Gdk.Atom.Intern ("HTML Format", false);
					} else {
						atom = Gdk.Atom.Intern ("text/html", false);
					}
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (atom, 0, id) };
				}
				else if (type == TransferDataType.Image) {
					Gtk.TargetList list = new Gtk.TargetList ();
					list.AddImageTargets (id, true);
					entries = (Gtk.TargetEntry[])list;
				}
				else {
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (Gdk.Atom.Intern ("application/" + type.Id, false), 0, id) };
				}
				
				foreach (var a in entries.Select (e => e.Target))
					atomToType [a] = type;
				return dragTargets [type] = entries;
			}
		}	
		
		static Dictionary<string,string> icons;

		public static GtkImage ToGtkStock (string id)
		{
			if (icons == null) {
				icons = new Dictionary<string, string> ();
				icons [StockIconId.ZoomIn] = Gtk.Stock.ZoomIn;
				icons [StockIconId.ZoomOut] = Gtk.Stock.ZoomOut;
				icons [StockIconId.Zoom100] = Gtk.Stock.Zoom100;
				icons [StockIconId.ZoomFit] = Gtk.Stock.ZoomFit;
				icons [StockIconId.OrientationPortrait] = Gtk.Stock.OrientationPortrait;
				icons [StockIconId.OrientationLandscape] = Gtk.Stock.OrientationLandscape;
				icons [StockIconId.Add] = Gtk.Stock.Add;
				icons [StockIconId.Remove] = Gtk.Stock.Remove;
				icons [StockIconId.Warning] = Gtk.Stock.DialogWarning;
				icons [StockIconId.Error] = Gtk.Stock.DialogError;
				icons [StockIconId.Information] = Gtk.Stock.DialogInfo;
				icons [StockIconId.Question] = Gtk.Stock.DialogQuestion;
			}
			string res;
			if (!icons.TryGetValue (id, out res))
				throw new NotSupportedException ("Unknown image: " + id);
			return new GtkImage (res);
		}


		public static Gtk.IconSize GetBestSizeFit (double size, Gtk.IconSize[] availablesizes = null)
		{
			// Find the size that better fits the requested size

			for (int n=0; n<iconSizes.Length; n++) {
				if (availablesizes != null && !availablesizes.Contains ((Gtk.IconSize)n))
					continue;
				if (size <= iconSizes [n].Width)
					return (Gtk.IconSize)n;
			}
			if (availablesizes == null || availablesizes.Contains (Gtk.IconSize.Dialog))
				return Gtk.IconSize.Dialog;
			else
				return Gtk.IconSize.Invalid;
		}

		public static double GetBestSizeFitSize (double size)
		{
			var s = GetBestSizeFit (size);
			return iconSizes [(int)s].Width;
		}
		
		public static ImageDescription WithDefaultSize (this ImageDescription image, Gtk.IconSize defaultIconSize)
		{
			if (image.Size.IsZero) {
				var s = iconSizes [(int)defaultIconSize];
				image.Size = s;
			}
			return image;
		}

		public static double GetScaleFactor (Gtk.Widget w)
		{
			return GtkWorkarounds.GetScaleFactor (w);
		}

		public static double GetDefaultScaleFactor ()
		{
			return 1;
		}

		internal static void SetSourceColor (this Cairo.Context cr, Cairo.Color color)
		{
			cr.SetSourceRGBA (color.R, color.G, color.B, color.A);
		}

		//this is needed for building against old Mono.Cairo versions
		[Obsolete]
		internal static void SetSource (this Cairo.Context cr, Cairo.Pattern pattern)
		{
			cr.Pattern = pattern;
		}

		[Obsolete]
		internal static Cairo.Surface GetTarget (this Cairo.Context cr)
		{
			return cr.Target;
		}

		[Obsolete]
		internal static void Dispose (this Cairo.Context cr)
		{
			((IDisposable)cr).Dispose ();
		}
	}
}

