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
using System.Text;
using AppKit;
using CoreGraphics;
using CoreImage;
using CoreText;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;
using Xwt.Drawing;

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

		public static void SetAttributedString (this NSTextView view, NSAttributedString str, bool canOverrideTextColor)
		{
			var textColor = view.TextColor;
			view.TextStorage.SetString (str);
			
			// Workaround:
			// Apply the previous view's TextColor,
			// otherwise it would be reset to Black by the line above.
			if (canOverrideTextColor && textColor != null)
				view.TextColor = textColor;
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

		public static Point WidgetLocation (this NSView v)
		{
			return new Point (v.WidgetX (), v.WidgetY ());
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
			var calibrated = col.UsingColorSpace (DeviceRGBString);
			if (calibrated != null)
				return new Color (calibrated.RedComponent, calibrated.GreenComponent, calibrated.BlueComponent, calibrated.AlphaComponent);
			// some system colors can not be calibrated and UsingColorSpace returns null.
			// Use CGColor in this case, which should match the device already.
			return col.CGColor.ToXwtColor();
		}
		
		public static Color ToXwtColor (this CGColor col)
		{
			var cs = col.Components;
			return new Color (cs[0], cs[1], cs[2], col.Alpha);
		}

		public static CGSize ToCGSize (this Size s)
		{
			return new CGSize ((nfloat)s.Width, (nfloat)s.Height);
		}

		public static Size ToXwtSize (this CGSize s)
		{
			return new Size (s.Width, s.Height);
		}

		public static CGRect ToCGRect (this Rectangle r)
		{
			return new CGRect ((nfloat)r.X, (nfloat)r.Y, (nfloat)r.Width, (nfloat)r.Height);
		}

		public static Rectangle ToXwtRect (this CGRect r)
		{
			return new Rectangle (r.X, r.Y, r.Width, r.Height);
		}

		public static CGPoint ToCGPoint (this Point r)
		{
			return new CGPoint ((nfloat)r.X, (nfloat)r.Y);
		}

		public static Point ToXwtPoint (this CGPoint p)
		{
			return new Point (p.X, p.Y);
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

		public static CGSize ToIconSize (IconSize size)
		{
			switch (size) {
			case IconSize.Small: return new CGSize (16f, 16f);
			case IconSize.Large: return new CGSize (64f, 64f);
			}
			return new CGSize (32f, 32f);
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
		/*public static void MakeCopiable<T> () where T:ICopiableObject
		{
			// Nothing to do for XamMac
		}
		public static void DrainObjectCopyPool ()
		{
			// Nothing to do for XamMac
		}*/

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

		public static bool TriggersContextMenu (this NSEvent theEvent)
		{
			if (theEvent.ButtonNumber == 1 &&
					(NSEvent.CurrentPressedMouseButtons & 1 | NSEvent.CurrentPressedMouseButtons & 4) == 0) {
				return true;
			}

			if (theEvent.ButtonNumber == 0 && (theEvent.ModifierFlags & NSEventModifierMask.ControlKeyMask) != 0 &&
					(NSEvent.CurrentPressedMouseButtons & 2 | NSEvent.CurrentPressedMouseButtons & 4) == 0) {
				return true;
			}

			return false;
		}

		public static NSImage ToNSImage (this ImageDescription idesc)
		{
			if (idesc.IsNull)
				return null;
			var img = (NSImage)idesc.Backend;
			if (img is CustomImage) {
				img = ((CustomImage)img).Clone ();
				((CustomImage)img).Image = idesc;
			} else {
				img = (NSImage)img.Copy ();
			}
			img.Size = new CGSize ((nfloat)idesc.Size.Width, (nfloat)idesc.Size.Height);
			return img;
		}


		public static int ToUnderlineStyle (FontStyle style)
		{
			return 1;
		}

		public static int ToMacValue (this FontWeight weight)
		{
			switch (weight) {
			case FontWeight.Ultrathin:
				return 0;
			case FontWeight.Thin:
				return 1;
			case FontWeight.Ultralight:
				return 2;
			case FontWeight.Light:
				return 3;
			case FontWeight.Book:
				return 4;
			case FontWeight.Normal:
				return 5;
			case FontWeight.Medium:
				return 6;
			case FontWeight.Mediumbold:
				return 7;
			case FontWeight.Semibold:
				return 8;
			case FontWeight.Bold:
				return 9;
			case FontWeight.Ultrabold:
				return 10;
			case FontWeight.Heavy:
				return 11;
			case FontWeight.Ultraheavy:
				return 12;
			case FontWeight.Semiblack:
				return 13;
			case FontWeight.Black:
				return 14;
			case FontWeight.Ultrablack:
				return 15;
			default:
				return 15;
			}
		}

		public static NSFont WithWeight (this NSFont font, FontWeight weight)
		{
			int w = weight.ToMacValue ();
			var traits = NSFontManager.SharedFontManager.TraitsOfFont (font);
			traits |= weight >= FontWeight.Bold ? NSFontTraitMask.Bold : NSFontTraitMask.Unbold;
			traits &= weight >= FontWeight.Bold ? ~NSFontTraitMask.Unbold : ~NSFontTraitMask.Bold;
			return NSFontManager.SharedFontManager.FontWithFamily (font.FamilyName, traits, w, font.PointSize);
		}

		public static NSFont WithSize (this NSFont font, float size)
		{
			var w = NSFontManager.SharedFontManager.WeightOfFont (font);
			var traits = NSFontManager.SharedFontManager.TraitsOfFont (font);
			return NSFontManager.SharedFontManager.FontWithFamily (font.FamilyName, traits, w, size);
		}

		static Selector applyFontTraits = new Selector ("applyFontTraits:range:");

		public static NSMutableAttributedString ToAttributedString (this FormattedText ft)
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
				else if (att is FontSizeTextAttribute)
				{
					var xa = (FontSizeTextAttribute)att;
					ns.EnumerateAttribute (NSStringAttributeKey.Font, r, NSAttributedStringEnumeration.None, (NSObject value, NSRange range, ref bool stop) => {
						var font = value as NSFont;
						if (font == null) {
							font = NSFont.SystemFontOfSize (xa.Size);
						} else {
							font = font.WithSize (xa.Size);
						}
						ns.RemoveAttribute (NSStringAttributeKey.Font, r);
						ns.AddAttribute (NSStringAttributeKey.Font, font, r);
					});
				} 
				else if (att is LinkTextAttribute) {
					var xa = (LinkTextAttribute)att;
					if (xa.Target != null)
						ns.AddAttribute (NSStringAttributeKey.Link, new NSUrl (xa.Target.ToString ()), r);
					ns.AddAttribute (NSStringAttributeKey.ForegroundColor, Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToNSColor (), r);
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

					ns.EnumerateAttribute (NSStringAttributeKey.Font, r, NSAttributedStringEnumeration.None, (NSObject value, NSRange range, ref bool stop) => {
						var font = value as NSFont;
						if (font == null) {
							font = nf;
						} else {
							var w = NSFontManager.SharedFontManager.WeightOfFont (font);
							var traits = NSFontManager.SharedFontManager.TraitsOfFont (font);
							font = NSFontManager.SharedFontManager.FontWithFamily (nf.FamilyName, traits, w, font.PointSize);
						}
						ns.RemoveAttribute (NSStringAttributeKey.Font, r);
						ns.AddAttribute (NSStringAttributeKey.Font, font, r);
					});
				}
			}
			ns.EndEditing ();
			return ns;
		}

		public static NSMutableAttributedString WithAlignment (this NSMutableAttributedString ns, NSTextAlignment alignment)
		{
			if (ns == null)
				return null;
			
			ns.BeginEditing ();
			var r = new NSRange (0, ns.Length);
			ns.RemoveAttribute (NSStringAttributeKey.ParagraphStyle, r);
			var pstyle = NSParagraphStyle.Default.MutableCopy () as NSMutableParagraphStyle;
			pstyle.Alignment = alignment;
			ns.AddAttribute (NSStringAttributeKey.ParagraphStyle, pstyle, r);
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
			var newText = new StringBuilder (str.Length);
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

		static readonly Selector selConvertSizeToBacking = new Selector ("convertSizeToBacking:");

		public static void DrawWithColorTransform (this NSView view, Color? color, Action drawDelegate)
		{
			if (color.HasValue) {
				var size = view.Frame.Size;
				if (size.Width <= 0 || size.Height <= 0)
					return;

				// render view to image
				var image = new NSImage(size);
				image.LockFocusFlipped(!view.IsFlipped);
				drawDelegate ();
				image.UnlockFocus();

				// create Core image for transformation
				var ciImage = CIImage.FromCGImage(image.CGImage);

				CGSize displaySize;
				#pragma warning disable iOSAndMacApiUsageIssue
				if (view.RespondsToSelector (selConvertSizeToBacking))
					displaySize = view.ConvertSizeToBacking (size);
				else
					displaySize = view.ConvertSizeToBase (size);
				#pragma warning restore iOSAndMacApiUsageIssue

				// apply color matrix
				var transformColor = new CIColorMatrix();
				transformColor.SetDefaults();
				transformColor.Image = ciImage;
				transformColor.RVector = new CIVector(0, (float)color.Value.Red, 0);
				transformColor.GVector = new CIVector((float)color.Value.Green, 0, 0);
				transformColor.BVector = new CIVector(0, 0, (float)color.Value.Blue);
				using (var key = new NSString("outputImage"))
					ciImage = (CIImage)transformColor.ValueForKey(key);

				var ciCtx = CIContext.FromContext(NSGraphicsContext.CurrentContext.GraphicsPort, null);
				ciCtx.DrawImage (ciImage, new CGRect (CGPoint.Empty, size), new CGRect (CGPoint.Empty, displaySize));
			} else
				drawDelegate();
		}

		public static CGPoint ConvertPointFromEvent(this NSView view, NSEvent theEvent)
		{
			var point = theEvent.LocationInWindow;
			if (view.Window != null && theEvent.WindowNumber != view.Window.WindowNumber)
			{
				point = theEvent.Window.ConvertBaseToScreen(point);
				point = view.Window.ConvertScreenToBase(point);
			}
			return view.ConvertPointFromView(point, null);
		}

		public static PointerButton GetPointerButton (this NSEvent theEvent)
		{
			switch (theEvent.ButtonNumber) {
			case 0: return PointerButton.Left;
			case 1: return PointerButton.Right;
			case 2: return PointerButton.Middle;
			case 3: return PointerButton.ExtendedButton1;
			case 4: return PointerButton.ExtendedButton2;
			}
			return (PointerButton)0;
		}

		public static Accessibility.Role GetXwtRole (INSAccessibility widget)
		{
			var r = widget.AccessibilityRole;
			var sr = widget.AccessibilitySubrole;
			if (r == NSAccessibilityRoles.ButtonRole) {
				if (sr == NSAccessibilitySubroles.CloseButtonSubrole)
					return Accessibility.Role.ButtonClose;
				if (sr == NSAccessibilitySubroles.MinimizeButtonSubrole)
					return Accessibility.Role.ButtonMinimize;
				if (sr == NSAccessibilitySubroles.ZoomButtonSubrole)
					return Accessibility.Role.ButtonMaximize;
				if (sr == NSAccessibilitySubroles.FullScreenButtonSubrole)
					return Accessibility.Role.ButtonFullscreen;
				return Accessibility.Role.Button;
			}
			if (r == NSAccessibilityRoles.CellRole)
				return Accessibility.Role.Cell;
			if (r == NSAccessibilityRoles.CheckBoxRole)
				return Accessibility.Role.CheckBox;
			if (r == NSAccessibilityRoles.ColorWellRole)
				return Accessibility.Role.ColorChooser;
			if (r == NSAccessibilityRoles.ColumnRole)
				return Accessibility.Role.Column;
			if (r == NSAccessibilityRoles.ComboBoxRole)
				return Accessibility.Role.ComboBox;
			if (r == NSAccessibilityRoles.ComboBoxRole)
				return Accessibility.Role.ComboBox;
			if (r == NSAccessibilityRoles.DisclosureTriangleRole)
				return Accessibility.Role.Disclosure;
			if (r == NSAccessibilityRoles.GroupRole)
				return Accessibility.Role.Group;
			if (r == NSAccessibilityRoles.ImageRole)
				return Accessibility.Role.Image;
			if (r == NSAccessibilityRoles.LevelIndicatorRole)
				return Accessibility.Role.LevelIndicator;
			if (r == NSAccessibilityRoles.LinkRole)
				return Accessibility.Role.Link;
			if (r == NSAccessibilityRoles.ListRole)
				return Accessibility.Role.List;
			if (r == "NSAccessibilityMenuBarRole")
				return Accessibility.Role.MenuBar;
			if (r == NSAccessibilityRoles.MenuBarItemRole)
				return Accessibility.Role.MenuBarItem;
			if (r == NSAccessibilityRoles.MenuItemRole)
				return Accessibility.Role.MenuItem;
			if (r == NSAccessibilityRoles.MenuRole)
				return Accessibility.Role.Menu;
			if (r == NSAccessibilityRoles.OutlineRole)
				return Accessibility.Role.Tree;
			if (r == NSAccessibilityRoles.PopUpButtonRole)
				return Accessibility.Role.MenuButton;
			if (r == NSAccessibilityRoles.PopoverRole)
				return Accessibility.Role.Popup;
			if (r == NSAccessibilityRoles.ProgressIndicatorRole)
				return Accessibility.Role.ProgressBar;
			if (r == NSAccessibilityRoles.RadioButtonRole)
				return Accessibility.Role.RadioButton;
			if (r == NSAccessibilityRoles.RadioGroupRole)
				return Accessibility.Role.RadioGroup;
			if (r == NSAccessibilityRoles.RowRole)
				return Accessibility.Role.Row;
			if (r == NSAccessibilityRoles.ScrollAreaRole)
				return Accessibility.Role.ScrollView;
			if (r == NSAccessibilityRoles.ScrollBarRole)
				return Accessibility.Role.ScrollBar;
			if (r == NSAccessibilityRoles.SliderRole)
				return Accessibility.Role.Slider;
			if (r == NSAccessibilityRoles.SplitGroupRole)
				return Accessibility.Role.Paned;
			if (r == NSAccessibilityRoles.SplitterRole)
				return Accessibility.Role.PanedSplitter;
			if (r == NSAccessibilityRoles.StaticTextRole)
				return Accessibility.Role.Label;
			if (r == NSAccessibilityRoles.TabGroupRole)
				return Accessibility.Role.Notebook;
			if (r == NSAccessibilityRoles.TableRole)
				return Accessibility.Role.Table;
			if (r == NSAccessibilityRoles.TextAreaRole)
				return Accessibility.Role.TextEntry;
			if (r == NSAccessibilityRoles.TextFieldRole) {
				if (sr == NSAccessibilitySubroles.SearchFieldSubrole)
					return Accessibility.Role.TextEntrySearch;
				if (sr == NSAccessibilitySubroles.SecureTextFieldSubrole)
					return Accessibility.Role.TextEntryPassword;
				return Accessibility.Role.TextEntry;
			}
			if (r == NSAccessibilityRoles.ToolbarRole)
				return Accessibility.Role.ToolBar;
			if (r == NSAccessibilityRoles.ValueIndicatorRole)
				return Accessibility.Role.SpinButton;
			//if (r == NSAccessibilityRoles.WindowRole)
			//	return Accessibility.Role.Window;
			
			return Accessibility.Role.Custom;

			// TODO:
			//NSAccessibilityRoles.ApplicationRole;
			//NSAccessibilityRoles.BrowserRole;
			//NSAccessibilityRoles.BusyIndicatorRole;
			//NSAccessibilityRoles.DrawerRole;
			//NSAccessibilityRoles.GridRole;
			//NSAccessibilityRoles.GrowAreaRole;
			//NSAccessibilityRoles.HandleRole;
			//NSAccessibilityRoles.HelpTagRole;
			//NSAccessibilityRoles.IncrementorRole;
			//NSAccessibilityRoles.LayoutAreaRole;
			//NSAccessibilityRoles.LayoutItemRole;
			//NSAccessibilityRoles.IncrementorRole
			//NSAccessibilityRoles.MatteRole;
			//NSAccessibilityRoles.MenuButtonRole;
			//NSAccessibilityRoles.RelevanceIndicatorRole;
			//NSAccessibilityRoles.RulerMarkerRole;
			//NSAccessibilityRoles.RulerRole;
			//NSAccessibilityRoles.SheetRole;
			//NSAccessibilityRoles.UnknownRole;
		}

		public static NSString GetMacRole (this Accessibility.Role role)
		{
			switch (role) {
				case Accessibility.Role.Button:
					return NSAccessibilityRoles.ButtonRole;
				//case Accessibility.Role.Calendar:
				//	break;
				case Accessibility.Role.Cell:
					return NSAccessibilityRoles.CellRole;
				case Accessibility.Role.CheckBox:
					return NSAccessibilityRoles.CheckBoxRole;
				case Accessibility.Role.ColorChooser:
					return NSAccessibilityRoles.ColorWellRole;
				case Accessibility.Role.Column:
					return NSAccessibilityRoles.ColumnRole;
				case Accessibility.Role.ComboBox:
					return NSAccessibilityRoles.ComboBoxRole;
				//case Accessibility.Role.Custom:
				//	break;
				//case Accessibility.Role.Dialog:
				//	return NSAccessibilityRoles.WindowRole;
				case Accessibility.Role.Disclosure:
					return NSAccessibilityRoles.DisclosureTriangleRole;
				//case Accessibility.Role.Grid:
				//	break;
				case Accessibility.Role.Group:
					return NSAccessibilityRoles.GroupRole;
				case Accessibility.Role.Image:
					return NSAccessibilityRoles.ImageRole;
				case Accessibility.Role.Label:
					return NSAccessibilityRoles.StaticTextRole;
				case Accessibility.Role.LevelIndicator:
					return NSAccessibilityRoles.LevelIndicatorRole;
				case Accessibility.Role.Link:
					return NSAccessibilityRoles.LinkRole;
				case Accessibility.Role.List:
					return NSAccessibilityRoles.ListRole;
				case Accessibility.Role.Menu:
					return NSAccessibilityRoles.MenuRole;
				case Accessibility.Role.MenuBar:
					return new NSString ("NSAccessibilityMenuBarRole");
				case Accessibility.Role.MenuBarItem:
					return NSAccessibilityRoles.MenuBarItemRole;
				case Accessibility.Role.MenuButton:
					return NSAccessibilityRoles.PopUpButtonRole;
				case Accessibility.Role.MenuItem:
				case Accessibility.Role.MenuItemCheckBox:
				case Accessibility.Role.MenuItemRadio:
					return NSAccessibilityRoles.MenuItemRole;
				case Accessibility.Role.Notebook:
					return NSAccessibilityRoles.TabGroupRole;
				//case Accessibility.Role.NotebookTab:
				//	break;
				case Accessibility.Role.Popup:
					return NSAccessibilityRoles.PopoverRole;
				case Accessibility.Role.ProgressBar:
					return NSAccessibilityRoles.ProgressIndicatorRole;
				case Accessibility.Role.RadioButton:
					return NSAccessibilityRoles.RadioButtonRole;
				case Accessibility.Role.RadioGroup:
					return NSAccessibilityRoles.RadioGroupRole;
				case Accessibility.Role.Row:
					return NSAccessibilityRoles.RowRole;
				case Accessibility.Role.ScrollBar:
					return NSAccessibilityRoles.ScrollBarRole;
				case Accessibility.Role.ScrollView:
					return NSAccessibilityRoles.ScrollAreaRole;
				//case Accessibility.Role.Separator:
				//	break;
				case Accessibility.Role.Slider:
					return NSAccessibilityRoles.SliderRole;
				case Accessibility.Role.SpinButton:
					return NSAccessibilityRoles.ValueIndicatorRole;
				case Accessibility.Role.Paned:
					return NSAccessibilityRoles.SplitGroupRole;
				case Accessibility.Role.PanedSplitter:
					return NSAccessibilityRoles.SplitterRole;
				case Accessibility.Role.Table:
					return NSAccessibilityRoles.TableRole;
				case Accessibility.Role.TextArea:
					return NSAccessibilityRoles.TextAreaRole;
				case Accessibility.Role.TextEntry:
					return NSAccessibilityRoles.TextFieldRole;
				case Accessibility.Role.ToggleButton:
					return NSAccessibilityRoles.ButtonRole;
				case Accessibility.Role.ToolBar:
					return NSAccessibilityRoles.ToolbarRole;
				//case Accessibility.Role.ToolTip:
				//	break;
				case Accessibility.Role.Tree:
					return NSAccessibilityRoles.OutlineRole;
				//case Accessibility.Role.Window:
				//	return NSAccessibilityRoles.WindowRole;
			}
			return NSAccessibilityRoles.UnknownRole;
		}

		public static NSString GetMacSubrole (this Accessibility.Role role)
		{
			switch (role) {
			case Accessibility.Role.ButtonClose:
				return NSAccessibilitySubroles.CloseButtonSubrole;
			case Accessibility.Role.ButtonMaximize:
				return NSAccessibilitySubroles.ZoomButtonSubrole;
			case Accessibility.Role.ButtonMinimize:
				return NSAccessibilitySubroles.MinimizeButtonSubrole;
			case Accessibility.Role.ButtonFullscreen:
				return NSAccessibilitySubroles.FullScreenButtonSubrole;
			case Accessibility.Role.TextEntrySearch:
				return NSAccessibilitySubroles.SearchFieldSubrole;
			case Accessibility.Role.TextEntryPassword:
				return NSAccessibilitySubroles.SecureTextFieldSubrole;
			}
			return null;
		}
	}

	public interface ICopiableObject
	{
		void CopyFrom (object other);
	}
}

