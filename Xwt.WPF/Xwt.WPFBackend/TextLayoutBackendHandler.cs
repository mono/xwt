//
// TextLayoutBackendHandler.cs
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

using System;
using System.Drawing;
using Xwt.Backends;
using Xwt.Drawing;

using Font = Xwt.Drawing.Font;
using System.Collections.Generic;

namespace Xwt.WPFBackend
{
	public class WpfTextLayoutBackendHandler
		: TextLayoutBackendHandler
	{
		public override object Create (Context context)
		{
			var drawingContext = (DrawingContext)Toolkit.GetBackend (context);
			return new TextLayoutContext (drawingContext);
		}

		public override object Create (ICanvasBackend canvas)
		{
			var drawingContext = new DrawingContext (Graphics.FromImage (new Bitmap (1, 1)));
			return new TextLayoutContext (drawingContext);
		}

		public override void SetWidth (object backend, double value)
		{
			((TextLayoutContext) backend).Width = value;
		}

		public override void SetHeight (object backend, double value)
		{
			((TextLayoutContext) backend).Height = value;
		}

		public override void SetText (object backend, string text)
		{
			((TextLayoutContext) backend).Text = text;
		}

		public override void SetFont (object backend, Font font)
		{
			((TextLayoutContext) backend).Font = font.ToDrawingFont();
		}

		public override void SetTrimming (object backend, TextTrimming textTrimming)
		{
			((TextLayoutContext) backend).StringTrimming = textTrimming.ToDrawingStringTrimming();
			
		}

		public override Size GetSize (object backend)
		{
			return ((TextLayoutContext) backend).GetSize ();
		}
		
		public override void SetTextAttributes(object backend, IEnumerable<TextAttribute> textAttributes)
		{
			throw new NotImplementedException ();
		}

		public override int GetIndexFromCoordinates (object backend, double x, double y)
		{
			throw new NotImplementedException ();
		}
		
		public override Point GetCoordinateFromIndex (object backend, int index)
		{
			throw new NotImplementedException ();
		}

		public override void DisposeBackend (object backend)
		{
			// nothing
		}
	}
}
