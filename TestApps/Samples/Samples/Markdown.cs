using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Xwt;

namespace Samples
{
	class MarkDownSample: VBox
	{
		string MarkDownText
		{
			get
			{
				return @"
# Xamarin Mobile API

Project Site: http://xamarin.com/mobileapi

Overview
========

Xamarin.Mobile is a library that runs on **iPhone**, __Android__ and Windows Phone which abstracts the underlying functionality and exposes a common API that developers can build against:

- Contacts
- GPS/Location
- Camera/Video


# Download / Installation

Download: http://xamarin.com/xamarinmobileapipreview.zip

Installation: Extract the .zip file and copy the *.dll file into your project directory. Then simply ""Add Reference…"" the the *.dll into your MonoTouch or Mono for Android project.

Note: At this time, the Windows Phone version of the library requires the Visual Studio Async CTP (http://msdn.microsoft.com/en-us/vstudio/gg316360). As this CTP installs to a user-specific directory, you'll likely need to correct references to this library in the samples to use them.

# Using the Addon

Contacts API:

	var smiths = new List<Contact>();
	var book = new AddressBook ();
	foreach (Contact contact in book.Where(c => c.LastName == ""Smiths"")) {
		smiths.Add(contact);
	}

Geolocation API:

	var geolocator = new Geolocator { DesiredAccuracy = 50 };
	geolocator.GetPositionAsync (timeout: 10000)
		.ContinueWith (t =>
		{
			Console.WriteLine (""Position Status: {0}"", t.Result.Timestamp);
			Console.WriteLine (""Position Latitude: {0}"", t.Result.Latitude);
			Console.WriteLine (""Position Longitude: {0}"", t.Result.Longitude);
		});

MediaPicker API:

	var picker = new MediaPicker ();
	picker.TakePhotoAsync (new StoreCameraMediaOptions
	{
		Name = ""test.jpg"",
		Directory = ""MediaPickerSample""
	})
	.ContinueWith (t =>
	{
		//Present Image here
	});

Foo API:

```csharp
Console.WriteLine (""Foo!"")
Console.WriteLine (""Testing fenced block support!"")
```

# Documentation

- Technical Docs: http://betaapi.xamarin.com/?link=root:/Xamarin.Mobile


# Contact / Discuss

- Email: support@xamarin.com
- Bugzilla: http://bugzilla.xamarin.com/enter_bug.cgi?product=Xamarin%20Mobile%20API%20Preview
- Suggest features: https://xamarin.uservoice.com/forums/150476-xamarin-mobile-api


# Changelog

## Release 0.4

Features:

 - Includes a build against Mono for Android 4.2

Fixes:

 - Fixed memory leaks in Geolocator
 - Fixed an issue with MediaPicker picking on iPads
 - Fixed an issue with MediaPicker on iOS devices with no camera
 - Fixed an issue with cancelling MediaPicker on iOS devices
 - Fixed an issue with rotation in MediaPicker on Android

Release 0.3
-----------

Features:

 - MediaPicker class, providing asynchronous methods to invoke the system UI for taking and picking photos and video.
 - Windows Phone version of all existing APIs

Enhancements:

 - Improved AddressBook iteration performance on Andriod by 2x
 - Many queries now translate to native queries on Android, improving performance on many simple queries.
 - Removed Contact.PhotoThumbnail
 - Added `Contact.GetThumbnail()`
 - Added `Task<MediaFile> Contact.SaveThumbnailAsync(string)`
 - Added `bool AddressBook.LoadSupported`

Fixes:

 - Fixed an issue where iterating the AddressBook without a query would always return aggregate contacts, regardless of PreferContactAggregation
 - Fixed an AddressBook crash with the latest version of MonoTouch
 - Fixed an occassional exception from Geolocator.GetPositionAsync timeouts

## Release 0.2

Features:

 - iOS and Android AddressBook

## Release 0.1

Features:

 - iOS and Android Geolocator";
			}
		}

		public MarkDownSample()
		{
			var openFileDialog = new OpenFileDialog ("Select File");
			var markdown = new MarkdownView() {
				Markdown = MarkDownText,
				LineSpacing = 3,
			};
			var scrolled = new ScrollView (markdown) {
				MinHeight = 400
			};

			var button = new Button ("Open File");
			button.Clicked += delegate {
				if (openFileDialog.Run (ParentWindow)) {
					markdown.Markdown = File.ReadAllText (openFileDialog.FileName);
				}
			};

			PackStart (button, true);
			PackStart (scrolled, true);
		}
	}
}