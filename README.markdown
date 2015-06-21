This document is an introduction to XWT, a cross-platform UI toolkit
for creating desktop applications.

If you have any question about XWT or do you want to contribute
a discussion group for XWT is available here:

http://groups.google.com/group/xwt-list

Introduction
============

Xwt is a new .NET framework for creating desktop applications that run
on multiple platforms from the same codebase.   Xwt works by exposing
one unified API across all environments that is mapped to a set of
native controls on each platform.

This means that Xwt tends to focus on providing controls that will
work across all platforms. However, that doesn't mean that the
functionality available is a common denominator of all platforms.
If a specific feature or widget is not available in the
native framework of a platform, it will be emulated or implemented
as a set of native widgets.

Xwt can be used as a standalone framework to power the entire application
or it can be embedded into an existing host.  This allows developers
to develop their "shell" using native components (for example a Ribbon
on Windows, toolbars on Linux) and use Xwt for specific bits of the
application, like dialog boxes or cross platform surfaces. 

Xwt works by creating an engine at runtime that will map to the
underlying platform.   These are the engines that are supported on
each platform:

* Windows: WPF engine, Gtk engine (using Gtk#)
* MacOS X: Cocoa engine (using MonoMac) and Gtk engine (using Gtk#)
* Linux: Gtk engine (using Gtk#)

This means for example that you can write code for Xwt on Windows that
can be hosted on an existing WPF application (like Visual Studio) or
an existing Gtk# application (like MonoDevelop).   Or on Mac, you can
host Xwt on an existing Cocoa/MonoMac application or you can host it
in our own MonoDevelop IDE.

Getting Started
---------------

Open the Xwt.sln with MonoDevelop (or VisualStudio on Windows) and
build the solution.   You should end up with the libraries that you
can use in your project and a couple of sample applications.

Using Xwt in your app
---------------------

Based on your platform and the backend that you want to use, you need
to pick the libraries that you want to use in your project.

* Windows+WPF: Xwt.dll + Xwt.WPF.dll (requires WPF)
* Windows+Gtk: Xwt.dll + Xwt.Gtk.dll (requires Gtk#)
* Linux+Gtk: Xwt.dll + Xwt.Gtk.dll (requires Gtk#)
* Mac+Gtk: Xwt.dll + Xwt.Gtk.dll (requires Gtk#)
* Mac+Cocoa: Xwt.dll + Xwt.Mac.dll (requires MonoMac.dll)

Hello World
-----------

To write your first application, create an empty .NET project in your
favorite language in MonoDevelop or Visual Studio and reference the
Xwt.dll library. This is the only library that you need to reference
at compile time.

This is the simplest Xwt program you can write:

	using System;
	using Xwt;
	
	class XwtDemo {
		[STAThread]
		static void Main ()
		{
			Application.Initialize (ToolkitType.Gtk);
			var mainWindow = new Window (){
				Title = "Xwt Demo Application",
				Width = 500,
				Height = 400
			};
			mainWindow.Show ();
			Application.Run ();
			mainWindow.Dispose ();
		}
	}

You use the Application.Initialize() method to get the backend
initialized. In this example we are using the Gtk backend. If you
want to use another backend, just change the parameter provided
to the Initialize() method. Also make sure the appropiate backend
DLL is available in the application directory.

Then we create an instance of the Window class, this class exposes two
interesting properties, MainMenu which can be used to set the Window's
main menu and "Content" which is of type "Widget" and allows you to
add some content to the window.

Finally, the Application.Run method is called to get the UI events
processing going.

Widget Class Hierarchy
======================

You will be using widgets to create the contents for your
application.   Xwt.Widget is the abstract base class from which all
the other components are created.  

Some Widgets can contain other widgets, these are container widgets,
and in Xwt those are Canvas, Paned, HBox, VBox and Table.  The first
two implement a box layout system, while the last one implements a
Table layout that allows widgets to be attached to different
anchor-points in a grid.

The layout system uses an auto-sizing system similar to what is
availble in Gtk and HTML allowing the user interface to grow or shrink
based on the contents of the childrens on it.

* XwtComponent 
    * Menu
    * MenuItem
    * Widget
        * Box (Container)
            * HBox (Container)
            * VBox (Container)
        * Button
            * MenuButton
            * ToggleButton
        * Calendar
        * Canvas (Container)
        * Checkbox
        * ComboBox
        * Frame
        * ImageView
        * Label
        * ListView
        * NoteBook
        * Paned (Container)
            * HPaned (Container)
            * VPaned (Container)
        * ProgressBar
        * ScrollView
        * Separator
            * VSeparator
            * HSeparator
        * Table (Container)
        * TextEntry
        * TreeView
    * WindowFrame
        * Window
            * Dialog

For example, the following attaches various labels and data entries to
a Table:

	t = new Table ();
	t.Attach (new Label ("One:"), 0, 1, 0, 1);
	t.Attach (new TextEntry (), 1, 2, 0, 1);
	t.Attach (new Label ("Two:"), 0, 1, 1, 2);
	t.Attach (new TextEntry (), 1, 2, 1, 2);
	t.Attach (new Label ("Three:"), 0, 1, 2, 3);
	t.Attach (new TextEntry (), 1, 2, 2, 3);


The Application Class
=====================

The Application class is a static class that provides services to run
your application.  

Initialization 
--------------

The Application.Initialize API will instruct Xwt to initialize its
binding to the native toolkit. You can pass an optional parameter to
this method that specifies the full type name to load as the backend.

For example, you can force the initialization of the backend to be
specifically Gtk+ or specifically MonoMac based on MacOS.   This is
currently done like this:

	Application.Initialize ("Xwt.GtkBackend.GtkEngine, Xwt.Gtk, Version=1.0.0.0");

or:

	Application.Initialize ("Xwt.Mac.MacEngine, Xwt.Mac, Version=1.0.0.0");

As you saw from the Hello World sample, toplevel windows are created
by creating an instance of the "Xwt.Window" class.   This class
exposes a couple of properties that you can use to spice it up.   The
MainMenu property is used to control the contents of the application
menus while the "Content" property is used to hold a Widget.

Timers
------

The Application.TimeoutInvoke method takes a timespan and a Func<bool>
action method and invokes that method in the main user interface
loop.  

If the provided function returns true, then the timer is restarted,
otherwise the timer ends.

Background Threads
------------------

It is very common to perform tasks in the background and for those
tasks in the background to later update the user interface.   The Xwt
API is not thread safe, which means that calls to the Xwt API must
only be done from the main user interface thread.

This is a trait from the underlying toolkits used by Xwt.

If you want a background thread to run some code on the main loop, you
use the Application.Invoke (Action action) method.   The provided
"action" method is guaranteed to run on the main loop.

