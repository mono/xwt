Imports System
Imports Xwt

Namespace Samples
    Public Class Tooltips
        Inherits VBox

        Public Sub New()
            MyBase.PackStart(New Label("This label has a tooltip") With {.TooltipText = "Hi there!"})
        End Sub
    End Class
End Namespace
