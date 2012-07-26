Imports System
Imports Xwt

Namespace Samples
    Public Class ClipboardSample
        Inherits VBox

        Public Sub New()
            Dim box As HBox = New HBox()
            Dim source As TextEntry = New TextEntry()
            box.PackStart(source)
            Dim b As Button = New Button("Copy")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Clipboard.SetText(source.Text)
                                  End Sub
            MyBase.PackStart(box)
            box = New HBox()
            Dim dest As TextEntry = New TextEntry()
            box.PackStart(dest)
            b = New Button("Paste")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      dest.Text = Clipboard.GetText()
                                  End Sub
            MyBase.PackStart(box)
            MyBase.PackStart(New HSeparator())
            box = New HBox()
            b = New Button("Copy complex object")
            box.PackStart(b)
            Dim n As Integer = 0
            AddHandler b.Clicked, Sub()
                                      Dim obj As New ComplexObject
                                      obj.Data = String.Format("Hello world {0}", n + 1)
                                  End Sub
            MyBase.PackStart(box)
            box = New HBox()
            Dim destComplex As TextEntry = New TextEntry()
            box.PackStart(destComplex)
            b = New Button("Paste complex object")
            box.PackStart(b)
            AddHandler b.Clicked, Sub()
                                      Dim ob As ComplexObject = Clipboard.GetData(Of ComplexObject)()
                                      If ob IsNot Nothing Then
                                          destComplex.Text = ob.Data
                                      Else
                                          destComplex.Text = "Data not found"
                                      End If
                                  End Sub
            MyBase.PackStart(box)
        End Sub
    End Class

    <Serializable()>
    Friend Class ComplexObject
        Public Data As String
    End Class

End Namespace
