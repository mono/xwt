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
using Xwt.Backends;
using WindowsClipboard = System.Windows.Clipboard;

namespace Xwt.WPFBackend
{
	public class WpfClipboardBackend
		: ClipboardBackend
	{
		public override void Clear ()
		{
			WindowsClipboard.Clear();
		}

		public override void SetData (TransferDataType type, Func<object> dataSource)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (dataSource == null)
				throw new ArgumentNullException ("dataSource");
			if (type == TransferDataType.Html) {
				WindowsClipboard.SetData (type.ToWpfDataFormat (), GenerateCFHtml (dataSource ().ToString ()));
			} else {
				WindowsClipboard.SetData (type.ToWpfDataFormat (), dataSource ());
			}
		}

		static readonly string emptyCFHtmlHeader = GenerateCFHtmlHeader (0, 0, 0, 0);

		/// <summary>
		/// Generates a CF_HTML cliboard format document
		/// </summary>
		string GenerateCFHtml (string htmlFragment)
		{
			int startHTML     = emptyCFHtmlHeader.Length;
			int startFragment = startHTML;
			int endFragment   = startFragment + System.Text.Encoding.UTF8.GetByteCount (htmlFragment);
			int endHTML       = endFragment;
			return GenerateCFHtmlHeader (startHTML, endHTML, startFragment, endFragment) + htmlFragment;
		}

		/// <summary>
		/// Generates a CF_HTML clipboard format header.
		/// </summary>
		static string GenerateCFHtmlHeader (int startHTML, int endHTML, int startFragment, int endFragment)
		{
			return
				"Version:0.9" + Environment.NewLine +
					string.Format ("StartHTML: {0:d8}", startHTML) + Environment.NewLine +
					string.Format ("EndHTML: {0:d8}", endHTML) + Environment.NewLine +
					string.Format ("StartFragment: {0:d8}", startFragment) + Environment.NewLine +
					string.Format ("EndFragment: {0:d8}", endFragment) + Environment.NewLine;
		}

		public override bool IsTypeAvailable (TransferDataType type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			return WindowsClipboard.ContainsData (type.ToWpfDataFormat ());
		}

		public override object GetData (TransferDataType type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (!IsTypeAvailable (type))
				return null;

			return WindowsClipboard.GetData (type.ToWpfDataFormat ());
		}

		public override IAsyncResult BeginGetData (TransferDataType type, AsyncCallback callback, object state)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (callback == null)
				throw new ArgumentNullException ("callback");

			return Task<object>.Factory.StartNew (s => GetData (type), state)
				.ContinueWith (t => callback (t));
		}

		public override object EndGetData (IAsyncResult ares)
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
