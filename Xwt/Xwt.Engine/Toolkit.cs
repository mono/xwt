// 
// Toolkit.cs
//  
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc
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

namespace Xwt.Engine
{
	public static class Toolkit
	{
		static int inUserCode;
		static Queue<Action> exitActions = new Queue<Action> ();
		
		public static bool Invoke (Action a)
		{
			try {
				EnterUserCode ();
				a ();
				ExitUserCode (null);
				return true;
			} catch (Exception ex) {
				ExitUserCode (ex);
				return false;
			}
		}
		
		public static void InvokePlatformCode (Action a)
		{
			try {
				ExitUserCode (null);
				a ();
			} finally {
				EnterUserCode ();
			}
		}
		
		public static void EnterUserCode ()
		{
			inUserCode++;
		}
		
		public static void ExitUserCode (Exception error)
		{
			if (error != null) {
				Invoke (delegate {
					Application.NotifyException (error);
				});
			}
			if (inUserCode == 1) {
				while (exitActions.Count > 0) {
					try {
						exitActions.Dequeue ()();
					} catch (Exception ex) {
						Invoke (delegate {
							Application.NotifyException (ex);
						});
					}
				}
			}
			inUserCode--;
		}
		
		public static void QueueExitAction (Action a)
		{
			exitActions.Enqueue (a);
		}
	}
}

