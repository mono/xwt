// 
// TextLayoutBackendHandler.cs
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
using System.Linq;
using AppKit;
using CoreGraphics;
using CoreText;
using Foundation;
using Xwt.Backends;
using Xwt.Drawing;

namespace Xwt.Mac
{
	public class MacTextLayoutBackendHandler: TextLayoutBackendHandler
	{
		class LayoutInfo : IDisposable
		{
			string text = String.Empty;
			NSFont font;
			float? width, height;
			TextTrimming textTrimming;
			Alignment textAlignment;
			readonly public List<TextAttribute> Attributes;
			readonly ApplicationContext ApplicationContext;
			readonly NSTextStorage TextStorage;
			readonly NSTextContainer TextContainer;

			public string Text
			{
				get { return text; }
				set {
					text = value;
					Attributes.Clear ();
					ResetAttributes ();
				}
			}

			public NSFont Font
			{
				get { return font; }
				set {
					font = value;
					ResetAttributes ();
				}
			}

			public TextTrimming TextTrimming
			{
				get { return textTrimming; }
				set {
					textTrimming = value;
					ResetAttributes ();
				}
			}

			public Alignment TextAlignment
			{
				get { return textAlignment; }
				set
				{
					textAlignment = value;
					ResetAttributes ();
				}
			}

			public float? Width
			{
				get { return width; }
				set
				{
					width = value;
					TextContainer.Size = new CGSize (value ?? double.MaxValue, TextContainer.Size.Height);
				}
			}

			public float? Height
			{
				get { return height; }
				set
				{
					height = value;
					TextContainer.Size = new CGSize (TextContainer.Size.Width, value ?? double.MaxValue);
				}
			}

			public LayoutInfo (ApplicationContext actx)
			{
				ApplicationContext = actx;
				Attributes = new List<TextAttribute> ();
				TextStorage = new NSTextStorage ();
				TextContainer = new NSTextContainer {
					LineFragmentPadding = 0.0f
				};
			}

			public void AddAttribute (TextAttribute attribute)
			{
				Attributes.Add (attribute);
				AddAttributeInternal (attribute);
			}

			public void ClearAttributes ()
			{
				Attributes.Clear ();
				ResetAttributes ();
			}

			void ResetAttributes (NSColor foregroundColorOverride = null)
			{
				// clear user attributes
				TextStorage.SetAttributes (new NSDictionary (), new NSRange (0, TextStorage.Length));

				var r = new NSRange (0, Text.Length);

				TextStorage.SetString (new NSAttributedString (Text));
				TextStorage.SetAlignment (TextAlignment.ToNSTextAlignment (), r);

				if (Font != null)
					// set a global font
					TextStorage.AddAttribute (NSStringAttributeKey.Font, Font, r);

				// paragraph style
				TextContainer.LineBreakMode = TextTrimming == TextTrimming.WordElipsis ? NSLineBreakMode.TruncatingTail : NSLineBreakMode.ByWordWrapping;
				var pstyle = NSParagraphStyle.DefaultParagraphStyle.MutableCopy () as NSMutableParagraphStyle;
				pstyle.Alignment = TextAlignment.ToNSTextAlignment ();
				if (TextTrimming == TextTrimming.WordElipsis)
					pstyle.LineBreakMode = NSLineBreakMode.TruncatingTail;
				TextStorage.AddAttribute (NSStringAttributeKey.ParagraphStyle, pstyle, r);

				// set foreground color override
				if (foregroundColorOverride != null)
					TextStorage.AddAttribute(NSStringAttributeKey.ForegroundColor, foregroundColorOverride, r);

				// restore user attributes
				foreach (var att in Attributes)
					AddAttributeInternal (att);
			}

			void AddAttributeInternal (TextAttribute attribute)
			{
				var r = new NSRange (attribute.StartIndex, attribute.Count);

				if (attribute is BackgroundTextAttribute)
				{
					var xa = (BackgroundTextAttribute)attribute;
					TextStorage.AddAttribute (NSStringAttributeKey.BackgroundColor, xa.Color.ToNSColor (), r);
				}
				else if (attribute is ColorTextAttribute)
				{
					var xa = (ColorTextAttribute)attribute;
					TextStorage.AddAttribute (NSStringAttributeKey.ForegroundColor, xa.Color.ToNSColor (), r);
				}
				else if (attribute is UnderlineTextAttribute)
				{
					var xa = (UnderlineTextAttribute)attribute;
					var style = xa.Underline ? NSUnderlineStyle.Single : NSUnderlineStyle.None;
					TextStorage.AddAttribute(NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 ((int)style), r);
				}
				else if (attribute is FontStyleTextAttribute)
				{
					var xa = (FontStyleTextAttribute)attribute;
					if (xa.Style == FontStyle.Italic)
					{
						TextStorage.ApplyFontTraits (NSFontTraitMask.Italic, r);
					}
					else if (xa.Style == FontStyle.Oblique)
					{
						// copy Pango.Style.Oblique behaviour (25% skew)
						TextStorage.AddAttribute (NSStringAttributeKey.Obliqueness, NSNumber.FromFloat ((float)0.25), r);
					}
					else
					{
						TextStorage.RemoveAttribute (NSStringAttributeKey.Obliqueness, r);
						TextStorage.ApplyFontTraits (NSFontTraitMask.Unitalic, r);
					}
				}
				else if (attribute is FontWeightTextAttribute)
				{
					var xa = (FontWeightTextAttribute)attribute;
					NSRange er;
					// get the effective font to modify for the given range
					var ft = TextStorage.GetAttribute (NSStringAttributeKey.Font, attribute.StartIndex, out er, r) as NSFont;
					ft = ft.WithWeight (xa.Weight);
					TextStorage.AddAttribute (NSStringAttributeKey.Font, ft, r);
				}
				else if (attribute is LinkTextAttribute)
				{
					TextStorage.AddAttribute (NSStringAttributeKey.ForegroundColor, Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToNSColor (), r);
					TextStorage.AddAttribute (NSStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 ((int)NSUnderlineStyle.Single), r);
				}
				else if (attribute is StrikethroughTextAttribute)
				{
					var xa = (StrikethroughTextAttribute)attribute;
					var style = xa.Strikethrough ? NSUnderlineStyle.Single : NSUnderlineStyle.None; 
					TextStorage.AddAttribute (NSStringAttributeKey.StrikethroughStyle, NSNumber.FromInt32 ((int)style), r);
				} 
				else if (attribute is FontSizeTextAttribute) 
				{
					var xa = (FontSizeTextAttribute)attribute;
					NSRange er;
					var ft = TextStorage.GetAttribute (NSStringAttributeKey.Font, attribute.StartIndex, out er, r) as NSFont;
					ft = ft.WithSize (xa.Size);
					TextStorage.AddAttribute (NSStringAttributeKey.Font, ft, r);
				} 
				else if (attribute is FontTextAttribute)
				{
					var xa = (FontTextAttribute)attribute;
					var nf = ((FontData)ApplicationContext.Toolkit.GetSafeBackend (xa.Font)).Font;
					TextStorage.AddAttribute (NSStringAttributeKey.Font, nf, r);
				}
			}

			public void Draw (CGContext ctx, CGColor foregroundColor, double x, double y)
			{
				bool tempForegroundSet = false;
				// if no color attribute is set for the whole string,
				// NSLayoutManager will use the default control foreground color.
				// To override the default color we need to apply the current CGContext stroke color
				// before all other attributes are set, otherwise it will remove all other foreground colors.
				if (foregroundColor != null && !Attributes.Any (a => a is ColorTextAttribute && a.StartIndex == 0 && a.Count == Text.Length))
				{
					// FIXME: we need to find a better way to accomplish this without the need to reset all attributes.
					ResetAttributes(NSColor.FromCGColor(foregroundColor));
					tempForegroundSet = true;
				}

				ctx.SaveState ();
				NSGraphicsContext.GlobalSaveGraphicsState ();
				var nsContext = NSGraphicsContext.FromCGContext (ctx, true);
				NSGraphicsContext.CurrentContext = nsContext;

				using (var TextLayout = new NSLayoutManager ())
				{
					TextLayout.AddTextContainer (TextContainer);
					TextStorage.AddLayoutManager (TextLayout);

					TextLayout.DrawBackgroundForGlyphRange (new NSRange(0, Text.Length), new CGPoint (x, y));
					TextLayout.DrawGlyphsForGlyphRange (new NSRange(0, Text.Length), new CGPoint (x, y));
					TextStorage.RemoveLayoutManager (TextLayout);
					TextLayout.RemoveTextContainer (0);
				}

				// reset foreground color change
				if (tempForegroundSet)
					ResetAttributes();

				NSGraphicsContext.GlobalRestoreGraphicsState ();
				ctx.RestoreState ();
			}

			public CGSize GetSize ()
			{
				using (var TextLayout = new NSLayoutManager ())
				{
					TextLayout.AddTextContainer (TextContainer);
					TextStorage.AddLayoutManager (TextLayout);
					TextLayout.GlyphRangeForBoundingRect (new CGRect (CGPoint.Empty, TextContainer.Size), TextContainer);
					var s = TextLayout.GetUsedRectForTextContainer (TextContainer);
					TextStorage.RemoveLayoutManager (TextLayout);
					TextLayout.RemoveTextContainer (0);
					return s.Size;
				}
			}

			public double GetBaseLine ()
			{
				using (var line = new CTLine (TextStorage))
				{
					nfloat ascent, descent, leading;
					line.GetTypographicBounds (out ascent, out descent, out leading);
					return ascent;
				}
			}

			public nuint GetIndexFromCoordinates (double x, double y)
			{
				using (var TextLayout = new NSLayoutManager ())
				{
					TextLayout.AddTextContainer (TextContainer);
					TextStorage.AddLayoutManager (TextLayout);
					TextLayout.GlyphRangeForBoundingRect (new CGRect (CGPoint.Empty, TextContainer.Size), TextContainer);
					nfloat fraction = 0;
					var index = TextLayout.CharacterIndexForPoint (new CGPoint (x, y), TextContainer, ref fraction);
					TextStorage.RemoveLayoutManager (TextLayout);
					TextLayout.RemoveTextContainer (0);
					return index;
				}
			}

			public CGPoint GetCoordinateFromIndex(int index)
			{
				using (var TextLayout = new NSLayoutManager ())
				{
					TextLayout.AddTextContainer (TextContainer);
					TextStorage.AddLayoutManager (TextLayout);
					TextLayout.GlyphRangeForBoundingRect (new CGRect (CGPoint.Empty, TextContainer.Size), TextContainer);
					var glyphIndex = TextLayout.GlyphIndexForCharacterAtIndex (index);
					var p = TextLayout.LocationForGlyphAtIndex ((nint)glyphIndex);
					TextStorage.RemoveLayoutManager (TextLayout);
					TextLayout.RemoveTextContainer (0);
					return p;
				}
			}

			public void Dispose ()
			{
				TextStorage.Dispose ();
				TextContainer.Dispose ();
			}
		}
		
		public override object Create ()
		{
			return new LayoutInfo (ApplicationContext);
		}
		
		public override void SetText (object backend, string text)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Text = text == null ? String.Empty : text.Replace ("\r\n", "\n");
		}

		public override void SetFont (object backend, Font font)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Font = ((FontData)ApplicationContext.Toolkit.GetSafeBackend (font)).Font;
		}
		
		public override void SetWidth (object backend, double value)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Width = value < 0 ? null : (float?)value;
		}
		
		public override void SetHeight (object backend, double value)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Height = value < 0 ? null : (float?)value;
		}
		
		public override void SetTrimming (object backend, TextTrimming textTrimming)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.TextTrimming = textTrimming;
		}

		public override void SetAlignment (object backend, Alignment alignment)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.TextAlignment = alignment;
		}

		public override Size GetSize (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			return li.GetSize().ToXwtSize();
		}

		public override double GetBaseline (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			return li.GetBaseLine();
		}

		public override double GetMeanline (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			return GetBaseline (backend) - li.Font.XHeight / 2;
		}
		
		internal static void Draw (CGContextBackend ctx, object layout, double x, double y)
		{
			LayoutInfo li = (LayoutInfo)layout;
			li.Draw (ctx.Context, ctx.CurrentStatus.GlobalColor, x, y);
		}

		public override void AddAttribute (object backend, TextAttribute attribute)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.AddAttribute (attribute);
		}

		public override void ClearAttributes (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.ClearAttributes ();
		}
		
		public override int GetIndexFromCoordinates (object backend, double x, double y)
		{
			LayoutInfo li = (LayoutInfo)backend;
			return (int)li.GetIndexFromCoordinates (x, y);
		}
		
		public override Point GetCoordinateFromIndex (object backend, int index)
		{
			LayoutInfo li = (LayoutInfo)backend;
			return li.GetCoordinateFromIndex (index).ToXwtPoint ();
		}

		public override void Dispose (object backend)
		{
			((LayoutInfo)backend).Dispose ();
		}
	}
}

