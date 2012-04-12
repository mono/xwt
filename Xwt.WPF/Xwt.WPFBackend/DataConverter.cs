//
// DataConverter.cs
//
// Authors:
//       Carlos Alberto Cortez <calberto.cortez@gmail.com>
//       Luís Reis <luiscubal@gmail.com>
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2011-2012 Carlos Alberto Cortez
// Copyright (c) 2012 Luís Reis
// Copyright (c) 2012 Xamarin, Inc.
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
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using SW = System.Windows;
using SWC = System.Windows.Controls;
using SWM = System.Windows.Media;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;
using Xwt.Drawing;
using Color = Xwt.Drawing.Color;
using FontStretch = Xwt.Drawing.FontStretch;
using FontStyle = Xwt.Drawing.FontStyle;
using FontWeight = Xwt.Drawing.FontWeight;
using ImageFormat = Xwt.Drawing.ImageFormat;

namespace Xwt.WPFBackend
{
	internal static class DataConverter
	{
		//
		// Rect/Point
		//
		public static Rectangle ToXwtRect (this SW.Rect rect)
		{
			return new Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static SW.Rect ToWpfRect (this Rectangle rect)
		{
			return new SW.Rect (rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static SD.RectangleF ToSDRectF (this Rectangle rect)
		{
			return new SD.RectangleF ((float) rect.X, (float) rect.Y, (float) rect.Width, (float) rect.Height);
		}

		public static Int32Rect ToInt32Rect (this Rectangle rect)
		{
			return new Int32Rect ((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height);
		}

		public static Point ToXwtPoint (this SW.Point point)
		{
			return new Point (point.X, point.Y);
		}

		public static SW.Point ToWpfPoint (this Point point)
		{
			return new SW.Point (point.X, point.Y);
		}

		public static Size ToXwtSize (this SD.SizeF self)
		{
			return new Size (self.Width, self.Height);
		}

		//
		// Alignment
		//
		public static Alignment ToXwtAlignment (this SW.HorizontalAlignment alignment)
		{
			switch (alignment) {
				case SW.HorizontalAlignment.Left: return Alignment.Start;
				case SW.HorizontalAlignment.Center: return Alignment.Center;
				default: return Alignment.End;
			}
		}

		public static SW.HorizontalAlignment ToWpfAlignment (this Alignment alignment)
		{
			switch (alignment) {
				case Alignment.Start: return SW.HorizontalAlignment.Left;
				case Alignment.Center: return SW.HorizontalAlignment.Center;
				default: return SW.HorizontalAlignment.Right;
			}
		}

		//
		// Color
		//
		public static Color ToXwtColor (this SW.Media.Color color)
		{
			return Color.FromBytes (color.R, color.G, color.B, color.A);
		}

		public static Color ToXwtColor (this SW.Media.Brush brush)
		{
			var solidBrush = brush as SW.Media.SolidColorBrush;
			if (solidBrush == null)
				throw new ArgumentException();

			return solidBrush.Color.ToXwtColor();
		}

		public static SW.Media.Color ToWpfColor (this Color color)
		{
			return SW.Media.Color.FromArgb (
				(byte)(color.Alpha * 255.0),
				(byte)(color.Red * 255.0),
				(byte)(color.Green * 255.0),
				(byte)(color.Blue * 255.0));
		}

		public static System.Drawing.Color ToDrawingColor (this Color color)
		{
			return System.Drawing.Color.FromArgb (
				(byte) (color.Alpha * 255.0),
				(byte) (color.Red * 255.0),
				(byte) (color.Green * 255.0),
				(byte) (color.Blue * 255.0));
		}

		//
		// Font
		//
		public static SD.Font ToDrawingFont (this Font font)
		{
			SD.FontStyle style = font.Style.ToDrawingFontStyle ();
			if (font.Weight > FontWeight.Normal)
				style |= SD.FontStyle.Bold;

			return new SD.Font (font.Family, (float)font.Size, style);
		}
		
		public static SD.StringTrimming ToDrawingStringTrimming (this Xwt.Drawing.TextTrimming value)
		{
			if (value == Xwt.Drawing.TextTrimming.Word) return SD.StringTrimming.Word;
			if (value == Xwt.Drawing.TextTrimming.WordElipsis) return SD.StringTrimming.EllipsisWord;

			return SD.StringTrimming.Word;
		}
		
		public static FontStyle ToXwtFontStyle (this SW.FontStyle value)
		{
			// No, SW.FontStyles is not an enum
			if (value == SW.FontStyles.Italic) return FontStyle.Italic;
			if (value == SW.FontStyles.Oblique) return FontStyle.Oblique;

			return FontStyle.Normal;
		}

		public static SW.FontStyle ToWpfFontStyle (this FontStyle value)
		{
			if (value == FontStyle.Italic) return SW.FontStyles.Italic;
			if (value == FontStyle.Oblique) return SW.FontStyles.Oblique;
			
			return SW.FontStyles.Normal;
		}

		public static SD.FontStyle ToDrawingFontStyle (this FontStyle value)
		{
			switch (value) {
				case FontStyle.Normal:
					return SD.FontStyle.Regular;
				case FontStyle.Italic:
					return SD.FontStyle.Italic;
				
				default:
					throw new NotImplementedException();
			}
		}

		public static FontStretch ToXwtFontStretch (this SW.FontStretch value)
		{
			// No, SW.FontStretches is not an enum
			if (value == SW.FontStretches.UltraCondensed) return FontStretch.UltraCondensed;
			if (value == SW.FontStretches.ExtraCondensed) return FontStretch.ExtraCondensed;
			if (value == SW.FontStretches.Condensed) return FontStretch.Condensed;
			if (value == SW.FontStretches.SemiCondensed) return FontStretch.SemiCondensed;
			if (value == SW.FontStretches.SemiExpanded) return FontStretch.SemiExpanded;
			if (value == SW.FontStretches.Expanded) return FontStretch.Expanded;
			if (value == SW.FontStretches.ExtraExpanded) return FontStretch.ExtraExpanded;
			if (value == SW.FontStretches.UltraExpanded) return FontStretch.UltraExpanded;

			return FontStretch.Normal;
		}

		public static SW.FontStretch ToWpfFontStretch (this FontStretch value)
		{
			if (value == FontStretch.UltraCondensed) return SW.FontStretches.UltraCondensed;
			if (value == FontStretch.ExtraCondensed) return SW.FontStretches.ExtraCondensed;
			if (value == FontStretch.Condensed) return SW.FontStretches.Condensed;
			if (value == FontStretch.SemiCondensed) return SW.FontStretches.SemiCondensed;
			if (value == FontStretch.SemiExpanded) return SW.FontStretches.SemiExpanded;
			if (value == FontStretch.Expanded) return SW.FontStretches.Expanded;
			if (value == FontStretch.ExtraExpanded) return SW.FontStretches.ExtraExpanded;
			if (value == FontStretch.UltraExpanded) return SW.FontStretches.UltraExpanded;

			return SW.FontStretches.Normal;
		}

		public static FontWeight ToXwtFontWeight (this SW.FontWeight value)
		{
			// No, SW.FontWeights is not an enum
			if (value == SW.FontWeights.UltraLight) return FontWeight.Ultralight;
			if (value == SW.FontWeights.Light) return FontWeight.Light;
			if (value == SW.FontWeights.SemiBold) return FontWeight.Semibold;
			if (value == SW.FontWeights.Bold) return FontWeight.Bold;
			if (value == SW.FontWeights.UltraBold) return FontWeight.Ultrabold;
			if (value == SW.FontWeights.Black) return FontWeight.Heavy;

			return FontWeight.Normal;
		}

		public static SW.FontWeight ToWpfFontWeight (this FontWeight value)
		{
			if (value == FontWeight.Ultralight) return SW.FontWeights.UltraLight;
			if (value == FontWeight.Light) return SW.FontWeights.Light;
			if (value == FontWeight.Semibold) return SW.FontWeights.SemiBold;
			if (value == FontWeight.Bold) return SW.FontWeights.Bold;
			if (value == FontWeight.Ultrabold) return SW.FontWeights.UltraBold;
			if (value == FontWeight.Heavy) return SW.FontWeights.Black;
			
			return SW.FontWeights.Normal;
		}

		// Dock

		public static SW.Controls.Dock ToWpfDock (this ContentPosition value)
		{
			if (value == ContentPosition.Left) return SW.Controls.Dock.Left;
			if (value == ContentPosition.Top) return SW.Controls.Dock.Top;
			if (value == ContentPosition.Bottom) return SW.Controls.Dock.Bottom;

			return SW.Controls.Dock.Right;
		}

		// 
		// Mouse/Pointer Button
		//

		public static PointerButton ToXwtButton (this MouseButton value)
		{
			switch (value) {
				case MouseButton.Left: return PointerButton.Left;
				case MouseButton.Middle: return PointerButton.Middle;
				case MouseButton.Right: return PointerButton.Right;
				case MouseButton.XButton1: return PointerButton.ExtendedButton1;
				case MouseButton.XButton2: return PointerButton.ExtendedButton2;

				default: throw new ArgumentException();
			}
		}

		public static MouseButton ToWpfButton (this PointerButton value)
		{
			switch (value) {
				case PointerButton.Left: return MouseButton.Left;
				case PointerButton.Middle: return MouseButton.Middle;
				case PointerButton.Right: return MouseButton.Right;
				case PointerButton.ExtendedButton1: return MouseButton.XButton1;
				case PointerButton.ExtendedButton2: return MouseButton.XButton2;

				default: throw new ArgumentException();
			}
		}

		public static SDI.PixelFormat ToPixelFormat (this ImageFormat self)
		{
			switch (self) {
				case ImageFormat.ARGB32:
					return SDI.PixelFormat.Format32bppArgb;
				case ImageFormat.RGB24:
					return SDI.PixelFormat.Format24bppRgb;
				default:
					throw new ArgumentException();
			}
		}

		public static SDI.PixelFormat ToPixelFormat (this SW.Media.PixelFormat self)
		{
			if (self == SWM.PixelFormats.Rgb24)
				return SDI.PixelFormat.Format24bppRgb;
			if (self == SWM.PixelFormats.Bgra32)
				return SDI.PixelFormat.Format32bppArgb;
			if (self == SWM.PixelFormats.Pbgra32)
				return SDI.PixelFormat.Format32bppPArgb;
			if (self == SWM.PixelFormats.Prgba64)
				return SDI.PixelFormat.Format64bppPArgb;
			if (self == SWM.PixelFormats.Indexed1)
				return SDI.PixelFormat.Format1bppIndexed;
			if (self == SWM.PixelFormats.Indexed4)
				return SDI.PixelFormat.Format4bppIndexed;
			if (self == SWM.PixelFormats.Indexed8)
				return SDI.PixelFormat.Format8bppIndexed;
			if (self == SWM.PixelFormats.Gray16)
				return SDI.PixelFormat.Format16bppGrayScale;
			if (self == SWM.PixelFormats.Bgr24)
				return SDI.PixelFormat.Format24bppRgb;
			if (self == SWM.PixelFormats.Bgr32)
				return SDI.PixelFormat.Format32bppRgb;

			throw new ArgumentException();
		}

		public static SD.Bitmap AsBitmap (object backend)
		{
			var bmp = backend as SD.Bitmap;
			if (bmp == null) {
				var bs = backend as SWM.Imaging.BitmapSource;
				if (bs != null) {
					bmp = new SD.Bitmap (bs.PixelWidth, bs.PixelHeight, bs.Format.ToPixelFormat ());
					SDI.BitmapData data = bmp.LockBits (new System.Drawing.Rectangle (0, 0, bmp.Width, bmp.Height), SDI.ImageLockMode.WriteOnly,
					                                bmp.PixelFormat);
					bs.CopyPixels (new Int32Rect (0, 0, bmp.Width, bmp.Height), data.Scan0, data.Height * data.Stride, data.Stride);
					bmp.UnlockBits (data);
				}
			}

			return bmp;
		}

		[DllImport ("gdi32")]
		private static extern int DeleteObject (IntPtr o);

		public static SWM.ImageSource AsImageSource (object nativeImage)
		{
			var source = nativeImage as SWM.ImageSource;
			if (source == null) {
				var bitmap = nativeImage as SD.Bitmap;
				if (bitmap != null) {
					IntPtr ptr = bitmap.GetHbitmap ();

					try {
						return SW.Interop.Imaging.CreateBitmapSourceFromHBitmap (ptr, IntPtr.Zero, Int32Rect.Empty,
																	  SWM.Imaging.BitmapSizeOptions.FromEmptyOptions ());
					}
					finally {
						DeleteObject (ptr);
					}
				}
			}

			return source;
		}

		//
		// Drag and Drop
		//
		public static DragDropAction ToXwtDropAction (this DragDropEffects value)
		{
			var action = DragDropAction.None;
			if ((value & DragDropEffects.Copy) > 0) action |= DragDropAction.Copy;
			if ((value & DragDropEffects.Move) > 0) action |= DragDropAction.Move;
			if ((value & DragDropEffects.Link) > 0) action |= DragDropAction.Link;
			return action;
		}

		public static DragDropEffects ToWpfDropEffect (this DragDropAction value)
		{
			var effects = DragDropEffects.None;
			if ((value & DragDropAction.Copy) > 0) effects |= DragDropEffects.Copy;
			if ((value & DragDropAction.Move) > 0) effects |= DragDropEffects.Move;
			if ((value & DragDropAction.Link) > 0) effects |= DragDropEffects.Link;
			return effects;
		}

		public static string ToWpfDataFormat (this TransferDataType type)
		{
			if (type == TransferDataType.Text) return DataFormats.UnicodeText;
			if (type == TransferDataType.Rtf) return DataFormats.Rtf;
			if (type == TransferDataType.Uri) return DataFormats.FileDrop;
			if (type == TransferDataType.Image) return DataFormats.Bitmap;
			return type.Id;
		}

		public static TransferDataType ToXwtTransferType (this string type)
		{
			if (type == DataFormats.UnicodeText) return TransferDataType.Text;
			if (type == DataFormats.Rtf) return TransferDataType.Rtf;
			if (type == DataFormats.FileDrop) return TransferDataType.Uri;
			if (type == DataFormats.Bitmap) return TransferDataType.Image;
			return TransferDataType.FromId (type);
		}

		// Scrollbar visibility

		public static SWC.ScrollBarVisibility ToWpfScrollBarVisibility (this ScrollPolicy policy)
		{
			switch (policy) {
				case ScrollPolicy.Always:
					return SWC.ScrollBarVisibility.Visible;
				case ScrollPolicy.Automatic:
					return SWC.ScrollBarVisibility.Auto;
				case ScrollPolicy.Never:
					return SWC.ScrollBarVisibility.Hidden;

				default:
					throw new NotSupportedException ();
			}
		}

		public static ScrollPolicy ToXwtScrollPolicy (this SWC.ScrollBarVisibility visibility)
		{
			switch (visibility) {
				case SWC.ScrollBarVisibility.Auto:
					return ScrollPolicy.Automatic;
				case SWC.ScrollBarVisibility.Visible:
					return ScrollPolicy.Always;
				case SWC.ScrollBarVisibility.Hidden:
					return ScrollPolicy.Never;

				default:
					throw new NotSupportedException ();
			}
		}

		public static DataObject ToDataObject (this TransferDataSource data)
		{
			var retval = new DataObject ();
			foreach (var type in data.DataTypes) {
				var value = data.GetValue (type);

				if (type == TransferDataType.Text)
					retval.SetText ((string)value);
				else if (type == TransferDataType.Uri) {
					var uris = new StringCollection ();
					uris.Add (((Uri)value).LocalPath);
					retval.SetFileDropList (uris);
				} else
					retval.SetData (type.Id, TransferDataSource.SerializeValue (value));
			}

			return retval;
		}
	}
}
