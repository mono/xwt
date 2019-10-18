//
// FileDialogBackend.cs
//  
// Author:
//       James Clancey <clancey@xamarin.com>
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
using System.Linq;
using AppKit;
using Foundation;
using ObjCRuntime;
using Xwt.Backends;

namespace Xwt.Mac
{
	public abstract class FileDialogBackend : IFileDialogBackend
	{
		NSSavePanel panel;

		protected NSSavePanel Panel {
			get {
				if (panel == null) {
					//TODO: since we use static out-of-proc panels, we should store initial values and reset on Cleanup
					panel = GetFilePanel ();
				}
				return panel;
			}
		}

		#region IFileDialogBackend implementation
		public void Initialize (System.Collections.Generic.IEnumerable<FileDialogFilter> filters, bool multiselect, string initialFileName)
		{
			if (!string.IsNullOrEmpty (initialFileName))
				Panel.DirectoryUrl = new NSUrl (initialFileName, true);

			Panel.Prompt = Application.TranslationCatalog.GetPluralString ("Select File", "Select Files", multiselect ? 2 : 1);

			OnInitialize (filters, multiselect, initialFileName);
		}

		protected virtual void OnInitialize (System.Collections.Generic.IEnumerable<FileDialogFilter> filters, bool multiselect, string initialFileName)
		{
		}

		protected abstract NSSavePanel GetFilePanel ();

		public bool Run (IWindowFrameBackend parent)
		{
			var returnValue = Panel.RunModal ();
			if (parent != null) {
				var win = parent as NSWindow ?? Runtime.GetNSObject (parent.NativeHandle) as NSWindow;
				if (win != null) {
					win.MakeKeyAndOrderFront (win);
				}
			}
			return returnValue == 1;
		}

		public void Cleanup ()
		{
			//TODO: restore shared panel properties
		}

		public string Title {
			get {
				return Panel.Title;
			}
			set {
				Panel.Title = value;
			}
		}

		public string FileName {
			get {
				return Panel.Url == null ? string.Empty : Panel.Url.Path;
			}
		}

		public string [] FileNames {
			get {
				var openPanel = Panel as NSOpenPanel;
				if (openPanel != null) {
					return openPanel.Urls.Length == 0 ? new string [0] : openPanel.Urls.Select (x => x.Path).ToArray ();
				}
				return new string [] { FileName };
			}
		}

		public string CurrentFolder {
			get {
				return Panel.DirectoryUrl.AbsoluteString;
			}
			set {
				Panel.DirectoryUrl = new NSUrl (value, true);
			}
		}

		public FileDialogFilter ActiveFilter {
			get {
				return null;
			}
			set {

			}
		}

		#endregion

		#region IBackend implementation

		public void InitializeBackend (object frontend, ApplicationContext context)
		{

		}

		public void EnableEvent (object eventId)
		{

		}

		public void DisableEvent (object eventId)
		{

		}

		#endregion
	}
}

