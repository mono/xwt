//
// Binding.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
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

namespace Xwt
{
	public class Binding: IBinding
	{
		public T GetValue<T> (object instance)
		{
			return (T)((IBinding)this).GetValue (instance, typeof(T));
		}

		object IBinding.GetValue (object instance, Type type)
		{
			return OnGetValue (type, instance);
		}

		public void SetValue (object instance, object value)
		{
			OnSetValue (instance, value);
		}

		protected virtual object OnGetValue (Type type, object instance)
		{
			return null;
		}

		protected virtual void OnSetValue (object instance, object value)
		{
		}
	}

	public class PropertyBinding<T>: Binding
	{
		public PropertyBinding (string propertyName)
		{
		}

		public PropertyBinding (System.Linq.Expressions.Expression<Func<T,object>> exp)
		{
		}
	}

	public class CustomBinding: Binding {
		public Func<object,object> Getter { get; set; }
		public Action<object,object> Setter { get; set; }
	}

	public class CustomBinding<T>: Binding
	{
		public Func<T,object> Getter { get; set; }
		public Action<T,object> Setter { get; set; }
	}

	public interface IBinding
	{
		object GetValue (object instance, Type type);
		void SetValue (object instance, object value);
	}
}

