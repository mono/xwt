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
using MonoMac.AppKit;
using Xwt.Drawing;
using MonoMac.CoreGraphics;
using SizeF = System.Drawing.SizeF;
using RectangleF = System.Drawing.RectangleF;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;
using System.Collections.Generic;
using Xwt.Backends;
using System.Runtime.InteropServices;

namespace Xwt.Mac
{
	public static class Util
	{
		public static readonly string DeviceRGBString = NSColorSpace.DeviceRGB.ToString ();
		static CGColorSpace deviceRGB, pattern;

		public static CGColorSpace DeviceRGBColorSpace {
			get {
				if (deviceRGB == null)
					deviceRGB = CGColorSpace.CreateDeviceRGB ();
				return deviceRGB;
			}
		}

		public static CGColorSpace PatternColorSpace {
			get {
				if (pattern == null)
					pattern = CGColorSpace.CreatePattern (null);
				return pattern;
			}
		}

		public static double WidgetX (this NSView v)
		{
			return (double) v.Frame.X;
		}
		
		public static double WidgetY (this NSView v)
		{
			return (double) v.Frame.Y;
		}
		
		public static double WidgetWidth (this NSView v)
		{
			return (double) (v.Frame.Width);
		}
		
		public static double WidgetHeight (this NSView v)
		{
			return (double) (v.Frame.Height);
		}
		
		public static Rectangle WidgetBounds (this NSView v)
		{
			return new Rectangle (v.WidgetX(), v.WidgetY(), v.WidgetWidth(), v.WidgetHeight());
		}
		
		public static void SetWidgetBounds (this NSView v, Rectangle rect)
		{
			float y = (float)rect.Y;
			if (v.Superview != null)
				y = v.Superview.Frame.Height - y - (float)rect.Height;
			v.Frame = new System.Drawing.RectangleF ((float)rect.X, y, (float)rect.Width, (float)rect.Height);
		}

		public static Alignment ToAlignment (this NSTextAlignment align)
		{
			switch (align) {
			case NSTextAlignment.Center: return Alignment.Center;
			case NSTextAlignment.Right: return Alignment.End;
			default: return Alignment.Start;
			}
		}

		public static NSTextAlignment ToNSTextAlignment (this Alignment align)
		{
			switch (align) {
			case Alignment.Center: return NSTextAlignment.Center;
			case Alignment.End: return NSTextAlignment.Right;
			default: return NSTextAlignment.Left;
			}
		}
		
		public static NSColor ToNSColor (this Color col)
		{
			return NSColor.FromDeviceRgba ((float)col.Red, (float)col.Green, (float)col.Blue, (float)col.Alpha);
		}
		
		public static CGColor ToCGColor (this Color col)
		{
			return new CGColor ((float)col.Red, (float)col.Green, (float)col.Blue, (float)col.Alpha);
		}

		public static Color ToXwtColor (this NSColor col)
		{
			col = col.UsingColorSpace (DeviceRGBString);
			return new Color (col.RedComponent, col.GreenComponent, col.BlueComponent, col.AlphaComponent);
		}
		
		public static Color ToXwtColor (this CGColor col)
		{
			var cs = col.Components;
			return new Color (cs[0], cs[1], cs[2], col.Alpha);
		}

		public static Size ToXwtSize (this SizeF s)
		{
			return new Size (s.Width, s.Height);
		}

		public static RectangleF ToRectangleF (this Rectangle r)
		{
			return new RectangleF ((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
		}

		// /System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Headers/IconsCore.h
		public static int ToIconType (string id)
		{
			switch (id) {
			case StockIconId.Error:       return 1937010544; // 'stop'
			case StockIconId.Warning:     return 1667331444; // 'caut'
			case StockIconId.Information: return 1852798053; // 'note'
			case StockIconId.Question:    return 1903519091; // 'ques'
			case StockIconId.Remove:      return 1952736620; // 'tdel'
			}
			return 0;
		}

		/* To get the above int values, pass the four char code thru this:
		public static int C (string code)
		{
			return ((int)code[0]) << 24
			     | ((int)code[1]) << 16
			     | ((int)code[2]) << 8
			     | ((int)code[3]);
		}
		*/

		public static SizeF ToIconSize (IconSize size)
		{
			switch (size) {
			case IconSize.Small: return new SizeF (16f, 16f);
			case IconSize.Large: return new SizeF (64f, 64f);
			}
			return new SizeF (32f, 32f);
		}

		public static string ToUTI (this TransferDataType dt)
		{
			if (dt == TransferDataType.Uri)
				return NSPasteboard.NSUrlType;
			if (dt == TransferDataType.Text)
				return NSPasteboard.NSStringType;
			if (dt == TransferDataType.Rtf)
				return NSPasteboard.NSRtfType;
			if (dt == TransferDataType.Html)
				return NSPasteboard.NSHtmlType;
			if (dt == TransferDataType.Image)
				return NSPasteboard.NSTiffType;

			return dt.Id;
		}

		static Selector selCopyWithZone = new Selector ("copyWithZone:");
		static Selector selRetainCount = new Selector ("retainCount");
		static DateTime lastCopyPoolDrain = DateTime.Now;
		static List<object> copyPool = new List<object> ();

		/// <summary>
		/// Implements the NSCopying protocol in a class. The class must implement ICopiableObject.
		/// The method ICopiableObject.CopyFrom will be called to make the copy of the object
		/// </summary>
		/// <typeparam name="T">Type for which to enable copying</typeparam>
		public static void MakeCopiable<T> () where T:ICopiableObject
		{
			Class c = new Class (typeof(T));
			c.AddMethod (selCopyWithZone.Handle, new Func<IntPtr, IntPtr, IntPtr, IntPtr> (MakeCopy), "i@:@");
		}
		
		static IntPtr MakeCopy (IntPtr sender, IntPtr sel, IntPtr zone)
		{
			var thisOb = (ICopiableObject) Runtime.GetNSObject (sender);

			// Makes a copy of the object by calling the default implementation of copyWithZone
			IntPtr copyHandle = Messaging.IntPtr_objc_msgSendSuper_IntPtr(((NSObject)thisOb).SuperHandle, selCopyWithZone.Handle, zone);
			var copyOb = (ICopiableObject) Runtime.GetNSObject (copyHandle);

			// Copy of managed data
			copyOb.CopyFrom (thisOb);

			// Copied objects are for internal use of the Cocoa framework. We need to keep a reference of the
			// managed object until the the framework doesn't need it anymore.

			if ((DateTime.Now - lastCopyPoolDrain).TotalSeconds > 2)
				DrainObjectCopyPool ();

			copyPool.Add (copyOb);

			return ((NSObject)copyOb).Handle;
		}

		public static void DrainObjectCopyPool ()
		{
			// Objects in the pool have been created by Cocoa, so there should be no managed references
			// other than the ones we keep in the pool. An object can be removed from the pool if it
			// has only 1 reference left (the managed one)

			List<NSObject> markedForDelete = new List<NSObject> ();
			
			foreach (NSObject ob in copyPool) {
				uint count = Messaging.UInt32_objc_msgSend (ob.Handle, selRetainCount.Handle);
				if (count == 1)
					markedForDelete.Add (ob);
			}
			foreach (NSObject ob in markedForDelete)
				copyPool.Remove (ob);

			lastCopyPoolDrain = DateTime.Now;
		}

		public static NSBitmapImageFileType ToMacFileType (this ImageFileType type)
		{
			switch (type) {
			case ImageFileType.Png: return NSBitmapImageFileType.Png;
			case ImageFileType.Jpeg: return NSBitmapImageFileType.Jpeg;
			case ImageFileType.Bmp: return NSBitmapImageFileType.Bmp;
			default:
				throw new NotSupportedException ();
			}
		}

		public static NSImage ToNSImage (this ImageDescription idesc)
		{
			var img = (NSImage)idesc.Backend;
			if (img is CustomImage)
				img = ((CustomImage)img).Clone ();
			else {
				img = (NSImage)img.Copy ();
			}
			img.Size = new System.Drawing.SizeF ((float)idesc.Size.Width, (float)idesc.Size.Height);
			return img;
		}


		public static int ToUnderlineStyle (FontStyle style)
		{
			return 1;
		}

		static Selector applyFontTraits = new Selector ("applyFontTraits:range:");

		public static NSAttributedString ToAttributedString (this FormattedText ft)
		{
			NSMutableAttributedString ns = new NSMutableAttributedString (ft.Text);
			foreach (var att in ft.Attributes) {
				var r = new NSRange (att.StartIndex, att.Count);
				if (att is BackgroundTextAttribute) {
					var xa = (BackgroundTextAttribute)att;
					ns.AddAttribute (NSAttributedString.BackgroundColorAttributeName, xa.Color.ToNSColor (), r);
				}
				else if (att is ColorTextAttribute) {
					var xa = (ColorTextAttribute)att;
					ns.AddAttribute (NSAttributedString.ForegroundColorAttributeName, xa.Color.ToNSColor (), r);
				}
				else if (att is UnderlineTextAttribute) {
					var xa = (UnderlineTextAttribute)att;
					int style = xa.Underline ? 0x01 /*NSUnderlineStyleSingle*/ : 0;
					ns.AddAttribute (NSAttributedString.UnderlineStyleAttributeName, (NSNumber)style, r);
				}
				else if (att is FontStyleTextAttribute) {
					var xa = (FontStyleTextAttribute)att;
					if (xa.Style == FontStyle.Italic) {
						Messaging.void_objc_msgSend_int_NSRange (ns.Handle, applyFontTraits.Handle, (int)NSFontTraitMask.Italic, r);
					} else if (xa.Style == FontStyle.Oblique) {
						ns.AddAttribute (NSAttributedString.ObliquenessAttributeName, (NSNumber)0.2f, r);
					} else {
						ns.AddAttribute (NSAttributedString.ObliquenessAttributeName, (NSNumber)0.0f, r);
						Messaging.void_objc_msgSend_int_NSRange (ns.Handle, applyFontTraits.Handle, (int)NSFontTraitMask.Unitalic, r);
					}
				}
				else if (att is FontWeightTextAttribute) {
					var xa = (FontWeightTextAttribute)att;
					var trait = xa.Weight >= FontWeight.Bold ? NSFontTraitMask.Bold : NSFontTraitMask.Unbold;
					Messaging.void_objc_msgSend_int_NSRange (ns.Handle, applyFontTraits.Handle, (int) trait, r);
				}
				else if (att is LinkTextAttribute) {
					var xa = (LinkTextAttribute)att;
					ns.AddAttribute (NSAttributedString.LinkAttributeName, (NSString)xa.Target, r);
				}
				else if (att is StrikethroughTextAttribute) {
					var xa = (StrikethroughTextAttribute)att;
					int style = xa.Strikethrough ? 0x01 /*NSUnderlineStyleSingle*/ : 0;
					ns.AddAttribute (NSAttributedString.StrikethroughStyleAttributeName, (NSNumber)style, r);
				}
				else if (att is FontTextAttribute) {
					var xa = (FontTextAttribute)att;
					var nf = (NSFont)Toolkit.GetBackend (xa.Font);
					ns.AddAttribute (NSAttributedString.FontAttributeName, nf, r);
				}
			}
			return ns;
		}
	}

	public interface ICopiableObject
	{
		void CopyFrom (object other);
	}
}

