
'Clipboard.vb

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
    Public Class ClipboardSample
        Inherits VBox

        Public Sub New()

            Dim box As HBox = New HBox()
            Dim source As TextEntry = New TextEntry()
            box.PackStart(source)

            Dim b As Button = New Button("Copy")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Clipboard.SetText(source.Text)
                                  End Sub

            MyBase.PackStart(box)
            box = New HBox()
            Dim dest As TextEntry = New TextEntry()
            box.PackStart(dest)
            b = New Button("Paste")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      dest.Text = Clipboard.GetText()
                                  End Sub

            MyBase.PackStart(box)
            MyBase.PackStart(New HSeparator())
            box = New HBox()
            b = New Button("Copy complex object")
            box.PackStart(b)
            Dim n As Integer = 0
            AddHandler b.Clicked, Sub()
                                      Dim obj As New ComplexObject
                                      obj.Data = String.Format("Hello world {0}", n + 1)
                                  End Sub

            MyBase.PackStart(box)

            box = New HBox()
            Dim destComplex As TextEntry = New TextEntry()
            box.PackStart(destComplex)
            b = New Button("Paste complex object")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim ob As ComplexObject = Clipboard.GetData(Of ComplexObject)()
                                      If ob IsNot Nothing Then
                                          destComplex.Text = ob.Data
                                      Else
                                          destComplex.Text = "Data not found"
                                      End If
                                  End Sub

            MyBase.PackStart(box)
        End Sub

    End Class

    <Serializable()>
    Friend Class ComplexObject
        Public Data As String
    End Class

End Namespace
