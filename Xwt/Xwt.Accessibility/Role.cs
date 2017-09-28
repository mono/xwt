//
// Role.cs
//
// Author:
//       Vsevolod Kukol <sevoku@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corporation
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
namespace Xwt.Accessibility
{
	public enum Role
	{
		Button,
		ButtonClose,
		ButtonMinimize,
		ButtonMaximize,
		ButtonFullscreen,
		Calendar,
		Cell,
		CheckBox,
		ColorChooser,
		Column,
		ComboBox,
		Custom,
		//Dialog,
		Disclosure, // ATK_ROLE_ARROW
		Filler,
		//Grid,
		Group, //ATK_ROLE_PANEL
		Image,
		Label,
		LevelIndicator, // ATK_ROLE_LEVEL_BAR
		Link,
		List,
		Menu,
		MenuBar,
		MenuBarItem,
		MenuButton, // button with a menu, not a button inside a menu
		//MenuButtonItem,
		MenuItem,
		MenuItemCheckBox,
		MenuItemRadio,
		Notebook, // ATK_ROLE_PAGE_TAB_LIST
		NotebookTab, //ATK_ROLE_PAGE_TAB
		Paned, // ATK_ROLE_SPLIT_PANE
		PanedSplitter,
		Popup,
		ProgressBar,
		RadioButton,
		RadioGroup, // ATK_ROLE_GROUPING
		Row,
		ScrollBar,
		ScrollView,
		Separator,
		Slider,
		SpinButton,
		Table,
		TextArea, // ATK_ROLE_TEXT
		TextEntry, // ATK_ROLE_ENTRY
		TextEntryPassword, // ATK_ROLE_ENTRY
		TextEntrySearch, // ATK_ROLE_ENTRY
		ToggleButton,
		ToolBar,
		ToolTip,
		Tree,
		//Window,
		None = -1,
	}
}
