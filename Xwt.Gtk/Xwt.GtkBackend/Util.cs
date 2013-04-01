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
			else if (val is Xwt.Drawing.Image)
				data.SetPixbuf ((Gdk.Pixbuf) Toolkit.GetBackend (val));
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
			else if (type == TransferDataType.Uri) {
				var uris = System.Text.Encoding.UTF8.GetString (data.Data).Split ('\n').Where (u => !string.IsNullOrEmpty(u)).Select (u => new Uri (u)).ToArray ();
				target.AddUris (uris);
			}
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
				else {
					entries = new Gtk.TargetEntry[] { new Gtk.TargetEntry (Gdk.Atom.Intern ("application/" + type.Id, false), 0, id) };
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
		
		public static Gdk.Color ToGdkColor (this Xwt.Drawing.Color color)
		{
			return new Gdk.Color ((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));
		}
		
		public static Color ToXwtColor (this Gdk.Color color)
		{
			return new Color ((double)color.Red / (double)ushort.MaxValue, (double)color.Green / (double)ushort.MaxValue, (double)color.Blue / (double)ushort.MaxValue);
		}
		
		public static ScrollPolicy ConvertScrollPolicy (Gtk.PolicyType p)
		{
			switch (p) {
			case Gtk.PolicyType.Always:
				return ScrollPolicy.Always;
			case Gtk.PolicyType.Automatic:
				return ScrollPolicy.Automatic;
			case Gtk.PolicyType.Never:
				return ScrollPolicy.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}
		
		public static Gtk.PolicyType ConvertScrollPolicy (ScrollPolicy p)
		{
			switch (p) {
			case ScrollPolicy.Always:
				return Gtk.PolicyType.Always;
			case ScrollPolicy.Automatic:
				return Gtk.PolicyType.Automatic;
			case ScrollPolicy.Never:
				return Gtk.PolicyType.Never;
			}
			throw new InvalidOperationException ("Invalid policy value:" + p);
		}

        public static ScrollDirection ConvertScrollDirection(Gdk.ScrollDirection d)
        {
            switch(d) {
            case Gdk.ScrollDirection.Up:
                return Xwt.ScrollDirection.Up;
            case Gdk.ScrollDirection.Down:
                return Xwt.ScrollDirection.Down;
            case Gdk.ScrollDirection.Left:
                return Xwt.ScrollDirection.Left;
            case Gdk.ScrollDirection.Right:
                return Xwt.ScrollDirection.Right;
            }
            throw new InvalidOperationException("Invalid mouse scroll direction value: " + d);
        }

        public static Gdk.ScrollDirection ConvertScrollDirection(ScrollDirection d)
        {
            switch (d) {
            case ScrollDirection.Up:
                return Gdk.ScrollDirection.Up;
            case ScrollDirection.Down:
                return Gdk.ScrollDirection.Down;
            case ScrollDirection.Left:
                return Gdk.ScrollDirection.Left;
            case ScrollDirection.Right:
                return Gdk.ScrollDirection.Right;
            }
            throw new InvalidOperationException("Invalid mouse scroll direction value: " + d);
        }

		public static Gtk.IconSize GetBestSizeFit (double size)
		{
			// Find the size that better fits the requested size

			for (int n=0; n<iconSizes.Length; n++) {
				if (size <= iconSizes [n].Width)
					return (Gtk.IconSize)n;
			}
			return Gtk.IconSize.Dialog;
		}

		public static double GetBestSizeFitSize (double size)
		{
			var s = GetBestSizeFit (size);
			return iconSizes [(int)s].Width;
		}
		
		public static Gdk.Pixbuf ToPixbuf (this Image image, Gtk.IconSize defaultIconSize)
		{
			if (!image.HasFixedSize) {
				var s = iconSizes [(int)defaultIconSize];
				image = image.WithSize (s.Width, s.Height);
			}
			return (Gdk.Pixbuf)Toolkit.GetBackend (image.ToBitmap ());
		}
		
	}
}

