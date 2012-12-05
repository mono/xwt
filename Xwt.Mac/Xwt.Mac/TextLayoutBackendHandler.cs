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
using Xwt.Backends;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreText;
using MonoMac.CoreGraphics;
using Xwt.Engine;
using Xwt.Drawing;

using PointF = System.Drawing.PointF;
using SizeF = System.Drawing.SizeF;
using RectangleF = System.Drawing.RectangleF;

namespace Xwt.Mac
{
	public class MacTextLayoutBackendHandler: TextLayoutBackendHandler
	{
		class LayoutInfo
		{
			public CTFramesetter Framesetter;
			public string Text;
			public NSFont Font;
			public float? Width, Height;
			public TextTrimming TextTrimming;
		}
		
		public override object Create (Xwt.Drawing.Context context)
		{
			return new LayoutInfo ();
		}
		
		public override object Create (ICanvasBackend canvas)
		{
			return new LayoutInfo {
				Font = (NSFont)canvas.Font
			};
		}

		public override void SetText (object backend, string text)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Text = text;
			InvalidateFramesetter (li);
		}

		public override void SetFont (object backend, Xwt.Drawing.Font font)
		{
			LayoutInfo li = (LayoutInfo)backend;
			li.Font = (NSFont)Toolkit.GetBackend (font);
			InvalidateFramesetter (li);
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

		public override Size GetSize (object backend)
		{
			LayoutInfo li = (LayoutInfo)backend;
			if (li.Framesetter == null)
				return Size.Zero;

			NSRange unused;
			SizeF constraint = new SizeF (li.Width ?? float.MaxValue, float.MaxValue);
			return li.Framesetter.SuggestFrameSize (new NSRange (0, li.Text.Length), null, constraint, out unused).ToXwtSize ();
		}

		static void InvalidateFramesetter (LayoutInfo li)
		{
			if (li.Framesetter != null) {
				li.Framesetter.Dispose ();
				li.Framesetter = null;
			}
			if (!string.IsNullOrEmpty (li.Text))
				li.Framesetter = new CTFramesetter (CreateAttributedString (li));
		}

		static NSAttributedString CreateAttributedString (LayoutInfo li, string overrideText = null)
		{
			NSDictionary dict;
			if (li.Font != null) {
				dict = NSDictionary.FromObjectsAndKeys (
					new object[] { li.Font, new NSNumber (true) },
					new object[] { CTStringAttributeKey.Font, CTStringAttributeKey.ForegroundColorFromContext }
				);
			} else {
				dict = NSDictionary.FromObjectsAndKeys (
					new object[] { new NSNumber (true) },
					new object[] { CTStringAttributeKey.ForegroundColorFromContext }
				);
			}
			return new NSAttributedString (overrideText ?? li.Text, dict);
		}
		
		internal static void Draw (CGContext ctx, object layout, double x, double y)
		{
			LayoutInfo li = (LayoutInfo)layout;
			if (li.Framesetter == null)
				return;

			CGPath path = new CGPath ();
			path.AddRect (new RectangleF (0, 0, li.Width ?? float.MaxValue, li.Height ?? float.MaxValue));
			CTFrame frame = li.Framesetter.GetFrame (new NSRange (0, li.Text.Length), path, null);

			CTLine ellipsis = null;
			bool ellipsize = li.Width.HasValue && li.TextTrimming == TextTrimming.WordElipsis;
			if (ellipsize)
				ellipsis = new CTLine (CreateAttributedString (li, "..."));

			float lineHeight = layoutManager.DefaultLineHeightForFont (li.Font);

			ctx.SaveState ();
			ctx.TextMatrix = CGAffineTransform.MakeScale (1f, -1f);
			ctx.TranslateCTM ((float)x, (float)y + li.Font.Ascender);
			foreach (var line in frame.GetLines ()) {
				ctx.TextPosition = PointF.Empty;
				if (ellipsize) // we need to create a new CTLine here because the framesetter already truncated the text for the line
					new CTLine (CreateAttributedString (li, li.Text.Substring (line.StringRange.Location)))
						.GetTruncatedLine (li.Width.Value, CTLineTruncation.End, ellipsis).Draw (ctx);
				else
					line.Draw (ctx);
				ctx.TranslateCTM (0, lineHeight);
			}
			ctx.RestoreState ();
		}

		static readonly NSLayoutManager layoutManager = new NSLayoutManager ();
	}
}

