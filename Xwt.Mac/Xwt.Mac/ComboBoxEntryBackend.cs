// 
// ComboBoxEntryBackend.cs
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
using Xwt.Backends;
using MonoMac.AppKit;
using MonoMac.Foundation;


namespace Xwt.Mac
{
	public class ComboBoxEntryBackend: ViewBackend<NSComboBox,IComboBoxEventSink>, IComboBoxEntryBackend
	{
		IListDataSource source;
		ComboDataSource tsource;
		TextEntryBackend entryBackend;
		int textColumn;
		
		public ComboBoxEntryBackend ()
		{
		}
		
		public override void Initialize ()
		{
			base.Initialize ();
			ViewObject = new MacComboBox (EventSink, ApplicationContext);
			Widget.SizeToFit ();
		}
		
		protected override Size GetNaturalSize ()
		{
			var s = base.GetNaturalSize ();
			return new Size (EventSink.GetDefaultNaturalSize ().Width, s.Height);
		}

		#region IComboBoxEntryBackend implementation
		public ITextEntryBackend TextEntryBackend {
			get {
				if (entryBackend == null)
					entryBackend = new Xwt.Mac.TextEntryBackend ((MacComboBox)ViewObject);
				return entryBackend;
			}
		}
		#endregion

		#region IComboBoxBackend implementation
		public void SetViews (CellViewCollection views)
		{
		}

		public void SetSource (IListDataSource source, IBackend sourceBackend)
		{
			this.source = source;
			tsource = new ComboDataSource (source);
			tsource.TextColumn = textColumn;
			Widget.UsesDataSource = true;
			Widget.DataSource = tsource;
		}

		public int SelectedRow {
			get {
				return Widget.SelectedIndex;
			}
			set {
				Widget.SelectItem (value);
			}
		}
		
		public void SetTextColumn (int column)
		{
			textColumn = column;
			if (tsource != null)
				tsource.TextColumn = column;
		}
		#endregion
	}
	
	class MacComboBox: NSComboBox, IViewObject
	{
		IComboBoxEventSink eventSink;
		ITextEntryEventSink entryEventSink;
		ApplicationContext context;
		
		public MacComboBox (IComboBoxEventSink eventSink, ApplicationContext context)
		{
			this.context = context;
			this.eventSink = eventSink;
		}
		
		public void SetEntryEventSink (ITextEntryEventSink entryEventSink)
		{
			this.entryEventSink = entryEventSink;
		}
		
		public NSView View {
			get {
				return this;
			}
		}

		public Widget Frontend { get; set; }
		
		public override void DidChange (MonoMac.Foundation.NSNotification notification)
		{
			base.DidChange (notification);
			if (entryEventSink != null) {
				context.InvokeUserCode (delegate {
					entryEventSink.OnChanged ();
				});
			}
		}
	}
	
	class ComboDataSource: NSComboBoxDataSource
	{
		IListDataSource source;
		
		public int TextColumn;
		
		public ComboDataSource (IListDataSource source)
		{
			this.source = source;
		}
		
		public override NSObject ObjectValueForItem (NSComboBox comboBox, int index)
		{
			return NSObject.FromObject (source.GetValue (index, TextColumn));
		}
		
		public override int ItemCount (NSComboBox comboBox)
		{
			return source.RowCount;
		}
	}
}

