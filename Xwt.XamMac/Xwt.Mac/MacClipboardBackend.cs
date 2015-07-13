//
// MacClipboardBackend.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.IO;
using System.Linq;
using Xwt.Backends;

#if MONOMAC
using nint = System.Int32;
using nfloat = System.Single;
using MonoMac.Foundation;
using MonoMac.AppKit;
#else
using Foundation;
using AppKit;
#endif

namespace Xwt.Mac
{
	public class MacClipboardBackend: ClipboardBackend
	{
		PasteboardOwner owner;

		public MacClipboardBackend ()
		{
			owner = new PasteboardOwner ();
		}

		#region implemented abstract members of ClipboardBackend

		public override void Clear ()
		{
			NSPasteboard.GeneralPasteboard.ClearContents ();
		}

		public override void SetData (TransferDataType type, Func<object> dataSource)
		{
			var pboard = NSPasteboard.GeneralPasteboard;
			pboard.ClearContents ();
			owner.DataSource = dataSource;
			pboard.AddTypes (new[] { type.ToUTI () }, owner);
		}

		public override bool IsTypeAvailable (TransferDataType type)
		{
			return NSPasteboard.GeneralPasteboard.Types.Contains (type.ToUTI ());
		}

		public override object GetData (TransferDataType type)
		{
			if (type == TransferDataType.Uri)
				return (Uri)NSUrl.FromPasteboard (NSPasteboard.GeneralPasteboard);

			var data = NSPasteboard.GeneralPasteboard.GetDataForType (type.ToUTI ());
			if (data == null)
				return null;

			if (type == TransferDataType.Text)
				return data.ToString ();
			if (type == TransferDataType.Image)
				return new NSImage (data);

			unsafe {
				var bytes = new byte [data.Length];
				using (var stream = new UnmanagedMemoryStream ((byte*)data.Bytes, bytes.Length))
					stream.Read (bytes, 0, bytes.Length);
				return TransferDataSource.DeserializeValue (bytes);
			}
		}

		public override IAsyncResult BeginGetData (TransferDataType type, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public override object EndGetData (IAsyncResult ares)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}

	[Register ("XwtPasteboardOwner")]
	class PasteboardOwner : NSObject
	{
		public Func<object> DataSource {
			get;
			set;
		}

		[Export ("pasteboard:provideDataForType:")]
		public void ProvideData (NSPasteboard pboard, NSString type)
		{
			NSData data;
			var obj = DataSource ();
			if (obj is NSImage)
				data = ((NSImage)obj).AsTiff ();
			else if (obj is Uri)
				data = NSData.FromUrl ((NSUrl)((Uri)obj));
			else if (obj is string)
				data = NSData.FromString ((string)obj);
			else
				data = NSData.FromArray (TransferDataSource.SerializeValue (obj));
			pboard.SetDataForType (data, type);
		}
	}
}

