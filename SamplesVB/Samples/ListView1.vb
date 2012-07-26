Imports System
Imports Xwt
Imports Xwt.Drawing

Namespace Samples
    Public Class ListView1
        Inherits VBox

        Private name As DataField(Of String) = New DataField(Of String)()

        Private icon As DataField(Of Image) = New DataField(Of Image)()

        Private text As DataField(Of String) = New DataField(Of String)()

        Private icon2 As DataField(Of Image) = New DataField(Of Image)()

        Public Sub New()
            Dim list As ListView = New ListView()
            Dim store As ListStore = New ListStore(Me.name, Me.icon, Me.text, Me.icon2)
            list.DataSource = store
            list.Columns.Add("Name", {Me.icon, Me.name})
            list.Columns.Add("Text", {Me.icon2, Me.text})
            Dim png As Image = Image.FromResource(GetType(App), "class.png")
            For i As Integer = 0 To 100 - 1
                Dim r As Integer = store.AddRow()
                store.SetValue(Of Image)(r, Me.icon, png)
                store.SetValue(Of String)(r, Me.name, "Value " + i)
                store.SetValue(Of Image)(r, Me.icon2, png)
                store.SetValue(Of String)(r, Me.text, "Text " + i)
            Next
            MyBase.PackStart(list, BoxMode.FillAndExpand)
        End Sub
    End Class
End Namespace
