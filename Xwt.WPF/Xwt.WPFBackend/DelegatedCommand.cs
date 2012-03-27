//
// DelegatedCommand.cs
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
using System.Collections.Generic;
using System.Windows.Input;

namespace Xwt.WPFBackend
{
	internal class DelegatedCommand<T>
		: DelegatedCommand
	{
		public DelegatedCommand (Action<T> execute)
			: base (s => execute ((T)s))
		{
			if (execute == null)
				throw new ArgumentNullException ("execute");
		}

		public DelegatedCommand (Action<T> execute, Func<T, bool> canExecute)
			: base (s => execute ((T)s), s => canExecute ((T)s))
		{
			if (execute == null)
				throw new ArgumentNullException ("execute");
			if (canExecute == null)
				throw new ArgumentNullException ("canExecute");
		}
	}

	internal class DelegatedCommand
		: ICommand
	{
		public DelegatedCommand (Action<object> execute)
			: this (execute, s => true)
		{
		}

		public DelegatedCommand (Action<object> execute, Func<object, bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException ("execute");
			if (canExecute == null)
				throw new ArgumentNullException ("canExecute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (this.listeners.Count == 0)
					CommandManager.RequerySuggested += RequerySuggested;

				this.listeners.Add (value);
			}

			remove
			{
				this.listeners.Remove (value);

				if (this.listeners.Count == 0)
					CommandManager.RequerySuggested -= RequerySuggested;
			}
		}

		public void Execute (object parameter)
		{
			this.execute (parameter);
		}

		public bool CanExecute (object parameter)
		{
			return this.canExecute (parameter);
		}

		public void NotifyExecutabilityChanged()
		{
			for (int i = 0; i < this.listeners.Count; ++i)
				this.listeners [i] (this, EventArgs.Empty);
		}

		private readonly Action<object> execute;
		private readonly Func<object, bool> canExecute;

		private readonly List<EventHandler> listeners = new List<EventHandler> ();

		private void RequerySuggested (object sender, EventArgs eventArgs)
		{
			NotifyExecutabilityChanged();
		}
	}
}