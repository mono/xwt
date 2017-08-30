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
		class LayoutInfo
		{
			public string Text = String.Empty;
			public NSFont Font;
			public float? Width, Height;
			public TextTrimming TextTrimming;
			public Alignment TextAlignment;
			readonly public List<TextAttribute> Attributes = new List<TextAttribute> ();
			readonly public ApplicationContext ApplicationContext;

			public LayoutInfo (ApplicationContext actx)
			{
				ApplicationContext = actx;
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

		public override void SetFont (object backend, Xwt.Drawing.Font font)
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
		
		public override void SetTrimming (object backend, TextTrimming value)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.TextTrimming = value;
		}

		public override void SetAlignment (object backend, Alignment alignment)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.TextAlignment = alignment;
		}

		public override Size GetSize (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			using (CTFrame frame = CreateFrame (li)) {
				if (frame == null)
					return Size.Zero;

				Size result = Size.Zero;
				CTLine [] lines = frame.GetLines ();
				nfloat lineHeight = li.Font.Ascender - li.Font.Descender + li.Font.Leading;

				CTLine ellipsis = null;
				bool ellipsize = li.Width.HasValue && li.TextTrimming == TextTrimming.WordElipsis;
				if (ellipsize)
					ellipsis = new CTLine (CreateAttributedString (li, "..."));

				// try to approximate Pango's layout
				foreach (var line in lines) {
					var l = line;
					if (ellipsize) { // we need to create a new CTLine here because the framesetter already truncated the text for the line
						l = new CTLine (CreateAttributedString (li, li.Text.Substring ((int)line.StringRange.Location)))
							.GetTruncatedLine (li.Width.Value, CTLineTruncation.End, ellipsis);
						line.Dispose ();
					}

					result.Width = Math.Max (result.Width, l.GetTypographicBounds ());
					result.Height += lineHeight;

					// clean up after ourselves as we go
					l.Dispose ();
				}

				// CoreText throws away trailing line breaks..
				if (li.Text.EndsWith ("\n"))
					result.Height += lineHeight;

				result.Width = Math.Ceiling (result.Width);
				result.Height = Math.Ceiling (result.Height);
				return result;
			}
		}

		public override double GetBaseline (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			using (var line = new CTLine (CreateAttributedString (li))) {
				nfloat ascent, descent, leading;
				line.GetTypographicBounds (out ascent, out descent, out leading);
				return (double)ascent;
			}
		}

		public override double GetMeanline (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			return GetBaseline (backend) - li.Font.XHeight / 2;
		}

		static CTFrame CreateFrame (LayoutInfo li)
		{
			if (string.IsNullOrEmpty (li.Text))
				return null;

			using (CTFramesetter framesetter = new CTFramesetter (CreateAttributedString (li))) {
				CGPath path = new CGPath ();
				bool ellipsize = li.Width.HasValue && li.TextTrimming == TextTrimming.WordElipsis;
				path.AddRect (new CGRect (0, 0, li.Width.HasValue && !ellipsize ? li.Width.Value : float.MaxValue, li.Height ?? float.MaxValue));

				return framesetter.GetFrame (new NSRange (0, li.Text.Length), path, null);
			}
		}

		static NSAttributedString CreateAttributedString (LayoutInfo li, string overrideText = null)
		{
			if (overrideText != null || li.Attributes.Count == 0)
				return CreateAttributedString (overrideText ?? li.Text, li.Font);

			var ns = new NSMutableAttributedString (li.Text);
			ns.BeginEditing ();
			var r = new NSRange (0, li.Text.Length);
			if (li.Font != null)
				ns.AddAttribute (CTStringAttributeKey.Font, li.Font, r);
			ns.AddAttribute (CTStringAttributeKey.ForegroundColorFromContext, new NSNumber (true), r);

			foreach (var att in li.Attributes) {
				r = new NSRange (att.StartIndex, att.Count);
				if (att is BackgroundTextAttribute) {
					var xa = (BackgroundTextAttribute)att;
					ns.AddAttribute (CTStringAttributeKey.BackgroundColor, xa.Color.ToNSColor (), r);
				} else if (att is ColorTextAttribute) {
					var xa = (ColorTextAttribute)att;
					// FIXME: CTStringAttributeKey.ForegroundColor has no effect
					ns.AddAttribute (CTStringAttributeKey.ForegroundColor, xa.Color.ToNSColor (), r);
				} else if (att is UnderlineTextAttribute) {
					var xa = (UnderlineTextAttribute)att;
					var style = xa.Underline ? CTUnderlineStyle.Single : CTUnderlineStyle.None;
					ns.AddAttribute (CTStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 ((int)style), r);
				} else if (att is FontStyleTextAttribute) {
					var xa = (FontStyleTextAttribute)att;
					if (xa.Style == FontStyle.Italic) {
						ns.ApplyFontTraits (NSFontTraitMask.Italic, r);
					} else if (xa.Style == FontStyle.Oblique) {
						// FIXME: CoreText has no Obliqueness support
					} else {
						// FIXME: CoreText has no Obliqueness support
						ns.ApplyFontTraits (NSFontTraitMask.Unitalic, r);
					}
				} else if (att is FontWeightTextAttribute) {
					var xa = (FontWeightTextAttribute)att;
					NSRange er;
					// get the effective font to modify for the given range
					var ft = ns.GetAttribute (CTStringAttributeKey.Font, att.StartIndex, out er, r) as NSFont;
					ft = ft.WithWeight (xa.Weight);
					ns.AddAttribute (CTStringAttributeKey.Font, ft, r);
				} else if (att is LinkTextAttribute) {
					ns.AddAttribute (CTStringAttributeKey.ForegroundColor, Toolkit.CurrentEngine.Defaults.FallbackLinkColor.ToNSColor (), r);
					ns.AddAttribute (CTStringAttributeKey.UnderlineStyle, NSNumber.FromInt32 ((int)CTUnderlineStyle.Single), r);
				} else if (att is StrikethroughTextAttribute) {
					//FIXME: CoreText has no Strikethrough support
				} else if (att is FontTextAttribute) {
					var xa = (FontTextAttribute)att;
					var nf = ((FontData)li.ApplicationContext.Toolkit.GetSafeBackend (xa.Font)).Font;
					ns.AddAttribute (CTStringAttributeKey.Font, nf, r);
				}
			}

			ns.EndEditing ();
			return ns;
		}

		static NSAttributedString CreateAttributedString (string text, NSFont font)
		{
			NSDictionary dict;
			if (font != null) {
				dict = NSDictionary.FromObjectsAndKeys (
					new object [] { font, new NSNumber (true) },
					new object [] { CTStringAttributeKey.Font, CTStringAttributeKey.ForegroundColorFromContext }
				);
			} else {
				dict = NSDictionary.FromObjectsAndKeys (
					new object [] { new NSNumber (true) },
					new object [] { CTStringAttributeKey.ForegroundColorFromContext }
				);
			}
			return new NSAttributedString (text, dict);
		}
		
		internal static void Draw (CGContext ctx, object layout, double x, double y)
		{
			LayoutInfo li = (LayoutInfo)layout;
			using (CTFrame frame = CreateFrame (li)) {
				if (frame == null)
					return;

				CTLine ellipsis = null;
				bool ellipsize = li.Width.HasValue && li.TextTrimming == TextTrimming.WordElipsis;
				if (ellipsize)
					ellipsis = new CTLine (CreateAttributedString (li, "..."));

				nfloat lineHeight = li.Font.Ascender - li.Font.Descender + li.Font.Leading;

				ctx.SaveState ();
				ctx.TextMatrix = CGAffineTransform.MakeScale (1f, -1f);
				ctx.TranslateCTM ((float)x, (float)y + li.Font.Ascender);
				foreach (var line in frame.GetLines ()) {
					ctx.TextPosition = CGPoint.Empty;
					// Determine final line
					var ln = line;
					if (ellipsize) {
						// we need to create a new CTLine here because the framesetter already truncated the text for the line
						ln = new CTLine (CreateAttributedString (li, li.Text.Substring ((int)line.StringRange.Location)))
							.GetTruncatedLine (li.Width.Value, CTLineTruncation.End, ellipsis);
						line.Dispose ();
					} else if (li.Width.HasValue && li.TextAlignment != Alignment.Start) {
						var tx = li.Width.Value - ln.GetTypographicBounds ();
						if (li.TextAlignment == Alignment.Center)
							tx /= 2d;
						ctx.TextPosition = new CGPoint ((nfloat)tx, 0);
					}
					ln.Draw (ctx);
					ctx.TranslateCTM (0, lineHeight);
					ln.Dispose ();
				}
				ctx.RestoreState ();
			}
		}

		public override void AddAttribute (object backend, TextAttribute attribute)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Attributes.Add (attribute);
		}

		public override void ClearAttributes (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Attributes.Clear ();
		}
		
		public override int GetIndexFromCoordinates (object backend, double x, double y)
		{
			return 0;
		}
		
		public override Point GetCoordinateFromIndex (object backend, int index)
		{
			return new Point (0,0);
		}

		public override void Dispose (object backend)
		{
			// nothing
		}
	}
}

