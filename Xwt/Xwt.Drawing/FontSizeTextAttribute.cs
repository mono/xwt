//
// FontWeightTextAttribute.cs
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

using System.Linq;
using System.Collections.Generic;

namespace Xwt.Drawing
{
	public sealed class FontSizeTextAttribute : TextAttribute
	{
		const float PangoScale = 1024;
		public const string XXSmall = "xx-small",
			XSmall = "x-small", 
			Small = "small", 
			Medium = "medium", 
			Large = "large", 
			XLarge = "x-large", 
			XXLarge = "xx-large",
			Smaller = "smaller", 
			Larger = "larger";

		public float Size { get; private set; }
		public string OriginalSizeStringValue { get; private set; }

		public static Dictionary<string, float> SizeAbsoluteValues = GetAbsoluteSizeValues ();
		static Dictionary<string, float> GetAbsoluteSizeValues ()
		{
			var mediumSize = (float)Font.SystemFont.Size;
			var result = new Dictionary<string, float> {
				{ XXLarge, mediumSize + 16 },
				{ XLarge, mediumSize + 8 },
				{ Large, mediumSize + 4 },
				{ Medium, mediumSize },
				{ Small, mediumSize - 2 },
				{ XSmall, mediumSize - 4 },
				{ XXSmall, mediumSize - 8 }
			};
			return result;
		}

		public override bool Equals (object t)
		{
			var ot = t as FontSizeTextAttribute;
			return ot != null && Size.Equals (ot.Size) && base.Equals (t);
		}

		public void SetSize (string value)
		{
			OriginalSizeStringValue = value;
			float size;
			if (float.TryParse (value, out size)) {
				if (size >= PangoScale) {
					Size = size / PangoScale; //we need convert this to pt. values
				} else {
					Size = size;
				}
				return;
			}
			if (SizeAbsoluteValues.TryGetValue (value,out size)) {
				Size = size;
				return;
			}
			//we don't cover relative values (smaller/larger) we set medium as default
			Size = SizeAbsoluteValues[Medium];
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode () ^ Size.GetHashCode ();
		}
	}
}
