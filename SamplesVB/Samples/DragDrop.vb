
'DragDrop.vb

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
    Public Class DragDrop
        Inherits VBox

        Private b2 As Button

        Public Sub New()
            Dim box As HBox = New HBox()
            Dim b1 As SimpleBox = New SimpleBox(30.0)
            box.PackStart(b1, BoxMode.None)
            Me.b2 = New Button("Drop here")
            box.PackEnd(Me.b2, BoxMode.None)
            AddHandler b1.ButtonPressed, Sub()
                                             Dim d As DragOperation = b1.CreateDragOperation()
                                             d.Data.AddValue(Of String)("Hola")

                                             Dim ms As New IO.MemoryStream()
                                             SamplesVB.My.Resources._class.Save(ms, System.Drawing.Imaging.ImageFormat.Png)

                                             Dim img As Image = Image.FromStream(ms)
                                             d.SetDragImage(img, CDec((CInt(img.Size.Width))), CDec((CInt(img.Size.Height))))
                                             d.AllowedActions = DragDropAction.All
                                             d.Start()
                                         End Sub

            Me.b2.SetDragDropTarget(TransferDataType.Text, TransferDataType.Uri)
            MyBase.PackStart(box)

            AddHandler Me.b2.DragDrop, AddressOf Me.HandleB2DragDrop
            AddHandler Me.b2.DragOver, AddressOf Me.HandleB2DragOver
        End Sub

        Private Sub HandleB2DragOver(sender As Object, e As DragOverEventArgs)
            If e.Action = DragDropAction.All Then
                e.AllowedAction = DragDropAction.Move
            Else
                e.AllowedAction = e.Action
            End If
        End Sub

        Private Sub HandleB2DragDrop(sender As Object, e As DragEventArgs)
            Console.WriteLine("Dropped! " & e.Action)
            Console.WriteLine("Text: " & e.Data.GetValue(TransferDataType.Text).ToString)
            Console.WriteLine("Uris:")
            Dim uris As Uri() = e.Data.Uris
            For i As Integer = 0 To uris.Length - 1
                Dim u As Uri = uris(i)
                Console.WriteLine("u:" & u.ToString)
            Next
            e.Success = True
            Me.b2.Label = "Dropped!"
        End Sub
    End Class
End Namespace
