// 
// Clipboard.cs
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
using System.Linq;
using Xwt.Backends;
using Xwt.Drawing;


namespace Xwt
{
	public static class Clipboard
	{
		static ClipboardBackend Backend {
			get { return Toolkit.CurrentEngine.ClipboardBackend; }
		}
		
		public static void Clear ()
		{
			Backend.Clear ();
		}
		
		public static bool ContainsData (TransferDataType type)
		{
			return Backend.IsTypeAvailable (type);
		}
		
		public static bool ContainsData<T> ()
		{
			return Backend.IsTypeAvailable (TransferDataType.FromType (typeof(T)));
		}
		
		public static bool ContainsText ()
		{
			return ContainsData (TransferDataType.Text);
		}
		
		public static bool ContainsImage ()
		{
			return ContainsData (TransferDataType.Image);
		}
		
		public static Image GetImage ()
		{
			return (Image) GetData (TransferDataType.Image);
		}
		
		public static IAsyncResult BeginGetImage (AsyncCallback callback, object state)
		{
			return Backend.BeginGetData (TransferDataType.Image, callback, state);
		}
		
		public static Image EndGetImage (IAsyncResult ares)
		{
			return (Image) Backend.EndGetData (ares);
		}
		
		public static string GetText ()
		{
			return (string) GetData (TransferDataType.Text);
		}
		
		public static IAsyncResult BeginGetText (AsyncCallback callback, object state)
		{
			return Backend.BeginGetData (TransferDataType.Text, callback, state);
		}
		
		public static string EndGetText (IAsyncResult ares)
		{
			return (string) Backend.EndGetData (ares);
		}
		
		public static object GetData (TransferDataType type)
		{
			return Backend.GetData (type);
		}
		
		public static T GetData<T> ()
		{
			return (T) Backend.GetData (TransferDataType.FromType (typeof(T)));
		}
		
		public static IAsyncResult BeginGetData (TransferDataType type, AsyncCallback callback, object state)
		{
			return Backend.BeginGetData (type, callback, state);
		}
		
		public static object EndGetData (IAsyncResult ares)
		{
			return Backend.EndGetData (ares);
		}
		
		public static IAsyncResult BeginGetData<T> (AsyncCallback callback, object state)
		{
			return Backend.BeginGetData (TransferDataType.FromType (typeof(T)), callback, state);
		}
		
		public static T EndGetData<T> (IAsyncResult ares)
		{
			return (T) Backend.EndGetData (ares);
		}
		
		public static void SetText (string text)
		{
			Backend.SetData (TransferDataType.Text, delegate {
				return text;
			});
		}
		
		public static void SetText (Func<string> textSource)
		{
			Backend.SetData (TransferDataType.Text, textSource);
		}
		
		public static void SetImage (Image image)
		{
			Backend.SetData (TransferDataType.Image, delegate {
				return image;
			});
		}
		
		public static void SetImage (Func<Image> imageSource)
		{
			Backend.SetData (TransferDataType.Image, imageSource);
		}
		
		public static void SetData<T> (T data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			SetData (TransferDataType.FromType (data.GetType ()), data);
		}
		
		public static void SetData (TransferDataType type, object data)
		{
			Backend.SetData (type, delegate {
				return data;
			});
		}
		
		public static void SetData (TransferDataType type, Func<object> dataSource)
		{
			Backend.SetData (type, dataSource);
		}
		
		public static void SetData<T> (Func<T> dataSource)
		{
			Backend.SetData (TransferDataType.FromType (typeof(T)), delegate () {
				return dataSource ();
			});
		}
	}
}

