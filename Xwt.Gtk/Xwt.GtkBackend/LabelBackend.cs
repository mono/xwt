// 
// LabelBackend.cs
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
using Xwt.Backends;
using Xwt.Drawing;
using Xwt.CairoBackend;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;


namespace Xwt.GtkBackend
{
	public partial class LabelBackend: WidgetBackend, ILabelBackend
	{
		Color? textColor;
		List<LabelLink> links;
		TextIndexer indexer;

		public LabelBackend ()
		{
			Widget = new Gtk.Label ();
			Label.Show ();
			Label.Xalign = 0;
			Label.Yalign = 0.5f;
		}
		
		new ILabelEventSink EventSink {
			get { return (ILabelEventSink)base.EventSink; }
		}

		protected Gtk.Label Label {
			get {
				if (Widget is Gtk.Label)
					return (Gtk.Label) Widget;
				else
					return (Gtk.Label) ((Gtk.EventBox)base.Widget).Child;
			}
		}

		bool linkEventEnabled;

		void EnableLinkEvents ()
		{
			if (!linkEventEnabled) {
				linkEventEnabled = true;
				AllocEventBox ();
				EventsRootWidget.AddEvents ((int)Gdk.EventMask.PointerMotionMask);
				EventsRootWidget.MotionNotifyEvent += HandleMotionNotifyEvent;
				EventsRootWidget.AddEvents ((int)Gdk.EventMask.ButtonReleaseMask);
				EventsRootWidget.ButtonReleaseEvent += HandleButtonReleaseEvent;
				EventsRootWidget.AddEvents ((int)Gdk.EventMask.LeaveNotifyMask);
				EventsRootWidget.LeaveNotifyEvent += HandleLeaveNotifyEvent;
			}
		}

		bool mouseInLink;
		CursorType normalCursor;

		void HandleMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			var li = FindLink (args.Event.X, args.Event.Y);
			if (li != null) {
				if (!mouseInLink) {
					mouseInLink = true;
					normalCursor = CurrentCursor;
					SetCursor (CursorType.Hand);
				}
			} else {
				if (mouseInLink) {
					mouseInLink = false;
					SetCursor (normalCursor ?? CursorType.Arrow);
				}
			}
		}
		
		void HandleButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			var li = FindLink (args.Event.X, args.Event.Y);

			if (li != null) {
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnLinkClicked (li.Target);
				});
				args.RetVal = true;
			};
		}
		
		void HandleLeaveNotifyEvent (object o, Gtk.LeaveNotifyEventArgs args)
		{
			if (mouseInLink) {
				mouseInLink = false;
				SetCursor (normalCursor ?? CursorType.Arrow);
			}
		}

		LabelLink FindLink (double px, double py)
		{
			if (links == null)
				return null;

			var alloc = Label.Allocation;

			int offsetX, offsetY;
			Label.GetLayoutOffsets (out offsetX, out offsetY);

			var x = (px - offsetX + alloc.X) * Pango.Scale.PangoScale;
			var y = (py - offsetY + alloc.Y) * Pango.Scale.PangoScale;

			int byteIndex, trailing;
			if (!Label.Layout.XyToIndex ((int)x, (int)y, out byteIndex, out trailing))
				return null;

			foreach (var li in links)
				if (byteIndex >= li.StartIndex && byteIndex <= li.EndIndex)
					return li;

			return null;
		}

		public virtual string Text {
			get { return Label.Text; }
			set {
				links = null;
				indexer = null;
				Label.Text = value;
			}
		}

		public void SetFormattedText (FormattedText text)
		{
			Label.Text = text.Text;
			var list = new FastPangoAttrList ();
			indexer = new TextIndexer (text.Text);
			list.AddAttributes (indexer, text.Attributes);
			gtk_label_set_attributes (Label.Handle, list.Handle);

			if (links != null)
				links.Clear ();

			foreach (var attr in text.Attributes.OfType<LinkTextAttribute> ()) {
				LabelLink ll = new LabelLink () {
					StartIndex = indexer.IndexToByteIndex (attr.StartIndex),
					EndIndex = indexer.IndexToByteIndex (attr.StartIndex + attr.Count),
					Target = attr.Target
				};
				if (links == null) {
					links = new List<LabelLink> ();
					EnableLinkEvents ();
				}
				links.Add (ll);
			}

			if (links == null || links.Count == 0) {
				links = null;
				indexer = null;
			}
		}

		[DllImport (GtkInterop.LIBGTK, CallingConvention=CallingConvention.Cdecl)]
		static extern void gtk_label_set_attributes (IntPtr label, IntPtr attrList);

		public Xwt.Drawing.Color TextColor {
			get {
				return textColor.HasValue ? textColor.Value : Widget.Style.Foreground (Gtk.StateType.Normal).ToXwtValue ();
			}
			set {
				var color = value.ToGtkValue ();
				var attr = new Pango.AttrForeground (color.Red, color.Green, color.Blue);
				var attrs = new Pango.AttrList ();
				attrs.Insert (attr);

				Label.Attributes = attrs;

				textColor = value;
				Label.QueueDraw ();
			}
		}


		Alignment alignment;

		public Alignment TextAlignment {
			get {
				return alignment;
			}
			set {
				alignment = value;
				SetAlignment ();
			}
		}

		protected void SetAlignment ()
		{
			switch (alignment) {
				case Alignment.Start:
					Label.Justify = Gtk.Justification.Left;
					break;
				case Alignment.End:
					Label.Justify = Gtk.Justification.Right;
					break;
				case Alignment.Center:
					Label.Justify = Gtk.Justification.Center;
					break;
			}
			SetAlignmentGtk ();
		}
		
		public EllipsizeMode Ellipsize {
			get {
				return Label.Ellipsize.ToXwtValue ();
			}
			set {
				Label.Ellipsize = value.ToGtkValue ();
			}
		}

		public WrapMode Wrap {
			get {
				if (!Label.LineWrap)
					return WrapMode.None;
				else {
					switch (Label.LineWrapMode) {
					case Pango.WrapMode.Char:
						return WrapMode.Character;
					case Pango.WrapMode.Word:
						return WrapMode.Word;
					case Pango.WrapMode.WordChar:
						return WrapMode.WordAndCharacter;
					default:
						return WrapMode.None;
					}
				}
			}
			set {
				ToggleSizeCheckEventsForWrap (value);
				if (value == WrapMode.None){
					if (Label.LineWrap) {
						Label.LineWrap = false;
					}
				} else {
					if (!Label.LineWrap) {
						Label.LineWrap = true;
					}
					switch (value) {
					case WrapMode.Character:
						Label.LineWrapMode = Pango.WrapMode.Char;
						break;
					case WrapMode.Word:
						Label.LineWrapMode = Pango.WrapMode.Word;
						break;
					case WrapMode.WordAndCharacter:
						Label.LineWrapMode = Pango.WrapMode.WordChar;
						break;
					}
				}
				SetAlignment ();
			}
		}
	}

	class LabelLink
	{
		public int StartIndex;
		public int EndIndex;
		public Uri Target;
	}
}

