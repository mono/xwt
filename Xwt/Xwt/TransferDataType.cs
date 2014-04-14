// 
// TransferDataType.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xwt.Drawing;
using Xwt.Backends;


namespace Xwt
{
	/// <summary>
	/// Data type to be used in drag &amp; drop and copy &amp; paste operations
	/// </summary>
	public class TransferDataType
	{
		string id;
		
		/// <summary>
		/// The URI data type. This is used for file and url copy and drag operations
		/// </summary>
		public static readonly TransferDataType Uri = FromId ("uri");
		
		/// <summary>
		/// The text data type
		/// </summary>
		public static readonly TransferDataType Text = FromId ("text");
		
		/// <summary>
		/// The RTF data type
		/// </summary>
		public static readonly TransferDataType Rtf = FromId ("rtf");

		/// <summary>
		/// The image data type
		/// </summary>
		public static readonly TransferDataType Image = FromId ("image");

		/// <summary>
		/// The HTML data type
		/// </summary>
		public static readonly TransferDataType Html = FromId ("html");

		private TransferDataType (string id)
		{
			this.id = id;
		}
		
		/// <summary>
		/// Gets the identifier of the data type
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		public string Id {
			get { return id; }
		}
		
		/// <summary>
		/// Creates a data type using a custom identifier
		/// </summary>
		/// <returns>
		/// The data type
		/// </returns>
		/// <param name='id'>
		/// The identifier.
		/// </param>
		public static TransferDataType FromId (string id)
		{
			return new TransferDataType (id);
		}
		
		/// <summary>
		/// Creates a transfer data type from a CLR type
		/// </summary>
		/// <returns>
		/// The data type.
		/// </returns>
		/// <param name='type'>
		/// A type
		/// </param>
		/// <remarks>
		/// This is the data type to be used when transferring
		/// whole objects through drag &amp; drop or copy &amp; paste
		/// </remarks>
		public static TransferDataType FromType (Type type)
		{
			if (type == typeof(string))
				return TransferDataType.Text;
			else if (type == typeof(Xwt.Drawing.Image))
				return TransferDataType.Image;
			else
				return FromId (type.AssemblyQualifiedName);
		}
		
		public override bool Equals (object obj)
		{
			TransferDataType t = obj as TransferDataType;
			return t != null && t.id == id;
		}
		
		public override int GetHashCode ()
		{
			return id.GetHashCode ();
		}
		
		public static bool operator == (TransferDataType c1, TransferDataType c2) 
		{
			if (object.ReferenceEquals (c1, c2))
				return true;
			
			if ((object)c1 == null || (object)c2 == null)
				return false;
			
			return c1.id == c2.id;
		}
		
		public static bool operator != (TransferDataType c1, TransferDataType c2) 
		{
			return !(c1 == c2);
		}
	}
}
