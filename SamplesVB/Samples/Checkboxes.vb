
'Checkboxes.vb

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

Namespace Samples
    Public Class Checkboxes
        Inherits VBox

        Public Sub New()
            MyBase.PackStart(New CheckBox("Normal checkbox"))
            MyBase.PackStart(New CheckBox("Mixed to start") With {.Mixed = True})
            Dim b As CheckBox = New CheckBox("Allows mixed") With {.AllowMixed = True}
            MyBase.PackStart(b)
            Dim clicks As Integer = 0
            Dim toggles As Integer = 0
            Dim la As Label = New Label()
            MyBase.PackStart(la)
            AddHandler b.Clicked, Sub()
                                      clicks += 1
                                      la.Text = String.Format("active:{0}, mixed:{1}, clicks:{2}, toggles:{3}", b.Active, b.Mixed, clicks, toggles)
                                  End Sub
            AddHandler b.Toggled, Sub()
                                      toggles += 1
                                      la.Text = String.Format("active:{0}, mixed:{1}, clicks:{2}, toggles:{3}", b.Active, b.Mixed, clicks, toggles)
                                  End Sub
        End Sub
    End Class
End Namespace
