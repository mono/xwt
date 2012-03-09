// 
// TransferDataStore.cs
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
using Xwt.Engine;

namespace Xwt.Backends
{
	/// <summary>
	/// A collection of data that has been transferred through drag & drop or the clipboard
	/// </summary>
	public class TransferDataStore: ITransferData
	{
		Dictionary<TransferDataType,object> data = new Dictionary<TransferDataType,object> ();
		
		public void AddText (string text)
		{
			data [TransferDataType.Text] = text;
		}
		
		public void AddImage (Xwt.Drawing.Image image)
		{
			data [TransferDataType.Image] = image;
		}
		
		public void AddUris (Uri[] uris)
		{
			data [TransferDataType.Uri] = uris;
		}
		
		public void AddValue (TransferDataType type, byte[] value)
		{
			Type t = Type.GetType (type.Id);
			if (t != null)
				data [type] = TransferDataSource.DeserializeValue (value);
			else
				data [type] = value;
		}
		
		object GetValue (TransferDataType type)
		{
			object val;
			if (data.TryGetValue (type, out val)) {
				if (val != null)
					return val;
			}
			return null;
		}
		
		object ITransferData.GetValue (TransferDataType type)
		{
			return GetValue (type);
		}
		
		T ITransferData.GetValue<T> ()
		{
			object ob = GetValue (TransferDataType.FromType (typeof(T)));
			if (ob == null || ob.GetType () == typeof(Type))
				return (T) ob;
			if (ob is byte[]) {
				T val = (T) TransferDataSource.DeserializeValue ((byte[])ob);
				data[TransferDataType.FromType (typeof(T))] = val;
				return val;
			}
			return (T) ob;
		}
		
		bool ITransferData.HasType (TransferDataType type)
		{
			return data.ContainsKey (type);
		}
		
		string ITransferData.Text {
			get {
				return (string) GetValue (TransferDataType.Text);
			}
		}
		
		Uri[] ITransferData.Uris {
			get {
				var u = (Uri[]) GetValue (TransferDataType.Uri);
				return u ?? new Uri [0];
			}
		}
		
		Xwt.Drawing.Image ITransferData.Image {
			get {
				return (Xwt.Drawing.Image) GetValue (TransferDataType.Image);
			}
		}
	}
}
