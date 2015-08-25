//
// NSStringAttributeKey.cs
//
// Author:
//       Sebastian Krysmanski <noreply@manski.net>
//       Vsevolod Kukol <sevo@sevo.org>
//
// Copyright (c) 2015 Sebastian Krysmanski
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

using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using System;

namespace Xwt.Mac
{
	public static class NSStringAttributeKey
	{
		private static readonly IntPtr _AppKitHandle = Dlfcn.dlopen ("/System/Library/Frameworks/AppKit.framework/AppKit", 0);

		private static NSString _CharacterShape;
		private static NSString _GlyphInfo;
		private static NSString _NSBaseURLDocumentOption;
		private static NSString _NSCharacterEncodingDocumentOption;
		private static NSString _NSDefaultAttributesDocumentOption;
		private static NSString _NSDocFormatTextDocumentType;
		private static NSString _NSDocumentTypeDocumentOption;
		private static NSString _NSFileTypeDocumentOption;
		private static NSString _NSHTMLTextDocumentType;
		private static NSString _NSMacSimpleTextDocumentType;
		private static NSString _NSOfficeOpenXMLTextDocumentType;
		private static NSString _NSOpenDocumentTextDocumentType;
		private static NSString _NSPlainTextDocumentType;
		private static NSString _NSRtfdTextDocumentType;
		private static NSString _NSRtfTextDocumentType;
		private static NSString _NSTextEncodingNameDocumentOption;
		private static NSString _NSTextSizeMultiplierDocumentOption;
		private static NSString _NSTimeoutDocumentOption;
		private static NSString _NSWebArchiveTextDocumentType;
		private static NSString _NSWebPreferencesDocumentOption;
		private static NSString _NSWebResourceLoadDelegateDocumentOption;
		private static NSString _NSWordMLTextDocumentType;
		private static NSString _SpellingState;
		private static NSString _TextAlternatives;
		private static NSString _TextEffect;

		public static NSString Attachment
		{
			get { return NSAttributedString.AttachmentAttributeName; }
		}

		public static NSString BackgroundColor
		{
			get { return NSAttributedString.BackgroundColorAttributeName; }
		}

		public static NSString BaselineOffset
		{
			get { return NSAttributedString.BaselineOffsetAttributeName; }
		}

		[Field ("NSCharacterShapeAttributeName", "AppKit")]
		public static NSString CharacterShape {
			get {
				if (NSStringAttributeKey._CharacterShape == null) {
					NSStringAttributeKey._CharacterShape = Dlfcn.GetStringConstant (_AppKitHandle, "NSCharacterShapeAttributeName");
				}
				return NSStringAttributeKey._CharacterShape;
			}
		}

		public static NSString Cursor
		{
			get { return NSAttributedString.CursorAttributeName; }
		}

		public static NSString Expansion
		{
			get { return NSAttributedString.ExpansionAttributeName; }
		}

		public static NSString Font
		{
			get { return NSAttributedString.FontAttributeName; }
		}

		public static NSString ForegroundColor
		{
			get { return NSAttributedString.ForegroundColorAttributeName; }
		}

		[Field ("NSGlyphInfoAttributeName", "AppKit")]
		public static NSString GlyphInfo {
			get {
				if (NSStringAttributeKey._GlyphInfo == null) {
					NSStringAttributeKey._GlyphInfo = Dlfcn.GetStringConstant (_AppKitHandle, "NSGlyphInfoAttributeName");
				}
				return NSStringAttributeKey._GlyphInfo;
			}
		}

		public static NSString KerningAdjustment
		{
			get { return NSAttributedString.KernAttributeName; }
		}

		public static NSString Ligature
		{
			get { return NSAttributedString.LigatureAttributeName; }
		}

		public static NSString Link
		{
			get { return NSAttributedString.LinkAttributeName; }
		}

		public static NSString MarkedClauseSegment
		{
			get { return NSAttributedString.MarkedClauseSegmentAttributeName; }
		}

		[Field ("NSBaseURLDocumentOption", "AppKit")]
		internal static NSString NSBaseURLDocumentOption {
			get {
				if (NSStringAttributeKey._NSBaseURLDocumentOption == null) {
					NSStringAttributeKey._NSBaseURLDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSBaseURLDocumentOption");
				}
				return NSStringAttributeKey._NSBaseURLDocumentOption;
			}
		}

		[Field ("NSCharacterEncodingDocumentOption", "AppKit")]
		internal static NSString NSCharacterEncodingDocumentOption {
			get {
				if (NSStringAttributeKey._NSCharacterEncodingDocumentOption == null) {
					NSStringAttributeKey._NSCharacterEncodingDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSCharacterEncodingDocumentOption");
				}
				return NSStringAttributeKey._NSCharacterEncodingDocumentOption;
			}
		}

		[Field ("NSDefaultAttributesDocumentOption", "AppKit")]
		internal static NSString NSDefaultAttributesDocumentOption {
			get {
				if (NSStringAttributeKey._NSDefaultAttributesDocumentOption == null) {
					NSStringAttributeKey._NSDefaultAttributesDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSDefaultAttributesDocumentOption");
				}
				return NSStringAttributeKey._NSDefaultAttributesDocumentOption;
			}
		}

		[Field ("NSDocFormatTextDocumentType", "AppKit")]
		internal static NSString NSDocFormatTextDocumentType {
			get {
				if (NSStringAttributeKey._NSDocFormatTextDocumentType == null) {
					NSStringAttributeKey._NSDocFormatTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSDocFormatTextDocumentType");
				}
				return NSStringAttributeKey._NSDocFormatTextDocumentType;
			}
		}

		[Field ("NSDocumentTypeDocumentOption", "AppKit")]
		internal static NSString NSDocumentTypeDocumentOption {
			get {
				if (NSStringAttributeKey._NSDocumentTypeDocumentOption == null) {
					NSStringAttributeKey._NSDocumentTypeDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSDocumentTypeDocumentOption");
				}
				return NSStringAttributeKey._NSDocumentTypeDocumentOption;
			}
		}

		[Field ("NSFileTypeDocumentOption", "AppKit")]
		internal static NSString NSFileTypeDocumentOption {
			get {
				if (NSStringAttributeKey._NSFileTypeDocumentOption == null) {
					NSStringAttributeKey._NSFileTypeDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSFileTypeDocumentOption");
				}
				return NSStringAttributeKey._NSFileTypeDocumentOption;
			}
		}

		[Field ("NSHTMLTextDocumentType", "AppKit")]
		internal static NSString NSHTMLTextDocumentType {
			get {
				if (NSStringAttributeKey._NSHTMLTextDocumentType == null) {
					NSStringAttributeKey._NSHTMLTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSHTMLTextDocumentType");
				}
				return NSStringAttributeKey._NSHTMLTextDocumentType;
			}
		}

		[Field ("NSMacSimpleTextDocumentType", "AppKit")]
		internal static NSString NSMacSimpleTextDocumentType {
			get {
				if (NSStringAttributeKey._NSMacSimpleTextDocumentType == null) {
					NSStringAttributeKey._NSMacSimpleTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSMacSimpleTextDocumentType");
				}
				return NSStringAttributeKey._NSMacSimpleTextDocumentType;
			}
		}

		[Field ("NSOfficeOpenXMLTextDocumentType", "AppKit")]
		internal static NSString NSOfficeOpenXMLTextDocumentType {
			get {
				if (NSStringAttributeKey._NSOfficeOpenXMLTextDocumentType == null) {
					NSStringAttributeKey._NSOfficeOpenXMLTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSOfficeOpenXMLTextDocumentType");
				}
				return NSStringAttributeKey._NSOfficeOpenXMLTextDocumentType;
			}
		}

		[Field ("NSOpenDocumentTextDocumentType", "AppKit")]
		internal static NSString NSOpenDocumentTextDocumentType {
			get {
				if (NSStringAttributeKey._NSOpenDocumentTextDocumentType == null) {
					NSStringAttributeKey._NSOpenDocumentTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSOpenDocumentTextDocumentType");
				}
				return NSStringAttributeKey._NSOpenDocumentTextDocumentType;
			}
		}

		[Field ("NSPlainTextDocumentType", "AppKit")]
		internal static NSString NSPlainTextDocumentType {
			get {
				if (NSStringAttributeKey._NSPlainTextDocumentType == null) {
					NSStringAttributeKey._NSPlainTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSPlainTextDocumentType");
				}
				return NSStringAttributeKey._NSPlainTextDocumentType;
			}
		}

		[Field ("NSRTFDTextDocumentType", "AppKit")]
		internal static NSString NSRtfdTextDocumentType {
			get {
				if (NSStringAttributeKey._NSRtfdTextDocumentType == null) {
					NSStringAttributeKey._NSRtfdTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSRTFDTextDocumentType");
				}
				return NSStringAttributeKey._NSRtfdTextDocumentType;
			}
		}

		[Field ("NSRTFTextDocumentType", "AppKit")]
		internal static NSString NSRtfTextDocumentType {
			get {
				if (NSStringAttributeKey._NSRtfTextDocumentType == null) {
					NSStringAttributeKey._NSRtfTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSRTFTextDocumentType");
				}
				return NSStringAttributeKey._NSRtfTextDocumentType;
			}
		}

		[Field ("NSTextEncodingNameDocumentOption", "AppKit")]
		internal static NSString NSTextEncodingNameDocumentOption {
			get {
				if (NSStringAttributeKey._NSTextEncodingNameDocumentOption == null) {
					NSStringAttributeKey._NSTextEncodingNameDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSTextEncodingNameDocumentOption");
				}
				return NSStringAttributeKey._NSTextEncodingNameDocumentOption;
			}
		}

		[Field ("NSTextSizeMultiplierDocumentOption", "AppKit")]
		internal static NSString NSTextSizeMultiplierDocumentOption {
			get {
				if (NSStringAttributeKey._NSTextSizeMultiplierDocumentOption == null) {
					NSStringAttributeKey._NSTextSizeMultiplierDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSTextSizeMultiplierDocumentOption");
				}
				return NSStringAttributeKey._NSTextSizeMultiplierDocumentOption;
			}
		}

		[Field ("NSTimeoutDocumentOption", "AppKit")]
		internal static NSString NSTimeoutDocumentOption {
			get {
				if (NSStringAttributeKey._NSTimeoutDocumentOption == null) {
					NSStringAttributeKey._NSTimeoutDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSTimeoutDocumentOption");
				}
				return NSStringAttributeKey._NSTimeoutDocumentOption;
			}
		}

		[Field ("NSWebArchiveTextDocumentType", "AppKit")]
		internal static NSString NSWebArchiveTextDocumentType {
			get {
				if (NSStringAttributeKey._NSWebArchiveTextDocumentType == null) {
					NSStringAttributeKey._NSWebArchiveTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSWebArchiveTextDocumentType");
				}
				return NSStringAttributeKey._NSWebArchiveTextDocumentType;
			}
		}

		[Field ("NSWebPreferencesDocumentOption", "AppKit")]
		internal static NSString NSWebPreferencesDocumentOption {
			get {
				if (NSStringAttributeKey._NSWebPreferencesDocumentOption == null) {
					NSStringAttributeKey._NSWebPreferencesDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSWebPreferencesDocumentOption");
				}
				return NSStringAttributeKey._NSWebPreferencesDocumentOption;
			}
		}

		[Field ("NSWebResourceLoadDelegateDocumentOption", "AppKit")]
		internal static NSString NSWebResourceLoadDelegateDocumentOption {
			get {
				if (NSStringAttributeKey._NSWebResourceLoadDelegateDocumentOption == null) {
					NSStringAttributeKey._NSWebResourceLoadDelegateDocumentOption = Dlfcn.GetStringConstant (_AppKitHandle, "NSWebResourceLoadDelegateDocumentOption");
				}
				return NSStringAttributeKey._NSWebResourceLoadDelegateDocumentOption;
			}
		}

		[Field ("NSWordMLTextDocumentType", "AppKit")]
		internal static NSString NSWordMLTextDocumentType {
			get {
				if (NSStringAttributeKey._NSWordMLTextDocumentType == null) {
					NSStringAttributeKey._NSWordMLTextDocumentType = Dlfcn.GetStringConstant (_AppKitHandle, "NSWordMLTextDocumentType");
				}
				return NSStringAttributeKey._NSWordMLTextDocumentType;
			}
		}

		public static NSString Obliqueness
		{
			get { return NSAttributedString.ObliquenessAttributeName; }
		}

		public static NSString ParagraphStyle
		{
			get { return NSAttributedString.ParagraphStyleAttributeName; }
		}

		public static NSString Shadow
		{
			get { return NSAttributedString.ShadowAttributeName; }
		}

		[Field ("NSSpellingStateAttributeName", "AppKit")]
		public static NSString SpellingState {
			get {
				if (NSStringAttributeKey._SpellingState == null) {
					NSStringAttributeKey._SpellingState = Dlfcn.GetStringConstant (_AppKitHandle, "NSSpellingStateAttributeName");
				}
				return NSStringAttributeKey._SpellingState;
			}
		}

		public static NSString StrikethroughColor
		{
			get { return NSAttributedString.StrikethroughColorAttributeName; }
		}

		public static NSString StrikethroughStyle
		{
			get { return NSAttributedString.StrikethroughStyleAttributeName; }
		}

		public static NSString StrokeColor
		{
			get { return NSAttributedString.StrokeColorAttributeName; }
		}

		public static NSString StrokeWidth
		{
			get { return NSAttributedString.StrokeWidthAttributeName; }
		}

		public static NSString Superscript
		{
			get { return NSAttributedString.SuperscriptAttributeName; }
		}

		[Field ("NSTextAlternativesAttributeName", "AppKit")]
		public static NSString TextAlternatives {
			get {
				if (NSStringAttributeKey._TextAlternatives == null) {
					NSStringAttributeKey._TextAlternatives = Dlfcn.GetStringConstant (_AppKitHandle, "NSTextAlternativesAttributeName");
				}
				return NSStringAttributeKey._TextAlternatives;
			}
		}

		[Field ("NSTextEffectAttributeName", "AppKit")]
		public static NSString TextEffect {
			get {
				if (NSStringAttributeKey._TextEffect == null) {
					NSStringAttributeKey._TextEffect = Dlfcn.GetStringConstant (_AppKitHandle, "NSTextEffectAttributeName");
				}
				return NSStringAttributeKey._TextEffect;
			}
		}

		public static NSString ToolTip
		{
			get { return NSAttributedString.ToolTipAttributeName; }
		}

		public static NSString UnderlineColor
		{
			get { return NSAttributedString.UnderlineColorAttributeName; }
		}

		public static NSString UnderlineStyle
		{
			get { return NSAttributedString.UnderlineStyleAttributeName; }
		}

		public static NSString VerticalGlyphForm
		{
			get { return NSAttributedString.VerticalGlyphFormAttributeName; }
		}

		public static NSString WritingDirection
		{
			get { return NSAttributedString.WritingDirectionAttributeName; }
		}
	}
}
