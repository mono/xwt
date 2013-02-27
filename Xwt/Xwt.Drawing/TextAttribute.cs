//
// TextAttribute.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
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

namespace Xwt.Drawing
{
	/// <summary>
	/// This is the base class for all Xwt text attributes.
	/// </summary>
	public abstract class TextAttribute
	{
		/// <summary>
		/// The start index of this attribute.
		/// </summary>
		public uint StartIndex { get; set; }

		/// <summary>
		/// The end index of this attribute.
		/// Invariant: <c>EndIndex == StartIndex + Length</c>
		/// </summary>
		public uint EndIndex { get { return StartIndex + Length; } }

		/// <summary>
		/// The length of this attribute.
		/// </summary>
		public uint Length { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttribute"/> class.
		/// </summary>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttribute (uint startIndex, uint length)
		{
			this.StartIndex = startIndex;
			this.Length = length;
		}
	}

	/// <summary>
	/// A text attribute that represents a foreground color.
	/// </summary>
	public sealed class TextAttributeForeground : TextAttribute
	{
		/// <summary>
		/// The color represented by this attribute.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeBackground"/> class.
		/// </summary>
		/// <param name="color">The color represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeForeground  (Color color, uint startIndex, uint endIndex) : base (startIndex, endIndex)
		{
			this.Color = color;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeBackground"/> class.
		/// </summary>
		/// <param name="color">The color represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeForeground  (Color color, int startIndex, int endIndex) : this (color, (uint)startIndex, (uint)endIndex)
		{
		}
	}
	
	/// <summary>
	/// A text attribute that represents a foreground color.
	/// </summary>
	public sealed class TextAttributeBackground : TextAttribute
	{
		/// <summary>
		/// The color represented by this attribute.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeBackground"/> class.
		/// </summary>
		/// <param name="color">The color represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeBackground  (Color color, uint startIndex, uint endIndex) : base (startIndex, endIndex)
		{
			this.Color = color;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeBackground"/> class.
		/// </summary>
		/// <param name="color">The color represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeBackground  (Color color, int startIndex, int endIndex) : this (color, (uint)startIndex, (uint)endIndex)
		{
		}
	}

	/// <summary>
	/// An enumeration specifying different text styles.
	/// </summary>
	public enum TextStyle
	{
		/// <summary>
		/// The font is upright.
		/// </summary>
		Normal,
		
		/// <summary>
		/// A font slanted in an italic style.
		/// </summary>
		Italic,
		
		/// <summary>
		/// A font slanted, but in a roman style.
		/// </summary>
		Oblique
	}
	
	/// <summary>
	/// A text attribute that represents a text style.
	/// </summary>
	public sealed class TextAttributeStyle : TextAttribute
	{
		/// <summary>
		/// The text style represented by this attribute.
		/// </summary>
		public TextStyle TextStyle { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeStyle"/> class.
		/// </summary>
		/// <param name="textStyle">The text style represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeStyle (TextStyle textStyle, uint startIndex, uint length) : base (startIndex, length)
		{
			this.TextStyle = textStyle;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeStyle"/> class.
		/// </summary>
		/// <param name="textStyle">The text style represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeStyle (TextStyle textStyle, int startIndex, int length) : this (textStyle, (uint)startIndex, (uint)length)
		{
		}
	}

	/// <summary>
	/// An enumeration specifying different text weights.
	/// </summary>
	public enum TextWeight
	{
		/// <summary>
		/// The default weight
		/// </summary>
		Normal,
		
		/// <summary>
		/// The bold weight
		/// </summary>
		Bold,
		
		/// <summary>
		/// The heavy weight
		/// </summary>
		Heavy,
		
		/// <summary>
		/// The light weight
		/// </summary>
		Light
	}
	
	/// <summary>
	/// A text attribute that represents a text style.
	/// </summary>
	public sealed class TextAttributeWeight : TextAttribute
	{
		/// <summary>
		/// The text weight represented by this attribute.
		/// </summary>
		public TextWeight TextWeight { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeWeight"/> class.
		/// </summary>
		/// <param name="textWeight">The text weight represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeWeight (TextWeight textWeight, uint startIndex, uint length) : base (startIndex, length)
		{
			this.TextWeight = textWeight;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeWeight"/> class.
		/// </summary>
		/// <param name="textWeight">The text weight represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeWeight (TextWeight textWeight, int startIndex, int length) : this (textWeight, (uint)startIndex, (uint)length)
		{
		}
	}

	/// <summary>
	/// An enumeration specifying different text decorations.
	/// </summary>
	[Flags]
	public enum TextDecoration
	{
		/// <summary>
		/// The default weight
		/// </summary>
		Underline,
		
		/// <summary>
		/// The bold weight
		/// </summary>
		Strikethrough
	}
	
	/// <summary>
	/// A text attribute that represents a text style.
	/// </summary>
	public sealed class TextAttributeDecoration : TextAttribute
	{
		/// <summary>
		/// The text decoration represented by this attribute.
		/// </summary>
		public TextDecoration TextDecoration { get; set; }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeDecoration"/> class.
		/// </summary>
		/// <param name="textDecoration">The text decoration represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeDecoration (TextDecoration textDecoration, uint startIndex, uint length) : base (startIndex, length)
		{
			this.TextDecoration = textDecoration;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Xwt.Drawing.TextAttributeStyle"/> class.
		/// </summary>
		/// <param name="textDecoration">The text decoration represented by this attribute.</param>
		/// <param name="startIndex">The start index of this attribute.</param>
		/// <param name="length">The length of this attribute.</param>
		public TextAttributeDecoration (TextDecoration textDecoration, int startIndex, int length) : this (textDecoration, (uint)startIndex, (uint)length)
		{
		}
	}
}
