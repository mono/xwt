//
// TextLayoutContext.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//       Lytico (http://limada.sourceforge.net)
//
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

#if !USE_WPF_RENDERING

using System.Drawing;

namespace Xwt.WPFBackend
{
	internal class TextLayoutContext
	{
		private readonly DrawingContext context;

		internal TextLayoutContext (DrawingContext context)
		{
			this.context = context;
			this.StringTrimming = StringTrimming.Word; 
		}

		internal double Width;
		internal double Height;
		internal string Text;
		internal Font Font;
		internal StringTrimming StringTrimming;
		
		public static readonly StringFormat StringFormat = 
			// The following FormatFlags are equivalent to StringFormat.GenericTypographic plus Word Trimming
			new StringFormat {
				Trimming = StringTrimming.Word,
				FormatFlags=StringFormatFlags.LineLimit | StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip
			};
			
		public Size GetSize ()
		{
			throw new System.NotImplementedException ();
//			return context.Graphics.MeasureString (Text, Font, (int)Width, StringFormat).ToXwtSize ();
		}
	}
}

#endif