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
		public Command (string id) : this (id, id)
		{
		}

		public Command (string label, Image icon) : this (label, label, icon)
		{
		}

		public Command (string id, string label, Image icon) : this (id, label)
		{
			Icon = icon;
		}

		public Command (string id, string label)
		{
			Id = id;
			Label = label;
		}
		
		public string Id { get; private set; }
		
		public string Label { get; private set; }
		
		public Image Icon { get; private set; }
		
		public bool IsStockButton { get; private set; }
		
		public static Command Ok = new Command ("Ok", Application.TranslationCatalog.GetString("Ok"));
		public static Command Cancel = new Command ("Cancel", Application.TranslationCatalog.GetString("Cancel"));
		public static Command Yes = new Command ("Yes", Application.TranslationCatalog.GetString("Yes"));
		public static Command No = new Command ("No", Application.TranslationCatalog.GetString("No"));
		public static Command Close = new Command ("Close", Application.TranslationCatalog.GetString("Close"));
		public static Command Delete = new Command ("Delete", Application.TranslationCatalog.GetString("Delete"));
		public static Command Add = new Command ("Add", Application.TranslationCatalog.GetString("Add"));
		public static Command Remove = new Command ("Remove", Application.TranslationCatalog.GetString("Remove"));
		public static Command Clear = new Command ("Clear", Application.TranslationCatalog.GetString("Clear"));
		public static Command Copy = new Command ("Copy", Application.TranslationCatalog.GetString("Copy"));
		public static Command Cut = new Command ("Cut", Application.TranslationCatalog.GetString("Cut"));
		public static Command Paste = new Command ("Paste", Application.TranslationCatalog.GetString("Paste"));
		public static Command Save = new Command ("Save", Application.TranslationCatalog.GetString("Save"));
		public static Command SaveAs = new Command ("SaveAs", Application.TranslationCatalog.GetString("Save As"));
		public static Command Stop = new Command ("Stop", Application.TranslationCatalog.GetString("Stop"));
		public static Command Apply = new Command ("Apply", Application.TranslationCatalog.GetString("Apply"));

		public override string ToString ()
		{
			return Id;
		}
	}
}

