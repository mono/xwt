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
                                             Dim img As Image = Image.FromResource(Me.[GetType](), "class.png")
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
            Console.WriteLine("Dropped! " + e.Action)
            Console.WriteLine("Text: " + e.Data.GetValue(TransferDataType.Text))
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
