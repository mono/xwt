// 
// Command.cs
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
using Xwt.Drawing;

namespace Xwt
{
	public class Command
	{
		public Command (string label) : this (label, label)
		{
		}

		public Command (string label, Image icon) : this (label, label, icon)
		{
		}

		public Command (string label, Accelerator accel) : this (label, label, accel)
		{
		}

		public Command (string label, Image icon, Accelerator accel) : this (label, label, icon, accel)
		{
		}

		public Command (string id, string label, Image icon) : this (id, label, icon, Accelerator.None)
		{
			
		}

		public Command (string id, string label, Image icon, Accelerator accel) : this (id, label, accel)
		{
			Icon = icon;
		}

		public Command (string id, string label) : this (id, label, Accelerator.None)
		{

		}

		public Command (string id, string label, Accelerator accel)
		{
			Id = id;
			Label = label;
			Accelerator = accel;
		}

		public string Id { get; private set; }
		
		public string Label { get; private set; }
		
		public Image Icon { get; private set; }

		public Accelerator Accelerator { get; private set; }
		
		public bool IsStockButton { get; private set; }
		
		public static Command Ok = new Command ("Ok");
		public static Command Cancel = new Command ("Cancel");
		public static Command Yes = new Command ("Yes");
		public static Command No = new Command ("No");
		public static Command Close = new Command ("Close");
		public static Command Delete = new Command ("Delete");
		public static Command Add = new Command ("Add");
		public static Command Remove = new Command ("Remove");
		public static Command Clear = new Command ("Clear");
		public static Command Copy = new Command ("Copy");
		public static Command Cut = new Command ("Cut");
		public static Command Paste = new Command ("Paste");
		public static Command Save = new Command ("Save");
		public static Command SaveAs = new Command ("SaveAs");
		public static Command Stop = new Command ("Stop");
		public static Command Apply = new Command ("Apply");

		public override string ToString ()
		{
			return Id;
		}
	}
}

