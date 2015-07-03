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
using System.Collections.Generic;
using System.Text;
using Xwt.Backends;
using Xwt.Drawing;

#if MONOMAC
using nfloat = System.Single;
using CGRect = System.Drawing.RectangleF;
using CGPoint = System.Drawing.PointF;
using CGSize = System.Drawing.SizeF;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.CoreImage;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#else
using AppKit;
using CoreGraphics;
using CoreImage;
using Foundation;
using ObjCRuntime;
#endif

using RectangleF = System.Drawing.RectangleF;
using SizeF = System.Drawing.SizeF;

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
			nfloat y = (nfloat)rect.Y;
			if (v.Superview != null)
				y = v.Superview.Frame.Height - y - (float)rect.Height;
			v.Frame = new CGRect ((nfloat)rect.X, y, (nfloat)rect.Width, (nfloat)rect.Height);
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

		static readonly CGColorSpace DeviceRgbColorSpace = CGColorSpace.CreateDeviceRGB ();
		
		public static CGColor ToCGColor (this Color col)
		{
			return new CGColor (DeviceRgbColorSpace, new nfloat[] {
				(nfloat)col.Red, (nfloat)col.Green, (nfloat)col.Blue, (nfloat)col.Alpha
			});
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

		public static Size ToXwtSize (this CGSize s)
		{
			return new Size (s.Width, s.Height);
		}

		public static CGRect ToCGRect (this Rectangle r)
		{
			return new CGRect ((nfloat)r.X, (nfloat)r.Y, (nfloat)r.Width, (nfloat)r.Height);
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

#if MONOMAC
		static Selector selCopyWithZone = new Selector ("copyWithZone:");
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
				var count = ob.RetainCount;
				if (count == 1)
					markedForDelete.Add (ob);
			}
			foreach (NSObject ob in markedForDelete)
				copyPool.Remove (ob);

			lastCopyPoolDrain = DateTime.Now;
		}
#else
		public static void MakeCopiable<T> () where T:ICopiableObject
		{
			// Nothing to do for XamMac
		}
		public static void DrainObjectCopyPool ()
		{
			// Nothing to do for XamMac
		}
#endif

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
			if (idesc.IsNull)
				return null;
			var img = (NSImage)idesc.Backend;
			if (img is CustomImage)
				img = ((CustomImage)img).Clone ();
			else {
				img = (NSImage)img.Copy ();
			}
			img.Size = new CGSize ((nfloat)idesc.Size.Width, (nfloat)idesc.Size.Height);
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
			ns.BeginEditing ();
			foreach (var att in ft.Attributes) {
				var r = new NSRange (att.StartIndex, att.Count);
				if (att is BackgroundTextAttribute) {
					var xa = (BackgroundTextAttribute)att;
					ns.AddAttribute (NSStringAttributeKey.BackgroundColor, xa.Color.ToNSColor (), r);
				}
				else if (att is ColorTextAttribute) {
					var xa = (ColorTextAttribute)att;
					ns.AddAttribute (NSStringAttributeKey.ForegroundColor, xa.Color.ToNSColor (), r);
				}
				else if (att is UnderlineTextAttribute) {
					var xa = (UnderlineTextAttribute)att;
					int style = xa.Underline ? 0x01 /*NSUnderlineStyleSingle*/ : 0;
					ns.AddAttribute (NSStringAttributeKey.UnderlineStyle, (NSNumber)style, r);
				}
				else if (att is FontStyleTextAttribute) {
					var xa = (FontStyleTextAttribute)att;
					if (xa.Style == FontStyle.Italic) {
						Messaging.void_objc_msgSend_int_NSRange (ns.Handle, applyFontTraits.Handle, (IntPtr)(long)NSFontTraitMask.Italic, r);
					} else if (xa.Style == FontStyle.Oblique) {
						ns.AddAttribute (NSStringAttributeKey.Obliqueness, (NSNumber)0.2f, r);
					} else {
						ns.AddAttribute (NSStringAttributeKey.Obliqueness, (NSNumber)0.0f, r);
						Messaging.void_objc_msgSend_int_NSRange (ns.Handle, applyFontTraits.Handle, (IntPtr)(long)NSFontTraitMask.Unitalic, r);
					}
				}
				else if (att is FontWeightTextAttribute) {
					var xa = (FontWeightTextAttribute)att;
					var trait = xa.Weight >= FontWeight.Bold ? NSFontTraitMask.Bold : NSFontTraitMask.Unbold;
					Messaging.void_objc_msgSend_int_NSRange (ns.Handle, applyFontTraits.Handle, (IntPtr)(long) trait, r);
				}
				else if (att is LinkTextAttribute) {
					var xa = (LinkTextAttribute)att;
					ns.AddAttribute (NSStringAttributeKey.Link, new NSUrl (xa.Target.ToString ()), r);
					ns.AddAttribute (NSStringAttributeKey.ForegroundColor, NSColor.Blue, r);
					ns.AddAttribute (NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 ((int)NSUnderlineStyle.Single), r);
				}
				else if (att is StrikethroughTextAttribute) {
					var xa = (StrikethroughTextAttribute)att;
					int style = xa.Strikethrough ? 0x01 /*NSUnderlineStyleSingle*/ : 0;
					ns.AddAttribute (NSStringAttributeKey.StrikethroughStyle, (NSNumber)style, r);
				}
				else if (att is FontTextAttribute) {
					var xa = (FontTextAttribute)att;
					var nf = ((FontData)Toolkit.GetBackend (xa.Font)).Font;
					ns.AddAttribute (NSStringAttributeKey.Font, nf, r);
				}
			}
			ns.EndEditing ();
			return ns;
		}

		/// <summary>
		/// Removes the mnemonics (underscore character) from a string.
		/// </summary>
		/// <returns>The string with the mnemonics unescaped.</returns>
		/// <param name="text">The string.</param>
		/// <remarks>
		/// Single underscores are removed. Double underscores are replaced with single underscores (unescaped).
		/// </remarks>
		public static string RemoveMnemonic(this string str)
		{
			if (str == null)
				return null;
			var newText = new StringBuilder ();
			for (int i = 0; i < str.Length; i++) {
				if (str [i] != '_')
					newText.Append (str [i]);
				else if (i < str.Length && str [i + 1] == '_') {
					newText.Append ('_');
					i++;
				}
			}
			return newText.ToString ();
		}

		public static CheckBoxState ToXwtState (this NSCellStateValue state)
		{
			switch (state) {
				case NSCellStateValue.Mixed:
					return CheckBoxState.Mixed;
				case NSCellStateValue.On:
					return CheckBoxState.On;
				case NSCellStateValue.Off:
					return CheckBoxState.Off;
				default:
					throw new ArgumentOutOfRangeException ();
			}
		}
		
		public static NSCellStateValue ToMacState (this CheckBoxState state)
		{
			switch (state) {
			case CheckBoxState.Mixed:
				return NSCellStateValue.Mixed;
			case CheckBoxState.On:
				return NSCellStateValue.On;
			case CheckBoxState.Off:
				return NSCellStateValue.Off;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		public static ModifierKeys ToXwtValue (this NSEventModifierMask e)
		{
			ModifierKeys m = ModifierKeys.None;
			if (e.HasFlag (NSEventModifierMask.ControlKeyMask))
				m |= ModifierKeys.Control;
			if (e.HasFlag (NSEventModifierMask.AlternateKeyMask))
				m |= ModifierKeys.Alt;
			if (e.HasFlag (NSEventModifierMask.CommandKeyMask))
				m |= ModifierKeys.Command;
			if (e.HasFlag (NSEventModifierMask.ShiftKeyMask))
				m |= ModifierKeys.Shift;
			return m;
		}



		public static NSTableViewGridStyle ToMacValue (this GridLines value)
		{
			switch (value)
			{
				case GridLines.Both:
					return (NSTableViewGridStyle.SolidHorizontalLine | NSTableViewGridStyle.SolidVerticalLine);
				case GridLines.Horizontal:
					return NSTableViewGridStyle.SolidHorizontalLine;
				case GridLines.Vertical:
					return NSTableViewGridStyle.SolidVerticalLine;
				case GridLines.None:
					return NSTableViewGridStyle.None;
			}
			throw new InvalidOperationException("Invalid GridLines value: " + value);
		}

		public static GridLines ToXwtValue (this NSTableViewGridStyle value)
		{
			if (value.HasFlag (NSTableViewGridStyle.SolidHorizontalLine)) {
				if (value.HasFlag (NSTableViewGridStyle.SolidVerticalLine))
					return GridLines.Both;
				else
					return GridLines.Horizontal;
			}
			if (value.HasFlag (NSTableViewGridStyle.SolidVerticalLine))
				return GridLines.Vertical;

			return GridLines.None;
		}

		public static NSDatePickerElementFlags ToMacValue (this DatePickerStyle style)
		{
			switch (style) {
				case DatePickerStyle.Date:
					return NSDatePickerElementFlags.YearMonthDateDay;
				case DatePickerStyle.DateTime:
					return NSDatePickerElementFlags.YearMonthDateDay | NSDatePickerElementFlags.HourMinuteSecond;
				case DatePickerStyle.Time:
					return NSDatePickerElementFlags.HourMinuteSecond;
			}
			return NSDatePickerElementFlags.YearMonthDate;
		}

		public static DatePickerStyle ToXwtValue (this NSDatePickerElementFlags flags)
		{
			switch (flags) {
				case NSDatePickerElementFlags.HourMinuteSecond:
					return DatePickerStyle.Time;
				case NSDatePickerElementFlags.YearMonthDate:
				case NSDatePickerElementFlags.YearMonthDateDay:
					return DatePickerStyle.Date;
				default:
					return DatePickerStyle.DateTime;
			}
		}

		public static void DrawWithColorTransform (this NSView view, Color? color, Action drawDelegate)
		{
			if (color.HasValue) {
				if (view.Frame.Size.Width <= 0 || view.Frame.Size.Height <= 0)
					return;

				// render view to image
				var image = new NSImage(view.Frame.Size);
				image.LockFocusFlipped(!view.IsFlipped);
				drawDelegate ();
				image.UnlockFocus();

				// create Core image for transformation
				var rr = new CGRect(0, 0, view.Frame.Size.Width, view.Frame.Size.Height);
				var ciImage = CIImage.FromCGImage(image.AsCGImage (ref rr, NSGraphicsContext.CurrentContext, null));

				// apply color matrix
				var transformColor = new CIColorMatrix();
				transformColor.SetDefaults();
				transformColor.Image = ciImage;
				transformColor.RVector = new CIVector(0, (float)color.Value.Red, 0);
				transformColor.GVector = new CIVector((float)color.Value.Green, 0, 0);
				transformColor.BVector = new CIVector(0, 0, (float)color.Value.Blue);
				ciImage = (CIImage)transformColor.ValueForKey(new NSString("outputImage"));

				var ciCtx = CIContext.FromContext(NSGraphicsContext.CurrentContext.GraphicsPort, null);
				ciCtx.DrawImage (ciImage, rr, rr);
			} else
				drawDelegate();
		}
	}

	public interface ICopiableObject
	{
		void CopyFrom (object other);
	}
}

