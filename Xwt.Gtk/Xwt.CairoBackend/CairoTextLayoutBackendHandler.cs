// 
// CairoTextLayoutBackend.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
// 
// Copyright (c) 2012 Xamarin Inc
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
using Xwt.Engine;
using System.Collections.Generic;

namespace Xwt.CairoBackend
{
	public class CairoTextLayoutBackendHandler: ITextLayoutBackendHandler
	{
		static Cairo.Context SharedContext;
		
		class LayoutBackend
		{
			public CairoContextBackend Context;
			public double Width = -1;
			public double Heigth = -1;
			public string Text;
			public Font Font;
			public TextTrimming TextTrimming;
			public bool Measured;
			public List<int> LineBreaks = new List<int> ();
			public List<double> LineHeights = new List<double> ();
		}
		
		static CairoTextLayoutBackendHandler ()
		{
			Cairo.Surface sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
			SharedContext = new Cairo.Context (sf);
		}
		
		public CairoTextLayoutBackendHandler ()
		{
		}

		#region ITextLayoutBackendHandler implementation
		public object Create (Context context)
		{
			CairoContextBackend c = (CairoContextBackend) WidgetRegistry.GetBackend (context);
			LayoutBackend b = new LayoutBackend ();
			b.Context = c;
			return b;
		}

		public object Create (ICanvasBackend canvas)
		{
			LayoutBackend b = new LayoutBackend ();
			CairoContextBackend ba = new CairoContextBackend ();
			ba.Context = SharedContext;
			b.Context = ba;
			return b;
		}

		public void SetWidth (object backend, double value)
		{
			LayoutBackend la = (LayoutBackend) backend;
			la.Width = value;
			la.Measured = false;
		}
		
		public void SetHeigth (object backend, double value)
		{
			LayoutBackend la = (LayoutBackend) backend;
			la.Heigth = value;
			la.Measured = false;
		}
		
		public void SetText (object backend, string text)
		{
			LayoutBackend la = (LayoutBackend) backend;
			la.Text = text;
			la.Measured = false;
		}

		public void SetFont (object backend, Font font)
		{
			LayoutBackend la = (LayoutBackend) backend;
			la.Measured = false;
			la.Font = font;
		}
		
		public void SetTrimming (object backend, TextTrimming textTrimming)
		{
			LayoutBackend la = (LayoutBackend) backend;
			la.TextTrimming = textTrimming;
			
		}
		
		public Size GetSize (object backend)
		{
			return Measure (backend);
		}
		
		static Size Measure (object backend)
		{
			LayoutBackend la = (LayoutBackend) backend;
			var ctx = la.Context.Context;
			var text = la.Text;
			
			if (la.Font != null) {
				ctx.SelectFont (la.Font);
				ctx.SetFontSize (la.Font.Size);
			}
			
			if (la.Width == -1) {
				var te = ctx.TextExtents (text);
				return new Size (te.Width, te.Height);
			}
			
			// Measure word by word
			
			double totalHeight = 0;
			double currentWidth = 0;
			double currentHeight = 0;
			la.LineBreaks.Clear ();
			
			double spaceWidth = ctx.TextExtents (" ").XAdvance;
			double spaceHeight = ctx.FontExtents.Height;
			var spaceExtents = new Cairo.TextExtents () {
				Width = spaceWidth,
				Height = spaceHeight,
				XAdvance = spaceWidth
			};
			
			int pos = 0;
			bool inLineStart = true;
			int prevPos = 0;
			string word = NextWord (text, ref pos);

			for (; word != null; prevPos=pos, word = NextWord (text, ref pos))
			{
				if (word.Length == 1 && word [0] == '\n') {
					if (inLineStart) {
						// Empty line
						currentHeight = spaceHeight;
					}
					totalHeight += currentHeight;
					la.LineBreaks.Add (pos);
					la.LineHeights.Add (currentHeight);
					currentHeight = 0;
					currentWidth = 0;
					inLineStart = true;
					continue;
				}
				
				inLineStart = false;
				bool isSpace = word.Length == 1 && word[0] == ' ';
				
				Cairo.TextExtents te;
				if (isSpace)
					te = spaceExtents;
				else
					te = ctx.TextExtents (word);
				
				if (currentWidth + te.Width > la.Width) {
					la.LineHeights.Add (currentHeight);
					la.LineBreaks.Add (isSpace ? pos : prevPos); // If a space causes a line break, we can ignore that space
					totalHeight += currentHeight;
					currentHeight = te.Height;
					if (isSpace)
						currentWidth = 0;
					else
						currentWidth = te.Width;
				}
				else {
					currentWidth += te.XAdvance;
					if (te.Height > currentHeight)
						currentHeight = te.Height;
				}
			}
			
			la.Measured = true;
			return new Size (la.Width, totalHeight);
		}
		
		static string NextWord (string text, ref int pos)
		{
			if (pos >= text.Length)
				return null;
			
			if (text[pos] == '\n') {
				pos ++;
				return "\n";
			}
			if (char.IsWhiteSpace (text[pos])) {
				pos ++;
				return " ";
			}
			int start = pos;
			while (pos < text.Length && !char.IsWhiteSpace (text[pos]))
				pos++;
			return text.Substring (start, pos - start);
		}
		
		public static void Draw (Cairo.Context ctx, object backend, double x, double y)
		{
			var la = (LayoutBackend) backend;
			
			var text = la.Text;
			
			var h = ctx.FontExtents.Ascent;
			y += h;
			
			ctx.MoveTo (x, y);
			
			if (la.Font != null) {
				ctx.SelectFont (la.Font);
				ctx.SetFontSize (la.Font.Size);
			}
			
			if (la.Width == -1) {
				ctx.ShowText (text);
				return;
			}
			
			if (!la.Measured)
				Measure (backend);
			
			// Render word by word
			
			int lastStart = 0;

			for (int i=0; i < la.LineBreaks.Count; i++) {
				if (la.Heigth != -1 && h > la.Heigth)
					break;

				var n = la.LineBreaks [i];
				string s = text.Substring (lastStart, n - lastStart).TrimEnd('\n','\r');
				ctx.ShowText (s);
				
				var lh = la.LineHeights [i];
				h += lh;
				y += lh;
				
				ctx.MoveTo (x, y);
				lastStart = n;
			}
		}
		
		#endregion
	}
}

