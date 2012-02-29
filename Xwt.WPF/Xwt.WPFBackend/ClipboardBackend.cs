// 
// ClipboardBackend.cs
//  
// Author:
//       Eric Maupin <ermau@xamarin.com>
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Xwt.Backends;
using WindowsClipboard = System.Windows.Clipboard;

namespace Xwt.WPFBackend
{
	public class ClipboardBackend
		: IClipboardBackend
	{
		public void Clear ()
		{
			WindowsClipboard.Clear();
		}

		public void SetData (TransferDataType type, Func<object> dataSource)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");
			
			if (type == TransferDataType.Image) {
				BitmapSource bmp = dataSource() as BitmapSource;
				if (bmp == null)
					throw new ArgumentException ("data is not the incorrect type", "data");

				WindowsClipboard.SetImage (bmp);
				return;
			}

			if (type == TransferDataType.Text) {
				string text = dataSource() as string;
				if (text == null)
					throw new ArgumentException ("data is not the correct type", "data");

				WindowsClipboard.SetText (text);
				return;
			}

			if (type == TransferDataType.Text) {
				string text = dataSource () as string;
				if (text == null)
					throw new ArgumentException ("data is not the correct type", "data");

				WindowsClipboard.SetText (text, TextDataFormat.Rtf);
				return;
			}
		}

		public bool IsTypeAvailable (TransferDataType type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type == TransferDataType.Image)
				return WindowsClipboard.ContainsImage ();
			if (type == TransferDataType.Text) {
				return WindowsClipboard.ContainsText (TextDataFormat.UnicodeText) ||
				       WindowsClipboard.ContainsText (TextDataFormat.Text);
			}
			if (type == TransferDataType.Rtf)
				return WindowsClipboard.ContainsText (TextDataFormat.Rtf);
			
			throw new NotImplementedException();
		}

		public object GetData (TransferDataType type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			while (!IsTypeAvailable (type))
				Thread.Sleep (1);

			if (type == TransferDataType.Image)
				return WindowsClipboard.GetImage ();
			if (type == TransferDataType.Rtf)
				return WindowsClipboard.GetText (TextDataFormat.Rtf);
			if (type == TransferDataType.Text) {
				if (WindowsClipboard.ContainsText (TextDataFormat.UnicodeText))
					return WindowsClipboard.GetText();
				
				return WindowsClipboard.GetText (TextDataFormat.Text);
			}

			throw new NotImplementedException();
		}

		public IAsyncResult BeginGetData (TransferDataType type, AsyncCallback callback, object state)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (callback == null)
				throw new ArgumentNullException ("callback");

			return Task<object>.Factory.StartNew (s => GetData (type), state)
				.ContinueWith (t => callback (t));
		}

		public object EndGetData (IAsyncResult ares)
		{
			if (ares == null)
				throw new ArgumentNullException ("ares");

			Task<object> t = ares as Task<object>;
			if (t == null)
				throw new ArgumentException ("ares is the incorrect type", "ares");

			return t.Result;
		}
	}
}
