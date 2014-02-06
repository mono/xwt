//
// Colors.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.ComponentModel;
using Xwt.Drawing;
using NUnit.Framework;

namespace Xwt
{
	[TestFixture]
	public class ColorTests
	{
		[Test]
		public void Red ()
		{
			var c = new Color ();
			c.Red = 1;
			Assert.AreEqual (1d, c.Red);
			Assert.AreEqual (0d, c.Green);
			Assert.AreEqual (0d, c.Blue);
			Assert.AreEqual (0d, c.Alpha);
		}

		[Test]
		public void Green ()
		{
			var c = new Color ();
			c.Green = 1;
			Assert.AreEqual (1d, c.Green);
			Assert.AreEqual (0d, c.Red);
			Assert.AreEqual (0d, c.Blue);
			Assert.AreEqual (0d, c.Alpha);
		}
		
		[Test]
		public void Blue ()
		{
			var c = new Color ();
			c.Blue = 1;
			Assert.AreEqual (1d, c.Blue);
			Assert.AreEqual (0d, c.Green);
			Assert.AreEqual (0d, c.Red);
			Assert.AreEqual (0d, c.Alpha);
		}
		
		[Test]
		public void Alpha ()
		{
			var c = new Color ();
			c.Alpha = 1;
			Assert.AreEqual (1d, c.Alpha);
			Assert.AreEqual (0d, c.Green);
			Assert.AreEqual (0d, c.Blue);
			Assert.AreEqual (0d, c.Red);
		}
		
		[Test]
		public void Constructor ()
		{
			var c = new Color (0.1d, 0.2d, 0.3d);
			Assert.AreEqual (0.1d, c.Red);
			Assert.AreEqual (0.2d, c.Green);
			Assert.AreEqual (0.3d, c.Blue);
			Assert.AreEqual (1d, c.Alpha);
		}

		[Test]
		public void ConstructorWithAlpha ()
		{
			var c = new Color (0.1d, 0.2d, 0.3d, 0.4d);
			Assert.AreEqual (0.1d, c.Red);
			Assert.AreEqual (0.2d, c.Green);
			Assert.AreEqual (0.3d, c.Blue);
			Assert.AreEqual (0.4d, c.Alpha);
		}
		
		[Test]
		public void FromBytes ()
		{
			var c = Color.FromBytes (255, 255, 255);
			Assert.AreEqual (1d, c.Red);
			Assert.AreEqual (1d, c.Green);
			Assert.AreEqual (1d, c.Blue);
			Assert.AreEqual (1d, c.Alpha);
			c = Color.FromBytes (128, 128, 128);
			Assert.AreEqual (128, (int)(c.Red * 255));
			Assert.AreEqual (128, (int)(c.Green * 255));
			Assert.AreEqual (128, (int)(c.Blue * 255));
			Assert.AreEqual (1, c.Alpha);
		}
		
		[Test]
		public void ParseColor ()
		{
			Color c;
			Assert.IsTrue (Color.TryParse ("#3a02b9", out c));
			Assert.AreEqual (0x3a, (int)(c.Red * 255));
			Assert.AreEqual (0x02, (int)(c.Green * 255));
			Assert.AreEqual (0xb9, (int)(c.Blue * 255));
			Assert.AreEqual (1d, c.Alpha);
		}
		
		[Test]
		public void ParseColorWithAlpha ()
		{
			Color c;
			Assert.IsTrue (Color.TryParse ("#3a02b9dd", out c));
			Assert.AreEqual (0x3a, (int)(c.Red * 255));
			Assert.AreEqual (0x02, (int)(c.Green * 255));
			Assert.AreEqual (0xb9, (int)(c.Blue * 255));
			Assert.AreEqual (0xdd, (int)(c.Alpha * 255));
		}
		
		[Test]
		public void ParseColorFailures ()
		{
			Color c;
			Assert.IsFalse (Color.TryParse ("#3a02b9ddff", out c));
			Assert.IsFalse (Color.TryParse ("3a02b9dd", out c));
			Assert.IsFalse (Color.TryParse ("#3a02b9j", out c));
		}

		[Test]
		public void Equality ()
		{
			var c1 = new Color (1, 0, 0);
			var c2 = new Color (1, 0, 0);
			Assert.IsTrue (c1 == c2);
			Assert.IsTrue (c1.Equals (c2));
			Assert.IsFalse (c1 != c2);

			c1 = new Color (0.9, 0, 0);
			Assert.IsFalse (c1 == c2);
			Assert.IsFalse (c1.Equals (c2));
			Assert.IsTrue (c1 != c2);
		}

		[Test]
		public void ColorConverterTest ()
		{
			var converter = TypeDescriptor.GetConverter (typeof(Color));
			var color = Color.FromBytes (30, 60, 90);
			Assert.AreEqual (color, converter.ConvertFromInvariantString (converter.ConvertToInvariantString (color)));
		}

		[Test]
		public void ColorConverterTest_WithAlpha ()
		{
			var converter = TypeDescriptor.GetConverter (typeof(Color));
			var color = Color.FromBytes (34, 56, 67, 20);
			Assert.AreEqual (color, converter.ConvertFromInvariantString (converter.ConvertToInvariantString (color)));
		}
	}
}

