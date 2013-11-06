//
// ToolkitNotInitializedException.cs
//
// Author:
//       Lluis Sanchez <lluis@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
//
using System;

namespace Xwt
{
	public class ToolkitNotInitializedException: Exception
	{
		public ToolkitNotInitializedException (): base ("XWT has not been initialized")
		{
		}
	}
}

