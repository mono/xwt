// 
// ImageTableCell.cs
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
using AppKit;
using CoreGraphics;
using Xwt.Backends;
using System.Linq;

namespace Xwt.Mac
{
	class ImageTableCell : NSImageView, ICellRenderer
	{
		NSTrackingArea trackingArea;

		IImageCellViewFrontend Frontend {
			get { return (IImageCellViewFrontend)Backend.Frontend; }
		}

		public CellViewBackend Backend { get; set; }

		public CompositeCell CellContainer { get; set; }

		public NSView CellView { get { return this; } }

		public ImageTableCell ()
		{
			Cell = new ImageViewCell ();
		}

		public void Fill ()
		{
			if (Frontend.Image != null) {
				Image = Frontend.Image.ToImageDescription (Backend.Context).ToNSImage ();
				SetFrameSize (Image.Size);
			} else
				SetFrameSize (CoreGraphics.CGSize.Empty);
			Hidden = !Frontend.Visible;
			this.ApplyAcessibilityProperties ();
		}

		public override CoreGraphics.CGSize FittingSize {
			get {
				if (Image == null)
					return CGSize.Empty;
				return Image.Size;
			}
		}

		public override void SizeToFit()
		{
			if (Frame.Size.IsEmpty && Image != null)
				SetFrameSize (Image.Size);
		}

		public void CopyFrom (object other)
		{
			var ob = (ImageTableCell)other;
			Backend = ob.Backend;
		}

		public override NSImage Image {
			get {
				return base.Image;
			}
			set {
				base.Image = value;
				var cell = Cell as ImageViewCell;
				if (cell != null) {
					var customImage = value as CustomImage;
					// don't switch styles automatically if "sel" hes been explicitely set
					cell.AutoselectImageStyle = !customImage?.Image.Styles.Contains ("sel") ?? false;
				}
			}
		}

		public override void UpdateTrackingAreas ()
		{
			if (trackingArea != null) {
				RemoveTrackingArea (trackingArea);
				trackingArea.Dispose ();
			}
			var options = NSTrackingAreaOptions.MouseMoved | NSTrackingAreaOptions.ActiveInKeyWindow | NSTrackingAreaOptions.MouseEnteredAndExited;
			trackingArea = new NSTrackingArea (Bounds, options, this, null);
			AddTrackingArea (trackingArea);
		}

		public override void RightMouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.RightMouseDown (theEvent); 
		}

		public override void RightMouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.RightMouseUp (theEvent); 
		}

		public override void MouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.MouseDown (theEvent); 
		}

		public override void MouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.MouseUp (theEvent); 
		}

		public override void OtherMouseDown (NSEvent theEvent)
		{
			if (!this.HandleMouseDown (theEvent))
				base.OtherMouseDown (theEvent);
		}

		public override void OtherMouseUp (NSEvent theEvent)
		{
			if (!this.HandleMouseUp (theEvent))
				base.OtherMouseUp (theEvent);
		}

		public override void MouseEntered (NSEvent theEvent)
		{
			this.HandleMouseEntered (theEvent);
				base.MouseEntered (theEvent);
		}

		public override void MouseExited (NSEvent theEvent)
		{
			this.HandleMouseExited (theEvent);
				base.MouseExited (theEvent);
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.MouseMoved (theEvent);
		}

		public override void MouseDragged (NSEvent theEvent)
		{
			if (!this.HandleMouseMoved (theEvent))
				base.MouseDragged (theEvent);
		}

		public override void KeyDown (NSEvent theEvent)
		{
			if (!this.HandleKeyDown (theEvent))
				base.KeyDown (theEvent);
		}

		public override void KeyUp (NSEvent theEvent)
		{
			if (!this.HandleKeyUp (theEvent))
				base.KeyUp (theEvent);
		}
	}

	class ImageViewCell : NSImageCell
	{
		internal bool AutoselectImageStyle { get; set; }

		public override NSBackgroundStyle BackgroundStyle {
			get {
				return base.BackgroundStyle;
			}
			set {
				base.BackgroundStyle = value;
				if (AutoselectImageStyle) {
					var customImage = Image as CustomImage;
					if (customImage != null) {
						// no need to care about light/dark NSAppearance, BackgroundStyle won't be flipped
						if (value == NSBackgroundStyle.Dark) {
							customImage.Image.Styles = customImage.Image.Styles.Add ("sel");
						} else {
							customImage.Image.Styles = customImage.Image.Styles.Remove ("sel");
						}
					}
				}
			}
		}
	}
}
