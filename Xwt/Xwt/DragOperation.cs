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
using Xwt.Drawing;
using Xwt.Backends;
using Xwt.Engine;

namespace Xwt
{
	public class DragOperation
	{
		TransferDataSource data = new TransferDataSource ();
		Widget source;
		DragDropAction action;
		bool started;
		Image image;
		double hotX;
		double hotY;
		
		public event EventHandler<DragFinishedEventArgs> Finished;
		
		internal DragOperation (Widget w)
		{
			source = w;
			AllowedActions = DragDropAction.All;
		}
		
		/// <summary>
		/// A bitmask of the allowed drag actions for this drag.
		/// </summary>
		public DragDropAction AllowedActions {
			get { return action; } 
			set {
				if (started)
					throw new InvalidOperationException ("The drag action must be set before starting the drag operation");
				action = value;
			}
		}
		
		public TransferDataSource Data {
			get { return data; }
		}
		
		public void SetDragImage (Image image, double hotX, double hotY)
		{
			if (started)
				throw new InvalidOperationException ("The drag image must be set before starting the drag operation");
			this.image = image;
			this.hotX = hotX;
			this.hotY = hotY;
		}
		
		public void Start ()
		{
			if (!started) {
				started = true;
				source.DragStart (GetStartData ());
			}
		}

		internal void NotifyFinished (DragFinishedEventArgs args)
		{
			if (Finished != null)
				Finished (this, args);
		}
		
		internal DragStartData GetStartData ()
		{
			return new DragStartData (data, action, WidgetRegistry.GetBackend (image), hotX, hotY);
		}
		
	}
	
	public sealed class TransferDataSource
	{
		public DataRequestDelegate DataRequestCallback { get; set; }
		Dictionary<TransferDataType,object> data = new Dictionary<TransferDataType,object> ();
		
		public void AddValue (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			data [TransferDataType.FromType (value.GetType ())] = value;
		}
		
		public void AddType (TransferDataType type)
		{
			data [type] = null;
		}
		
		public void AddType (Type type)
		{
			data [TransferDataType.FromType (type)] = null;
		}
		
		public TransferDataType[] DataTypes {
			get {
				return data.Keys.ToArray ();
			}
		}
		
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
	
	public interface ITransferData
	{
		string Text { get; }
		Uri[] Uris { get; }
		Xwt.Drawing.Image Image { get; }
		
		object GetValue (TransferDataType type);
		T GetValue<T> () where T:class;
		bool HasType (TransferDataType type);
	}
	
	public class TransferDataType
	{
		string id;
		
		public static readonly TransferDataType Uri = FromId ("uri");
		public static readonly TransferDataType Text = FromId ("text");
		public static readonly TransferDataType Rtf = FromId ("rtf");
		public static readonly TransferDataType Image = FromId ("image");
		
		private TransferDataType (string id)
		{
			this.id = id;
		}
		
		public string Id {
			get { return id; }
		}
		
		public static TransferDataType FromId (string name)
		{
			return new TransferDataType (name);
		}
		
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
	
	public delegate object DataRequestDelegate (TransferDataType type);
}

