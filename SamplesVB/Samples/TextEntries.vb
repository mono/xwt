
'TextEntries.vb

'Author:
'      Peter Gill <peter@majorsilence.com>


'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:

'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.

'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.

Imports System
Imports Xwt

Namespace Samples
	Public Class TextEntries
		Inherits VBox

		Public Sub New()
			Dim te1 As TextEntry = New TextEntry()
			MyBase.PackStart(te1)

			Dim la As Label = New Label()
			MyBase.PackStart(la)
			AddHandler te1.Changed, Sub()
										la.Text = "Text: " + te1.Text
									End Sub

			MyBase.PackStart(New Label("Entry with small font"))
			Dim te2 As TextEntry = New TextEntry()
			te2.Font = te2.Font.WithSize(te2.Font.Size / 2.0)
			MyBase.PackStart(te2)

			MyBase.PackStart(New Label("Entry with placeholder text"))
			MyBase.PackStart(New TextEntry() With {.PlaceholderText = "Placeholder text"})

			MyBase.PackStart(New Label("Entry with no frame"))
			MyBase.PackStart(New TextEntry() With {.ShowFrame = False})
		End Sub
	End Class
End Namespace
