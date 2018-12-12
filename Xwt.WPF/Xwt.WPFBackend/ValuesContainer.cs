//
// ValuesContainer.cs
//
// Author:
//       Eric Maupin <ermau@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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
using System.ComponentModel;

namespace Xwt.WPFBackend
{
	internal class ValuesContainer
		: INotifyPropertyChanged
	{
		internal ValuesContainer (int size)
		{
			if (size < 0)
				throw new ArgumentOutOfRangeException ();

			this.values = new object[size];
		}

		internal ValuesContainer (object[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			this.values = values;
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;

		public object this[int index] {
			get { return this.values [index]; }
			set
			{
				this.values [index] = value;
				OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
			}
		}

		protected readonly object[] values;

		protected void OnPropertyChanged (PropertyChangedEventArgs e)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler (this, e);
		}

		/// <summary>
		/// ToString is used by the default ComboBoxItem automation peer to get the automation Name for the item.
		/// Normally, we want to be the same as the text in the combo, the first value.
		/// </summary>
		public override string ToString ()
		{
			if (values.Length > 0 && values[0] is string)
				return (string) values[0];
			else return base.ToString ();
		}
	}
}
