Imports System
Imports Xwt

Namespace Samples
    Public Class TextEntries
        Inherits VBox

        Public Sub New()
            Dim te1 As TextEntry = New TextEntry()
            MyBase.PackStart(te1)
            Dim la As Label = New Label()
            MyBase.PackStart(la)
            AddHandler te1.Changed, Sub()
                                        la.Text = "Text: " + te1.Text
                                    End Sub
            MyBase.PackStart(New Label("Entry with small font"))
            Dim te2 As TextEntry = New TextEntry()
            te2.Font = te2.Font.WithSize(te2.Font.Size / 2.0)
            MyBase.PackStart(te2)
            MyBase.PackStart(New Label("Entry with placeholder text"))
            MyBase.PackStart(New TextEntry() With {.PlaceholderText = "Placeholder text"})
            MyBase.PackStart(New Label("Entry with no frame"))
            MyBase.PackStart(New TextEntry() With {.ShowFrame = False})
        End Sub
    End Class
End Namespace
