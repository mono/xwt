Imports System
Imports Xwt
Imports Xwt.Design

Namespace Samples
    Public Class Designer
        Inherits VBox

        Public Sub New()
            Dim box As VBox = New VBox()
            Dim b As Button = New Button("Hi there")
            box.PackStart(b)

            Dim la As Label = New Label("Some label")
            box.PackStart(la)

            Dim hb As HBox = New HBox()
            hb.PackStart(New Label("Text"))

            Dim cb As New ComboBox
            cb.Items.Add("One")
            cb.Items.Add("Two")
            cb.SelectedIndex = 0
            hb.PackStart(cb)
            box.PackStart(hb)

            Dim ds As DesignerSurface = New DesignerSurface()
            ds.Load(box)

            MyBase.PackStart(ds, BoxMode.FillAndExpand)
        End Sub
    End Class
End Namespace
