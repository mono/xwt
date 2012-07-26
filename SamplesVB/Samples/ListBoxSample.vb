
'ListBoxSample.vb

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
    Public Class ListBoxSample
        Inherits VBox

        Private name As DataField(Of String) = New DataField(Of String)()

        Private icon As DataField(Of Image) = New DataField(Of Image)()

        Public Sub New()
            Dim list As ListBox = New ListBox()
            For i As Integer = 0 To 100 - 1
                list.Items.Add("Value " & i)
            Next
            MyBase.PackStart(list, BoxMode.FillAndExpand)
            Dim customList As ListBox = New ListBox()
            Dim store As ListStore = New ListStore(Me.name, Me.icon)
            customList.DataSource = store
            customList.Views.Add(New ImageCellView(Me.icon))
            customList.Views.Add(New TextCellView(Me.name))

            Dim ms As New IO.MemoryStream()
            SamplesVB.My.Resources._class.Save(ms, System.Drawing.Imaging.ImageFormat.Png)

            Dim png As Image = Image.FromStream(ms)
            For i As Integer = 0 To 100 - 1
                Dim r As Integer = store.AddRow()
                store.SetValue(Of Image)(r, Me.icon, png)
                store.SetValue(Of String)(r, Me.name, "Value " & i)
            Next
            MyBase.PackStart(customList, BoxMode.FillAndExpand)
        End Sub
    End Class
End Namespace
