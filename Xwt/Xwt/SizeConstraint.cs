//
// SizeConstraint.cs
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
	public struct SizeContraint
	{
		// The value '0' is used for Unconstrained, since that's the default value of SizeContraint
		// Since a constraint of '0' is valid, we use NegativeInfinity as a marker for constraint=0.
		double value;

		public static readonly SizeContraint Unconstrained; 

		static public implicit operator SizeContraint (double size)
		{
			return new SizeContraint () { RequiredSize = size };
		}

		public static SizeContraint RequireSize (double size)
		{
			return new SizeContraint () { RequiredSize = size };
		}

		public double RequiredSize {
			get {
				if (double.IsNegativeInfinity (value))
					return 0;
				else
					return value;
			}
			set {
 				if (value <= 0)
					this.value = double.NegativeInfinity;
				else
					this.value = value; 
			}
		}

		public bool IsConstrained {
			get { return value != 0; }
		}

		public static bool operator == (SizeContraint s1, SizeContraint s2)
		{
			return (s1.value == s2.value);
		}

		public static bool operator != (SizeContraint s1, SizeContraint s2)
		{
			return (s1.value != s2.value);
		}
		
		public static SizeContraint operator + (SizeContraint c, double s)
		{
			if (!c.IsConstrained)
				return c;
			else
				return SizeContraint.RequireSize (c.RequiredSize + s);
		}

		public static SizeContraint operator - (SizeContraint c, double s)
		{
			if (!c.IsConstrained)
				return c;
			else
				return SizeContraint.RequireSize (Math.Max (c.RequiredSize - s, 0));
		}

		public override bool Equals (object ob)
		{
			return (ob is SizeContraint) && this == (SizeContraint)ob;
		}

		public override int GetHashCode ()
		{
			return value.GetHashCode ();
		}

		public override string ToString ()
		{
			if (IsConstrained)
				return value.ToString ();
			else
				return "Unconstrained";
		}
	}
}

