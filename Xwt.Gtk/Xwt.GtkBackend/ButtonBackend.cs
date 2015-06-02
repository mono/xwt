// 
// ButtonBackend.cs
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


namespace Xwt.GtkBackend
{
	public partial class ButtonBackend: WidgetBackend, IButtonBackend
	{
		protected bool ignoreClickEvents;
		ImageDescription image;
		Pango.FontDescription customFont;
		
		public ButtonBackend ()
		{
		}

		public override void Initialize ()
		{
			NeedsEventBox = false;
			Widget = new Gtk.Button ();
			base.Widget.Show ();
			
		}
		
		protected new Gtk.Button Widget {
			get { return (Gtk.Button)base.Widget; }
			set { base.Widget = value; }
		}
		
		protected new IButtonEventSink EventSink {
			get { return (IButtonEventSink)base.EventSink; }
		}

		protected override void OnSetBackgroundColor (Color color)
		{
			Widget.SetBackgroundColor (color);
		}

		public override object Font {
			get {
				return base.Font;
			}
			set {
				base.Font = value;
				customFont = value as Pango.FontDescription;
				SetButtonType (ButtonType.Normal);
			}
		}
		
		public void SetContent (string label, bool useMnemonic, ImageDescription image, ContentPosition position)
		{			
			Widget.UseUnderline = useMnemonic;
			this.image = image;

			if (label != null && label.Length == 0)
				label = null;
			
			Button b = (Button) Frontend;
			if (label != null && image.Backend == null && b.Type == ButtonType.Normal) {
				Widget.Label = label;
				return;
			}
			
			if (b.Type == ButtonType.Disclosure) {
				Widget.Label = null;
				Widget.Image = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
				Widget.Image.ShowAll ();
				return;
			}
			
			Gtk.Widget contentWidget = null;
			
			Gtk.Widget imageWidget = null;
			if (image.Backend != null)
				imageWidget = new ImageBox (ApplicationContext, image.WithDefaultSize (Gtk.IconSize.Button));

			Gtk.Label labelWidget = null;

			if (label != null && imageWidget == null) {
				contentWidget = labelWidget = new Gtk.Label (label);
			}
			else if (label == null && imageWidget != null) {
				contentWidget = imageWidget;
			}
			else if (label != null && imageWidget != null) {
				Gtk.Box box = position == ContentPosition.Left || position == ContentPosition.Right ? (Gtk.Box) new Gtk.HBox (false, 3) : (Gtk.Box) new Gtk.VBox (false, 3);
				labelWidget = new Gtk.Label (label) { UseUnderline = useMnemonic };
				
				if (position == ContentPosition.Left || position == ContentPosition.Top) {
					box.PackStart (imageWidget, false, false, 0);
					box.PackStart (labelWidget, false, false, 0);
				} else {
					box.PackStart (labelWidget, false, false, 0);
					box.PackStart (imageWidget, false, false, 0);
				}
				
				contentWidget = box;
			}
			var expandButtonContent = false;
			if (b.Type == ButtonType.DropDown) {
				if (contentWidget != null) {
					Gtk.HBox box = new Gtk.HBox (false, 3);
					box.PackStart (contentWidget, true, true, 3);
					box.PackStart (new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out), false, false, 0);
					contentWidget = box;
					expandButtonContent = true;
				} else
					contentWidget = new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.Out);
			}
			if (contentWidget != null) {
				contentWidget.ShowAll ();
				Widget.Label = null;
				Widget.Image = contentWidget;
				if (expandButtonContent) {
					var alignment = Widget.Child as Gtk.Alignment;
					if (alignment != null) {
						var box = alignment.Child as Gtk.Box;
						if (box != null) {
							alignment.Xscale = 1;
							box.SetChildPacking (box.Children [0], true, true, 0, Gtk.PackType.Start);
							if (labelWidget != null)
								labelWidget.Xalign = 0;
						}
					}
				}
				if (labelWidget != null) {
					labelWidget.UseUnderline = useMnemonic;
					if (customFont != null)
						labelWidget.ModifyFont (customFont);
				}
			} else
				Widget.Label = null;
		}
		
		public void SetButtonStyle (ButtonStyle style)
		{
			switch (style) {
			case ButtonStyle.Normal:
				SetMiniMode (false);
				Widget.Relief = Gtk.ReliefStyle.Normal;
				break;
			case ButtonStyle.Flat:
				SetMiniMode (false);
				Widget.Relief = Gtk.ReliefStyle.None;
				break;
			case ButtonStyle.Borderless:
				SetMiniMode (true);
				Widget.Relief = Gtk.ReliefStyle.None;
				break;
			}
		}
		
		public void SetButtonType (ButtonType type)
		{
			Button b = (Button) Frontend;
			SetContent (b.Label, b.UseMnemonic, image, b.ImagePosition);
		}
		
		public override void EnableEvent (object eventId)
		{
			base.EnableEvent (eventId);
			if (eventId is ButtonEvent) {
				switch ((ButtonEvent)eventId) {
				case ButtonEvent.Clicked: Widget.Clicked += HandleWidgetClicked; break;
				}
			}
		}
		
		public override void DisableEvent (object eventId)
		{
			base.DisableEvent (eventId);
			if (eventId is ButtonEvent) {
				switch ((ButtonEvent)eventId) {
				case ButtonEvent.Clicked: Widget.Clicked -= HandleWidgetClicked; break;
				}
			}
		}

		void HandleWidgetClicked (object sender, EventArgs e)
		{
			if (!ignoreClickEvents) {
				ApplicationContext.InvokeUserCode (delegate {
					EventSink.OnClicked ();
				});
			}
		}
		
		bool miniMode;
		
		protected void SetMiniMode (bool miniMode)
		{
//			Gtk.Rc.ParseString ("style \"Xwt.GtkBackend.CustomButton\" {\n GtkButton::inner-border = {0,0,0,0} GtkButton::child-displacement-x = {0} GtkButton::child-displacement-y = {0}\n }\n");
//			Gtk.Rc.ParseString ("widget \"*.Xwt.GtkBackend.CustomButton\" style  \"Xwt.GtkBackend.CustomButton\"\n");
//			Name = "Xwt.GtkBackend.CustomButton";
			
			if (this.miniMode == miniMode)
				return;
			this.miniMode = miniMode;
			if (miniMode) {
				Widget.SizeAllocated += HandleSizeAllocated;
			}
			SetMiniModeGtk(miniMode);
			Widget.QueueResize ();
		}

		[GLib.ConnectBefore]
		void HandleSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			Widget.Child.SizeAllocate (args.Allocation);
			args.RetVal = true;
		}
	}
}

