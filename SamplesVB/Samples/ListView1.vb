
'ListView1.vb

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
Imports Xwt.Drawing

Namespace Samples
    Public Class ListView1
        Inherits VBox

        Private name As DataField(Of String) = New DataField(Of String)()

        Private icon As DataField(Of Image) = New DataField(Of Image)()

        Private text As DataField(Of String) = New DataField(Of String)()

        Private icon2 As DataField(Of Image) = New DataField(Of Image)()

        Public Sub New()
            Dim list As ListView = New ListView()
            Dim store As ListStore = New ListStore(Me.name, Me.icon, Me.text, Me.icon2)
            list.DataSource = store
            list.Columns.Add("Name", {Me.icon, Me.name})
            list.Columns.Add("Text", {Me.icon2, Me.text})
            Dim png As Image = Image.FromResource(GetType(App), "class.png")
            For i As Integer = 0 To 100 - 1
                Dim r As Integer = store.AddRow()
                store.SetValue(Of Image)(r, Me.icon, png)
                store.SetValue(Of String)(r, Me.name, "Value " + i)
                store.SetValue(Of Image)(r, Me.icon2, png)
                store.SetValue(Of String)(r, Me.text, "Text " + i)
            Next
            MyBase.PackStart(list, BoxMode.FillAndExpand)
        End Sub
    End Class
End Namespace
