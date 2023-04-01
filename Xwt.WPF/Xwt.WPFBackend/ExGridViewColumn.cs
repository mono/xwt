using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace Xwt.WPFBackend
{
	public class ExGridViewColumn
		: GridViewColumn
	{
		private readonly BindingSource bindingSource;

		public ExGridViewColumn (Action onWidthUpdated)
		{
			bindingSource = new BindingSource (onWidthUpdated);

			var widthBinding = new Binding (BindingSource.WidthPropertyName) {
				Mode = BindingMode.TwoWay,
				Source = bindingSource,
			};
			BindingOperations.SetBinding (this, WidthProperty, widthBinding);
		}

		public bool Expands { get; set; }

		public bool CanResize {
			get { return bindingSource.CanResize; }
			set { bindingSource.CanResize = value; }
		}

		public void SetWidthForced (double width)
		{
			bindingSource.SetWidthForced (width);
		}

		class BindingSource
			: INotifyPropertyChanged
		{
			public const string WidthPropertyName = nameof (Width);

			private readonly Action onWidthUpdated;
			private double width = double.NaN;

			public BindingSource (Action onWidthUpdated)
			{
				this.onWidthUpdated = onWidthUpdated;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public bool CanResize { get; set; }

			public double Width {
				get {
					return width;
				}
				set {
					if (CanResize) {
						width = value;
						onWidthUpdated ();
					}
					OnPropertyChanged (WidthPropertyName);
				}
			}

			public void SetWidthForced (double width)
			{
				bool savedCanResize = CanResize;
				CanResize = true;
				Width = width;
				CanResize = savedCanResize;
			}

			private void OnPropertyChanged (string name)
			{
				PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
			}
		}
	}
}
