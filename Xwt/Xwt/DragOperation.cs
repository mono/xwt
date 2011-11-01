// 
// DragOperation.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

namespace Xwt
{
	public class DragOperation
	{
		TransferDataSource data = new TransferDataSource ();
		Widget source;
		
		internal DragOperation (Widget w)
		{
			source = w;
			Action = DragDropAction.Copy;
		}
		
		public DragDropAction Action { get; set; }
		
		public TransferDataSource Data {
			get { return data; }
		}
		
		public void Start ()
		{
			source.DragStart (data, Action);
		}
	}
	
	public class TransferDataSource
	{
		public DataRequestDelegate DataRequestCallback { get; set; }
		Dictionary<string,object> data = new Dictionary<string,object> ();
		
		public void AddValue (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value is string)
				data [TransferDataType.Text] = value;
			else if (value is Xwt.Drawing.Image)
				data [TransferDataType.Image] = value;
			else
				data [value.GetType ().FullName] = value;
		}
		
		public void AddType (string type)
		{
			data [type] = null;
		}
		
		public void AddType (Type type)
		{
			data [type.FullName] = null;
		}
		
		public string[] DataTypes {
			get {
				return data.Keys.ToArray ();
			}
		}
		
		public object GetValue (string type)
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
		
		public static byte[] SerializeValue (object val)
		{
			using (MemoryStream ms = new MemoryStream ()) {
				BinaryFormatter bf = new BinaryFormatter ();
				bf.Serialize (ms, val);
				return ms.ToArray ();
			}
		}
		
		public static object DeserializeValue (byte[] data)
		{
			using (MemoryStream ms = new MemoryStream (data)) {
				BinaryFormatter bf = new BinaryFormatter ();
				return bf.Deserialize (ms);
			}
		}
	}
	
	public class TransferDataStore: ITransferData
	{
		Dictionary<string,object> data = new Dictionary<string,object> ();
		
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
		
		public void AddValue (string type, byte[] value)
		{
			Type t = Type.GetType (type);
			if (t != null)
				data [type] = TransferDataSource.DeserializeValue (value);
			else
				data [type] = value;
		}
		
		object GetValue (string type)
		{
			object val;
			if (data.TryGetValue (type, out val)) {
				if (val != null)
					return val;
			}
			return null;
		}
		
		object ITransferData.GetValue (string type)
		{
			return GetValue (type);
		}
		
		T ITransferData.GetValue<T> ()
		{
			object ob = GetValue (typeof(T).FullName);
			if (ob == null || ob.GetType () == typeof(Type))
				return (T) ob;
			if (ob is byte[]) {
				T val = (T) TransferDataSource.DeserializeValue ((byte[])ob);
				data[typeof(T).FullName] = val;
				return val;
			}
			return (T) ob;
		}
		
		bool ITransferData.HasType (string type)
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
	
	public interface ITransferData
	{
		string Text { get; }
		Uri[] Uris { get; }
		Xwt.Drawing.Image Image { get; }
		
		object GetValue (string type);
		T GetValue<T> () where T:class;
		bool HasType (string type);
	}
	
	public static class TransferDataType
	{
		public const string Uri = "uri";
		public const string Text = "text";
		public const string Rtf = "rtf";
		public const string Image = "image";
	}
	
	public delegate object DataRequestDelegate (string type);
}

