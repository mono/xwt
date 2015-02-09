// 
// TransferDataSource.cs
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
	/// A collection of data to be transferred through drag and drop or the clipboard
	/// </summary>
	public sealed class TransferDataSource
	{
		Dictionary<TransferDataType,object> data = new Dictionary<TransferDataType,object> ();

		/// <summary>
		/// Gets or sets the data request callback.
		/// </summary>
		/// <value>
		/// The data request callback.
		/// </value>
		/// <remarks>
		/// This callback can be used in combination with the AddType method to
		/// generate the data on demand. In some scenarios, the drop/paste
		/// side of a drag&amp;drop or clipboard operation can decide if a drop/paste
		/// is allowed or not by checking the available data type in this
		/// data source. Once the operation is accepted, the DataRequestCallback
		/// callback will be invoked to get the data for the type.
		/// </remarks>
		public DataRequestDelegate DataRequestCallback { get; set; }
		
		/// <summary>
		/// Adds a value to the data source
		/// </summary>
		/// <param name='value'>
		/// Value.
		/// </param>
		public void AddValue<T> (T value) where T : class
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			data [TransferDataType.FromType (typeof (T))] = value;
		}
		
		/// <summary>
		/// Registers that the data store contains data of the provided type
		/// </summary>
		/// <param name='type'>
		/// The transfer data type
		/// </param>
		/// <remarks>
		/// This method can be used in combination with DataRequestCallback to
		/// generate the data on demand. In some scenarios, the drop/paste
		/// side of a drag&amp;drop or clipboard operation can decide if a drop/paste
		/// is allowed or not by checking the available data type in this
		/// data source. Once the operation is accepted, the DataRequestCallback
		/// callback will be invoked to get the data for the type.
		/// </remarks>
		public void AddType (TransferDataType type)
		{
			data [type] = null;
		}
		
		/// <summary>
		/// Registers that the data store contains data of the provided type
		/// </summary>
		/// <param name='type'>
		/// A type
		/// </param>
		/// <remarks>
		/// This method can be used in combination with DataRequestCallback to
		/// generate the data on demand. In some scenarios, the drop/paste
		/// side of a drag&amp;drop or clipboard operation can decide if a drop/paste
		/// is allowed or not by checking the available data type in this
		/// data source. Once the operation is accepted, the DataRequestCallback
		/// callback will be invoked to get the data for the type.
		/// </remarks>
		public void AddType (Type type)
		{
			data [TransferDataType.FromType (type)] = null;
		}
		
		/// <summary>
		/// Gets the types included in this data source
		/// </summary>
		public TransferDataType[] DataTypes {
			get {
				return data.Keys.ToArray ();
			}
		}

		/// <summary>
		/// Gets the value for a specific type
		/// </summary>
		/// <returns>
		/// The value, or null if there is not value for this type
		/// </returns>
		/// <param name='type'>
		/// A type.
		/// </param>
		public object GetValue (TransferDataType type)
		{
			object val;
			if (data.TryGetValue (type, out val)) {
				if (val != null)
					return val;
				if (DataRequestCallback != null)
					return DataRequestCallback (type);
			}
			return null;
		}
		
		/// <summary>
		/// Serializes a value to a byte array using <see cref="System.Runtime.Serialization.Formatters.Binary.BinaryFormatter"/> .
		/// </summary>
		/// <returns>The serialized value.</returns>
		/// <param name="val">The value to serialize.</param>
		public static byte[] SerializeValue (object val)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (ms, val);
				return ms.ToArray ();
			}
		}
		
		/// <summary>
		/// Deserializes a value from a byte array.
		/// </summary>
		/// <returns>The deserialized value.</returns>
		/// <param name="data">The byte array containing the serialized value.</param>
		public static object DeserializeValue (byte[] data)
		{
			using (MemoryStream ms = new MemoryStream (data)) {
				BinaryFormatter bf = new BinaryFormatter ();
				return bf.Deserialize (ms);
			}
		}
	}
	
	/// <summary>
	/// Data request delegate, returns the data for a specific transfer data type request.
	/// </summary>
	public delegate object DataRequestDelegate (TransferDataType type);
}
