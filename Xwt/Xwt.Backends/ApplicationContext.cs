//
// ToolkitContext.cs
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


namespace Xwt.Backends
{
	/// <summary>
	/// The Application context is used to manage transitions between user handlers and internal Xwt code.
	/// </summary>
	public class ApplicationContext
	{
		Toolkit toolkit;

		/// <summary>
		/// Initializes a new <see cref="Xwt.Backends.ApplicationContext"/> for a <see cref="Xwt.Toolkit"/>.
		/// </summary>
		/// <param name="toolkit">The toolkit.</param>
		internal ApplicationContext (Toolkit toolkit)
		{
			this.toolkit = toolkit;
		}

		/// <summary>
		/// Invokes the user handler on the main GUI thread.
		/// </summary>
		/// <returns><c>true</c>, if user code was invoked successfully, <c>false</c> otherwise.</returns>
		/// <param name="a">The action to invoke as user code.</param>
		/// <remarks>
		/// The return value indicates whether the user code was executed without exceptions (<c>true</c>).
		/// The user can handle the exceptions by subscribing the <see cref="Xwt.Application.UnhandledException"/> event.
		/// </remarks>
		public bool InvokeUserCode (Action a)
		{
			return toolkit.Invoke (a);
		}

		/// <summary>
		/// Enters the user code.
		/// </summary>
		/// <remarks>EnterUserCode must be called before executing any user code.</remarks>
		public void EnterUserCode ()
		{
			toolkit.EnterUserCode ();
		}

		/// <summary>
		/// Exits the user code.
		/// </summary>
		/// <param name="error">Exception thrown during user code execution, or <c>null</c></param>
		public void ExitUserCode (Exception error)
		{
			toolkit.ExitUserCode (error);
		}

		/// <summary>
		/// Gets the toolkit using this <see cref="Xwt.Backends.ApplicationContext"/>.
		/// </summary>
		/// <value>The toolkit.</value>
		public Toolkit Toolkit {
			get { return toolkit; }
		}
	}
}

