//
// ToolkitNotInitializedException.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
//
using System;
using Mono.Unix;

namespace Xwt
{
	public class ToolkitNotInitializedException: Exception
	{
		public ToolkitNotInitializedException () : base (Catalog.GetString ("XWT has not been initialized"))
		{
		}
	}
}

