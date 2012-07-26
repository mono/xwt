Imports System
Imports Xwt

Namespace Samples
    Public Class MenuSamples
        Inherits VBox

        Private menu As Menu

        Public Sub New()
            Dim la As Label = New Label("Right click here to show the context menu")
            Me.menu = New Menu()
            Me.menu.Items.Add(New MenuItem("One"))
            Me.menu.Items.Add(New MenuItem("Two"))
            Me.menu.Items.Add(New MenuItem("Three"))
            AddHandler la.ButtonPressed, AddressOf Me.HandleButtonPressed
            MyBase.PackStart(la)
        End Sub

        Private Sub HandleButtonPressed(sender As Object, e As ButtonEventArgs)
            If e.Button = PointerButton.Right Then
                Me.menu.Popup()
            End If
        End Sub
    End Class
End Namespace
