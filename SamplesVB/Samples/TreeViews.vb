
'TreeViews.vb

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
    Public Class TreeViews
        Inherits VBox

        Private text As DataField(Of String) = New DataField(Of String)()

        Private desc As DataField(Of String) = New DataField(Of String)()

        Public Sub New()
            Dim view As TreeView = New TreeView()
            Dim store As TreeStore = New TreeStore(Me.text, Me.desc)
            view.Columns.Add("Item", Me.text)
            view.Columns.Add("Desc", Me.desc)
            store.AddNode().SetValue(Of String)(Me.text, "One").SetValue(Of String)(Me.desc, "First")
            store.AddNode().SetValue(Of String)(Me.text, "Two").SetValue(Of String)(Me.desc, "Second").AddChild().SetValue(Of String)(Me.text, "Sub two").SetValue(Of String)(Me.desc, "Sub second")
            store.AddNode().SetValue(Of String)(Me.text, "Three").SetValue(Of String)(Me.desc, "Third").AddChild().SetValue(Of String)(Me.text, "Sub three").SetValue(Of String)(Me.desc, "Sub third")
            MyBase.PackStart(view)
            view.DataSource = store
            Dim la As Label = New Label()
            MyBase.PackStart(la)
            view.SetDragDropTarget(DragDropAction.All, TransferDataType.Text)
            view.SetDragSource(DragDropAction.All, TransferDataType.Text)
            AddHandler view.DragDrop, Sub(sender As Object, e As DragEventArgs)
                                          Dim pos As RowDropPosition
                                          Dim node As TreePosition
                                          view.GetDropTargetRow(e.Position.X, e.Position.Y, pos, node)
                                          Dim nav As TreeNavigator = store.GetNavigatorAt(node)
                                          la.Text = "Dropped """ & e.Data.Text & """ into """ & nav.GetValue(Of String)(Me.text) & """ " & pos & vbLf
                                          e.Success = True
                                      End Sub

            AddHandler view.DragOver, Sub(sender As Object, e As DragOverEventArgs)
                                          Dim pos As RowDropPosition
                                          Dim node As TreePosition
                                          view.GetDropTargetRow(e.Position.X, e.Position.Y, pos, node)
                                          If pos = RowDropPosition.Into Then
                                              e.AllowedAction = DragDropAction.None
                                          Else
                                              e.AllowedAction = e.Action
                                          End If
                                      End Sub

            AddHandler view.DragStarted, Sub(sender As Object, e As DragStartedEventArgs)
                                             Dim val As String = store.GetNavigatorAt(view.SelectedRow).GetValue(Of String)(Me.text)
                                             e.DragOperation.Data.AddValue(Of String)(val)
                                             AddHandler e.DragOperation.Finished, Sub(s As Object, args As DragFinishedEventArgs)
                                                                                      Console.WriteLine("D:" & args.DeleteSource)
                                                                                  End Sub
                                         End Sub

            Dim addButton As Button = New Button("Add")
            AddHandler addButton.Clicked, Sub(sender As Object, e As EventArgs)
                                              store.AddNode().SetValue(Of String)(Me.text, "Added").SetValue(Of String)(Me.desc, "Desc")
                                          End Sub
            MyBase.PackStart(addButton)
            Dim removeButton As Button = New Button("Remove Selection")
            AddHandler removeButton.Clicked, Sub(sender As Object, e As EventArgs)
                                                 Dim selectedRows As TreePosition() = view.SelectedRows
                                                 For i As Integer = 0 To selectedRows.Length - 1
                                                     Dim row As TreePosition = selectedRows(i)
                                                     store.GetNavigatorAt(row).Remove()
                                                 Next
                                             End Sub
            MyBase.PackStart(removeButton)
        End Sub

        Private Sub HandleDragOver(sender As Object, e As DragOverEventArgs)
            e.AllowedAction = e.Action
        End Sub
    End Class
End Namespace
