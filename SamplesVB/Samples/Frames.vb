
'Frames.vb

'Author:
'      Lluis Sanchez <lluis@xamarin.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2011 Xamarin Inc

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
Imports Xwt.Drawing

Namespace Samples
    Public Class Frames
        Inherits VBox

        Public Sub New()
            Dim f As New Frame()
            f.Label = "Simple widget box"
            f.Content = New SimpleBox(50)
            PackStart(f)

            f = New Frame()
            f.Content = New Label("No label")
            PackStart(f)

            f = New Frame()
            f.Type = FrameType.Custom
            f.BorderWidth.Left = 1
            f.BorderWidth.Top = 2
            f.BorderWidth.Right = 3
            f.BorderWidth.Bottom = 4
            f.BorderColor = New Color(0, 0, 1)
            f.Content = New Label("Custom")
            PackStart(f)

            f = New Frame()
            f.Type = FrameType.Custom
            f.BorderWidth.SetAll(2)
            f.Padding.Left = 10
            f.Padding.Top = 20
            f.Padding.Right = 30
            f.Padding.Bottom = 40
            f.Content = New SimpleBox(50)
            PackStart(f)

            f = New Frame()
            f.Type = FrameType.Custom
            f.BorderWidth.SetAll(2)
            f.Padding.SetAll(10)
            f.Content = New Label("With red background")
            f.BackgroundColor = New Color(1, 0, 0)
            PackStart(f)
        End Sub
    End Class
End Namespace
