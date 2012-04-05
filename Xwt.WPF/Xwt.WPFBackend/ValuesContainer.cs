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
	}
}
