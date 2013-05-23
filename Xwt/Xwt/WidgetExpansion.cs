//
// WidgetExpansion.cs
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

namespace Xwt
{
	public struct WidgetExpansion
	{
		float value;

		public static readonly WidgetExpansion Shrink = WidgetExpansion.FromValue (0f);
		public static readonly WidgetExpansion Expand = WidgetExpansion.FromValue (1f);

		public float Value {
			get {
				return value;
			}
			set {
				if (value < 0)
					this.value = 0;
				else if (value > 1)
					this.value = 1;
				else
					this.value = value;
			}
		}

		public static WidgetExpansion FromValue (float value)
		{
			return new WidgetExpansion () { Value = value };
		}

		public static bool operator == (WidgetExpansion s1, WidgetExpansion s2)
		{
			return s1.value == s2.value;
		}

		public static bool operator != (WidgetExpansion s1, WidgetExpansion s2)
		{
			return s1.value != s2.value;
		}

		public override bool Equals (object ob)
		{
			return (ob is WidgetExpansion) && value == ((WidgetExpansion)ob).value;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public override string ToString ()
		{
			if (this == Shrink)
				return "Start";
			else if (this == Expand)
				return "Center";
			else
				return value.ToString ();
		}
	}
}

