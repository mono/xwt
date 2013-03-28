//
// FontTests.cs
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

using NUnit.Framework;
using Xwt.Drawing;

namespace Xwt
{
	[TestFixture]
	public class FontTests
	{
		[Test]
		public void FromNameEmpty ()
		{
			var f = Font.FromName ("");
			Assert.AreEqual (Font.SystemFont, f);
		}

		[Test]
		public void WithFamily ()
		{
			var f1 = Font.SystemFont;
			var f2 = f1.WithFamily ("Courier");
			Assert.AreEqual ("Courier", f2.Family);
			Assert.AreEqual (f1.Size, f2.Size);
			Assert.AreEqual (f1.Stretch, f2.Stretch);
			Assert.AreEqual (f1.Style, f2.Style);
			Assert.AreEqual (f1.Weight, f2.Weight);
		}
		
		[Test]
		public void WithSize ()
		{
			var f1 = Font.SystemFont;
			var f2 = f1.WithSize (33);
			Assert.AreEqual (f1.Family, f2.Family);
			Assert.AreEqual (33d, f2.Size);
			Assert.AreEqual (f1.Stretch, f2.Stretch);
			Assert.AreEqual (f1.Style, f2.Style);
			Assert.AreEqual (f1.Weight, f2.Weight);
		}
		
		[Test]
		public void WithStretch ()
		{
			var f1 = Font.SystemFont;
			var f2 = f1.WithStretch (FontStretch.Condensed);
			Assert.AreEqual (f1.Family, f2.Family);
			Assert.AreEqual (f1.Size, f2.Size);
			Assert.AreEqual (FontStretch.Condensed, f2.Stretch);
			Assert.AreEqual (f1.Style, f2.Style);
			Assert.AreEqual (f1.Weight, f2.Weight);
		}
		
		[Test]
		public void WithStyle ()
		{
			var f1 = Font.SystemFont;
			var f2 = f1.WithStyle (FontStyle.Oblique);
			Assert.AreEqual (f1.Family, f2.Family);
			Assert.AreEqual (f1.Size, f2.Size);
			Assert.AreEqual (f1.Stretch, f2.Stretch);
			Assert.AreEqual (FontStyle.Oblique, f2.Style);
			Assert.AreEqual (f1.Weight, f2.Weight);
		}
		
		[Test]
		public void WithWeight ()
		{
			var f1 = Font.SystemFont;
			var f2 = f1.WithWeight (FontWeight.Bold);
			Assert.AreEqual (f1.Family, f2.Family);
			Assert.AreEqual (f1.Size, f2.Size);
			Assert.AreEqual (f1.Stretch, f2.Stretch);
			Assert.AreEqual (f1.Style, f2.Style);
			Assert.AreEqual (FontWeight.Bold, f2.Weight);
		}

		[Test]
		public void FromNameOnlyFamily ()
		{
			var f = Font.FromName ("Arial");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (Font.SystemFont.Size, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameOnlySize ()
		{
			var f = Font.FromName ("33");
			Assert.AreEqual (Font.SystemFont.Family, f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameOnlyFamilyAndSize ()
		{
			var f = Font.FromName ("Arial 33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameOnlyFamilyAndSizeWithFallbackSecondChoice ()
		{
			var f = Font.FromName ("Foobar, Arial, dummy, 33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameOnlyFamilyAndSizeWithFallbackFirstChoice ()
		{
			var f = Font.FromName ("Arial, Foobar, dummy 33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameOnlyFamilyAndSizeWithFallbackNoChoice ()
		{
			var f = Font.FromName ("Foobar, dummy 33");
			Assert.AreEqual (Font.SystemFont.Family, f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}

		[Test]
		public void FromNameWithStyle ()
		{
			var f = Font.FromName ("Arial, dummy italic  33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Italic, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameWithWeight ()
		{
			var f = Font.FromName ("Arial, dummy bold 33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Normal, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Bold, f.Weight);
		}
		
		[Test]
		public void FromNameWithStretch ()
		{
			var f = Font.FromName ("Arial, dummy condensed 33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Condensed, f.Stretch);
			Assert.AreEqual (FontStyle.Normal, f.Style);
			Assert.AreEqual (FontWeight.Normal, f.Weight);
		}
		
		[Test]
		public void FromNameWithAllStyles ()
		{
			var f = Font.FromName ("Arial, dummy  expanded  Oblique Light  33");
			Assert.AreEqual ("Arial", f.Family);
			Assert.AreEqual (33d, f.Size);
			Assert.AreEqual (FontStretch.Expanded, f.Stretch);
			Assert.AreEqual (FontStyle.Oblique, f.Style);
			Assert.AreEqual (FontWeight.Light, f.Weight);
		}
		
		[Test]
		public void FontEquals ()
		{
			var f1 = Font.FromName ("Arial expanded Oblique Light 33");
			var f2 = Font.FromName ("Arial expanded Oblique Light 33");
			Assert.IsTrue (f1.Equals (f2));
		}
	}
}

