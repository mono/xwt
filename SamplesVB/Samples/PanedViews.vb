
'PanedViews.vb

'Author:
'      Lluis Sanchez <lluis@xamarin.com>
'      Peter Gill <peter@majorsilence.com>

'Copyright (c) 2012 Xamarin Inc

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
    Public Class PanedViews
        Inherits HPaned

        Public Sub New()
            MyBase.Panel1.Content = Me.CreateFrame("Fixed panel at the left", False)

            Dim centralPaned As HPaned = New HPaned()
            centralPaned.Panel1.Content = Me.CreateFrame("Should expand" & vbLf & "horizontally and vertically", True)
            centralPaned.Panel1.Resize = True

            Dim verticalPaned As VPaned = New VPaned()
            verticalPaned.Panel1.Content = Me.CreateFrame("Fixed panel" & vbLf & "at the top", False)

            verticalPaned.Panel2.Content = Me.CreateFrame("Should expand vertically", True)
            verticalPaned.Panel2.Resize = True

            centralPaned.Panel2.Content = verticalPaned

            MyBase.Panel2.Content = centralPaned
            MyBase.Panel2.Resize = True
        End Sub

        Private Function CreateFrame(text As String, fixedp As Boolean) As Frame
            Dim f As Frame = New Frame(FrameType.Custom)
            f.BorderColor = (If(fixedp, New Color(1.0, 0.0, 0.0), New Color(0.0, 0.0, 1.0)))
            f.BorderWidth.SetAll(1.0)
            f.Margin.SetAll(10.0)
            f.Content = New Label(text)
            f.Content.Margin.SetAll(10.0)
            Return f
        End Function
    End Class
End Namespace
