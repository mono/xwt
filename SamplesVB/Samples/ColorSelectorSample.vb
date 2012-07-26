Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class ColorSelectorSample
        Inherits VBox

        Public Sub New()
            Dim sel As ColorSelector = New ColorSelector()
            sel.Color = Colors.AliceBlue
            MyBase.PackStart(sel)
        End Sub
    End Class
End Namespace
