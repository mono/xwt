// 
// ClipboardBackend.cs
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
using System.Collections.Generic;
using System.Threading;


namespace Xwt.GtkBackend
{
	public class GtkClipboardBackend: ClipboardBackend
	{
//		Gtk.Clipboard primaryClipboard;
		Gtk.Clipboard clipboard;
		
		public GtkClipboardBackend ()
		{
			clipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("CLIPBOARD", false));
//			primaryClipboard = Gtk.Clipboard.Get (Gdk.Atom.Intern ("PRIMARY", false));
		}

		#region IClipboardBackend implementation
		public override void Clear ()
		{
			clipboard.Clear ();
		}

		public override void SetData (TransferDataType type, Func<object> dataSource)
		{
			clipboard.SetWithData ((Gtk.TargetEntry[])Util.BuildTargetTable (new TransferDataType[] { type }), 
			  delegate (Gtk.Clipboard cb, Gtk.SelectionData data, uint id) {
				TransferDataType ttype = Util.AtomToType (data.Target.Name);
				if (ttype == type)
					Util.SetSelectionData (data, data.Target.Name, dataSource ());
			},
			delegate {
			});
		}

		public override bool IsTypeAvailable (TransferDataType type)
		{
			if (type == TransferDataType.Text)
				return clipboard.WaitIsTextAvailable ();
			if (type == TransferDataType.Image)
				return clipboard.WaitIsImageAvailable ();
			
			foreach (var at in GetAtomsForType (type)) {
				if (clipboard.WaitIsTargetAvailable (at))
					return true;
			}
			return false;
		}
		
		IEnumerable<Gdk.Atom> GetAtomsForType (TransferDataType type)
		{
			foreach (Gtk.TargetEntry te in (Gtk.TargetEntry[])Util.BuildTargetTable (new TransferDataType[] { type }))
				yield return Gdk.Atom.Intern (te.Target, false);
		}

		public override object GetData (TransferDataType type)
		{
			if (type == TransferDataType.Text)
				return clipboard.WaitForText ();
			if (type == TransferDataType.Image)
				return ApplicationContext.Toolkit.WrapImage (new GtkImage (clipboard.WaitForImage()));
			
			TransferDataStore store = new TransferDataStore ();
			
			foreach (var at in GetAtomsForType (type)) {
				var data = clipboard.WaitForContents (at);
				Util.GetSelectionData (ApplicationContext, data, store);
			}
			return ((ITransferData)store).GetValue (type);
		}

		public override IAsyncResult BeginGetData (TransferDataType type, AsyncCallback callback, object state)
		{
			var atts = GetAtomsForType (type).ToArray ();
			return new DataRequest (ApplicationContext, clipboard, callback, state, type, atts);
		}

		public override object EndGetData (IAsyncResult ares)
		{
			return ((DataRequest)ares).Result;
		}
		
		#endregion
	}
	
	class DataRequest: IAsyncResult
	{
		Gtk.Clipboard clipboard;
		Gdk.Atom[] atoms;
		int index = 0;
		ManualResetEvent doneEvent;
		bool complete;
		TransferDataType type;
		AsyncCallback callback;
		ApplicationContext context;
		
		public DataRequest (ApplicationContext context, Gtk.Clipboard clipboard, AsyncCallback callback, object state, TransferDataType type, Gdk.Atom[] atoms)
		{
			this.context = context;
			this.callback = callback;
			this.type = type;
			AsyncState = state;
			this.atoms = atoms;
			this.clipboard = clipboard;
			RequestData ();
		}
		
		public object Result { get; set; }
		
		void RequestData ()
		{
			clipboard.RequestContents (atoms[index], DataReceived);
		}
		
		void DataReceived (Gtk.Clipboard cb, Gtk.SelectionData data)
		{
			TransferDataStore store = new TransferDataStore ();
			if (Util.GetSelectionData (context, data, store)) {
				Result = ((ITransferData)store).GetValue (type);
				SetComplete ();
			} else {
				if (++index < atoms.Length)
					RequestData ();
				else
					SetComplete ();
			}
		}
		
		void SetComplete ()
		{
			lock (atoms) {
				complete = true;
				if (doneEvent != null)
					doneEvent.Set ();
			}
			if (callback != null) {
				Application.Invoke (delegate {
					callback (this);
				});
			}
		}
		
		#region IAsyncResult implementation
		public object AsyncState { get; set; }

		public WaitHandle AsyncWaitHandle {
			get {
				lock (atoms) {
					if (doneEvent == null)
						doneEvent = new ManualResetEvent (complete);
				}
				return doneEvent;
			}
		}

		public bool CompletedSynchronously {
			get {
				return false;
			}
		}

		public bool IsCompleted {
			get {
				return complete;
			}
		}
		#endregion
	}
}

