//
// StyleSet.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.CodeDom;

namespace Xwt.Drawing
{
	public struct StyleSet: IEnumerable<string>, IEquatable<StyleSet>
	{
		string [] styles;

		public static readonly StyleSet Empty = new StyleSet ();

		public StyleSet AddRange (StyleSet styleSet)
		{
			if (styles == null)
				return styleSet;
			return AddRange (styleSet.styles);
		}

		public StyleSet AddRange (string[] newStyles)
		{
			if (newStyles == null || newStyles.Length == 0)
				return this;
			
			if (styles == null) {
				return new StyleSet {
					styles = newStyles.ToArray ()
				};
			} else {
				var ss = styles;
				if (newStyles.All (s => ss.Contains (s)))
					return this;
				return new StyleSet {
					styles = styles.Union (newStyles).ToArray ()
				};
			}
		}

		public StyleSet Add (string style)
		{
			if (style == null)
				throw new ArgumentNullException ("style");
			
			if (styles != null && styles.Contains (style))
				return this;
			var newStyles = new string [styles != null ? styles.Length + 1 : 1];
			if (styles != null)
				Array.Copy (styles, newStyles, styles.Length);
			newStyles [newStyles.Length - 1] = style;
			return new StyleSet {
				styles = newStyles
			};
		}

		public StyleSet Remove (string style)
		{
			if (style == null)
				throw new ArgumentNullException ("style");

			if (styles == null)
				return this;
			
			int i = Array.IndexOf (styles, style);
			if (i == -1)
				return this;
			
			if (styles.Length == 1)
				return Empty;
			
			var newStyles = new string [styles.Length - 1];
			for (int n = 0, m = 0; n < styles.Length; n++) {
				if (n == i)
					continue;
				newStyles [m++] = styles [n];
			}
			return new StyleSet {
				styles = newStyles
			};
		}

		public StyleSet RemoveAll (string [] styles)
		{
			if (styles == null || styles.Length == 0)
				return this;

			if (this.styles == null)
				return this;

			return new StyleSet {
				styles = this.styles.Except (styles).ToArray ()
			};
		}

		public IEnumerator<string> GetEnumerator ()
		{
			if (styles == null)
				return Enumerable.Empty<string> ().GetEnumerator ();
			else
				return ((IEnumerable<string>)styles).GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public static bool operator == (StyleSet c1, StyleSet c2)
		{
			return c1.Equals (c2);
		}

		public static bool operator != (StyleSet c1, StyleSet c2)
		{
			return !c1.Equals (c2);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is StyleSet))
				return false;
			return Equals ((StyleSet)obj);
		}

		public bool Equals (StyleSet other)
		{
			if (other.styles == styles)
				return true;
			if (other.styles == null || styles == null || other.styles.Length != styles.Length)
				return false;
			for (int n = 0; n < styles.Length; n++)
				if (styles [n] != other.styles [n])
					return false;
			return true;
		}

		public override int GetHashCode ()
		{
			if (styles == null)
				return -1;
			unchecked {
				int n = 0;
				foreach (var v in styles)
					n ^= v.GetHashCode ();
				return n;
			}
		}
	}
}

